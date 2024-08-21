using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Slider))]
public class SliderAnimatorComponent : MonoBehaviour
{
    private Slider slider;

    [SerializeField] private Transform shakeTarget; // The transform to apply the shake animation to

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    /// <summary>
    /// Moves the slider to a specific value, animating if duration is greater than 0.
    /// </summary>
    /// <param name="value">The target value to move the slider to.</param>
    /// <param name="duration">The duration over which to animate the slider. If 0, the change is instant.</param>
    public void SetValue(float value, float duration = 0f)
    {
        if ( value == slider.value)
        {
            return;
        }

        if (duration > 0)
        {
            StartCoroutine(AnimateSlider(value, duration));
        }
        else
        {
            slider.value = Mathf.Clamp(value, slider.minValue, slider.maxValue);
        }
    }

    /// <summary>
    /// Moves the slider to a specific percentage (0-1), animating if duration is greater than 0.
    /// </summary>
    /// <param name="percentage">The target percentage (0 to 1) to move the slider to.</param>
    /// <param name="duration">The duration over which to animate the slider. If 0, the change is instant.</param>
    public void SetPercentage(float percentage, float duration = 0f)
    {
        float value = Mathf.Lerp(slider.minValue, slider.maxValue, percentage);

        if (duration > 0)
        {
            StartCoroutine(AnimateSlider(value, duration));
        }
        else
        {
            slider.value = value;
        }
    }

    /// <summary>
    /// Increments the slider's value by a specific amount, animating if duration is greater than 0.
    /// </summary>
    /// <param name="increment">The amount by which to increase the slider's value.</param>
    /// <param name="duration">The duration over which to animate the slider. If 0, the change is instant.</param>
    public void IncrementValue(float increment, float duration = 0f)
    {
        if (duration > 0)
        {
            StartCoroutine(AnimateSlider(slider.value + increment, duration));
        }
        else
        {
            slider.value = Mathf.Clamp(slider.value + increment, slider.minValue, slider.maxValue);
        }
    }

    /// <summary>
    /// Decrements the slider's value by a specific amount, animating if duration is greater than 0.
    /// </summary>
    /// <param name="decrement">The amount by which to decrease the slider's value.</param>
    /// <param name="duration">The duration over which to animate the slider. If 0, the change is instant.</param>
    public void DecrementValue(float decrement, float duration = 0f)
    {
        if (duration > 0)
        {
            StartCoroutine(AnimateSlider(slider.value - decrement, duration));
        }
        else
        {
            slider.value = Mathf.Clamp(slider.value - decrement, slider.minValue, slider.maxValue);
        }
    }

    /// <summary>
    /// Moves the slider to its minimum value, animating if duration is greater than 0.
    /// </summary>
    /// <param name="duration">The duration over which to animate the slider to its minimum value. If 0, the change is instant.</param>
    public void MoveToMin(float duration = 0f)
    {
        if (duration > 0)
        {
            StartCoroutine(AnimateSlider(slider.minValue, duration));
        }
        else
        {
            slider.value = slider.minValue;
        }
    }

    /// <summary>
    /// Moves the slider to its maximum value, animating if duration is greater than 0.
    /// </summary>
    /// <param name="duration">The duration over which to animate the slider to its maximum value. If 0, the change is instant.</param>
    public void MoveToMax(float duration = 0f)
    {
        if (duration > 0)
        {
            StartCoroutine(AnimateSlider(slider.maxValue, duration));
        }
        else
        {
            slider.value = slider.maxValue;
        }
    }



    public void SetMax(int max)
    {
        slider.maxValue = max;
    }

    public void SetMin(int min)
    {
        slider.minValue = min;
    }


    /// <summary>
    /// Creates a ping-pong animation between two values, animating if duration is greater than 0.
    /// </summary>
    /// <param name="startValue">The starting value for the ping-pong animation.</param>
    /// <param name="endValue">The ending value for the ping-pong animation.</param>
    /// <param name="duration">The duration over which to animate between the values. If 0, the slider will just set to the startValue.</param>
    public void PingPong(float startValue, float endValue, float duration)
    {
        if (duration > 0)
        {
            StartCoroutine(PingPongSlider(startValue, endValue, duration));
        }
        else
        {
            slider.value = startValue; // Start at the startValue if no animation is required
        }
    }

    /// <summary>
    /// Changes the slider's fill color, animating if duration is greater than 0.
    /// </summary>
    /// <param name="color">The new color for the slider's fill area.</param>
    /// <param name="duration">The duration over which to animate the color change. If 0, the change is instant.</param>
    public void ChangeFillColor(Color color, float duration = 0f)
    {
        if (slider.fillRect != null)
        {
            Image fillImage = slider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                if (duration > 0)
                {
                    StartCoroutine(AnimateColor(fillImage, fillImage.color, color, duration));
                }
                else
                {
                    fillImage.color = color;
                }
            }
        }
    }

    /// <summary>
    /// Changes the slider's handle color, animating if duration is greater than 0.
    /// </summary>
    /// <param name="color">The new color for the slider's handle.</param>
    /// <param name="duration">The duration over which to animate the color change. If 0, the change is instant.</param>
    public void ChangeHandleColor(Color color, float duration = 0f)
    {
        if (slider.handleRect != null)
        {
            Image handleImage = slider.handleRect.GetComponent<Image>();
            if (handleImage != null)
            {
                if (duration > 0)
                {
                    StartCoroutine(AnimateColor(handleImage, handleImage.color, color, duration));
                }
                else
                {
                    handleImage.color = color;
                }
            }
        }
    }

    /// <summary>
    /// Changes the slider's background color, animating if duration is greater than 0.
    /// </summary>
    /// <param name="color">The new color for the slider's background area.</param>
    /// <param name="duration">The duration over which to animate the color change. If 0, the change is instant.</param>
    public void ChangeBackgroundColor(Color color, float duration = 0f)
    {
        if (slider.targetGraphic != null)
        {
            Graphic backgroundGraphic = slider.targetGraphic;
            if (backgroundGraphic != null)
            {
                if (duration > 0)
                {
                    StartCoroutine(AnimateColor(backgroundGraphic, backgroundGraphic.color, color, duration));
                }
                else
                {
                    backgroundGraphic.color = color;
                }
            }
        }
    }

    /// <summary>
    /// Shakes the specified transform with the given duration and magnitude.
    /// </summary>
    /// <param name="duration">The duration of the shake effect.</param>
    /// <param name="magnitude">The magnitude of the shake effect.</param>
    public void Shake(float duration, float magnitude)
    {
        if (duration > 0 && shakeTarget != null)
        {
            StartCoroutine(ShakeTransform(shakeTarget, duration, magnitude));
        }
    }

    private IEnumerator AnimateSlider(float targetValue, float duration)
    {
        float startValue = slider.value;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, targetValue, elapsedTime / duration);
            yield return null;
        }

        slider.value = targetValue; // Ensure it ends on the exact value
    }

    private IEnumerator PingPongSlider(float startValue, float endValue, float duration)
    {
        while (true)
        {
            yield return StartCoroutine(AnimateSlider(endValue, duration));
            yield return StartCoroutine(AnimateSlider(startValue, duration));
        }
    }

    private IEnumerator AnimateColor(Graphic graphic, Color startColor, Color targetColor, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            graphic.color = Color.Lerp(startColor, targetColor, elapsedTime / duration);
            yield return null;
        }

        graphic.color = targetColor; // Ensure it ends on the exact color
    }

    private IEnumerator ShakeTransform(Transform target, float duration, float magnitude)
    {
        Vector3 originalPosition = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            target.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPosition; // Ensure it returns to the original position
    }
}
