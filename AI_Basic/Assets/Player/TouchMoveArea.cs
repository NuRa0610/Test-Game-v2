using UnityEngine;
using UnityEngine.EventSystems;

public class TouchMoveArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private float _maxDragDistance = 120f;
    [SerializeField, Range(0f, 0.9f)] private float _deadZone = 0.1f;

    private Vector2 _startScreenPosition;

    public void OnPointerDown(PointerEventData eventData)
    {
        _startScreenPosition = eventData.position;
        CrossPlatformInput.ClearVirtualMove();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - _startScreenPosition;
        float maxDistance = Mathf.Max(1f, _maxDragDistance);
        Vector2 normalized = Vector2.ClampMagnitude(delta / maxDistance, 1f);

        if (normalized.magnitude < _deadZone)
        {
            CrossPlatformInput.ClearVirtualMove();
            return;
        }

        CrossPlatformInput.SetVirtualMove(normalized);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CrossPlatformInput.ClearVirtualMove();
    }
}
