using BuildingModule;
using System;
using System.Diagnostics;
using UnityEngine.Events;

public class BuildingState_Construction : BuildingState
{
    private string m_TimerGuid;
    public Timer Timer => TimerManager.Instance.GetTimerOrNull(m_TimerGuid);

    public BuildingState_Construction(Building ownerBuilding, Param stateParam) : base(ownerBuilding, stateParam)
    {
    }

    #region BuildingState
    protected override void OnEnter()
    {
        Owner.SetBottomBubbleSprite(AssetLoader.LoadSprite("Info"));
        Owner.HideBottomBubble();

        if (Timer == null)
        {
            Owner.SetTopBubbleSprite(AssetLoader.LoadSprite("Checked"));
            Owner.HideTopBubble();

            Owner.SetStateSprite(AssetLoader.LoadSprite("ShopBuilding"));
            Owner.ShowStateSprite();

            Owner.HideBottomBubble();

            var timer = new Timer(Owner.Name, Owner.ProcessingDurationMs);
            m_TimerGuid = timer.Guid;

            TimerManager.Instance.Register(timer, false);
            timer.Play();
        }
        else
        {
            if (Timer.Status == TimerStatus.Finish)
            {
                Owner.ShowTopBubble();
            }
        }

        Owner.OnTopBubbleClickedEvent += OnTopBubbleClicked;
        Owner.OnBottomBubbleClickedEvent += OnBottomBubbleClicked;
        Owner.OnSelectedEvent += OnInteractBegin;
        Owner.OnPressedEvent += OnPressed;

        Timer.OnTimerFinishedEvent += OnFinishedConstruction;
    }

    protected override void OnExit()
    {
        Owner.OnTopBubbleClickedEvent -= OnTopBubbleClicked;
        Owner.OnBottomBubbleClickedEvent -= OnBottomBubbleClicked;
        Owner.OnSelectedEvent -= OnInteractBegin;
        Owner.OnPressedEvent -= OnPressed;

        if (Timer != null)
        {
            Timer.OnTimerFinishedEvent -= OnFinishedConstruction;
        }
    }

    protected override void OnUpdate()
    {
        
    }
    #endregion

    public void Complete()
    {
        StateParam.isConstructionCompleted = true;
        Timer.OnTimerFinishedEvent -= OnFinishedConstruction;
        TimerManager.Instance.Unregister(Timer);
        Owner.TransitionState(Building.State.Idle);
    }

    private void OnTopBubbleClicked()
    {
        Complete();
    }

    private void OnBottomBubbleClicked()
    {
        var popup = UiManager.Instance.OpenView<BuildStatusPopup>("Popup_BuildingStatus", (popup) => popup.Setup(this));
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