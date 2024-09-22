using System;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CustomTile : Tile
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/2D/Custom Tiles/CustomTile")]
    public static void CreateCustomTile()
    {
        var path = EditorUtility.SaveFilePanelInProject("Save Custom Tile", "New Custom Tile", "Asset", "Save Custom Tile", "Assets");
        if (path == "")
            return;

        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CustomTile>(), path);
    }

#endif

    [Serializable]
    public enum TileType
    {
        Ground,
        Path,
        Block,
    }

    [SerializeField]
    private TileType m_Type;
    public TileType Type 
    { 
        get { return m_Type; }
        set { m_Type = value; }
    }
    public uint Key { get; private set; }
}


