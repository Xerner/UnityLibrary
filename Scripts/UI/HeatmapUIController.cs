using UnityEngine;
using UnityEngine.UI;

public class HeatmapUIController : MonoBehaviour
{
    private HeatMap heatmap;
    [SerializeField]
    private Image heatmapImage;

    private void Start()
    {
        SensorManager.Instance.OnHeatMapUpdated += UpdateGraphic;
    }

    private void UpdateGraphic(HeatMap heatMap)
    {
        UpdateSpriteFromTexture(heatMap.CreatePNG());
    }

    public void CreateHeatMap(int width, int height)
    {
        heatmap = new HeatMap(width, height);
        UpdateSpriteFromTexture(heatmap.CreatePNG());
    }

    public void UpdateSpriteFromTexture(Texture2D texture)
    {
        heatmapImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 1));
    }
}
