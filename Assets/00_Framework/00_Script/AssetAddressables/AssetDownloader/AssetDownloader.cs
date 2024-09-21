using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class AssetDownloader : MonoBehaviour
{
    public enum State
    {
        WaitingForInitialize,
        Initializing,

        WaitingForCatalogCheck,
        CatalogChecking,

        WaitingForCatalogUpdate,
        CatalogUpdating,

        WaitingForSizeDownload,
        SizeDownloading,

        WaitingForDownload,
        Downloading,

        FinishDownload
    }

    #region Events
    public Action CatalogUpdatedEvent;        // 카탈로그가 업데이트 되었을때 발생
    public Action<long/*totalSize*/> SizeDownloadedEvent; // 다운로드 용량을 가져왔을떄 발생
    public Action DownloadFinishedEvent;
    public Action<string> ExceptionEvent;
    #endregion

    [SerializeField]
    protected AddressableLabels m_AddressableLabels;

    public State CurrentState { get; protected set; } = State.WaitingForInitialize;

    protected DownloadStatus m_DownloadStatus;
    protected List<string> m_Catalogs;
    protected abstract IReadOnlyList<AsyncOperationHandle> GetDownloadOperationHandles();


    public virtual void StartInitialize()
    {
        Debug.Assert(CurrentState == State.WaitingForInitialize);
    }

    public virtual void StartDownload()
    {
        Debug.Assert(CurrentState == State.WaitingForDownload);
        m_DownloadStatus.TotalBytes = 0;
        m_DownloadStatus.DownloadedBytes = 0;
        m_DownloadStatus.IsDone = false;
    }
    public DownloadStatus GetDownloadingStatus()
    {
        var isDone = true;
        foreach (var downloadHandle in GetDownloadOperationHandles())
        {
            if (!downloadHandle.IsValid() || downloadHandle.IsDone || downloadHandle.Status == AsyncOperationStatus.Failed)
                continue;

            var status = downloadHandle.GetDownloadStatus();

            isDone &= status.IsDone;
            m_DownloadStatus.TotalBytes += status.TotalBytes;
            m_DownloadStatus.DownloadedBytes += status.DownloadedBytes;
        }
        m_DownloadStatus.IsDone = isDone;

        return m_DownloadStatus;
    }


}
