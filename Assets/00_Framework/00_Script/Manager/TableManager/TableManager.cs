using System.Collections.Generic;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    [SerializeField]
    private List<Table> m_Tables;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
