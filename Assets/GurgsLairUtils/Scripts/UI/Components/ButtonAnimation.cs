using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour, IInteractivityState
{
    [Space(20)]
    [Header("References")]
    [SerializeField] Sprite _border;
    [SerializeField] Sprite _bordoWithHalo;
    [Space(5)]
    [SerializeField] Image _borderImage;
    [SerializeField] RawImage _background;
    [SerializeField] TMP_Text _textObject;
    [SerializeField] CanvasGroup _canvasGroup;

    //pretty sure the colours are actuually not used anyway, they should be another component 
    //this doesnt have to be public just serialized private
    [Space(20)]
    [Header("Colours")]
    public Color StartBordoColor = Color.white;
    public Color EndBordoColor = Color.black;
    [Space(5)]
    public Color StartFondoColor = Color.black;
    public Color EndFondoColor = Color.white;
    [Space(5)]
    public Color StartTextColor = Color.white;
    public Color EndTextColor = Color.black;
    public Color ErrorColor = Color.red;

    //so this too
    [Space(20)]
    [Header("Timings")]
    public float FadeInDuration = 0.2f; // Duration of the fade-in transition
    public float FadeOutDuration = 0.5f; // Duration of the fade-out transition
    public float PauseDuration = 0.4f; // Pause duration before reverting
    public float ShakeDuration = 0.5f;
    public float ShakeMagnitude = 10f;

    //this could be a script in it self SoundInteractivityComponent just be careful tho, if you want to generalize it using an instance script is not the way to go
    [Space(20)]
    [Header("Sound")]
    public bool AllowedToMakeSounds = true;  
    [SerializeField] AudioClip[] _onClickSFX;
    [SerializeField] AudioClip[] _onHoverEnterSFX;
    [SerializeField] AudioClip[] _onHoverLeaveSFX;

    [Space(20)]
    [Header("Interaction")]
    private bool _busy = false;
    [SerializeField] bool Interactable = true;

    //this isnt correct
    public bool Awaitable = true;  //does the user need to wait for the animation to finish before clicking again

    private void Start()
    {
        _borderImage.sprite = _border;
        _background.color = StartFondoColor;

        if (_textObject != null)
            _textObject.color = StartTextColor;

        SetInteractivity(Interactable);
    }
    private void OnEnable()
    {
        ResetButtonState();
    }

    public void OnPointerEnter()
    {
        if (!Interactable) return;
        _borderImage.sprite = _bordoWithHalo;
    }
    public void OnPointerExit()
    {
        if (!Interactable) return;
        _borderImage.sprite = _border;
    }
    public void OnPointerClick()
    {
        if (!Interactable) return;

        if (Awaitable && _busy) return;
        StartCoroutine(ClickEffect());
    }

    public void SetInteractivity(bool isInteractable)
    {
        Interactable = isInteractable;
        _canvasGroup.interactable = isInteractable;

        _canvasGroup.alpha = isInteractable ? 1 : 0.5f;
    }

    private IEnumerator ClickEffect()
    {
        float elapsedTime = 0;
        _busy = true;

        while (elapsedTime < FadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / FadeInDuration;

            _borderImage.color = Color.Lerp(StartBordoColor, EndBordoColor, t);
            _background.color = Color.Lerp(StartFondoColor, EndFondoColor, t);

            if (_textObject != null)
                _textObject.color = Color.Lerp(StartTextColor, EndTextColor, t);

            yield return null;
        }

        _borderImage.color = EndBordoColor;
        _background.color = EndFondoColor;

        if (_textObject != null)
            _textObject.color = EndTextColor;

        yield return new WaitForSeconds(PauseDuration);

        elapsedTime = 0;
        while (elapsedTime < FadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / FadeOutDuration;

            _borderImage.color = Color.Lerp(EndBordoColor, StartBordoColor, t);
            _background.color = Color.Lerp(EndFondoColor, StartFondoColor, t);

            if (_textObject != null)
                _textObject.color = Color.Lerp(EndTextColor, StartTextColor, t);

            yield return null;
        }

        _borderImage.color = StartBordoColor;
        _background.color = StartFondoColor;

        if (_textObject != null)
            _textObject.color = StartTextColor;

        _busy = false;
    }
    public void ResetButtonState()
    {
        _borderImage.color = StartBordoColor;
        _background.color = StartFondoColor;

        if (_textObject != null)
            _textObject.color = StartTextColor;

        _borderImage.sprite = _border;

        _busy = false;
    }

    public void TriggerErrorEffect()
    {
        StartCoroutine(FadeToRed());
        StartCoroutine(ShakeButton());
    }
    private IEnumerator FadeToRed()
    {
        float elapsedTime = 0;
        while (elapsedTime < FadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / FadeInDuration;

            _borderImage.color = Color.Lerp(_borderImage.color, ErrorColor, t);
            yield return null;
        }

        _borderImage.color = ErrorColor;
        yield return new WaitForSeconds(PauseDuration);

        elapsedTime = 0;
        while (elapsedTime < FadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / FadeOutDuration;

            _borderImage.color = Color.Lerp(ErrorColor, StartBordoColor, t);
            yield return null;
        }

        _borderImage.color = StartBordoColor;
    }
    private IEnumerator ShakeButton()
    {
        Vector3 originalPos = transform.localPosition;
        float elapsedTime = 0;

        while (elapsedTime < ShakeDuration)
        {
            elapsedTime += Time.deltaTime;
            float x = Random.Range(-1f, 1f) * ShakeMagnitude;
            float y = Random.Range(-1f, 1f) * ShakeMagnitude;
            transform.localPosition = new Vector3(x, y, originalPos.z);

            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
