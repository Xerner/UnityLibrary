using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    [Range(5, 100)]
    public int gridSize;

    private DigitalCursor cursor = DigitalCursor.Empty;
    private bool cursorEnabled = true; //When false, cursor will not be shown

    private bool oldCursorEnabled;
    private bool clickRecieved = false; //When true, update function will pick this up and signal to its state controller

    private GameObject ground = null;
    public LayerMask groundMask; // The mask used to find the ground plane

    public GameObject NavPointPrefab;
    [HideInInspector]
    public VehicleEntity entity;

    public Material TileMaterial;
    public Material TransparentMaterial;

    public GridController GridSM { get; private set; }  //Controls the state of build
    public TileGrid Grid; // Data object that holds the information about all tiles
    public static GridManager Instance { get; private set; } //Singleton pattern

    public bool CursorEnabled { get => cursorEnabled; set => SetCursor(value); }

    public void ToggleCursor() { SetCursor(!cursorEnabled); }

    public void SuspendCursor() {
        oldCursorEnabled = cursorEnabled;
        SetCursor(false);
    }

    public void ResumeCursor() { 
        SetCursor(oldCursorEnabled); 
    }

    private void SetCursor(bool value)
    {
        if (value == cursorEnabled) return;
        if (value)
        {
            cursor = new DigitalCursor();
            cursorEnabled = true;
            GridSM.ResumeState(cursor);
        } 
        else
        {
            GridSM.SuspendState(cursor);
            cursorEnabled = false;
            cursor = DigitalCursor.Empty;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (!TryLoadFile())
        {
            Grid = new TileGrid(gridSize, gridSize);
        }

        GridSM = new GridController(EGridControlState.PlaceRoads);
        if (Instance != null) Destroy(this);
        Instance = this;
        
        ground = CreateGround();

        SurroundGround(gridSize);

        Grid.RefreshGrid();
    }

    private void SurroundGround(int gridSize)
    {
        var mountainPrefab = Resources.Load<GameObject>("Prefabs/SurroundingTiles/SurroundingTile");

        var sourceGO = new GameObject("Surrounding Tiles");
        sourceGO.transform.parent = this.transform;
        sourceGO.transform.position = new Vector3(-0.5f, 0f, -0.5f);

        GenerateMountainRange(mountainPrefab, sourceGO.transform, gridSize, sourceGO.transform.position, Vector3.left, Vector3.forward);
        GenerateMountainRange(mountainPrefab, sourceGO.transform, gridSize, sourceGO.transform.position + Vector3.forward * gridSize, Vector3.forward, Vector3.right);
        GenerateMountainRange(mountainPrefab, sourceGO.transform, gridSize, sourceGO.transform.position + Vector3.forward * gridSize + Vector3.right * gridSize, Vector3.right, Vector3.back);
        GenerateMountainRange(mountainPrefab, sourceGO.transform, gridSize, sourceGO.transform.position + Vector3.right * gridSize, Vector3.back, Vector3.left);
    }

    private void GenerateMountainRange(GameObject mountainPrefab, Transform parent, int size, Vector3 rootPoint, Vector3 edgeNormal, Vector3 edgeDirection)
    {
        int tileRadius = 10;
        float distanceRemaining = size + tileRadius * 6;
        int tilesSpawned = 0;
        Vector3 rootPosition = rootPoint + edgeNormal * tileRadius + edgeDirection * tileRadius;
        while(distanceRemaining > 0)
        {
            Vector3 p = rootPosition + tilesSpawned * edgeDirection * tileRadius * 2;
            for (int i = 0; i < 4; i++)
            {
                Instantiate<GameObject>(mountainPrefab, p, GetRandomRotation(), parent).GetComponent<AdjustableMountain>().SetHeightScale(UnityEngine.Random.Range(0.75f, 1.25f));
                p += edgeNormal * tileRadius * 2;
            }

            tilesSpawned++;
            distanceRemaining -= tileRadius * 2;
        }
    }

    private Quaternion GetRandomRotation()
    {
        int choice = UnityEngine.Random.Range(0, 4);
        return choice switch
        {
            0 => Quaternion.identity,
            1 => Quaternion.Euler(0f, 90f, 0f),
            2 => Quaternion.Euler(0f, 180f, 0f),
            _ => Quaternion.Euler(0f, 270f, 0f),
        };
    }

    private bool TryLoadFile()
    {
        if (SaveGameManager.LoadFromFile != "")
        {
            Grid = SaveGameManager.LoadGame(SaveGameManager.LoadFromFile);
            if (Grid == null) return false;
            SaveGameManager.LoadFromFile = "";
            gridSize = Grid.Width;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Creates the ground object and attaches it as a child
    /// </summary>
    /// <returns></returns>
    private GameObject CreateGround()
    {
        var blueprint = new GameObject("Ground")
        {
            layer = LayerMask.NameToLayer("Grid")
        };  //Creates gameobject with name ground and assigns it to the grid layer


        var meshFilter = blueprint.AddComponent<MeshFilter>(); //Stores mesh data for objects
        var meshCollider = blueprint.AddComponent<MeshCollider>(); //Stores collider data for objects
        var meshRenderer = blueprint.AddComponent<MeshRenderer>(); //Stores material for rendering mesh with material
        meshRenderer.material = Resources.Load<Material>("Materials/Grass_Mat"); //Assigns material as grass

        var mesh = GenerateMesh(Grid.Width, Grid.Height); //Creates the mesh according to the specified grid size

        meshFilter.mesh = mesh; //Adds mesh to filter
        meshCollider.sharedMesh = mesh;  //Adds mesh to collider

        blueprint.transform.position = new Vector3(-.5f, 0f, -.5f); //Grid is offset by (0.5, 0.5) to account for the tiles being centered
        blueprint.transform.parent = this.transform;
        return blueprint;
    }

    /* This is how the mesh is drawn
     *    1 ---------------2
     *    |              _/|
     *    |     T1    _/   |
     *  Height    _ /      |
     *    |   _ /    T2    |
     *    | /              |
     *    0 ----Width----- 3
     * Unity uses a clockwise winding order to determine triangle fronts
     */
    private Mesh GenerateMesh(int width, int height)
    {
        Mesh mesh = new Mesh
        {
            vertices = new Vector3[] {
            Vector3.zero,                   //Point 0
            new Vector3(0, 0, height),       //Point 1
            new Vector3(width, 0, height),  //Point 2
            new Vector3(width, 0, 0)       //Point 3
        },
            uv = new Vector2[] {
            new Vector2(0,0),  //P0
            new Vector2(0,1),  //P1
            new Vector2(1,1),  //P2
            new Vector2(1,0)   //P3
        },

            //                    Triangle 1    Triangle 2
            triangles = new int[] { 0, 1, 2, 0, 2, 3 }
        };

        mesh.RecalculateBounds();
        mesh.RecalculateNormals(); //Calculates the normals according to the provided mesh

        return mesh;
    }

    /// <summary>
    /// When a place event occures this function handles it
    /// </summary>
    internal void PlaceHandler()
    {
        clickRecieved = true;
    }

    public void ChangeStateHandler(EGridControlState state) => ChangeState(state);

    /// <summary>
    /// Changes the functionality of the curser by modifying GridController's active State
    /// </summary>
    /// <param name="state"></param>
    public void ChangeState(EGridControlState state) { if (GridSM.state != state) GridSM.SetState(state, cursor); }

    // These functions are used on UI buttons onClick()
    public void SetPlaceRoadsState() => ChangeState(EGridControlState.PlaceRoads);
    public void SetPlaceHouseState() => ChangeState(EGridControlState.PlaceHouse);
    public void SetPlaceOfficeState() => ChangeState(EGridControlState.PlaceOffice);
    public void SetPlaceCameraState() => ChangeState(EGridControlState.PlaceCamera);
    public void SetPlaceTrafficLightState() => ChangeState(EGridControlState.PlaceSmartTrafficLight);
    public void SetDeleteTileState() => ChangeState(EGridControlState.DeleteTile);
    public void SetSelectEntityState() => ChangeState(EGridControlState.SelectEntity);
    public void SetExamineTileState() => ChangeState(EGridControlState.ExamineTile);

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) Debug.Log(Grid.ToString());

        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveGame();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            GameObject path = new GameObject("Path Holder");

            List<Vector2Int> points = Pathfinding.GetListOfPositionsFromTo(new Vector2Int(1, 1), Grid.GetBuildingLocations()[1]);
            points.Reverse();

            List<GameObject> pointInstances = new List<GameObject>();

            if (points.Count > 0) pointInstances.Add(Instantiate(NavPointPrefab, new Vector3(points[0].x, 0f, points[0].y), Quaternion.identity, path.transform));
            for (int i = 1; i < points.Count; i++)
            {
                var navPt = Instantiate(NavPointPrefab, new Vector3(points[i].x, 0f, points[i].y), Quaternion.identity, path.transform);
                navPt.GetComponent<NavPoint>().Connections.Add(new NavPointConnection(pointInstances[i - 1].GetComponent<NavPoint>(), NavPointConnection.ConnectionType.Directed));
                pointInstances.Add(navPt);
            }
        }

        if (CursorEnabled && CameraManager.Instance.isFollowingEntity is false)
        {
            HandleCursorMovement(); //Updates state with cursor movement
            if (clickRecieved)
            {
                GridSM.OnMouseDown(cursor); //Sends down press event to state
            }
        }
        clickRecieved = false;
    }

    public void SaveGame()
    {
        StringBuilder strBuilder = new StringBuilder();
        strBuilder.Clear();
        strBuilder.Append(Application.dataPath);
        strBuilder.Append("/Saves/");
        int count = Directory.GetFiles(strBuilder.ToString(), "*.xml").Length;
        strBuilder.Append("save ");
        strBuilder.Append(count.ToString());
        strBuilder.Append(".xml");
        GridSM.SuspendState(cursor);
        SaveGameManager.SaveGame(strBuilder.ToString(), Grid);
        GridSM.ResumeState(cursor);
    }

    public void LoadGame()
    {
        GridSM.SuspendState(cursor);
        SceneManager.LoadScene(0);
        GridSM.ResumeState(cursor);
    }

    private void HandleCursorMovement()
    {
        DigitalCursor newCursor = new DigitalCursor();
        
        if(newCursor.Position != cursor.Position || newCursor.SubDirection != cursor.SubDirection)
        {
            GridSM.MoveCursor(cursor, newCursor);
            cursor = newCursor;
        }
    }


    /// <summary>
    /// Gets tile from location
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public static Tile GetTile(Vector2Int location) => GridManager.Instance.Grid[location];


    /// <summary>
    /// Gets the node collection controller from the specified vector2int position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static NodeCollectionController GetCollectionAtTileLocation(Vector2Int position) => GetTile(position).NodeCollection;


    /// <summary>
    /// Sets transparency of the tile at this location
    /// </summary>
    /// <param name="location"></param>
    /// <param name="value"></param>
    public void SetTransparency(Vector2Int location, bool value) => Grid[location]?.SetTransparency(value);

    /// <summary>
    /// Makes the tile permanent at location
    /// </summary>
    /// <param name="point"></param>
    public void MakePermanent(Vector2Int point, bool animateSpawn = false)
    {
        Grid[point]?.MakePermanent(animateSpawn);
        Grid[point]?.SetTransparency(false);
        UpdateNeighbors(point);
    }

    /// <summary>
    /// Creates tile with default value for type. Calls to AddTileToGrid.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="point"></param>
    public void CreateTemporaryTile<T>(Vector2Int point) where T : Tile, new() => AddTileToGrid(point, new T());


    /// <summary>
    /// Creates tile with default value for types. And then makes permanent.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="point"></param>
    private void CreatePermanentTile<T>(Vector2Int point) where T : Tile, new()
    {
        AddTileToGrid(point, default(T));
        MakePermanent(point);
    }

    /// <summary>
    /// Adds tile to grid at location. If it is occupied, trys to delete. Updates neighbors of change
    /// </summary>
    /// <param name="point"></param>
    /// <param name="tile"></param>
    public void AddTileToGrid(Vector2Int point, Tile tile)
    {
        if (Grid[point] != null) //If there is a tile at this location already, try to remove
        {
            if (!RemoveTileIfTemporary(point)) return; //The tile could not be removed (is is permanent) so halting
        }
        //Tile location is ensured empty, can proceed to file location
        Grid[point] = tile;
        if (Grid[point].CreateManaged(point, Grid.GetNeighbors(point))) ForceRemoveTileDirty(point);
        else UpdateNeighbors(point);
    }

    /// <summary>
    /// Takes a tile that currently exists and ensures their model is correct for the current neighbor scenario.
    /// </summary>
    /// <param name="point"></param>
    private void RecalculateTile(Vector2Int point) 
    {
        if (Grid[point]?.RecalculateManaged(Grid.GetNeighbors(point)) ?? false) RemoveTile(point);
    }


    /// <summary>
    /// Removes tile from the grid based on if the tile if temporary. Updates Neighbors
    /// </summary>
    /// <param name="point">Point on grid to remove</param>
    /// <returns>Returns true if a tile was deleted</returns>
    public bool RemoveTileIfTemporary(Vector2Int point)
    {
        if (Grid[point]?.IsPermanent ?? true) return false; //Returns false when either the tile is permanent or it is null
        else if (ForceRemoveTileDirty(point))
        {
            UpdateNeighbors(point);
            return true;
        }
        else return false;
    }

    /// <summary>
    /// Removes tile and updates neighbors
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool RemoveTile(Vector2Int point)
    {
        if (ForceRemoveTileDirty(point))
        {
            UpdateNeighbors(point);
            return true;
        }
        else return false;
    }


    /// <summary>
    /// Removes a tile, no matter its state. Does not notify neighbors
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private bool ForceRemoveTileDirty(Vector2Int point)
    {
        Grid[point]?.DeleteManaged();
        Grid[point] = null;
        return true;
    }


    /// <summary>
    /// Tell all neighbors of a point to update it's model
    /// </summary>
    /// <param name="point"></param>
    private void UpdateNeighbors(Vector2Int point)
    {
        foreach (var direction in Tile.Directions) RecalculateTile(point + direction);
    }
}
