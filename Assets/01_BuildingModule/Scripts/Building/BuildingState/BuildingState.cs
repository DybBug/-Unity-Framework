public abstract class BuildingState
{
    public class Param
    {
        public bool isConstructionCompleted;
    }

    public Building Owner { get; private set; }
    
    protected Param StateParam { get; private set; }

    public BuildingState(Building ownerBuilding, Param stateParam)
    {
        Owner = ownerBuilding;
        StateParam = stateParam;
    }

    public void Enter() => OnEnter();
    public void Update() => OnUpdate();
    public void Exit() => OnExit();

    protected abstract void OnEnter();
    protected abstract void OnUpdate();
    protected abstract void OnExit();
}