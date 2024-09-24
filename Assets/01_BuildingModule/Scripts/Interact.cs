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
    public event UnityAction<InteractEventParam> OnInteractingEvent;
    public event UnityAction<InteractEventParam> OnInteractEndEvent;

    public void TriggerInteractBegin(Object interactObject, Vector3 mouseWorldPosition)
    {
        OnInteractBeginEvent?.Invoke(new InteractEventParam()
        {
            Owner = interactObject,
            mouseWorldPose = mouseWorldPosition
        });
    }

    public void TriggerInteracting(Object interactObject, Vector3 mouseWorldPosition)
    {
        OnInteractingEvent?.Invoke(new InteractEventParam()
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
}
