using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildStatusPopup : PopupView
{
    private enum State
    {
        InProgress,
        WaitingForCompletion
    }

    [Header("-- Top --")]
    [SerializeField] private Button m_CloseButton;

    [Header("-- Middle --")]
    [SerializeField] private TMP_Text m_StartTimeText;
    [SerializeField] private TMP_Text m_EndTimeText;

    [Header("-- Bottom --")]
    [SerializeField] private TMP_Text m_StatusText;
    [SerializeField] private Slider m_ProgressBar;
    [SerializeField] private TMP_Text m_ProgressText;
    [SerializeField] private Button m_SkipButton;
    [SerializeField] private Button m_CompletionButton;

    [Header("-- TEST --")]
    [SerializeField] private int m_Days;
    [SerializeField] private int m_Hours;
    [SerializeField] private int m_Minutes;
    [SerializeField] private int m_Seconds;

    private BuildingState_Construction m_ConstructionBuilding;
    private State m_CurrentState;

    protected override void OnInitialize()
    {
        m_CloseButton.onClick.AddListener(OnClickedCloseButton);
        m_SkipButton.onClick.AddListener(OnClickedSkipButton);
        m_CompletionButton.onClick.AddListener(OnClickedCompletionButton);
    }

    protected override void OnTick(float dt)
    {
        if (m_CurrentState == State.InProgress)
        {
            m_ProgressBar.value = m_ConstructionBuilding.Timer.GetProcessRate();
            m_ProgressText.text = TimeUtil.ConvertToDHMS(m_ConstructionBuilding.Timer.GetRemainTimeMs());
        }
    }
    protected override void OnOpen()
    {
        m_ProgressBar.value = m_ConstructionBuilding.Timer.GetProcessRate();
        m_ProgressText.text = TimeUtil.ConvertToDHMS(m_ConstructionBuilding.Timer.GetRemainTimeMs()); 

        if (m_ConstructionBuilding.Timer.Status == TimerStatus.Running)
        {
            SetState(State.InProgress);
        }
        else if (m_ConstructionBuilding.Timer.Status == TimerStatus.Finish)
        {
            SetState(State.WaitingForCompletion);
        }
        BindEvents();
    }

    protected override void OnClose()
    {
        UnbindEvents();
        m_ConstructionBuilding = null;
    }

    public void Setup(BuildingState_Construction constructionBuilding)
    {
        m_ConstructionBuilding = constructionBuilding;
    }


    private void SetState(State state)
    {
        switch (state)
        {
            case State.InProgress:
            {
                m_SkipButton.gameObject.SetActive(true);
                m_CompletionButton.gameObject.SetActive(false);
                m_StatusText.text = $"In Progress";
                break;
            }
            case State.WaitingForCompletion:
            {
                m_StatusText.text = $"Completion";
                m_SkipButton.gameObject.SetActive(false);
                m_CompletionButton.gameObject.SetActive(true);

                m_ProgressBar.value = 1.0f;
                m_ProgressText.text = TimeUtil.ConvertToDHMS(0);
                break;
            }
            default:
            {
                Debug.Assert(false, $"{state} is over range in switch");
                break;
            }
        }
        m_CurrentState = state;
    }

    private void OnClickedCloseButton()
    {
        UiManager.Instance.CloseView(this);
    }


    private void OnClickedSkipButton()
    {
        m_ConstructionBuilding.Timer.Reset(m_ConstructionBuilding.Timer.EndTimeMs, m_ConstructionBuilding.Timer.EndTimeMs);
    }

    private void OnClickedCompletionButton()
    {
        m_ConstructionBuilding.Complete();
        UiManager.Instance.CloseView(this);
    }

    private void OnFinishTimer(Timer timer)
    { 
        SetState(State.WaitingForCompletion);
    }

    private void BindEvents()
    {
        m_ConstructionBuilding.Timer.OnTimerFinishedEvent += OnFinishTimer;
    }

    private void UnbindEvents()
    {
        m_ConstructionBuilding.Timer.OnTimerFinishedEvent -= OnFinishTimer;
    }
}
