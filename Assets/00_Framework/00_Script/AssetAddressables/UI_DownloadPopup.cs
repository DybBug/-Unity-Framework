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
            m_TitleText.text = "�˸�";
            SetText("�ٿ�ε� ������ �������� �ֽ��ϴ�. ��ø� ��ٷ��ּ���.");
        }
        else if (m_CurrState == PopupState.WaitingForDownload)
        {
            m_TitleText.text = "����";
            SetText($"�ٿ�ε带 �����ðڽ��ϱ� ? �����Ͱ� ���� ���� �� �ֽ��ϴ�. <color=green>({$"{GetFormattedSize(m_DownloadStatus.TotalBytes)})</color>"}");
        }
        else if (m_CurrState == PopupState.Downloading)
        {
            m_TitleText.text = "�ٿ�ε���";

            double rate = m_DownloadStatus.TotalBytes == 0 ? 0 : (double)m_DownloadStatus.DownloadedBytes / (double)m_DownloadStatus.TotalBytes;
            ClearText();
            SetText($"�ٿ�ε����Դϴ�. ��ø� ��ٷ��ּ���. {(rate * 100).ToString("0.00")}% �Ϸ�");

            m_DownloadingProgress.value = (float)rate;
            m_DownloadingCapacity.text = $"{GetFormattedSize(m_DownloadStatus.DownloadedBytes)}/{GetFormattedSize(m_DownloadStatus.TotalBytes)}";
        }
        else if (m_CurrState == PopupState.WaitingForGamePlay)
        {
            m_TitleText.text = "�Ϸ�";
            SetText("������ �����Ͻðڽ��ϱ�?");
        }
    }

    public void OnClickStartDownload()
    {
        m_Downloader.StartDownload();
        ChangeState(PopupState.Downloading);
    }

    /// <summary> ��� ��ư Ŭ���� ȣ�� </summary>
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

    /// <summary> �ΰ��� ���� ��ư Ŭ���� ȣ�� </summary>
    public void OnClickEnterGame()
    {
        Debug.Log("Start Game!");

        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    private void OnCatalogUpdateNotFound()
    {
        SetText("������Ʈ ������ īŻ�α׸� ã�� �� �����ϴ�.");
    }

    /// <summary> īŻ�α� ������Ʈ �Ϸ�� ȣ�� </summary>
    private void OnCatalogUpdated()
    {
        ChangeState(PopupState.CalculateDownloadSize);
    }

    /// <summary> ������ �ٿ�ε� �Ϸ�� ȣ�� </summary>
    private void OnSizeDownloaded(long size)
    {
        SetText($"�ٿ�ε� ������ : {size} ����Ʈ");

        if (size <= 0)
        {
            ChangeState(PopupState.WaitingForGamePlay);
        }
        else
        {
            ChangeState(PopupState.WaitingForDownload);
        }
    }


    /// <summary> �ٿ�ε� �������� ȣ�� </summary>
    private void OnDownloadFinished()
    {
        SetText("�ٿ�ε� �Ϸ�!");
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
