using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Component that should make minimaps easier, givent the positions and size of the entity this should take a percentage and set them correctly in respect to the top right
/// </summary>
public class MinimapComponent : MonoBehaviour
{
    [SerializeField] RectTransform _parentRectTransform; // Assign your parent RectTransform here
    [SerializeField] RectTransform _spotOnMinimap;  // Assign your child RectTransform here

    public Vector2 Scale;
    public Vector2 MapSize; // Make the map size a public variable

    public UnityEvent<Vector2> OnMapClickEvent;

    private void Start()
    {
        Scale = new Vector2(_parentRectTransform.rect.width, _parentRectTransform.rect.height);

        _spotOnMinimap.anchorMin = new Vector2(0, 1);
        _spotOnMinimap.anchorMax = new Vector2(0, 1);
    }

    public void SetSpotPositionOMinimap(Vector2 percentagePosition)
    {
        _spotOnMinimap.gameObject.SetActive(true);

        float scaledX = Scale.x * percentagePosition.x;
        float scaledY = Scale.y * -percentagePosition.y;

        _spotOnMinimap.anchoredPosition = new Vector2(scaledX, scaledY);
    }

    public void EnableSpot(bool show)
    {
        _spotOnMinimap.gameObject.SetActive(show);
    }

    public void SetSpotSizeOnMinimap(Vector2 percentageSize)
    {
        _spotOnMinimap.gameObject.SetActive(true);

        float scaledX = Scale.x * percentageSize.x;
        float scaledY = Scale.y * percentageSize.y;

        _spotOnMinimap.sizeDelta = new Vector2(scaledX, scaledY);
    }

    public (Vector2 positionPercentage, Vector2 sizePercentage) CalculatePercentage(Vector2 objectPosition, Vector2 objectSize)
    {
        Vector2 positionPercentage = new Vector2(objectPosition.x / MapSize.x, objectPosition.y / MapSize.y);
        Vector2 sizePercentage = new Vector2(objectSize.x / MapSize.x, objectSize.y / MapSize.y);

        return (positionPercentage, sizePercentage);
    }

    /// <summary>
    /// Given the right size of the map this should return the point of the click in the form of a percentage, relative to the top right
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRectTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);

        localMousePosition += new Vector2(Scale.x / 2, Scale.y / 2);

        Vector2 mousePercentagePosition = new Vector2(localMousePosition.x / Scale.x,   Math.Abs(1- localMousePosition.y / Scale.y));

        OnMapClickEvent?.Invoke(mousePercentagePosition);
    }
}
