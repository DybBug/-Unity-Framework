using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class UI_DownloadPopup : MonoBehaviour
{
    public enum PopupState
    {
        None = -1,
        CalculateDownloadSize,
        WaitingForDownload,
        WaitingForGamePlay,
        Downloading,
    }

    [Serializable]
    public class SubGameObject
    {
        public PopupState State;
        public GameObject GameObject;
    }

    [SerializeField] private AssetDownloader m_Downloader;

    [Header("-- Header --")]
    [SerializeField] private TMP_Text m_TitleText;

    [Header("-- Body --")]
    [SerializeField] private TMP_Text m_DescText;

    [Header("-- Footer --")]
    [SerializeField] private List<SubGameObject> m_FooterObjects;
    [SerializeField] private TMP_Text m_DownloadingCapacity;
    [SerializeField] private Slider m_DownloadingProgress;
    [SerializeField] private Button m_DownloadButton;
    [SerializeField] private Button m_CancelButton;
    [SerializeField] private Button m_GamePlayButton;

    private PopupState m_CurrState;
    private DownloadStatus m_DownloadStatus;

    private void Awake()
    {
        m_Downloader.CatalogUpdatedEvent += OnCatalogUpdated;
        m_Downloader.SizeDownloadedEvent += OnSizeDownloaded;
        m_Downloader.DownloadFinishedEvent += OnDownloadFinished;
        m_Downloader.ExceptionEvent += OnException;

        m_DownloadButton.onClick.AddListener(OnClickStartDownload);
        m_CancelButton.onClick.AddListener(OnClickCancelBtn);
        m_GamePlayButton.onClick.AddListener(OnClickEnterGame);

    }

    void Start()
    {
        ChangeState(PopupState.None);
        m_Downloader.StartInitialize();
    }


    private void ClearText()
    {
        m_DescText.text = "";
    }

    private void SetText(string newText)
    {
        var text = m_DescText.text;
        text += $"\n{newText}";
        m_DescText.text = text;
    }




    // Update is called once per frame
    void Update()
    {
        if (m_Downloader.CurrentState == AssetDownloader.State.Downloading)
        {
            m_DownloadStatus = m_Downloader.GetDownloadingStatus();
            RefreshUI();
        }
    }

    private void ChangeState(PopupState newState)
    { 
        var prevGameObject = m_FooterObjects.Find(e => e.State == m_CurrState);
        if (prevGameObject != null)
        {
            prevGameObject.GameObject.SetActive(false);
        }

        var currGameObject = m_FooterObjects.Find(e => e.State == newState);
        if (currGameObject != null)
        {
            currGameObject.GameObject.SetActive(true);
        }

        m_CurrState = newState;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (m_CurrState == PopupState.CalculateDownloadSize)
        {
            m_TitleText.text = "알림";
            SetText("다운로드 정보를 가져오고 있습니다. 잠시만 기다려주세요.");
        }
        else if (m_CurrState == PopupState.WaitingForDownload)
        {
            m_TitleText.text = "주의";
            SetText($"다운로드를 받으시겠습니까 ? 데이터가 많이 사용될 수 있습니다. <color=green>({$"{GetFormattedSize(m_DownloadStatus.TotalBytes)})</color>"}");
        }
        else if (m_CurrState == PopupState.Downloading)
        {
            m_TitleText.text = "다운로드중";

            double rate = m_DownloadStatus.TotalBytes == 0 ? 0 : (double)m_DownloadStatus.DownloadedBytes / (double)m_DownloadStatus.TotalBytes;
            ClearText();
            SetText($"다운로드중입니다. 잠시만 기다려주세요. {(rate * 100).ToString("0.00")}% 완료");

            m_DownloadingProgress.value = (float)rate;
            m_DownloadingCapacity.text = $"{GetFormattedSize(m_DownloadStatus.DownloadedBytes)}/{GetFormattedSize(m_DownloadStatus.TotalBytes)}";
        }
        else if (m_CurrState == PopupState.WaitingForGamePlay)
        {
            m_TitleText.text = "완료";
            SetText("게임을 진행하시겠습니까?");
        }
    }

    public void OnClickStartDownload()
    {
        m_Downloader.StartDownload();
        ChangeState(PopupState.Downloading);
    }

    /// <summary> 취소 버튼 클릭시 호출 </summary>
    public void OnClickCancelBtn()
    {
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
#else
            Application.Quit();
#endif
    }

    /// <summary> 인게임 진입 버튼 클릭시 호출 </summary>
    public void OnClickEnterGame()
    {
        Debug.Log("Start Game!");

        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    private void OnCatalogUpdateNotFound()
    {
        SetText("업데이트 가능한 카탈로그를 찾을 수 없습니다.");
    }

    /// <summary> 카탈로그 업데이트 완료시 호출 </summary>
    private void OnCatalogUpdated()
    {
        ChangeState(PopupState.CalculateDownloadSize);
    }

    /// <summary> 사이즈 다운로드 완료시 호출 </summary>
    private void OnSizeDownloaded(long size)
    {
        SetText($"다운로드 사이즈 : {size} 바이트");

        if (size <= 0)
        {
            ChangeState(PopupState.WaitingForGamePlay);
        }
        else
        {
            ChangeState(PopupState.WaitingForDownload);
        }
    }


    /// <summary> 다운로드 마무리시 호출 </summary>
    private void OnDownloadFinished()
    {
        SetText("다운로드 완료!");
        ChangeState(PopupState.WaitingForGamePlay);
    }

    private void OnException(string message)
    {
        SetText(message);
    }

    private enum SizeUnits
    {
        Byte = 0,
        KB = 1,
        MB = 2,
        GB = 3
    }

    private SizeUnits GetByteUnit(long byteSize)
    {
        return (byteSize) switch
        {
            >= (1L << 30) => SizeUnits.GB, // 1 GB = 1024^3 bytes
            >= (1L << 20) => SizeUnits.MB, // 1 MB = 1024^2 bytes
            >= (1L << 10) => SizeUnits.KB, // 1 KB = 1024 bytes
            _ => SizeUnits.Byte,
        };
    }

    private double ConvertByteByUnit(long byteSize, SizeUnits unit)
    {
        return byteSize / Math.Pow(1024, (int)unit);
    }

    public string GetFormattedSize(long byteSize)
    {
        SizeUnits unit = GetByteUnit(byteSize);
        double size = ConvertByteByUnit(byteSize, unit);
        return $"{size:F2} {unit}";
    }
}
