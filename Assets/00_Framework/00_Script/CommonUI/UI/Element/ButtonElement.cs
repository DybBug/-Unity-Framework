using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonElement : UiElement
{
    [SerializeField] private Vector2 m_BounceScale = new Vector2(1.5f, 1.5f);
    [SerializeField] private float m_BounceTimeSec = 1.0f;
    [SerializeField] private AnimationCurve m_ScaleCurve;
    public UnityAction ClickEvent { get; set; }

    private Coroutine m_BounceCoroutine;

    protected override void OnInitialize()
    {
        throw new System.NotImplementedException();
    }

    private void Awake()
    {
        var button = GetComponent<Button>();
        button?.onClick.AddListener(_OnClicked_Button);
    }

    private void _OnClicked_Button()
    {
        if(m_BounceCoroutine == null)
        {
            m_BounceCoroutine = StartCoroutine("_Bounce");
        }
    }
    private IEnumerator _Bounce()
    {
        float accTime = 0f;

        Vector2 startScale = transform.localScale;
        Vector2 finishScale = m_BounceScale;

        while (true)
        {
            accTime += Time.deltaTime;
            transform.localScale = Vector2.Lerp(startScale, finishScale, m_ScaleCurve.Evaluate(accTime / m_BounceTimeSec));

            if(accTime >= m_BounceTimeSec)
            {
                m_BounceCoroutine = null;
                ClickEvent?.Invoke();
                yield break; // 코루틴 종료
            }
            yield return null;
        }
    }

    protected override void OnTick(float dt)
    {
        throw new System.NotImplementedException();
    }
}
