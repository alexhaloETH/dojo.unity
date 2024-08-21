using System.Collections.Generic;
using UnityEngine;

public interface IInteractivityState
{
    void SetInteractivity(bool state);
}

public class UIInteractivityManager : MonoBehaviour
{
    [Tooltip("The first object in the list will be treated as the starting element")]
    public List<GameObject> UiObjects;

    private List<IInteractivityState> _IinteractivityListOfObjects = new();
    
    //enum 
    [Space(20)]
    public OnEnabbleAction onEnableAction = OnEnabbleAction.None;
    public enum OnEnabbleAction { ActivateFromEnable, DisableFromEnable, None }

    public bool EffectOthers = true;
    public bool ReverseEffect = false;

    private void Awake()
    {
        //we do this becasue the Interfaces in unity are actually not serializable so to have them settable from the editor we need to do this 
        _IinteractivityListOfObjects.Clear();

        foreach (var item in UiObjects)
        {
            //if the object is null then continue
            if (item == null) continue;

            //get the component of the object
            var interactivity = item.GetComponent<IInteractivityState>();

            //if the component is null then continue
            if (interactivity == null) continue;

            //add the component to the list
            _IinteractivityListOfObjects.Add(interactivity);
        }
    }

    private void OnEnable()
    {
        Debug.Log("this is on the enable");
        switch (onEnableAction)
        {
            case OnEnabbleAction.ActivateFromEnable:
                ChangeUIInteractivityState(_IinteractivityListOfObjects[0], true);
                break;
            case OnEnabbleAction.DisableFromEnable:
                ChangeUIInteractivityState(_IinteractivityListOfObjects[0], false);
                break;
            case OnEnabbleAction.None:

                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameObject">The main object that dictates</param>
    /// <param name="state">Set the interactivity state</param>
    public void ChangeUIInteractivityState(IInteractivityState gameObject, bool state)
    {
        Debug.Log("this is on the activity change side");
        //go through all the objects
        foreach (var item in _IinteractivityListOfObjects)
        {
            if (item == gameObject)  // if the gameobject we are looking for then
            {
                item.SetInteractivity(state);   //set the state to the one we said   true for example
                continue;
            }
            
            if (EffectOthers)
            {
                item.SetInteractivity(!state);    // this would all be set to false
            }
        }
    }

    public void ChangeInteractivityState(int index, bool state)
    {
        if (index < 0 || index >= _IinteractivityListOfObjects.Count) return;

        ChangeUIInteractivityState(_IinteractivityListOfObjects[index], state);
    }

    public void ActivateGameObject(IInteractivityState button)
    {
        ChangeUIInteractivityState(button, true);
    }

    public void DisableGameObject(IInteractivityState button)
    {
        ChangeUIInteractivityState(button, false);
    }

    public int GetIndex(IInteractivityState button)
    {
        return _IinteractivityListOfObjects.IndexOf(button);
    }

    public void ActivateGameObject(int index)
    {
        ChangeInteractivityState(index, true);
    }

    public void DisableGameObject(int index)
    {
        ChangeInteractivityState(index, false);
    }
}
