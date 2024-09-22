using BuildingModule;
using UnityEngine;

public class BuildingRelocation : BuildingStrategy
{
    public override Type StrategyType => Type.BuildingRelocation;

    public Building OwnerBuilding { get; private set; }

    public BuildingRelocation(Building building)
    {
        OwnerBuilding = building;
    }

    public override void Enter()
    {
        BuildingSystem.Instance.TryPickup(this);
    }

    public override void Update()
    {
        BuildingSystem.Instance.FollowBuilding(BuildingInputSystem.Instance.GetMouseWorldPosition());
    }

    public override void Exit()
    {
        BuildingSystem.Instance.Place();
    }

    public void Place()
    {
        OwnerBuilding.HideTile();
        OwnerBuilding.HidePlaceConfirmBubble();
    }

    public void Pickup()
    {
        OwnerBuilding.ShowTile();
        OwnerBuilding.ShowPlaceConfirmBubble();
    }

    public void Buildable()
    {
        var color = Color.green;
        color.a *= 0.5f;
        OwnerBuilding.SetTileColor(color);
        OwnerBuilding.ActivatePlaceConfirmBubble();
    }

    public void Unbuildable()
    {
        var color = Color.red;
        color.a *= 0.5f;
        OwnerBuilding.SetTileColor(color);

        OwnerBuilding.DeactivatePlaceConfirmBubble();
    }
}