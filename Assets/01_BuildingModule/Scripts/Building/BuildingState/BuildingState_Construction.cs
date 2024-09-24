using BuildingModule;
using System;
using System.Diagnostics;

public class BuildingState_Construction : BuildingState
{
    private Timer m_Timer;

    public BuildingState_Construction(Building ownerBuilding, Param stateParam) : base(ownerBuilding, stateParam)
    {
    }

    #region BuildingState
    protected override void OnEnter()
    {
        Owner.SetBottomBubbleSprite(AssetLoader.LoadSprite("Info"));
        Owner.HideBottomBubble();

        if (m_Timer == null)
        {
            Owner.SetTopBubbleSprite(AssetLoader.LoadSprite("Checked"));
            Owner.HideTopBubble();

            Owner.SetStateSprite(AssetLoader.LoadSprite("ShopBuilding"));
            Owner.ShowStateSprite();

            Owner.HideBottomBubble();

            m_Timer = new Timer(Owner.Name, Owner.ProcessingDurationMs);

            TimerManager.Instance.Register(m_Timer, false);
            m_Timer.Play();
        }
        else
        {
            if (m_Timer.Status == TimerStatus.Finish)
            {
                Owner.ShowTopBubble();
            }
        }

        Owner.OnTopBubbleClickedEvent += OnTopBubbleClicked;
        Owner.OnBottomBubbleClickedEvent += OnBottomBubbleClicked;
        Owner.OnSelectedEvent += OnInteractBegin;
        Owner.OnPressedEvent += OnPressed;

        m_Timer.OnTimerFinishedEvent += OnFinishedConstruction;
    }

    protected override void OnExit()
    {
        Owner.OnTopBubbleClickedEvent -= OnTopBubbleClicked;
        Owner.OnBottomBubbleClickedEvent -= OnBottomBubbleClicked;
        Owner.OnSelectedEvent -= OnInteractBegin;
        Owner.OnPressedEvent -= OnPressed;

        if (m_Timer != null)
        {
            m_Timer.OnTimerFinishedEvent -= OnFinishedConstruction;
        }
    }

    protected override void OnUpdate()
    {
        
    }
    #endregion

    private void OnTopBubbleClicked()
    {
        StateParam.isConstructionCompleted = true;
        TimerManager.Instance.Unregister(m_Timer);
        m_Timer.OnTimerFinishedEvent -= OnFinishedConstruction;
        m_Timer = null;
        Owner.TransitionState(Building.State.Idle);
    }

    private void OnBottomBubbleClicked()
    {
        var popup = UiManager.Instance.OpenView<BuildStatusPopup>("Popup_BuildingStatus");
    }

    private void OnFinishedConstruction(Timer arg0)
    {
        Owner.ShowTopBubble();

    }
    private void OnInteractBegin(InteractEventParam eventParam)
    {
        Owner.ShowBottomBubble();
    }

    private void OnPressed(InteractEventParam eventParam)
    {
        Owner.TransitionState(Building.State.Relocation);
    }
}