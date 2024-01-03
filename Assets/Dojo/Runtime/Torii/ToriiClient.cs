using System;
using System.Collections;
using System.Collections.Generic;
using bottlenoselabs.C2CS.Runtime;
using UnityEngine;
using dojo_bindings;
using JetBrains.Annotations;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Plastic.Newtonsoft.Json;

namespace Dojo.Torii
{
    public unsafe class ToriiClient
    {
        private dojo.FnPtr_FieldElement_CArrayModel_Void.@delegate entityStateUpdateHandler;
        private dojo.FnPtr_Void.@delegate syncModelUpdateHandler;
        private dojo.ToriiClient* client;
        private IntPtr wasmClientPtr;

        public ToriiClient(string toriiUrl, string rpcUrl, string world)
        {
            // if we're in a webgl context
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // intiialize wasm runtime                
                ToriiWasmInterop.WasmBindgen();
                wasmClientPtr = ToriiWasmInterop.CreateClient(toriiUrl, rpcUrl, world);

                return;
            }

            CString ctoriiUrl = CString.FromString(toriiUrl);
            CString crpcUrl = CString.FromString(rpcUrl);
            CString cworld = CString.FromString(world);

            var result = dojo.client_new(ctoriiUrl, crpcUrl, cworld, (dojo.KeysClause*)0, (UIntPtr)0);
            if (result.tag == dojo.Result_____ToriiClient_Tag.Err_____ToriiClient)
            {
                throw new Exception(result.err.message);
            }

            client = result._ok;
        }

        // We assume the torii client won't be copied around.
        // So we can free the underlying c client when the managed client is garbage collected.
        ~ToriiClient()
        {
            dojo.client_free(client);
        }

        public dojo.WorldMetadata WorldMetadata()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                throw new Exception("WorldMetadata is not supported in webgl");

            // TODO: implement a managed type for WorldMetadata too
            dojo.WorldMetadata worldMetadata = dojo.client_metadata(client);

