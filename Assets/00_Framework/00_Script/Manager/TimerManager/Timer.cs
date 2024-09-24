using System;
using System.Diagnostics;
using UnityEngine.Events;

public enum TimerStatus
{
    Waiting,
    Running,
    Finish
}

public class Timer
{
    public event UnityAction<Timer> OnTimerFinishedEvent;
    public string Name { get; private set; }
    public TimerStatus Status { get; private set; }
    public long StartTimeMs { get; private set; }
    public long EndTimeMs { get; private set; }

    private long m_DurationMs = -1;

    private string m_Guid;
    public string Guid
    {
        get
        {
            if (m_Guid == null)
            {
                m_Guid = System.Guid.NewGuid().ToString();
            }
            return m_Guid;
        }
    }

    public Timer(string name, long durationMs)
    {
        Status = TimerStatus.Waiting;
        Name = $"{name}_{this.Guid}";
        m_DurationMs = durationMs;
    }

    public Timer(string name, long startTimeMs, long endTimeMs)
    {
        Debug.Assert(startTimeMs <= endTimeMs);
        Status = TimerStatus.Waiting;

        Name = $"{name}_{this.Guid}";
        StartTimeMs = startTimeMs;
        EndTimeMs = endTimeMs;
    }

    public void Play()
    {
        Status = TimerStatus.Running;
        if (m_DurationMs >= 0)
        {
            StartTimeMs = TimeUtil.NowUtc();
            EndTimeMs = StartTimeMs + m_DurationMs;
        }
    }

    public void Stop(bool isNotify)
    {
        Status = TimerStatus.Finish;
        StartTimeMs = 0;
        EndTimeMs = 0;
        m_DurationMs = -1;
        if (isNotify)
        {
            OnTimerFinishedEvent?.Invoke(this);
        }
    }

    public void Skip(long skipTimeMs)
    {
        EndTimeMs = skipTimeMs;
    }

    public void Reset(long startTimeMs, long endTimeMs)
    {
        StartTimeMs = startTimeMs;
        EndTimeMs = endTimeMs;
        m_DurationMs = -1;
    }

    public void AddEndTime(long addTimeMs)
    {
        EndTimeMs = Math.Max(StartTimeMs, EndTimeMs + addTimeMs);
    }

    public float GetProcessRate()
    {
        if (Status == TimerStatus.Waiting)
            return 0f;

        if (Status == TimerStatus.Finish)
            return 1.0f;

        if (m_DurationMs <= 0)
            return 0f;

        return (1.0f - (float)GetRemainTimeMs() / (float)m_DurationMs);
    }

    public long GetRemainTimeMs()
    {
        if (Status == TimerStatus.Waiting)
            return m_DurationMs;

        if (Status == TimerStatus.Finish)
            return 0;

        var remainMs = EndTimeMs - TimeUtil.NowUtc();
        return Math.Max(remainMs, 0);
    }
}
