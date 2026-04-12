using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private NoseJobMenu _menu;

    public void OnDrag(PointerEventData data)
    {
        Debug.Log("dragging");
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData data)
    {
        Debug.Log("pickup");
        _menu.PutDownNose(transform);
    }

    public void OnBeginDrag(PointerEventData data)
    {
        Debug.Log("putdown");
        _menu.PickupNose();
    }
}
