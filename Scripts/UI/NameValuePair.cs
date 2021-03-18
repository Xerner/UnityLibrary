using UnityEngine;
using TMPro;

public class NameValuePair : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI key;
    [SerializeField]
    private TextMeshProUGUI value;

    public TextMeshProUGUI Key { 
        get {
            if (key is null)
            {
                throw new System.Exception("No TextMeshProUGUI set for the Key in the NameValuePair prefab");
            }
            return key;
        }
        private set { }
    }

    public TextMeshProUGUI Value
    {
        get
        {
            if (value is null)
            {
                throw new System.Exception("No TextMeshProUGUI set for the Value in the NameValuePair prefab");
            }
            return value;
        }
        private set { }
    }
}
