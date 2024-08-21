using UnityEngine;
using System;
/// <summary>
/// TODO: Implement in the GrugsLair Tool
/// </summary>
/// 

public class MenuComponent : MonoBehaviour
{
    [Tooltip("Current state of the menu")]
    public bool Open;
    [Tooltip("If returnable is true when the one before is closed this one will show. This is done mainly for sub menus that are not important")]
    public bool Returnable;
    [Tooltip("Name of the menu, for no reference look up")]
    public string MenuName;

    /// <summary>
    /// Will Return true if the menu is open and false if the menu is closed
    /// </summary>
    public event Action<bool> OnMenuState;

    public void OpenMenu()
    {
        gameObject.SetActive(true);
        Open = true;
        OnMenuState?.Invoke(true);
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
        Open = false;
        OnMenuState?.Invoke(false);
    }
}


