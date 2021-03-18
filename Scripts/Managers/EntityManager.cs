
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    private List<Vector2Int> BuildingLocations => GridManager.Instance.Grid.GetBuildingLocations();
    private readonly List<Entity> Entities = new List<Entity>();
    private int spawnLimit = 5000;
    private float spawnDelay = .5f;
    private float totalTime = 0f;

    [Range(.5f, 4)]
    [SerializeField]
    [Tooltip("Scales the spawn cap at which entities can spawn.")]
    private float SpawnScalar = 2f;
    private float TargetTotal => BuildingLocations.Count * SpawnScalar;
    private void Start()
    {
        
    }
    private void Update()
    {
        totalTime += Time.deltaTime;
        while(totalTime >= spawnDelay)
        {
            if (Entities.Count < TargetTotal)
                PeriodicallySpawn();
            totalTime -= spawnDelay;
            RecalculateSpawnDelay();
            //Debug.Log($"Entity Count: {Entities.Count}  SpawnRate: {1f/spawnDelay} TargetTotal: {TargetTotal}");
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            RandomlySpawnVehicle(GetRandomVehicleType());
        }
        if (Input.GetKey(KeyCode.X))
        {
            RandomlySpawnPedestrian();
        }
        if(Input.GetKeyDown(KeyCode.E) && CameraManager.Instance.isFollowingEntity is false)
        {
            if(Entities.Count > 0)
                CameraManager.Instance.StartFollowEntity(Entities[UnityEngine.Random.Range(0, Entities.Count)]);
        }
        else if (Input.GetKeyDown(KeyCode.E) && CameraManager.Instance.isFollowingEntity is true)
        {
            CameraManager.Instance.StopFollowEntity();
        }
    }

    private void RandomlySpawnPedestrian()
    {
        if (GenerateStartStop(out Vector2Int spawnLocation, out Vector2Int targetLocation))
        {
            PedestrianEntity pedestrianEntity = SpawnPedestrian(spawnLocation);
            if(pedestrianEntity is Entity && !pedestrianEntity.TrySetDestination(targetLocation))
                    DestroyEntity(pedestrianEntity);
        }
    }

    private void RandomlySpawnVehicle(VehicleEntity.VehicleType type)
    {
        if (GenerateStartStop(out Vector2Int spawnLocation, out Vector2Int targetLocation))
        {
            VehicleEntity vehicleEntity = SpawnVehicle(type,spawnLocation);
            if (vehicleEntity is Entity && !vehicleEntity.TrySetDestination(targetLocation))
                DestroyEntity(vehicleEntity);
        }
    }

    public PedestrianEntity SpawnPedestrian(NodeController controller)
    {
        PedestrianEntity entity = PedestrianEntity.Spawn(controller);
        Register(entity);
        return entity;
    }
    public VehicleEntity SpawnVehicle(VehicleEntity.VehicleType type, NodeController controller)
    {
        VehicleEntity entity = VehicleEntity.Spawn(controller, type);
        Register(entity);
        return entity;
    }

    public PedestrianEntity SpawnPedestrian(Vector2Int spawnLocation)
    {
        if(GridManager.GetTile(spawnLocation) is Tile tile)
            return SpawnPedestrian(tile.NodeCollection.GetPedestrianSpawnNode(tile));
        else return null;

    }
    public VehicleEntity SpawnVehicle(VehicleEntity.VehicleType type, Vector2Int spawnLocation)
    {
        if (GridManager.GetTile(spawnLocation) is Tile tile)
            return SpawnVehicle(type, tile.NodeCollection.GetVehicleSpawnNode(tile));
        else return null;
    }
    private void Register(Entity entity)
    {
        if (Entities.Contains(entity))
            return;

        if (Entities.Count >= spawnLimit)
            HandleOverCapacity();

        entity.transform.SetParent(transform, true);
        Entities.Add(entity);
        entity.OnReachedDestination += StartDespawnCoroutine;

    }

    private void HandleOverCapacity()
    {
        DestroyEntity(Entities[0]);
    }
    private bool GenerateStartStop(out Vector2Int spawnLocation, out Vector2Int targetLocation)
    {
        var buildings = BuildingLocations;
        if (buildings.Count > 0)
        {
            var randIndex = UnityEngine.Random.Range(0, buildings.Count);
            spawnLocation = buildings[randIndex];
            if (buildings.Count > 1)
            {
                buildings.RemoveAt(randIndex);
                targetLocation = buildings[UnityEngine.Random.Range(0, buildings.Count)];
                return true;
            }

        }
        spawnLocation = Vector2Int.zero;
        targetLocation = Vector2Int.zero;
        return false;
    }
    private void DestroyEntity(Entity entity)
    {
        if (Entities.Contains(entity))
        {
            Destroy(entity.gameObject);
            Entities.Remove(entity);
        }
    }
    IEnumerator DestroyAfterDelay(Entity entity, float delay)
    {
        yield return new WaitForSeconds(delay);
        DestroyEntity(entity);
    }
    private void StartDespawnCoroutine(Entity entity, float delay)
    {
        StartCoroutine(DestroyAfterDelay(entity, delay));
    }

    private void PeriodicallySpawn()
    {
        if (UnityEngine.Random.Range(0,2) == 0)
            RandomlySpawnPedestrian();
        else 
            RandomlySpawnVehicle(GetRandomVehicleType());
    }

    private VehicleEntity.VehicleType GetRandomVehicleType() => (VehicleEntity.VehicleType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(VehicleEntity.VehicleType)).Length);
    private void RecalculateSpawnDelay() 
    {
        if (!(TargetTotal == 0))
            spawnDelay = Mathf.Lerp(10 / TargetTotal , 100/TargetTotal, Entities.Count / TargetTotal);
    }
    
}
    
