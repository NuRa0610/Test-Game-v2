using UnityEngine;
using UnityEngine.EventSystems;

public class TouchLookArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private float _sensitivity = 0.03f;
    [SerializeField] private bool _invertY = true;

    public void OnPointerDown(PointerEventData eventData)
    {
        CrossPlatformInput.ClearVirtualLook();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 lookDelta = eventData.delta * _sensitivity;
        if (_invertY)
        {
            lookDelta.y *= -1f;
        }

        CrossPlatformInput.SetVirtualLook(lookDelta);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CrossPlatformInput.ClearVirtualLook();
    }
}
