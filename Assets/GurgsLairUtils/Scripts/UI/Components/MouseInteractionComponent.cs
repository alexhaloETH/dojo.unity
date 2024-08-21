using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// this is not an interface, this needs to change
public class MouseInteractionComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private UnityEvent onClickCalls;
    [SerializeField] private UnityEvent onRightClickCalls;
    [SerializeField] private UnityEvent onMiddleClickCalls;
    [SerializeField] private UnityEvent onPointerEnter;
    [SerializeField] private UnityEvent onPointerExit;

    public bool PointerIsOver = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onClickCalls?.Invoke();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            onRightClickCalls?.Invoke();
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            onMiddleClickCalls?.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerIsOver = true;
        onPointerEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerIsOver = false;
        onPointerExit?.Invoke();
    }
}
