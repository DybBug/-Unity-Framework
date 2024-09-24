using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    #region Instance
    private static TimerManager instance;
    public static TimerManager Instance
    {
        get
        {
            if (instance == null)
            {
                var gameObject = new GameObject("TimerManager");
                instance = gameObject.gameObject.AddComponent<TimerManager>();
            }
            return instance;
        }
    }
    #endregion


    private class TimerTask
    {
        public Timer Timer { get; private set; }

        private bool m_IsRunning;
        private bool m_IsAutoUnregister;
        private TimerManager m_TimerManager;

        public TimerTask(TimerManager timerManager, Timer timer, bool isAutoUnregister)
        {
            Timer = timer;
            m_IsAutoUnregister = isAutoUnregister;
            m_TimerManager = timerManager;
        }

        public async UniTaskVoid Run()
        {
            m_IsRunning = true;

            var clock = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            while (m_IsRunning)
            {
                await UniTask.Yield();
                if (!m_IsRunning)
                    break;

                if (Timer.Status != TimerStatus.Running)
                    continue;

                if (TimeUtil.NowUtc() >= Timer.EndTimeMs)
                {
                    Timer.Stop(true);
                    if (m_IsAutoUnregister)
                    {
                        m_TimerManager.Unregister(Timer);
                    }
                }
            }
        }

        public void Exit()
        {
            Timer.Stop(false);
            Timer = null;
            m_TimerManager = null;
            m_IsRunning = false;
        }
    }
    private readonly Dictionary<string/*guid*/, TimerTask> m_TimersTaskByGuid = new();


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public Timer GetTimerOrNull(string timerGuid)
    {
        return m_TimersTaskByGuid.TryGetValue(timerGuid, out var timerTask) ? timerTask.Timer : null;
    }

    public void Register(Timer timer, bool isAutoUnregister)
    {
        if (m_TimersTaskByGuid.ContainsKey(timer.Guid))
            return;

        var timerTask = new TimerTask(this, timer, isAutoUnregister);
        m_TimersTaskByGuid.Add(timer.Guid, timerTask);
        timerTask.Run().Forget();
    }

    public void Unregister(Timer timer)
    {
        if (!m_TimersTaskByGuid.TryGetValue(timer.Guid, out var timerStatus))
            return;

        timerStatus.Exit();
        m_TimersTaskByGuid.Remove(timer.Guid);
    }
}
