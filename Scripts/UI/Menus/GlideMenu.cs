using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlideMenu : Menu
{
    public bool DeactivateOnClose = true;
    [SerializeField]
    private EUIPosition uiPosition = EUIPosition.Bottom;
    [SerializeField]
    private Canvas Canvas;
    private Rect menuBounds;
    private Vector2 closedPosition;
    private Vector2 openPosition;
    private Vector2 destination;
    private int glideSpeed = 25;

    protected override void Start()
    {
        // Assumed to be used in child classes for use in movement calculations
        menuBounds = gameObject.GetComponent<RectTransform>().rect;
        // 
        openPosition = transform.position;
        closedPosition = CalculateClosedPosition();
        base.Start();
        InstantlyClose();
    }

    /// <summary>
    /// If needed, glides the menu into or out of place
    /// </summary>
    private void Update()
    {
        // dont try to move if we're at our target position
        switch (uiPosition)
        {
            case EUIPosition.Top:
            case EUIPosition.Bottom:
                if (transform.position.y != destination.y) GlideTowardsDestination();
                break;
            case EUIPosition.Left:
            case EUIPosition.Right:
                if (transform.position.x != destination.x) GlideTowardsDestination();
                break;
            default:
                break;
        }
        // Turn off when we reach our 
        if (DeactivateOnClose && BasicallyAtClosedPosition()) gameObject.SetActive(false);
    }

    /// <summary>
    /// Glide the menu in and out of view
    /// </summary>
    private void GlideTowardsDestination()
    {
        // Move towards destination portions at a time
        Vector3 newPosition = transform.position;
        switch (uiPosition)
        {
            case EUIPosition.Top:
            case EUIPosition.Bottom:
                newPosition.y += YGlideAmount(newPosition.y);
                break;
            case EUIPosition.Left:
            case EUIPosition.Right:
                newPosition.x += XGlideAmount(newPosition.x);
                break;
        }
        transform.position = newPosition;
    }

    /// <summary>
    /// Opens the menu
    /// </summary>
    public override void Open() {
        destination = openPosition;
        OnOpen?.Invoke(this);
        if (!gameObject.activeSelf) gameObject.SetActive(true);
    }

    public override bool IsOpen()
    {
        return destination == openPosition;
    }

    /// <summary>
    /// Closes the menu
    /// </summary>
    public override void Close()
    {
        OnClose?.Invoke(this);
        destination = closedPosition;
    }

    /// <summary>
    /// Instantly closes the menu
    /// </summary>
    public void InstantlyClose()
    {
        transform.position = closedPosition;
        destination = closedPosition;
        if (DeactivateOnClose) gameObject.SetActive(false);
    }

    /// <summary>
    /// Set the Menu's glide amount based on its EUIPosition
    /// </summary>
    private Vector2 CalculateClosedPosition()
    {
        return uiPosition switch
        {
            EUIPosition.Bottom =>    new Vector2(openPosition.x, 0),
            EUIPosition.Top => new Vector2(openPosition.x, menuBounds.height + Canvas.transform.RectTransform().rect.height),
            EUIPosition.Left =>   new Vector2(-menuBounds.width, openPosition.y),
            EUIPosition.Right =>  new Vector2(Canvas.transform.RectTransform().rect.width, openPosition.y),
            _ => Vector2.zero,
        };
    }

    /// <summary>
    /// Calculates if its basically at the closed position, duh
    /// </summary>
    /// <returns></returns>
    private bool BasicallyAtClosedPosition()
    {
        return transform.position.IsBasicallyEqualTo(closedPosition);
    }

    /// <summary>
    /// Calculates the distance between the destination and current position, then divides by the glideSpeed
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    private float YGlideAmount(float y) => (destination.y - y) / glideSpeed;

    /// <summary>
    /// Calculates the distance between the destination and current position, then divides by the glideSpeed
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    private float XGlideAmount(float x) => (destination.x - x) / glideSpeed;
}
