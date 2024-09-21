using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;



public class AssetDownloaderAsync : AssetDownloader
{
    private readonly List<AsyncOperationHandle> m_DownloadHandles = new();
    protected override IReadOnlyList<AsyncOperationHandle> GetDownloadOperationHandles() => m_DownloadHandles;

    public override void StartInitialize()
    {
        base.StartInitialize();
        StartInitializeAsync();

    }

    public override void StartDownload()
    {
        base.StartDownload();
        StartDownloadAsync();
    }

    private async void StartInitializeAsync()
    {
        try
        {
            await InitializeAsync();
            await CheckAndUpdateCatalogAsync();
            await DownloadSizeAsync();
        }
        catch (Exception ex)
        {
            ExceptionEvent?.Invoke($"{ex.Message}\n[Call Stack]\n{ex.StackTrace}");
        }
    }

    public async void StartDownloadAsync()
    {
        Debug.Assert(CurrentState == State.WaitingForDownload);
        m_DownloadStatus.TotalBytes = 0;
        m_DownloadStatus.DownloadedBytes = 0;
        m_DownloadStatus.IsDone = false;

        try
        {
            await DownloadAsync();
        }
        catch (Exception ex)
        {
            ExceptionEvent?.Invoke($"{ex.Message}\n[Call Stack]\n{ex.StackTrace}");
        }
    }

    private async Task InitializeAsync()
    {
        var handle = Addressables.InitializeAsync(false);
        CurrentState = State.Initializing;
        if (!handle.IsValid())
        {
            Addressables.Release(handle);
            throw handle.OperationException;
        }
        await handle.Task;
        if (!handle.IsDone || handle.Status == AsyncOperationStatus.Failed)
        {
            Addressables.Release(handle);
            throw handle.OperationException;
        }
        CurrentState = State.WaitingForCatalogCheck;
        Addressables.Release(handle);
    }

    private async Task CheckAndUpdateCatalogAsync()
    {
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        CurrentState = State.CatalogChecking;
        if (!checkHandle.IsValid())
        {
            Addressables.Release(checkHandle);
            throw checkHandle.OperationException;
        }
        await checkHandle.Task;
        if (!checkHandle.IsDone || checkHandle.Status == AsyncOperationStatus.Failed)
        {
            Addressables.Release(checkHandle);
            throw checkHandle.OperationException;
        }
        CurrentState = State.WaitingForCatalogUpdate;

        var catalogs = checkHandle.Result;
        if (catalogs.Count <= 0)
        {
            Addressables.Release(checkHandle);
            CurrentState = State.WaitingForSizeDownload;
            CatalogUpdatedEvent?.Invoke();
            return;
        }
        Addressables.Release(checkHandle);

        var updateHandle = Addressables.UpdateCatalogs(catalogs);
        CurrentState = State.CatalogUpdating;
        if (!updateHandle.IsValid())
        {
            Addressables.Release(updateHandle);
            throw updateHandle.OperationException;
        }

        await updateHandle.Task;
        if (!updateHandle.IsDone || updateHandle.Status == AsyncOperationStatus.Failed)
        {
            Addressables.Release(updateHandle);
            throw updateHandle.OperationException;
        }

        Addressables.Release(updateHandle);
        CurrentState = State.WaitingForSizeDownload;
        CatalogUpdatedEvent?.Invoke();
    }

    private async Task DownloadSizeAsync()
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

        await Task.WhenAll(tasksByHandle.Values);

        var totalSize = 0L;
        foreach (var handle in tasksByHandle.Keys)
        {
            totalSize += handle.Result;
            Addressables.Release(handle);
        }
        m_DownloadHandles.Clear();

        CurrentState = State.WaitingForDownload;
        SizeDownloadedEvent?.Invoke(totalSize);
    }

    private async Task DownloadAsync()
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

        await Task.WhenAll(tasksByHandle.Values);

        foreach (var handle in tasksByHandle.Keys)
        {
            Addressables.Release(handle);
        }

        m_DownloadHandles.Clear();

        CurrentState = State.FinishDownload;
        DownloadFinishedEvent?.Invoke();
    }
}
