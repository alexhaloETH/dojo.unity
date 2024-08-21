using UnityEngine;
using UnityEngine.EventSystems;

// this component could do with a bit of a rewrite, maybe it could also be worth implementing the drag handler interface in the MouseInteractionComponent but that would be a bit of a stretch
public class DoubleHandSlider : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    [SerializeField] private RectTransform _handleMinTransform;
    [SerializeField] private RectTransform _handleMaxTransform;
    [SerializeField] private RectTransform _sliderTrackTransform;

    public float MinValue = 1f;
    public float MaxValue = 20f;

    public float CurrentMinValue = 1f;
    public float CurrentMaxValue = 20f;

    [SerializeField] private bool _useWholeNumbers = true;
    [SerializeField] private int _decimalPlaces = 2;

    [SerializeField] private bool _draggingMinHandle = false;
    [SerializeField] private bool _draggingMaxHandle = false;

    public delegate void ValueChangedDelegate(float newMinValue, float newMaxValue);
    public ValueChangedDelegate onValueChanged;

    private void Start()
    {
        CurrentMaxValue = MaxValue;
    }
    private void Update()
    {
        if (!Input.GetMouseButton(0))
        {
            _draggingMinHandle = false;
            _draggingMaxHandle = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_draggingMinHandle)
        {
            MoveHandle(eventData.position, true);
        }
        else if (_draggingMaxHandle)
        {
            MoveHandle(eventData.position, false);
        }
    }
    private void MoveHandle(Vector2 position, bool isMinHandle)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_sliderTrackTransform, position, null, out localPoint);

        float percent = Mathf.Clamp01((localPoint.x - _sliderTrackTransform.rect.min.x) / _sliderTrackTransform.rect.width );
        float value = percent * (MaxValue - MinValue) + MinValue;

        value = AdjustValuePrecision(value);

        if (isMinHandle)
        {
            float newMinValue = Mathf.Clamp(value, MinValue, CurrentMaxValue);

            if (  newMinValue >= CurrentMaxValue) {
            }
            else if (newMinValue != CurrentMinValue)
            {
                CurrentMinValue = newMinValue;
                _handleMinTransform.anchoredPosition = new Vector2((percent * _sliderTrackTransform.rect.width) - _sliderTrackTransform.rect.width/2, _handleMinTransform.anchoredPosition.y);
            }
        }
        else
        {
            float newMaxValue = Mathf.Clamp(value, CurrentMinValue, MaxValue);

            if (newMaxValue <= CurrentMinValue)
            {
            }
            else if (newMaxValue != CurrentMaxValue)
            {
                CurrentMaxValue = newMaxValue;
                _handleMaxTransform.anchoredPosition = new Vector2(percent * _sliderTrackTransform.rect.width - _sliderTrackTransform.rect.width / 2, _handleMaxTransform.anchoredPosition.y);
            }
        }
    }
    private float AdjustValuePrecision(float value)
    {
        if (_useWholeNumbers)
        {
            return Mathf.Round(value);
        }
        else
        {
            return (float)System.Math.Round(value, _decimalPlaces);
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Mathf.Abs(eventData.pressPosition.x - _handleMinTransform.position.x) < Mathf.Abs(eventData.pressPosition.x - _handleMaxTransform.position.x))
        {
            _draggingMinHandle = true;
        }
        else
        {
            _draggingMaxHandle = true;
        }
    }
}
