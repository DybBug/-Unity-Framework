public abstract class BuildingStrategy
{
    public enum Type
    {
        BuildingRelocation
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
    public abstract BuildingStrategy.Type StrategyType { get; }
}