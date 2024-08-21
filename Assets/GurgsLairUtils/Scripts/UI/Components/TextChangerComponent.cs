using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Component that changes the text of the attached TextMeshPro component periodically. by cycling through a list of strings.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TextChangerComponent : MonoBehaviour
{
    public List<string> Explanations = new();
    private TMP_Text _textComponent;

    public float TransitionDurationFadeToZero = 0.5f;
    public float TransitionDurationFadeToOne = 0.5f;
    public bool ShouldFade = true;

    private void Awake()
    {
        _textComponent = GetComponent<TMP_Text>();
    }

    public void AddExplanation(string explanation)
    {
        Explanations.Add(explanation);
    }
    public void RemoveExplanation(string explanation)
    {
        Explanations.Remove(explanation);
    }


    private void OnEnable()
    {
        StartCoroutine(ChangeTextPeriodically());
    }

    #region Animations Section
    public IEnumerator ChangeTextPeriodically()
    {
        int index = 0;
        while (true)
        {
            if (ShouldFade)
            {
                yield return FadeTextToZeroAlpha();
            }

            _textComponent.text = Explanations[index];
            index = (index + 1) % Explanations.Count;

            if (ShouldFade)
            {
                yield return FadeTextToOneAlpha();
            }

            yield return new WaitForSeconds(10);
        }
    }
    private IEnumerator FadeTextToZeroAlpha()
    {
        _textComponent.color = new Color(_textComponent.color.r, _textComponent.color.g, _textComponent.color.b, 1);
        while (_textComponent.color.a > 0.0f)
        {
            _textComponent.color = new Color(_textComponent.color.r, _textComponent.color.g, _textComponent.color.b, _textComponent.color.a - (Time.deltaTime / TransitionDurationFadeToZero));
            yield return null;
        }
    }
    private IEnumerator FadeTextToOneAlpha()
    {
        _textComponent.color = new Color(_textComponent.color.r, _textComponent.color.g, _textComponent.color.b, 0);
        while (_textComponent.color.a < 1.0f)
        {
            _textComponent.color = new Color(_textComponent.color.r, _textComponent.color.g, _textComponent.color.b, _textComponent.color.a + (Time.deltaTime / TransitionDurationFadeToOne));
            yield return null;
        }
    }
    #endregion
}
