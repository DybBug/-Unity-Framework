using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildStatusPopup : PopupView
{
    private static Timer _Timer;
    private enum State
    {
        WaitingForStart,
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
    [SerializeField] private Button m_StartButton;
    [SerializeField] private Button m_SkipButton;
    [SerializeField] private Button m_CompletionButton;

    [Header("-- TEST --")]
    [SerializeField] private int m_Days;
    [SerializeField] private int m_Hours;
    [SerializeField] private int m_Minutes;
    [SerializeField] private int m_Seconds;

    private State m_CurrentState;
    private long m_StartTimeMs;
    private long m_EndTimeMs;

    private void Awake()
    {
        m_CloseButton.onClick.AddListener(OnClickedCloseButton);
        m_StartButton.onClick.AddListener(OnClickedStartButton);
        m_SkipButton.onClick.AddListener(OnClickedSkipButton);
        m_CompletionButton.onClick.AddListener(OnClickedCompletionButton);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_CurrentState == State.InProgress)
        {
            m_ProgressBar.value = _Timer.GetProcessRate();
            m_ProgressText.text = TimeUtil.ConvertToDHMS(_Timer.GetRemainTimeMs());
        }
    }


    protected override void OnInitialize()
    {
        if (_Timer == null || _Timer.Status != TimerStatus.Running)
        {
            SetState(State.WaitingForStart);
            m_StartTimeMs = TimeUtil.NowUtc();
            m_EndTimeMs = m_StartTimeMs + (long)new TimeSpan(m_Days, m_Hours, m_Minutes, m_Seconds).TotalMilliseconds;
        }
        else
        {
            SetState(State.InProgress);
            m_StartTimeMs = _Timer.StartTimeMs;
            m_EndTimeMs = _Timer.EndTimeMs;
        }

        m_StartTimeText.text = DateTimeOffset.FromUnixTimeMilliseconds(m_StartTimeMs).LocalDateTime.ToString();
        m_EndTimeText.text = DateTimeOffset.FromUnixTimeMilliseconds(m_EndTimeMs).LocalDateTime.ToString();

        m_ProgressText.text = TimeUtil.ConvertToDHMS(m_EndTimeMs - m_StartTimeMs);
    }

    private void SetState(State state)
    {
        switch (state)
        {
            case State.WaitingForStart:
            {
                m_StartButton.gameObject.SetActive(true);
                m_SkipButton.gameObject.SetActive(false);
                m_CompletionButton.gameObject.SetActive(false);
                m_StatusText.text = $"Waiting";
                break;
            }
            case State.InProgress:
            {
                m_StartButton.gameObject.SetActive(false);
                m_SkipButton.gameObject.SetActive(true);
                m_CompletionButton.gameObject.SetActive(false);
                m_StatusText.text = $"In Progress";
                break;
            }
            case State.WaitingForCompletion:
            {
                m_StatusText.text = $"Completion";
                m_StartButton.gameObject.SetActive(false);
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

    private void OnClickedStartButton()
    {
        SetState(State.InProgress);
        if (_Timer == null)
        {
            _Timer = new Timer("BuildTimer", m_StartTimeMs, m_EndTimeMs);
            _Timer.OnTimerFinishedEvent += OnFinishTimer;
            TimerManager.Instance.Register(_Timer, false);
        }
        else
        {
            _Timer.Reset(m_StartTimeMs, m_EndTimeMs);
        }

        _Timer.Play();
    }

    private void OnClickedSkipButton()
    {
        _Timer.Reset(m_StartTimeMs, m_StartTimeMs);

    }

    private void OnClickedCompletionButton()
    {
        Initialize();
    }

    private void OnFinishTimer(Timer timer)
    {
        SetState(State.WaitingForCompletion);
    }
}
