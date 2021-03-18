using System;
using System.Collections.Generic;
using UnityEngine;

public class SensorManager : MonoBehaviour
{
    [SerializeField]
    private SensorLogUIController sensorLogger;


    public Action<HeatMap> OnHeatMapUpdated;
   // public Action<List<>>

    private float totalTime = 0f;
    private float callDelay;
    protected Rigidbody ownerRigidBody = null;
    public int TargetCallsPerSecond
    {
        get => (int)(1 / callDelay);
        set => callDelay = (value > 0) ? (1f / value) : 1f;
    }

    public HashSet<CameraSensor> CameraSet = new HashSet<CameraSensor>();
    public HashSet<TrafficLightSensor> TrafficLightSet = new HashSet<TrafficLightSensor>();
    public static SensorManager Instance { get; private set; }

    private HeatMap heatMap;

    private List<Vector2Int> TrackedPoints = new List<Vector2Int>();

    private void Awake()
    {
        ForceSingleInstance();
    }

    private void Start()
    {
        TargetCallsPerSecond = 4;
        int mapSize = GridManager.Instance.gridSize;
        heatMap = new HeatMap(mapSize, mapSize);
    }

    private void Update()
    {
        totalTime += Time.deltaTime;
        if(totalTime > callDelay)
        {
            UpdateManagedServices();
            totalTime -= callDelay;
        }
    }

    private void UpdateManagedServices()
    {
        UpdateHeatMap();
    }

    private void UpdateHeatMap()
    {
        heatMap.Update(TrackedPoints);
        TrackedPoints.Clear();
        OnHeatMapUpdated?.Invoke(heatMap);
    }

    private void ForceSingleInstance()
    {
        if (Instance != null) Destroy(this);
        else Instance = this;
    }

    public void RegisterToManager(ISensor sensor)
    {
        if (sensor is CameraSensor camera)
        {
            if (!CameraSet.Contains(camera))
            {
                CameraSet.Add(camera);
                camera.DataCollected += OnReceiveCameraData;
            }
            else throw new Exception("Camera already registered");
        }
        else if (sensor is TrafficLightSensor trafficLight)
        {
            if (!TrafficLightSet.Contains(trafficLight))
            {
                TrafficLightSet.Add(trafficLight);
                trafficLight.DataCollected += OnReceiveTrafficData;
            }
            else throw new Exception("Traffic Light already registered");
        }
        else throw new Exception("Sensor regstriation not implemented yet");
        sensorLogger.RegisterSensor(sensor);
    }

    public void DeregisterFromManager(ISensor sensor)
    {
        if (sensor is CameraSensor camera)
        {
            if (CameraSet.Contains(camera))
            {
                CameraSet.Remove(camera);
                camera.DataCollected -= OnReceiveCameraData;
            }
            else throw new Exception("Camera already deregistered");
        }
        else if (sensor is TrafficLightSensor trafficLight)
        {
            if (TrafficLightSet.Contains(trafficLight))
            {
                TrafficLightSet.Remove(trafficLight);
                trafficLight.DataCollected -= OnReceiveTrafficData;
            }
            else throw new Exception("Traffic Light already deregistered");
        }
        else throw new Exception("Sensor deregstriation not implemented yet");
        sensorLogger.DeregisterSensor(sensor);
    }

    private void OnReceiveTrafficData(List<TrafficLightSensorData> sensorData)
    {
        var foundDirections = new HashSet<NodeCollectionController.Direction>();
        Vector2Int? tile = null;
        foreach (var data in sensorData)
        {
            if (data.Entity == NodeCollectionController.TargetUser.Vehicles && data.IsInbound && !foundDirections.Contains(data.EstimatedDirection))
            {
                foundDirections.Add(data.EstimatedDirection);
                tile = data.TilePosition;
            }
        }

        if(foundDirections.Count > 0 && GridManager.GetTile(tile.Value) is RoadTile road)
        {
            foreach (var dir in foundDirections) road.TrafficLight.VehicleFoundInDirection(dir);
        }
    }

    public void OnReceiveCameraData(List<CameraSensorData> sensorData)
    {
        foreach (var data in sensorData) TrackedPoints.Add(data.Position.ToGridInt());
    }

    public static bool RemoveSensorAt(Vector2Int tilePosition, SensorType sensorType)
    {
        if (GridManager.GetTile(tilePosition) is Tile tile)
        {
            tile.RemoveSensor(sensorType);
        }
        return true;
    }

    public static bool TryCreateSensorAt(Vector2Int tilePosition, SensorType sensorType)
    {
        //Makes sure the tile exists and doesn't have a T type sensor on the tile already
        if(GridManager.GetTile(tilePosition) is Tile tile)
        {
            return tile.TryAddSensor(sensorType);
        }
        return false;
    }
}
