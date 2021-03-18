using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public Action<bool> OnUIToggle;
    public Action OnEnteringUI;
    public Action OnExitingUI;
    public Menu defaultMenu; 
    public TileSensorMenu TileSensorPreview; 
    [HideInInspector]
    public Menu ActiveMenu;

    private Dictionary<Key, Menu> keyToMenuDict;
    private List<Menu> activeMenus = new List<Menu>();

    public static readonly Dictionary<UIBackgroundSprite, Sprite> BackgroundSprites = new Dictionary<UIBackgroundSprite, Sprite>();
    public static UIManager Instance;

    private void Start()
    {
        Instance = this;
        if (BackgroundSprites.Count == 0)
        {
            BackgroundSprites.Add(UIBackgroundSprite.Red,    Resources.Load<Sprite>("UI/UI Elements/Buttons/red button"));
            BackgroundSprites.Add(UIBackgroundSprite.Green,  Resources.Load<Sprite>("UI/UI Elements/Buttons/green button"));
            BackgroundSprites.Add(UIBackgroundSprite.Blue,   Resources.Load<Sprite>("UI/UI Elements/Buttons/blue button"));
            BackgroundSprites.Add(UIBackgroundSprite.Yellow, Resources.Load<Sprite>("UI/UI Elements/Buttons/yellow button"));
            BackgroundSprites.Add(UIBackgroundSprite.Orange, Resources.Load<Sprite>("UI/UI Elements/Buttons/Orange background"));
        }
    }

    public void Subscribe(Menu menu)
    {
        if (keyToMenuDict == null)
        {
            keyToMenuDict = new Dictionary<Key, Menu>();
        }
        keyToMenuDict.Add(menu.Key, menu);
        menu.OnOpen += AddMenu;
        menu.OnClose += RemoveMenu;
        menu.OnEnter += OnPointerEnter;
        menu.OnExit += OnPointerExit;
    }

    public void ReceiveMenuKey(Key key)
    {
        if (keyToMenuDict.ContainsKey(key)) 
        {
            ToggleMenu(keyToMenuDict[key]);
        } 
        else 
        {
            Debug.Log("That key is not bound to any menu! Key: " + key.ToString());
        }
    }

    public void ToggleMenu(Menu menu)
    {
        menu.Toggle();
    }

    private void RemoveMenu(Menu menu)
    {
        activeMenus.Remove(menu);
        // only re-set ActiveMenu if the menu being turned off was the ActiveMenu
        if (activeMenus.Count > 0)
        {
            if (menu == ActiveMenu) ActiveMenu = activeMenus[activeMenus.Count - 1];
        }
        else
        {
            ActiveMenu = null;
        }
        OnUIToggle?.Invoke(IsUIActive());
    }

    private void AddMenu(Menu menu)
    {
        activeMenus.Add(menu);
        ActiveMenu = menu;
        OnUIToggle?.Invoke(true);
    }

    /// <summary>
    /// Returns true if any Menu is open
    /// </summary>
    /// <returns></returns>
    public bool IsUIActive()
    {
        foreach (Menu menu in activeMenus)
        {
            if (menu.IsOpen()) return true;
        }
        return false;
    }

    #region Tab group methods

    public void OnNumberKeyPress(int value)
    {
        if (ActiveMenu != null)
        {
            ActiveMenu.OnNumberKeyPress(value);
        }
        else
        {
            defaultMenu.OnNumberKeyPress(value);
        }
    }

    /// <summary>
    /// Switches menu tabs
    /// </summary>
    public void NextTab()
    {
        if (ActiveMenu != null)
        {
            ActiveMenu.NextTab();
        }
        else
        {
            defaultMenu.NextTab();
        }
    }

    #endregion

    #region Sensor popup methods

    public void ToggleSensorPopup() => ToggleMenu(TileSensorPreview);

    internal void InspectTile(Vector2Int position)
    {
        TileSensorPreview.FocusTile(position);
        if (!activeMenus.Contains(TileSensorPreview)) ToggleSensorPopup();
    }

    #endregion

    /// <summary>
    /// Called whenever the pointer hovers over a Menu
    /// </summary>
    public void OnPointerEnter()
    {
        OnEnteringUI?.Invoke();
    }

    /// <summary>
    /// Called whenever the pointer hovers off of a Menu
    /// </summary>
    public void OnPointerExit()
    {
        OnExitingUI?.Invoke();
    }
}

public enum UIBackgroundSprite
{
    Red,
    Green,
    Blue,
    Yellow,
    Orange,
}