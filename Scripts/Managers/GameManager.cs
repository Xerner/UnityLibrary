using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public InputManager inputManager;
    public CameraManager cameraManager;
    public GridManager gridManager;
    public UIManager uiManager;
    public SensorInfoMenu sensorInfoMenu;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        // Please be mindful. Order of handler assignment could matters
        
        // Input Manager subscriptions

        uiManager.OnUIToggle += inputManager.DisableCameraPan;

        // Camera Manager subscriptions

        inputManager.OnCameraPan += cameraManager.PanHandler;

        inputManager.OnCameraRotation += cameraManager.RotationHandler;

        inputManager.OnCameraZoom += cameraManager.ZoomHandler;

        // Grid Manager subscriptions

        inputManager.OnPlaceTile += gridManager.PlaceHandler;

        inputManager.OnCPressed += gridManager.ToggleCursor;

        uiManager.OnExitingUI += gridManager.ResumeCursor;

        uiManager.OnEnteringUI += gridManager.SuspendCursor;

        // UI subscriptions

        inputManager.OnNumberPressed += uiManager.OnNumberKeyPress;

        inputManager.OnMenuKeyPress += uiManager.ReceiveMenuKey;

        //inputManager.OnTildePressed += uiManager.ReceiveMenuKey;

        inputManager.OnTabPressed += uiManager.NextTab;

        uiManager.OnUIToggle += inputManager.DisableCameraPan;

        sensorInfoMenu.DisableCameraControls += inputManager.DisableCameraPan;

        sensorInfoMenu.DisableCameraControls += inputManager.SetTopDownMode;

        sensorInfoMenu.DisableCameraControls += inputManager.DisableCameraRotation;

        sensorInfoMenu.DisableCameraControls += inputManager.DisableCameraZoom;

        sensorInfoMenu.ToggleCursor += gridManager.ToggleCursor;
    }

    public void HandleLog(int numer)
    {
        Debug.Log(numer);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(inputManager.Cursor);
    }

}
