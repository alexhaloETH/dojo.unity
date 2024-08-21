using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the opening, closing, and navigation of menus, uses a stack to keep track of open menus.
/// 
/// TODO: Currently there is a bit of the code which checks for the returnable state of the menu type, the code is repeated in multiple places, it can be refactored into a method.
/// </summary>
public class MenuManager : MonoBehaviour
{
    public List<MenuComponent> Menus; // List of all available menus
    private Stack<MenuComponent> _menuStack = new Stack<MenuComponent>(); // Stack to manage open menus

    [Space(20)]
    [Header("Behaviours onEnable")]
    [Tooltip("Menu To Open The moment the object is shown")]
    public MenuComponent InitialMenu;
    [Tooltip("Clear the stack and reset everything when object is shown")]
    public bool ClearStackAtStart = true;

    /// <summary>
    /// Will Return true if a menu is opened and false if a menu is closed
    /// </summary>
    public event Action<bool> OnMenuManagerState;

    private void OnEnable()
    {
        if (ClearStackAtStart)
        {
            ClearStack(true);
        }

        if (InitialMenu != null)
        {
            OpenMenu(InitialMenu);
        }
    }

    /// <summary>
    /// Opens the specified menu.
    /// </summary>
    /// <param name="menu">The menu to open.</param>
    public void OpenMenu(MenuComponent menu)
    {
        // if the menu is already open, close it, similar to toggle
        if (menu.Open)
        {
            CloseMenu(menu);
            return;
        }

        //close the previous menu
        if (_menuStack.Count > 0)
        {
            _menuStack.Peek().CloseMenu();
        }

        //push it into the stack and open it
        _menuStack.Push(menu);

        OnMenuManagerState?.Invoke(true);
        menu.OpenMenu();
    }

    /// <summary>
    /// Opens a menu by its name. When a reference to the menu is not available.
    /// </summary>
    /// <param name="nameOfMenuToOpen">The name of the menu to open.</param>
    public void OpenByName(string nameOfMenuToOpen)
    {
        MenuComponent menu = GetMenu(nameOfMenuToOpen);
        if (menu != null)
        {
            OpenMenu(menu);
        }
    }

    /// <summary>
    /// Clears the menu stack.
    /// </summary>
    /// <param name="closeAll">If true, close all menus in the stack.</param>
    public void ClearStack(bool closeAll)
    {
        if (closeAll)
        {
            while (_menuStack.Count > 0)
            {
                _menuStack.Pop().CloseMenu();
            }
        }
        else
        {
            _menuStack.Clear();
        }
    }

    /// <summary>
    /// Closes the specified menu. To then open the previous one, Menu types that are not returnable will be skipped until a returnable one is found.
    /// </summary>
    /// <param name="menu">The menu to close.</param>
    public void CloseMenu(MenuComponent menu)
    {
        if (menu.Open)
        {
            menu.CloseMenu();

            while (_menuStack.Count > 0)
            {
                MenuComponent topMenu = _menuStack.Peek();
                if (topMenu.Returnable)
                {
                    topMenu.OpenMenu();
                    return;
                }
                else
                {
                    _menuStack.Pop().CloseMenu();
                }
            }

            OnMenuManagerState?.Invoke(false);
        }
    }

    /// <summary>
    /// Closes a menu by its name.
    /// </summary>
    /// <param name="nameOfMenuToClose">The name of the menu to close.</param>
    public void CloseByName(string nameOfMenuToClose)
    {
        MenuComponent menu = GetMenu(nameOfMenuToClose);
        if (menu != null)
        {
            menu.CloseMenu();
        }
    }

    /// <summary>
    /// Closes the last opened menu and optionally opens the previous menu.
    /// </summary>
    /// <param name="openLast">If true, opens the previous menu in the stack.</param>
    ///  /// <param name="skipReturnable">If false, opens the last menu regardless of its returnable state.</param>
    public void CloseLastPage(bool openLast = true, bool skipReturnable = true)
    {
        if (_menuStack.Count == 0)
        {
            return;
        }

        _menuStack.Pop().CloseMenu();

        if (!openLast || _menuStack.Count == 0)
        {
            return;
        }

        MenuComponent previousMenu = _menuStack.Peek();

        if (!skipReturnable)
        {
            previousMenu.OpenMenu();
            return;
        }

        if (previousMenu.Returnable)
        {
            previousMenu.OpenMenu();
            return;
        }
        else
        {
            _menuStack.Pop().CloseMenu();
        }
    }

    /// <summary>
    /// Checks if a menu is open by its name.
    /// </summary>
    /// <param name="nameOfMenuToCheck">The name of the menu to check.</param>
    /// <returns>True if the menu is open, false otherwise.</returns>
    public bool IsOpen(string nameOfMenuToCheck)
    {
        MenuComponent menu = GetMenu(nameOfMenuToCheck);
        return menu != null && menu.Open;
    }

    /// <summary>
    /// Checks if the specified menu is open.
    /// </summary>
    /// <param name="menuToCheck">The menu to check.</param>
    /// <returns>True if the menu is open, false otherwise.</returns>
    public bool IsOpen(MenuComponent menuToCheck)
    {
        return menuToCheck != null && menuToCheck.Open;
    }

    /// <summary>
    /// Gets a menu by its name.
    /// </summary>
    /// <param name="nameOfMenuToGet">The name of the menu to get.</param>
    /// <returns>The menu if found, null otherwise.</returns>
    public MenuComponent GetMenu(string nameOfMenuToGet)
    {
        foreach (MenuComponent menu in Menus)
        {
            if (menu.MenuName == nameOfMenuToGet)
            {
                return menu;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the currently open menu.
    /// </summary>
    /// <returns>The currently open menu if any, null otherwise.</returns>
    public MenuComponent GetCurrentlyOpenMenu()
    {
        return _menuStack.Count > 0 ? _menuStack.Peek() : null;
    }

    /// <summary>
    /// Gets the index of the specified menu in the menus list.
    /// </summary>
    /// <param name="menuToGet">The menu to get the index of.</param>
    /// <returns>The index of the menu if found, -1 otherwise.</returns>
    public int GetMenuIndex(MenuComponent menuToGet)
    {
        for (int i = 0; i < Menus.Count; i++)
        {
            if (Menus[i] == menuToGet)
            {
                return i;
            }
        }
        return -1;
    }
}
