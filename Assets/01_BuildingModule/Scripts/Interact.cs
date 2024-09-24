using UnityEngine;
using UnityEngine.Events;

public struct InteractEventParam
{
    public Object Owner;
    public Vector3 mouseWorldPose;
}


public class Interact : MonoBehaviour
{
    public event UnityAction<InteractEventParam> OnInteractBeginEvent;
    public event UnityAction<InteractEventParam> OnDragEvent;
    public event UnityAction<InteractEventParam> OnInteractEndEvent;
    public event UnityAction<InteractEventParam> OnPressEvent;

    public void TriggerInteractBegin(Object interactObject, Vector3 mouseWorldPosition)
    {
        OnInteractBeginEvent?.Invoke(new InteractEventParam()
        {
            Owner = interactObject,
            mouseWorldPose = mouseWorldPosition
        });
    }

    public void TriggerDrag(Object interactObject, Vector3 mouseWorldPosition)
    {
        OnDragEvent?.Invoke(new InteractEventParam()
        {
            Owner = interactObject,
            mouseWorldPose = mouseWorldPosition
        });
    }

    public void TriggerInteractEnd(Object interactObject, Vector3 mouseWorldPosition)
    {
        OnInteractEndEvent?.Invoke(new InteractEventParam()
        {
            Owner = interactObject,
            mouseWorldPose = mouseWorldPosition
        });
    }

    public void TriggerPress(Object interactObject, Vector3 mouseWorldPosition)
    {
        OnPressEvent?.Invoke(new InteractEventParam()
        {
            Owner = interactObject,
            mouseWorldPose = mouseWorldPosition
        });
    }
}
