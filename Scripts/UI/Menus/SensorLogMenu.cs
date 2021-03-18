using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorLogMenu : MonoBehaviour
{
    [SerializeField]
    private UICardManager menu;
    [SerializeField]
    private SensorInfoMenu sensorInfoMenu;

    private ISensor targetedSensor;

    private Dictionary<ISensor, NameAndValueCard> sensorMapping = new Dictionary<ISensor, NameAndValueCard>();
    private Dictionary<UIClickable, ISensor> cardMapping = new Dictionary<UIClickable, ISensor>();

    private SortMode currentSort = SortMode.None;

    public void AddSensor(ISensor sensor)
    {
        if (sensorMapping.ContainsKey(sensor)) throw new Exception("Sensor already found");
        var (statusName, statusMsg, statusEnum) = sensor.Status();
        var card = menu.AddNameValueCard(statusEnum.GetColor(), sensor.ToString(), statusName, statusMsg);
        sensorMapping.Add(sensor, card);
        cardMapping.Add(card, sensor);
        card.OnClick.AddListener(CardClicked);
    }

    private void CardClicked(UIClickable card)
    {
        if(cardMapping.TryGetValue(card, out ISensor sensor))
        {
            sensorInfoMenu.DisableUserInput();
            var tilePosition = sensor.GetTilePosition();
            CameraManager.Instance.OnReachedTarget += ReachedSensor;
            CameraManager.Instance.OnReachedTarget += sensorInfoMenu.SetVisible;
            targetedSensor = sensor;
            CameraManager.Instance.TrackPosition(tilePosition.ToGridVector3(), Config.minSize, true);
        }
    }

    private void ReachedSensor(Vector3 position)
    {
        if(targetedSensor != null && targetedSensor.GetTilePosition() == position.ToGridInt())
        {
        }
        else
        {
            Debug.Log("Something went wrong in the process");
        }
    }

    public void RemoveSensor(ISensor sensor)
    {
        if(!sensorMapping.ContainsKey(sensor)) throw new Exception("Sensor does not exist in this menu");
        var card = sensorMapping[sensor];
        if (card != null) Destroy(card.gameObject);
        sensorMapping.Remove(sensor);
    }

    public void UpdateSensorLog(ISensor sensor)
    {
        if (sensorMapping.TryGetValue(sensor, out var card))
        {
            var (statusName, statusMsg, statusEnum) = sensor.Status();
            UpdateCard(card, statusMsg, statusEnum.GetColor());            
        }
        else
        {
            AddSensor(sensor);
            UpdatePositionalStanding(card);
        }
    }

    public void SortByType() => SetSort(SortMode.Type);
    public void SortByStatus() => SetSort(SortMode.Status);

    private void SetSort(SortMode newSort)
    {
        if(newSort == SortMode.None)
        {
            currentSort = SortMode.None;
            return;
        } 
        else if(newSort == SortMode.Type)
        {
            foreach(var kvp in sensorMapping) SortCardType(kvp.Key, kvp.Value);
            currentSort = newSort;
        } else if(newSort == SortMode.Status)
        {

            foreach(var kvp in sensorMapping)
            {
                //Sort em by color!
            }
            currentSort = newSort;
        }


    }

    private void SortCardType(ISensor sensor, NameAndValueCard card)
    {
        if (sensor is CameraSensor) card.gameObject.RectTransform().SetAsLastSibling();
        else card.gameObject.RectTransform().SetAsFirstSibling();
    }

    private void UpdateCard(NameAndValueCard card, string message, UIBackgroundSprite sprite)
    {
        card.Value = message;
        card.BackgroundSprite = sprite;
        UpdatePositionalStanding(card);
    }

    private void UpdatePositionalStanding(NameAndValueCard card)
    {
        if (currentSort == SortMode.Type) SortCardType(cardMapping[card], card);
        return;
    }

    private enum SortMode
    {
        None,
        Status,
        Type
    }
}
