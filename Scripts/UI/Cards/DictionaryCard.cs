using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DictionaryCard : HeaderCard
{
    public static new readonly string prefabAddress = "Prefabs/UI/Cards/DetailedCard";
    private static GameObject staticPrefab = null;

    private Dictionary<string, NameValuePair> items = new Dictionary<string, NameValuePair>();
    [SerializeField]
    private GameObject textArea;
    [SerializeField]
    private GameObject NameValuePairPrefab;

    /// <summary>
    /// Adds a Key-Value pair to the Card
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void AddItem(string key, string value)
    {
        
        if (NameValuePairPrefab is null) throw new System.Exception("NameValuePair prefab not set in the DetailCard script");
        GameObject nameValueObj = Instantiate(NameValuePairPrefab, textArea.transform);
        if (nameValueObj.TryGetComponent(out NameValuePair pair))
        {
            pair.Key.text = key;
            pair.Value.text = value;
            items.Add(key, pair);
        }
        else
        {
            throw new System.Exception("Name value pair object has no NameValuePair script");
        }
    }

    /// <summary>
    /// Deletes a Key-Value pair from the Card
    /// </summary>
    /// <param name="key"></param>
    public void DeleteItem(string key)
    {
        Destroy(items[key].gameObject);
    }

    /// <summary>
    /// Gets access to the key-value pair with the given key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string this[string key]
    {
        get
        {
            if (items.ContainsKey(key))
            {
                return items[key].Value.text;
            }
            else
            {
                return "";
            }
        }
        set
        {
            if (items.ContainsKey(key))
            {
                items[key].Value.text = value;
            }
        }
    }

    /// <summary>
    /// Spawns in a DetailCard prefab with given background sprite and 1 NameValuePair with given key and value
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="backgroundSprite"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public new static DictionaryCard Spawn(Transform parent, UIBackgroundSprite backgroundSprite, string header)
    {
        DictionaryCard detailCard = CopyPrefabToParent(staticPrefab, parent, prefabAddress).GetComponent<DictionaryCard>();
        detailCard.Header  = header;
        detailCard.BackgroundSprite = backgroundSprite;
        return detailCard;
    }
}