            return worldMetadata;
        }

        [CanBeNull]
        public Ty Model(dojo.KeysClause query)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer) {
                var value = ToriiWasmInterop.GetModelValue(wasmClientPtr, query.model, JsonConvert.SerializeObject(query.keys.ToArray()));
                if (value == null) {
                    return null;
                }

                var model = new Model(result);
                return new Ty(model.ty);
            }

            dojo.Result_COption_____Ty result = dojo.client_model(client, &query);
            if (result.tag == dojo.Result_COption_____Ty_Tag.Err_COption_____Ty)
            {
                throw new Exception(result.err.message);
            }

            // can be None - nullable
            if (result.ok.tag == dojo.COption_____Ty_Tag.None_____Ty)
            {
                return null;
            }

            // we instantiate a new managed Ty object
            // which will free the underlying c ty when it is garbage collected
            return new Ty(result.ok._some);
        }

        public List<Entity> Entities(dojo.Query query)
        {
            dojo.Result_CArray_Entity result = dojo.client_entities(client, &query);
            if (result.tag == dojo.Result_CArray_Entity_Tag.Err_CArray_Entity)
            {
                throw new Exception(result.err.message);
            }

            var entities = new List<Entity>();
            for (var i = 0; i < (int)result._ok.data_len; i++)
            {
                entities.Add(new Entity(result._ok.data[i]));
                // dojo.entity_free(&result._ok.data[i]);
            }

            dojo.carray_free(result._ok.data, result._ok.data_len);
            return entities;
        }

        public ReadOnlySpan<dojo.KeysClause> SubscribedModels()
        {
            dojo.CArray_KeysClause models = dojo.client_subscribed_models(client);
            // NOTE: we could copy the data into a managed array
            // and free the c array from rust.
            // however, it is slower
            // dojo.EntityQuery[] arr = new Span<dojo.EntityQuery>(entities->data, (int)entities->data_len).ToArray();
            // dojo.carray_free(entities);

            // this just returns a span of the carray data
            // freeing the c array is up to the caller
            // dojo.carray_free(entities);
            var arr = new Span<dojo.KeysClause>(models.data, (int)models.data_len).ToArray();
            dojo.carray_free(models.data, models.data_len);
            return arr;
        }

        public void AddModelsToSync(dojo.KeysClause[] models)
        {
            dojo.KeysClause* modelsPtr;

            fixed (dojo.KeysClause* ptr = &models[0])
            {
                modelsPtr = ptr;
            }

            var result = dojo.client_add_models_to_sync(client, modelsPtr, (nuint)models.Length);
            if (result.tag == dojo.Result_bool_Tag.Err_bool)
            {
                throw new Exception(result.err.message);
            }
        }

        public void RemoveModelsToSync(dojo.KeysClause[] models)
        {
            dojo.KeysClause* modelsPtr;

            fixed (dojo.KeysClause* ptr = &models[0])
            {
                modelsPtr = ptr;
            }

            var result = dojo.client_remove_models_to_sync(client, modelsPtr, (nuint)models.Length);
            if (result.tag == dojo.Result_bool_Tag.Err_bool)
            {
                throw new Exception(result.err.message);
            }
        }

        public void RegisterSyncModelUpdates(dojo.KeysClause model, bool dispatchToMainThread = true)
        {
            syncModelUpdateHandler = () =>
            {
                if (dispatchToMainThread)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => ToriiEvents.Instance.SyncModelUpdated());
                }
                else
                {
                    ToriiEvents.Instance.SyncModelUpdated();
                }
            };

            dojo.Result_bool res = dojo.client_on_sync_model_update(client, model, new dojo.FnPtr_Void(syncModelUpdateHandler));
            if (res.tag == dojo.Result_bool_Tag.Err_bool)
            {
                throw new Exception(res.err.message);
            }
        }

        public void RegisterEntityStateUpdates(dojo.FieldElement[] entities, bool dispatchToMainThread = true)
        {
            dojo.FieldElement* entitiesPtr;

            fixed (dojo.FieldElement* ptr = entities)
            {
                entitiesPtr = ptr;
            }

            entityStateUpdateHandler = (key, models) =>
            {
                var mappedModels = new Model[(int)models.data_len];
                for (var i = 0; i < (int)models.data_len; i++)
                {
                    mappedModels[i] = new Model(models.data[i]);
                    // TODO: free the c model
                    // dojo.model_free(&models.data[i]);
                }

                dojo.carray_free(models.data, models.data_len);
                // only run this when in unity play mode
                // we need our unity main thread dispatcher to run this on the main thread
                if (dispatchToMainThread)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => ToriiEvents.Instance.EntityUpdated(key, mappedModels));
                }
                else
                {
                    ToriiEvents.Instance.EntityUpdated(key, mappedModels);
                }
            };


            // dojo.FnPtr_FieldElement_CArrayModel_Void.@delegate callbackHandler = HandleEntityStateUpdate;
            dojo.Result_bool res = dojo.client_on_entity_state_update(client, entitiesPtr, (nuint)entities.Length, new dojo.FnPtr_FieldElement_CArrayModel_Void(entityStateUpdateHandler));
            if (res.tag == dojo.Result_bool_Tag.Err_bool)
            {
                throw new Exception(res.err.message);
            }
        }

        public void OnEntityStateUpdateRaw(dojo.FieldElement[] entities, dojo.FnPtr_FieldElement_CArrayModel_Void.@delegate callbackHandler)
        {
            dojo.FieldElement* entitiesPtr;

            fixed (dojo.FieldElement* ptr = entities)
            {
                entitiesPtr = ptr;
            }

            dojo.Result_bool res = dojo.client_on_entity_state_update(client, entitiesPtr, (nuint)entities.Length, new dojo.FnPtr_FieldElement_CArrayModel_Void(callbackHandler));
            if (res.tag == dojo.Result_bool_Tag.Err_bool)
            {
                throw new Exception(res.err.message);
            }
        }

        public void OnSyncModelUpdateRaw(dojo.KeysClause model, dojo.FnPtr_Void.@delegate callbackHandler)
        {
            dojo.Result_bool res = dojo.client_on_sync_model_update(client, model, new dojo.FnPtr_Void(callbackHandler));
            if (res.tag == dojo.Result_bool_Tag.Err_bool)
            {
                throw new Exception(res.err.message);
            }
        }

        public void StartSubscription()
        {
            var result = dojo.client_start_subscription(client);
            if (result.tag == dojo.Result_bool_Tag.Err_bool)
            {
                throw new Exception(result.err.message);
            }
        }
    }
}