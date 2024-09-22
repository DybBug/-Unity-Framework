using UnityEngine;
using UnityEngine.Events;

public struct InteractEventListener
{
    public UnityAction<InteractEventParam> OnInteractBeginEvent;
    public UnityAction<InteractEventParam> OnInteractingEvent;
    public UnityAction<InteractEventParam> OnInteractEndEvent;
}

public struct InteractEventParam
{
    public Object Owner;
    public Vector3 mouseWorldPose;
}


public class Interact : MonoBehaviour
{
    private event UnityAction<InteractEventParam> m_InteractBeginEvent;
    private event UnityAction<InteractEventParam> m_InteractingEvent;
    private event UnityAction<InteractEventParam> m_InteractEndEvent;

    public void Register(InteractEventListener eventListener)
    {
        m_InteractBeginEvent = eventListener.OnInteractBeginEvent;
        m_InteractingEvent = eventListener.OnInteractingEvent;
        m_InteractEndEvent = eventListener.OnInteractEndEvent;
    }

    public void UnRegister()
    {
        m_InteractBeginEvent = null;
        m_InteractingEvent = null;
        m_InteractEndEvent = null;
    }

    public void TriggerInteractBegin(Object interactObject, Vector3 mouseWorldPosition)
    {
        m_InteractBeginEvent?.Invoke(new InteractEventParam()
        {
            Owner = interactObject,
            mouseWorldPose = mouseWorldPosition
        });
    }

    public void TriggerInteracting(Object interactObject, Vector3 mouseWorldPosition)
    {
        m_InteractingEvent?.Invoke(new InteractEventParam()
        {
            Owner = interactObject,
            mouseWorldPose = mouseWorldPosition
        });
    }

    public void TriggerInteractEnd(Object interactObject, Vector3 mouseWorldPosition)
    {
        m_InteractEndEvent?.Invoke(new InteractEventParam()
        {
            Owner = interactObject,
            mouseWorldPose = mouseWorldPosition
        });
    }
}
