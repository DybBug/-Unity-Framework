using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class AssetLoader
{
    #region Load Prefab
    private static readonly Dictionary<string, AsyncOperationHandle<GameObject>> m_CachedPrefabHandleMap = new();
    public static GameObject LoadPrefab(string key)
    {
        if (m_CachedPrefabHandleMap.TryGetValue(key, out var handle))
        {
            return handle.Result;
        }

        handle = Addressables.LoadAssetAsync<GameObject>(key);
        handle.WaitForCompletion();
        m_CachedPrefabHandleMap.Add(key, handle);
        return handle.Result;
    }

    public static void ReleasePrefab(string key)
    {
        if (m_CachedPrefabHandleMap.TryGetValue(key, out var handle))
        {
            m_CachedPrefabHandleMap.Remove(key);
            Addressables.Release(handle);
        }
    }
    #endregion

    #region Load ScriptableObject
    private static readonly Dictionary<string, AsyncOperationHandle<ScriptableObject>> m_CachedScriptableObjectHandleMap = new();
    public static ScriptableObject LoadScriptableObject(string key)
    {
        if (m_CachedScriptableObjectHandleMap.TryGetValue(key, out var handle))
        {
            return handle.Result;
        }

        handle = Addressables.LoadAssetAsync<ScriptableObject>(key);
        handle.WaitForCompletion();
        m_CachedScriptableObjectHandleMap.Add(key, handle);
        return handle.Result;
    }

    public static void ReleaseScriptableObject(string key)
    {
        if (m_CachedPrefabHandleMap.TryGetValue(key, out var handle))
        {
            m_CachedPrefabHandleMap.Remove(key);
            Addressables.Release(handle);
        }
    }
    #endregion

    #region Load Material
    private static readonly Dictionary<string, AsyncOperationHandle<Material>> m_CachedMaterialHandleMap = new();
    public static Material LoadMaterial(string key)
    {
        if (m_CachedMaterialHandleMap.TryGetValue(key, out var handle))
        {
            return handle.Result;
        }

        handle = Addressables.LoadAssetAsync<Material>(key);
        handle.WaitForCompletion();
        m_CachedMaterialHandleMap.Add(key, handle);
        return handle.Result;
    }

    public static void ReleaseMaterial(string key)
    {
        if (m_CachedMaterialHandleMap.TryGetValue(key, out var handle))
        {
            m_CachedMaterialHandleMap.Remove(key);
            Addressables.Release(handle);
        }
    }
    #endregion

    #region Load Atlas
    private static readonly Dictionary<string, AsyncOperationHandle<SpriteAtlas>> m_CachedAtlasHandleMap = new();
    public static SpriteAtlas LoadAtlas(string key)
    {
        if (m_CachedAtlasHandleMap.TryGetValue(key, out var handle))
        {
            return handle.Result;
        }

        handle = Addressables.LoadAssetAsync<SpriteAtlas>(key);
        handle.WaitForCompletion();
        m_CachedAtlasHandleMap.Add(key, handle);
        return handle.Result;
    }

    public static void ReleaseAtlas(string key)
    {
        if (m_CachedAtlasHandleMap.TryGetValue(key, out var handle))
        {
            m_CachedAtlasHandleMap.Remove(key);
            Addressables.Release(handle);
        }
    }
    #endregion

    #region Load Sprite
    private static readonly Dictionary<string, AsyncOperationHandle<Sprite>> m_CachedSpriteHandleMap = new();
    public static Sprite LoadSprite(string key)
    {
        if (m_CachedSpriteHandleMap.TryGetValue(key, out var handle))
        {
            return handle.Result;
        }

        handle = Addressables.LoadAssetAsync<Sprite>(key);
        handle.WaitForCompletion();
        m_CachedSpriteHandleMap.Add(key, handle);
        return handle.Result;
    }

    public static void ReleaseSprite(string key)
    {
        if (m_CachedSpriteHandleMap.TryGetValue(key, out var handle))
        {
            m_CachedSpriteHandleMap.Remove(key);
            Addressables.Release(handle);
        }
    }
    #endregion

    #region Load Scene
    private static readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> m_CachedSceneHandleMap = new();
    public static SceneInstance LoadScene(string key, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool isActiveOnLoad = true, int priority = 100)
    {
        if (m_CachedSceneHandleMap.TryGetValue(key, out var handle))
        { 
            return handle.Result;
        }

        handle = Addressables.LoadSceneAsync(key, loadSceneMode, isActiveOnLoad, 100);
        handle.WaitForCompletion();
        if (loadSceneMode == LoadSceneMode.Additive)
        {
            m_CachedSceneHandleMap.Add(key, handle);
        }

        return handle.Result;
    }

    public static void UnloadScene(string key)
    {
        if (m_CachedSceneHandleMap.TryGetValue(key, out var handle))
        {
            m_CachedSceneHandleMap.Remove(key);
            Addressables.UnloadSceneAsync(handle, true).WaitForCompletion();
            
        }
    }
    #endregion
}
