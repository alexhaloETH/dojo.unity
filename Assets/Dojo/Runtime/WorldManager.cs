using System.Linq;
using dojo_bindings;
using UnityEngine;
using Dojo.Torii;

namespace Dojo
{
    public class WorldManager : MonoBehaviour
    {
        [Header("RPC")]
        public string toriiUrl = "http://localhost:8080";
        public string rpcUrl = "http://localhost:5050";
        [Header("World")]
        public string worldAddress;
        public SynchronizationMaster synchronizationMaster;
        public ToriiClient toriiClient;

        // Start is called before the first frame update
        void Awake()
        {
            // create the torii client and start subscription service
            toriiClient = new ToriiClient(toriiUrl, rpcUrl, worldAddress);
            // start subscription service
            toriiClient.StartSubscription();

            // fetch entities from the world
            // TODO: maybe do in the start function of the SynchronizationMaster?
            // problem is when to start the subscription service
            synchronizationMaster.SynchronizeEntities();

            // listen for entity updates
            synchronizationMaster.RegisterEntityCallbacks();
        }

        // Update is called once per frame
        void Update()
        {
        }


        // Get a child entity from the WorldManager game object.
        // Name is usually the hashed_keys of the entity as a hex string.
        public GameObject Entity(string name)
        {
            var entity = transform.Find(name);
            if (entity == null)
            {
                Debug.LogError($"Entity {name} not found");
                return null;
            }

            return entity.gameObject;
        }

        // Return all children entities.
        public GameObject[] Entities()
        {
            return transform.Cast<Transform>()
                .Select(t => t.gameObject)
                .ToArray();
        }

        // Add a new entity game object as a child of the WorldManager game object.
        public GameObject AddEntity(string key)
        {
            // check if entity already exists
            var entity = transform.Find(key)?.gameObject;
            if (entity != null)
            {
                Debug.LogWarning($"Entity {key} already exists");
                return entity.gameObject;
            }

            entity = new GameObject(key);
            entity.transform.parent = transform;

            return entity;
        }

        // Remove an entity game object from the WorldManager game object.
        public void RemoveEntity(string key)
        {
            var entity = transform.Find(key);
            if (entity != null)
            {
                Destroy(entity.gameObject);
            }
        }
    }
}
