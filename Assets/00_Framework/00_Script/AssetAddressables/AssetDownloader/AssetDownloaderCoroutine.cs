using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;



public class AssetDownloaderCoroutine : AssetDownloader
{
    private readonly List<AsyncOperationHandle> m_DownloadHandles = new();
    protected override IReadOnlyList<AsyncOperationHandle> GetDownloadOperationHandles() => m_DownloadHandles;

    public override void StartInitialize()
    {
        base.StartInitialize();
        StartCoroutine(StartInitializeCoroutine());
    }

    public override void StartDownload()
    {
        base.StartDownload();
        StartCoroutine(DownloadCoroutine());
    }


    private IEnumerator StartInitializeCoroutine()
    {        
        yield return InitializeCoroutine();
        yield return CheckAndUpdateCatalogCoroutine();
        yield return DownloadSizeCoroutine();
    }

    private IEnumerator InitializeCoroutine()
    {
        CurrentState = State.Initializing;
        var handle = Addressables.InitializeAsync(false);
        if (!handle.IsValid())
        {
            Addressables.Release(handle);
            throw handle.OperationException;
        }
        yield return handle;

        if (!handle.IsDone || handle.Status == AsyncOperationStatus.Failed)
        {
            Addressables.Release(handle);
            throw handle.OperationException;
        }
        CurrentState = State.WaitingForCatalogCheck;
        Addressables.Release(handle);
    }

    private IEnumerator CheckAndUpdateCatalogCoroutine()
    {
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        CurrentState = State.CatalogChecking;
        if (!checkHandle.IsValid())
        {
            Addressables.Release(checkHandle);
            throw checkHandle.OperationException;
        }
        yield return checkHandle;
        if (!checkHandle.IsDone || checkHandle.Status == AsyncOperationStatus.Failed)
        {
            Addressables.Release(checkHandle);
            throw checkHandle.OperationException;
        }
        CurrentState = State.WaitingForCatalogUpdate;

        m_Catalogs = checkHandle.Result;
        if (m_Catalogs.Count <= 0)
        {
            Addressables.Release(checkHandle);
            CurrentState = State.WaitingForSizeDownload;
            CatalogUpdatedEvent?.Invoke();
            yield break;
        }
        Addressables.Release(checkHandle);

        var updateHandle = Addressables.UpdateCatalogs(m_Catalogs);
        CurrentState = State.CatalogUpdating;
        if (!updateHandle.IsValid())
        {
            Addressables.Release(updateHandle);
            throw updateHandle.OperationException;
        }

        yield return updateHandle;
        if (!updateHandle.IsDone || updateHandle.Status == AsyncOperationStatus.Failed)
        {
            Addressables.Release(updateHandle);
            throw updateHandle.OperationException;
        }

        Addressables.Release(updateHandle);
        CurrentState = State.WaitingForSizeDownload;
        CatalogUpdatedEvent?.Invoke();
    }

    private IEnumerator DownloadSizeCoroutine()
    {
        var tasksByHandle = new Dictionary<AsyncOperationHandle<long>, Task>();

        CurrentState = State.SizeDownloading;
        foreach (var assetLabelReference in m_AddressableLabels.AssetLabelReferences)
        {
            var handle = Addressables.GetDownloadSizeAsync(assetLabelReference.labelString);
            tasksByHandle.Add(handle, handle.Task);
            if (!handle.IsValid())
            {
                foreach (var handleInDic in tasksByHandle)
                {
                    Addressables.Release(handleInDic);
                }
                tasksByHandle.Clear();
                throw handle.OperationException;
            }
        }

        // 모든 코루틴이 완료될 때까지 대기
        var totalSize = 0L;
        foreach (var handle in tasksByHandle.Keys)
        {
            yield return handle;
            totalSize += handle.Result;
            Addressables.Release(handle);
        }

        CurrentState = State.WaitingForDownload;
        SizeDownloadedEvent?.Invoke(totalSize);
    }

    private IEnumerator DownloadCoroutine()
    {
        CurrentState = State.Downloading;
        var tasksByHandle = new Dictionary<AsyncOperationHandle, Task>();
        foreach (var assetLabelReference in m_AddressableLabels.AssetLabelReferences)
        {
            var handle = Addressables.DownloadDependenciesAsync(assetLabelReference.labelString, false);
            m_DownloadHandles.Add(handle);
            tasksByHandle.Add(handle, handle.Task);

            if (!handle.IsValid())
            {
                for (var i = 0; i < tasksByHandle.Count; ++i)
                {
                    Addressables.Release(m_DownloadHandles[i]);
                }
                tasksByHandle.Clear();
                m_DownloadHandles.Clear();
                throw handle.OperationException;
            }     
        }

        // 모든 코루틴이 완료될 때까지 대기
        foreach (var handle in tasksByHandle.Keys)
        {
            yield return handle;
            m_DownloadHandles.Remove(handle);
            Addressables.Release(handle);
        }

        CurrentState = State.FinishDownload;
        DownloadFinishedEvent?.Invoke();
    }
}
