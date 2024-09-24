using BuildingModule;
using System;

public class BuildingState_Relocation : BuildingState
{
    public BuildingState_Relocation(Building building, Param stateParam) : base(building, stateParam)
    {
    }

    #region BuildingState
    protected override void OnEnter()
    {
        BuildingSystem.Instance.TryPickup(Owner);

        Owner.SetBottomBubbleSprite(AssetLoader.LoadSprite("Checked"));
        Owner.ShowBottomBubble();

        Owner.HideTopBubble();
        Owner.OnSelectedEvent += OnInteractBegin;
        Owner.OnDragEvent += OnDrag;
        Owner.OnBottomBubbleClickedEvent += OnBottomBubble;
    }

    protected override void OnUpdate()
    {
        
    }

    protected override void OnExit()
    {
        Owner.OnSelectedEvent -= OnInteractBegin;
        Owner.OnDragEvent -= OnDrag;
    }

    private void OnInteractBegin(InteractEventParam eventParam)
    {
        BuildingSystem.Instance.TryPickup(Owner);
    }

    private void OnDrag(InteractEventParam eventParam)
    {
        BuildingSystem.Instance.FollowBuilding(BuildingInputSystem.Instance.GetMouseWorldPosition());
    }

    private void OnBottomBubble()
    {
        BuildingSystem.Instance.Place();
        if (!StateParam.isConstructionCompleted)
        {
            Owner.TransitionState(Building.State.Construction);
        }
        else
        {
            Owner.TransitionState(Building.State.Idle);
        }
    }
    #endregion

}