using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HeaderCard : UIClickable
{
    public static readonly string prefabAddress = "Prefabs/UI/Cards/HeaderCard";
    private static GameObject staticPrefab = null;

    public UnityEvent<UIClickable> OnRemoved;

    private Image bkgImage = null;
    [SerializeField]
    private TextMeshProUGUI header;
    [SerializeField]
    private Button closeButton;

    private UIBackgroundSprite spriteEnum;

    protected override void Start()
    {
        base.Start();
        if (bkgImage is null) bkgImage = GetComponent<Image>();
        closeButton.onClick.AddListener(() => Destroy(gameObject));
        closeButton.onClick.AddListener(OnRemove);
    }

    private void OnRemove()
    {
        OnRemoved?.Invoke(this);
    }

    #region Properties

    public string Header
    {
        
        get
        {
            HeaderErrorCheck();
            return header.text;
        }
        set {
            HeaderErrorCheck();
            header.text = value; 
        }
    }

    private void HeaderErrorCheck()
    {
        if (header is null) throw new System.Exception("Header was never set for this HeaderCard");
    }

    private void BackgroundImageErrorCheck()
    {
        if (bkgImage is null)
            bkgImage = GetComponent<Image>();
        if (bkgImage is null)
            throw new System.Exception("Cannot find Image component");
    }

    public UIBackgroundSprite BackgroundSprite
    {
        get { return spriteEnum; }
        set {
            BackgroundImageErrorCheck();
            if (bkgImage.sprite != UIManager.BackgroundSprites[value]) 
            {
                spriteEnum = value;
                bkgImage.sprite = UIManager.BackgroundSprites[value];
            }
        }
    }

    public Material Material
    {
        get { return bkgImage.material; }
        set { bkgImage.material = value; }
    }

    #endregion

    #region Factories

    /// </summary>
    /// <param name="parent"></param>
    /// <param name="backgroundColor"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static HeaderCard Spawn(Transform parent, UIBackgroundSprite backgroundSprite, string text)
    {
        HeaderCard simpleCard = CopyPrefabToParent(staticPrefab, parent, prefabAddress).GetComponent<HeaderCard>();
        simpleCard.Header = text;
        simpleCard.BackgroundSprite = backgroundSprite;
        return simpleCard;
    }

    #endregion
}
