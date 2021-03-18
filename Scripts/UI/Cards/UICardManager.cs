using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(VerticalLayoutGroup))]
public class UICardManager : MonoBehaviour
{
    private VerticalLayoutGroup menuContent;

    private void Start()
    {
        menuContent = GetComponent<VerticalLayoutGroup>();
    }

    public HeaderCard AddHeaderCard(UIBackgroundSprite spriteColor, string header)
    {
        return HeaderCard.Spawn(menuContent.transform, spriteColor, header);
    }
    public NameAndValueCard AddNameValueCard(UIBackgroundSprite spriteColor, string header, string name, string value)
    {
        return NameAndValueCard.Spawn(menuContent.transform, spriteColor, header, name, value);
    }

    public DictionaryCard AddDictionaryCard(UIBackgroundSprite spriteColor, string header)
    {
        return DictionaryCard.Spawn(menuContent.transform, spriteColor, header);
    }

    public void Clear()
    {
        foreach(Transform child in menuContent.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
