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

        Owner.OnSelectedEvent += OnInteractBegin;

        if (!StateParam.isConstructionCompleted)
        {
            Owner.TransitionState(Building.State.Construction);
        }
    }

    protected override void OnExit()
    {
        Owner.OnSelectedEvent -= OnInteractBegin;
    }

    protected override void OnUpdate()
    {
        
    }

    private void OnInteractBegin(InteractEventParam eventParam)
    {
        Owner.TransitionState(Building.State.Relocation);
    }
}