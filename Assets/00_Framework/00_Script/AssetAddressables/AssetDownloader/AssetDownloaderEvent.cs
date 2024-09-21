using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;


public class AssetDownloaderEvent : AssetDownloader
{
    private readonly Dictionary<AsyncOperationHandle<long>, long> m_DownloadSizesByHandle = new(); // default value = -1
    private readonly Dictionary<AsyncOperationHandle, bool> m_DownloadResultByHandle = new(); // default value = false

    public override void StartInitialize()
    {
        base.StartInitialize();
        ExecuteByWaitingState();
    }

    public override void StartDownload()
    {
        base.StartDownload();

        ExecuteByWaitingState();
    }

    protected override IReadOnlyList<AsyncOperationHandle> GetDownloadOperationHandles()
    {
        return m_DownloadResultByHandle.Keys.ToList();
    }

    private void ExecuteByWaitingState()
    {
        try
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                throw new Exception("network is NotReachable");
            }

            switch (CurrentState)
            {
                case State.WaitingForInitialize:
                {
                    Initialize();
                    break;
                }
                case State.WaitingForCatalogCheck:
                {
                    CheckCatalog();
                    break;
                }
                case State.WaitingForCatalogUpdate:
                {
                    UpdateCatalog();
                    break;
                }
                case State.WaitingForSizeDownload:
                {
                    DownloadSize();
                    break;
                }
                case State.WaitingForDownload:
                {
                    Download();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            ExceptionEvent?.Invoke($"{ex.Message}\n[Call Stack]\n{ex.StackTrace}");
        }
    }

    private void Initialize()
    {
        CurrentState = State.Initializing;
        Addressables.InitializeAsync().Completed += OnInitialized;          
    }

    private void OnInitialized(AsyncOperationHandle<IResourceLocator> handle)
    {
        CurrentState = State.WaitingForCatalogCheck;
        ExecuteByWaitingState();
    }

    private void CheckCatalog()
    {
        CurrentState = State.CatalogChecking;
        Addressables.CheckForCatalogUpdates().Completed += OnCheckedCatalogWithNotify;
    }

    private void OnCheckedCatalogWithNotify(AsyncOperationHandle<List<string>> handle)
    {
        var catalogs = handle.Result;
        if (catalogs.Count > 0)
        {
            m_Catalogs = catalogs;
            CurrentState = State.WaitingForCatalogUpdate;
        }
        else
        {
            CatalogUpdatedEvent?.Invoke();
            CurrentState = State.WaitingForSizeDownload;
        }
        ExecuteByWaitingState();
    }

    private void UpdateCatalog()
    {
        CurrentState = State.CatalogUpdating;
        Addressables.UpdateCatalogs(m_Catalogs).Completed += OnUpdatedCatalogWithNotify;
    }

    private void OnUpdatedCatalogWithNotify(AsyncOperationHandle<List<IResourceLocator>> handle)
    {
        CatalogUpdatedEvent?.Invoke();
        CurrentState = State.WaitingForSizeDownload;
        m_Catalogs.Clear();
    }

    private void DownloadSize()
    {
        CurrentState = State.SizeDownloading;
        foreach (var assetLabelReference in m_AddressableLabels.AssetLabelReferences)
        {
            var handle = Addressables.GetDownloadSizeAsync(assetLabelReference.labelString);
            handle.Completed += OnDownloadedSizeWithNotify;
            m_DownloadSizesByHandle.Add(handle, -1);
        }
    }

    private void OnDownloadedSizeWithNotify(AsyncOperationHandle<long> handle)
    {
        m_DownloadSizesByHandle[handle] = handle.Result;
        if (m_DownloadSizesByHandle.All(e => e.Value != -1))
        {
            var totalSize = 0L;
            foreach(var pair in m_DownloadSizesByHandle)
            {
                totalSize += pair.Value;
            }
            m_DownloadSizesByHandle.Clear();

            SizeDownloadedEvent?.Invoke(totalSize);
            CurrentState = State.WaitingForDownload;
        }
    }

    private void Download()
    {
        CurrentState = State.Downloading;
        foreach (var assetLabelReference in m_AddressableLabels.AssetLabelReferences)
        {
            var handle = Addressables.DownloadDependenciesAsync(assetLabelReference.labelString);
            handle.Completed += CompletedDownloadWithNotify;
            m_DownloadResultByHandle.Add(handle, false);
        }
    }

    private void CompletedDownloadWithNotify(AsyncOperationHandle handle)
    {
        m_DownloadResultByHandle[handle] = true;
        if (m_DownloadResultByHandle.All(e => e.Value == true))
        {
            m_DownloadResultByHandle.Clear();

            DownloadFinishedEvent?.Invoke();
            CurrentState = State.FinishDownload;
        }
    }
}
