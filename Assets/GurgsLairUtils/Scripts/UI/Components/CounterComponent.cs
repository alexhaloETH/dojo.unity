using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CounterComponent : MonoBehaviour
{
    // Serialized fields for UI components
    [SerializeField] private TMP_Text _valueText;
    [SerializeField] private Image _plusButtonRawImage;
    [SerializeField] private Image _minusButtonRawImage;

    // Properties for the counter values
    public int CurrentValue  = 1;
    public int AdditionValue = 1;

    public int LowestValue = 0;
    public int HighestValue = 0;

    // Tooltip for hiding buttons at boundaries
    [Tooltip("Hide the button if the counter is at a boundary")]
    public bool HideIfAtBoundaries = false;


    public enum BoundaryAction
    {
        None,
        Reset,   // reset is not implemented yet
        Cycle
    }
    public enum OnEnableAction
    {
        None,
        GoToHighest,
        GoToLowest,
        GoToMiddle
    }

    [Space(20)]
    [Header("Counter Behaviour")]
    public BoundaryAction ActionOnBoundary = BoundaryAction.None;
    public OnEnableAction ActionOnEnable = OnEnableAction.None;

    public UnityEvent<int> OnValueChanged; // Event triggered when the counter value changes

    private void OnEnable()
    {
        switch (ActionOnEnable)
        {
            case OnEnableAction.GoToHighest:
                CurrentValue = HighestValue;
                break;
            case OnEnableAction.GoToLowest:
                CurrentValue = LowestValue;
                break;
            case OnEnableAction.GoToMiddle:
                CurrentValue = (HighestValue + LowestValue) / 2;
                break;
            case OnEnableAction.None:
            default:
                break;
        }

        CheckValueAndDraw();
    }

    /// <summary>
    /// Method to change the counter value directly
    /// </summary>
    /// <param name="value"></param>
    public void HardChangeCounterValue(int value)
    {
        CurrentValue = value;
        CheckValueAndDraw();
    }

    /// <summary>
    ///  Method to change the counter value by cycling through a range
    /// </summary>
    /// <param name="value">Increment Value to go up by. This supports negative numbers too. The Addition value is Times on top of it </param>
    public void CycleCounterValue(int value)
    {
        value *= AdditionValue;

        if (ActionOnBoundary == BoundaryAction.Cycle)
        {
            CurrentValue = (CurrentValue + value - LowestValue) % (HighestValue - LowestValue + 1) + LowestValue;
        }
        else
        {
            if (value > 0)
            {
                if (CurrentValue + value <= HighestValue)
                {
                    CurrentValue += value;
                }
                else if (ActionOnBoundary == BoundaryAction.Reset)
                {
                    CurrentValue = LowestValue;
                }
            }
            else if (value < 0)
            {
                if (CurrentValue + value >= LowestValue)
                {
                    CurrentValue += value;
                }
                else if (ActionOnBoundary == BoundaryAction.Reset)
                {
                    CurrentValue = HighestValue;
                }
            }
        }

        CheckValueAndDraw();
    }


    public void CheckValueAndDraw()
    {
        CheckValueOutsideBoundaries();
        _valueText.text = CurrentValue.ToString();
        OnValueChanged?.Invoke(CurrentValue);
    }

    /// <summary>
    /// Method to ensure the counter value stays within defined boundaries
    /// </summary>
    private void CheckValueOutsideBoundaries()
    {
        if (CurrentValue < LowestValue)
        {
            CurrentValue = LowestValue;
        }
        else if (CurrentValue > HighestValue)
        {
            CurrentValue = HighestValue;
        }

        _valueText.text = CurrentValue.ToString();

        if (HideIfAtBoundaries)
        {
            if (_minusButtonRawImage != null)
            {
                _minusButtonRawImage.gameObject.SetActive(CurrentValue > LowestValue);
            }

            if (_plusButtonRawImage != null)
            {
                _plusButtonRawImage.gameObject.SetActive(CurrentValue < HighestValue);
            }
        }
    }
}
