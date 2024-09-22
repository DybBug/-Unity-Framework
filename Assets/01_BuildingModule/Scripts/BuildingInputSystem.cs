using BuildingModule;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingInputSystem : MonoBehaviour
{
    private Mouse m_Mouse;

    private void Awake()
    {
        m_Mouse = InputSystem.GetDevice<Mouse>();

        var playerInput = GetComponent<PlayerInput>();
        playerInput.actions.FindAction("Select").started += OnStartedSelect;
        playerInput.actions.FindAction("Select").canceled += OnCanceledSelect;
        playerInput.actions.FindAction("Move").performed += OnPerformedMove;
    }

    private void OnStartedSelect(InputAction.CallbackContext context)
    {
        var collider = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(m_Mouse.position.ReadValue()));
        if (collider != null)
        {
            BuildingSystem.Instance.Pickup(collider.gameObject.GetComponent<Building>());
        }
    }

    private void OnCanceledSelect(InputAction.CallbackContext context)
    {
        BuildingSystem.Instance.Place();
    }

    private void OnPerformedMove(InputAction.CallbackContext context)
    {
        BuildingSystem.Instance.FollowBuilding(Camera.main.ScreenToWorldPoint(m_Mouse.position.ReadValue()));
    }
}
