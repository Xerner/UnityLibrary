using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIClickable : MonoBehaviour
{
    public UnityEvent<UIClickable> OnClick;
    private Button button;

    protected virtual void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Click);
    }

    /// <summary>
    /// Invokes this objects OnClick UnityEvent with itself as the parameter
    /// </summary>
    public void Click()
    {
        OnClick?.Invoke(this);
    }

    /// <summary>
    /// Copies the given prefab and sets its parent Transform to the given Transform. If the prefab given is null, it attempts to load it in using the given address.
    /// </summary>
    /// <param name="staticPrefab"></param>
    /// <param name="parent"></param>
    /// <param name="prefabAddress"></param>
    /// <returns></returns>
    public static GameObject CopyPrefabToParent(GameObject staticPrefab, Transform parent, string prefabAddress)
    {
        if (staticPrefab == null) staticPrefab = Resources.Load<GameObject>(prefabAddress);
        if (staticPrefab == null) {
            StringBuilder str = new StringBuilder("Attempted to instantiate a Prefab. Given filepath was invalid\n");
            str.Append(prefabAddress);
            throw new System.Exception(str.ToString());
        }
        return GameObject.Instantiate(staticPrefab, parent.position, Quaternion.identity, parent);
    }
}
