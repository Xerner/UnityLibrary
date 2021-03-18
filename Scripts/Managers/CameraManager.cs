 using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public Camera mainCamera;
    public Camera entityCamera;

    public FirstPersonViewport vehicleViewport;
    public Action<Vector3> OnReachedTarget;

    [Range(0, 3)]
    public int defaultRotation = 0;

    public Vector2 defaultPosition;
    public bool isFollowingEntity { get; private set; } = false;
    private bool isLockedToPosition = false;
    private bool isTrackingToObject = false;
    private Entity trackedEntity;
    private Vector3 panVelocity = Vector3.zero;
    private float sizeVelocity = 0;

    private float trackedSize;
    public float Size { get => trackedSize; set => trackedSize = Mathf.Clamp(value, Config.minSize, Config.maxSize); }

    private int rotation;
    public int Rotation { get => rotation; set => rotation = value; }

    private Vector3 trackedPosition;

    public Vector3 Position { get => transform.localPosition; set => trackedPosition = new Vector3(value.x, 0f, value.z); }

    private float targetTrackHeight = 0f;
    private Quaternion trackedRotation = Quaternion.Euler(45, 45, 0);
    private float tiltVelocity = 0f;

    public static CameraManager Instance { get; private set; }
    public Vector3 CameraRigReturnTransform { get; private set; }
    public Quaternion CameraRigReturnRotation { get; private set; }

    void ResetSize() => Size = Config.defaultSize;
    void ResetPosition() => Position = defaultPosition;
    void ResetRotation() => Rotation = defaultRotation;

    private void HardSetPosition(Vector3 position)
    {
        Position = position;
        transform.localPosition = trackedPosition;
    }

    void ResetCamera()
    {
        ResetSize();
        ResetRotation();
        ResetPosition();
    }

    private void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        entityCamera.gameObject.SetActive(false);
        ResetCamera();
        int gridSize = GridManager.Instance.gridSize;
        HardSetPosition(new Vector3(gridSize/2f, 0f, gridSize/2f));
    }

    void RotateLeft()
    {
        transform.Rotate(new Vector3(0, 45, 0), Space.Self);
    }

    void RotateRight()
    {
        transform.Rotate(new Vector3(0, -45, 0), Space.Self);
    }

    // Update is called once per frame
    void Update()
    {
        ClampTrackedPosition();
        if (isFollowingEntity is false)
        {
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, trackedPosition, ref panVelocity, Config.smoothTime); ;
            mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, trackedSize, ref sizeVelocity, Config.smoothTime);
            if (mainCamera.transform.localRotation.eulerAngles.x != trackedRotation.eulerAngles.x)
            {
                float xAngle = Mathf.SmoothDampAngle(mainCamera.transform.localRotation.eulerAngles.x, trackedRotation.eulerAngles.x, ref tiltVelocity, Config.smoothTime);
                mainCamera.transform.localRotation = Quaternion.Euler(xAngle, mainCamera.transform.localRotation.eulerAngles.y, mainCamera.transform.localRotation.eulerAngles.z);
            }
            if (isTrackingToObject && isLockedToPosition && HasReachedTrackedObject())
            {
                OnReachedTarget?.Invoke(transform.position);
                OnReachedTarget = null;
                isTrackingToObject = false;
            }
        }
        if (isFollowingEntity is true)
        {
            FollowEntity(trackedEntity);
        }

        //example of how to call the focued camera using a keypress. Broken without a direct reference to an object. 
        //if (Input.GetKeyDown(KeyCode.I))
        //{
        //    if (isLockedToPosition is false)
        //        TrackObject(TestGameObject, Config.minSize, true);
        //    else
        //        StopTrackObject();
        //}
    }

    private bool HasReachedTrackedObject()
    {
        return Vector3.Distance(trackedPosition, transform.localPosition) < .15f && Math.Abs(trackedSize - mainCamera.orthographicSize) < .1f && Quaternion.Angle(trackedRotation, mainCamera.transform.localRotation) < 1f;
    }

    private void ClampTrackedPosition()
    {
        if (trackedPosition.x < -.5f) trackedPosition.x = -0.5f;
        else if (trackedPosition.x > GridManager.Instance.gridSize - .5f) trackedPosition.x = GridManager.Instance.gridSize - .5f;
        if (trackedPosition.z < -.5f) trackedPosition.z = -0.5f;
        else if (trackedPosition.z > GridManager.Instance.gridSize - .5f) trackedPosition.z = GridManager.Instance.gridSize - .5f;
    }

    internal void PanHandler(Vector3 panDelta)
    {
        Vector3 scaledDelta = panDelta * Time.deltaTime * Config.panSpeed;
        trackedPosition += Quaternion.Euler(0, transform.rotation.eulerAngles.y + 45f, 0) * scaledDelta * (trackedSize / Config.minSize) * Config.panZoomSenstivity;

    }
    internal void RotationHandler(float direction)
    {
        if (direction < 0)
        {
            RotateLeft();
        }
        if (direction > 0)
        {
            RotateRight();
        }
    }
    internal void ZoomHandler(float zoom)
    {
        Size += -zoom * Config.zoomScale;
    }
    public void DisableUserInput()
    {
        GridManager.Instance.CursorEnabled = false;
        GridManager.Instance.GridSM.SuspendState(new DigitalCursor());
    }
    public void EnableUserInput()
    {
        GridManager.Instance.GridSM.ResumeState(new DigitalCursor());
        GridManager.Instance.CursorEnabled = true;
    }
    public void StartFollowEntity(Entity entity)
    {

        DisableUserInput();
        isFollowingEntity = true;
        mainCamera.gameObject.SetActive(false);
        entityCamera.gameObject.SetActive(true);
        trackedEntity = entity;
        trackedEntity.OnBeingDestroy += TrackedEntityDestroyed;
        var collider = entity.GetComponent<BoxCollider>();//.bounds.size.y;
        if (entity is VehicleEntity vehicle)
        {
            vehicleViewport.gameObject.SetActive(true);
            vehicleViewport.SetTrackTo(vehicle);
            targetTrackHeight = collider.bounds.size.y * .75f;
            vehicle.SetModelVisibility(false);
        }
        else
        {
            vehicleViewport.gameObject.SetActive(false);
            targetTrackHeight = collider.bounds.size.y;
        }
    }

    private void TrackedEntityDestroyed()
    {
        StopFollowEntity();
    }

    public void StopFollowEntity()
    {

        entityCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
        isFollowingEntity = false;
        EnableUserInput();
        
        if(trackedEntity is Entity entity)
            entity.OnBeingDestroy -= TrackedEntityDestroyed;

        if (vehicleViewport.gameObject.activeSelf)
        {
            vehicleViewport.StopTracking();
            vehicleViewport.gameObject.SetActive(false);
            if(trackedEntity is VehicleEntity vehicleEntity)
            {
                vehicleEntity.SetModelVisibility(true);
            }
        }
        trackedEntity = null;
        trackedPosition = transform.position;
        mainCamera.orthographicSize = Config.minSize;
        
    }
    public void FollowEntity(Entity entity)
    {
        transform.position = entity.transform.position + Vector3.up * targetTrackHeight;
        entityCamera.transform.rotation = entity.transform.rotation;
    }

    public void TrackPosition(Vector3 position, float zoomLevel, bool isTopDown)
    {
        trackedPosition = position;
        Size = zoomLevel;
        isLockedToPosition = true;
        isTrackingToObject = true;
        if (isTopDown)
        {
            trackedRotation = Quaternion.Euler(90, 45, 0);
        }
    }

    public void TrackObject(GameObject gameObject, float zoomLevel, bool isTopDown)
    {
        TrackPosition(gameObject.transform.position, zoomLevel, isTopDown);
    }
    public void StopTrackObject()
    {
        trackedRotation = Quaternion.Euler(45, 45, 0);
        isLockedToPosition = false;
        isTrackingToObject = false;
        ResetSize();
    }

}
