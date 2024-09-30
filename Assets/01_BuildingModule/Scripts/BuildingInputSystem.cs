using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingInputSystem : MonoBehaviour
{
    private static BuildingInputSystem _instance;
    public static BuildingInputSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                var gameObject = new GameObject("BuildingInputSystem");
                _instance = gameObject.AddComponent<BuildingInputSystem>();
            }
            return _instance;
        }
    }


    private Mouse m_Mouse;
    private Interact m_InteractObject;
    private bool m_IsPress;
    private bool m_IsMove;

    private bool m_IsSelectedStartPos;
    private bool m_IsSelectedEndPos;
    private Vector3 m_StartPos;
    private Vector3 m_EndPos;
    private PathFinder m_PathFinder = new();

    private void Awake()
    {
        m_Mouse = InputSystem.GetDevice<Mouse>();

        var playerInput = GetComponent<PlayerInput>();
        playerInput.actions.FindAction("Select").started += OnStartedSelect;
        playerInput.actions.FindAction("Select").canceled += OnCanceledSelect;
        playerInput.actions.FindAction("Select").performed += OnPerformedSelect;

        playerInput.actions.FindAction("Move").performed += OnPerformedMove;

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (m_IsMove)
        {
            m_IsMove = m_Mouse.delta.ReadValue() != Vector2.zero;
            m_IsPress = false;
        }

        if (m_IsPress)
        {
            m_InteractObject.TriggerPress(m_InteractObject, GetMouseWorldPosition());
        }
    }

    public Vector3 GetMouseWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(m_Mouse.position.ReadValue());
    }

    private void OnStartedSelect(InputAction.CallbackContext context)
    {
        var mouseWorldPos = GetMouseWorldPosition();

        var collider = Physics2D.OverlapPoint(mouseWorldPos);
        if (collider != null)
        {
            if (collider.TryGetComponent<Interact>(out m_InteractObject))
            {
                m_InteractObject.TriggerInteractBegin(m_InteractObject, mouseWorldPos);
                return;
            }
        }

        if (!m_IsSelectedStartPos)
        {
            m_IsSelectedStartPos = true;
            m_StartPos = mouseWorldPos;
        }
        else
        {
            m_IsSelectedEndPos = true;
            m_EndPos = mouseWorldPos;
        }
    }

    private void OnPerformedSelect(InputAction.CallbackContext context)
    {
        if (m_InteractObject)
        {
            m_IsPress = true;
        }
    }

    private void OnCanceledSelect(InputAction.CallbackContext context)
    {
        if (m_IsSelectedEndPos)
        {
            m_IsSelectedStartPos = false;
            m_IsSelectedEndPos = false;
            var posList = m_PathFinder.FindPathOrNull(m_StartPos, m_EndPos);
            if (posList != null)
            {
                for (var i = 0; i < posList.Count - 1; ++i)
                {
                    Debug.DrawLine(posList[i], posList[i + 1], Color.red, 10000);
                }
            }
        }

        m_IsPress = false;
        if (m_InteractObject)
        {
            m_InteractObject.TriggerInteractEnd(m_InteractObject, GetMouseWorldPosition());
        }
        m_InteractObject = null;
    }

    private void OnPerformedMove(InputAction.CallbackContext context)
    {
        if (m_InteractObject)
        {
            m_IsMove = true;
            m_InteractObject.TriggerDrag(m_InteractObject, GetMouseWorldPosition());
        }
    }
}
