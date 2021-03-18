using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    [HideInInspector]
    public List<TabButton> TabButtons;
    public List<GameObject> Pages;
    public List<PageControls> PageControls = new List<PageControls>();
    private PageControls activeControls;
    public Sprite tabIdle;
    public Sprite tabHover;
    public Sprite tabActive;
    private TabButton ActiveTab;

    /// <summary>
    /// Subscribes a TabButton to this TabGroup
    /// </summary>
    /// <param name="button"></param>
    public void Subscribe(TabButton button)
    {
        if (TabButtons == null)
        {
            TabButtons = new List<TabButton>();
        }
        TabButtons.Add(button);
        if (ActiveTab == null && button.transform.GetSiblingIndex() < TabButtons.Count) 
        { 
            OnTabSelected(button);
        }
        else if(ActiveTab != null && button.transform.GetSiblingIndex() < ActiveTab.transform.GetSiblingIndex())
        {
            OnTabSelected(button);
        }
        TabButtons.Sort(CompareIndexes);
    }

    public static int CompareIndexes(TabButton button1, TabButton button2)
    {
        return button1.transform.GetSiblingIndex() - button2.transform.GetSiblingIndex();
    }

    /// <summary>
    /// Changes the hovered TabButtons sprite the Tab Hover sprite
    /// </summary>
    /// <param name="button"></param>
    public void OnTabEnter(TabButton button)
    {
        ResetTabs();
        if (ActiveTab == null || button != ActiveTab) 
        { 
            button.background.sprite = tabHover;
        }
    }

    /// <summary>
    /// Resets the TabButtons sprites to their defaults
    /// </summary>
    /// <param name="button"></param>
    public void OnTabExit(TabButton button)
    {
        ResetTabs();
    }

    /// <summary>
    /// Selects a TabButton
    /// </summary>
    /// <param name="button"></param>
    public void OnTabSelected(TabButton button)
    {
        ActiveTab = button;
        ResetTabs();
        button.background.sprite = tabActive;
        SwitchToTab(button.transform.GetSiblingIndex());
    }

    /// <summary>
    /// Resets each TabButtons sprites, except the active TabButton
    /// </summary>
    public void ResetTabs()
    {
        foreach (TabButton button in TabButtons)
        {
            if (ActiveTab != null && button == ActiveTab) continue;
            button.background.sprite = tabIdle;
        }
    }

    /// <summary>
    /// Switches to the next TabButton. If on the last tab, warps back to the 1st tab
    /// </summary>
    public void NextTab()
    {
        int index = ActiveTab.transform.GetSiblingIndex();
            if (index == Pages.Count - 1)
        {
            OnTabSelected(TabButtons[0]);
        }
        else
        {
            OnTabSelected(TabButtons[index + 1]);
        }
    }

    /// <summary>
    /// Switch to the ith TabButton
    /// </summary>
    /// <param name="index"></param>
    private void SwitchToTab(int index)
    {
        if (index >= 0 && index < TabButtons.Count)
        {
            for (int i = 0; i < Pages.Count; i++)
            {
                if (i == index)
                {
                    Pages[i].SetActive(true);
                    if (i < PageControls.Count)
                    {
                        activeControls = PageControls[i];
                    }
                    else
                    {
                        activeControls = null;
                    }
                }
                else
                {
                    Pages[i].SetActive(false);
                }
            }
        }
        else
        {
            throw new System.Exception("Tab index out of bounds " + index);
        }
    }

    /// <summary>
    /// Invokes the ith Number key button listed in the active PageControls component
    /// </summary>
    /// <param name="index"></param>
    public void OnNumberKeyPress(int index)
    {
        if (activeControls != null && index < activeControls.NumberKeyButtons.Count)
        {
            activeControls.NumberKeyButtons[index].onClick.Invoke();
        }
    }
}
