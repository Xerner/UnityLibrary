using UnityEngine;

public class NameAndValueCard : HeaderCard
{
    public static new readonly string prefabAddress = "Prefabs/UI/Cards/NameAndValueCard";
    private static GameObject staticPrefab = null;

    [SerializeField]
    private NameValuePair nameValuePair;

    /// <summary>
    /// The name of the value being defined
    /// </summary>
    public string Name 
    {
        get { return nameValuePair.Key.text; }
        set { nameValuePair.Key.text = value; }
    }

    /// <summary>
    /// The value text. The definition
    /// </summary>
    public string Value
    {
        get { return nameValuePair.Value.text; }
        set { nameValuePair.Value.text = value; }
    }

    /// <summary>
    /// Spawns in a NameAndValueCard prefab with given background sprite and a NameValuePair with given key and value
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="backgroundSprite"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static NameAndValueCard Spawn(Transform parent, UIBackgroundSprite backgroundSprite, string header, string name, string value)
    {
        NameAndValueCard nameValueCard = CopyPrefabToParent(staticPrefab, parent, prefabAddress).GetComponent<NameAndValueCard>();
        nameValueCard.Header = header;
        nameValueCard.Name = name;
        nameValueCard.Value = value;
        nameValueCard.BackgroundSprite = backgroundSprite;
        return nameValueCard;
    }
}
