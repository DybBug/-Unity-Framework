using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        }

        public async UniTaskVoid Run()
        {
            m_IsRunning = true;

            var clock = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            while (m_IsRunning)
            {
                await UniTask.Yield();
                if (!m_IsRunning)
                {
                    await UniTask.Yield();
                    TimerManager.Instance.m_TimersTaskByGuid.Remove(Timer.Guid);
                    Timer = null;
                    break;
                }

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
        if (string.IsNullOrEmpty(timerGuid))
            return null;

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
        if (!m_TimersTaskByGuid.TryGetValue(timer.Guid, out var timerTask))
            return;

        timerTask.Exit();       
    }
}
