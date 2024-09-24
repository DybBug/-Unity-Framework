using BuildingModule;

public class BuildingState_Idle : BuildingState
{
    public BuildingState_Idle(Building ownerBuilding, Param stateParam) : base(ownerBuilding, stateParam)
    {

    }

    protected override void OnEnter()
    {
        Owner.HideTopBubble();
        Owner.HideStateSprite();
        Owner.HidePlaceConfirmBubble();

        Owner.OnSelectedEvent += OnSelected;
        Owner.OnPressedEvent += OnPressed;

        if (!StateParam.isConstructionCompleted)
        {
            Owner.TransitionState(Building.State.Construction);
        }
    }

    protected override void OnExit()
    {
        Owner.OnSelectedEvent -= OnSelected;
        Owner.OnPressedEvent -= OnPressed;

    }

    protected override void OnUpdate()
    {
        
    }

    private void OnSelected(InteractEventParam eventParam)
    {
        
    }

    private void OnPressed(InteractEventParam eventParam)
    {
        Owner.TransitionState(Building.State.Relocation);
    }
}