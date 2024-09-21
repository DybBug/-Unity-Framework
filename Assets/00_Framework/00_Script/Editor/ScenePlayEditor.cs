using UnityEditor;
using UnityEditor.SceneManagement;

public class ScenePlayEditor
{
    [MenuItem("CustomEditor/PlayScene/0번쨰 씬 플레이 _F5")] // 단축키 F5
   public static void PlayFromFirstScene()
    {
        // Build Settings에 설정된 0번 씬의 경로
        string firstScenePath = EditorBuildSettings.scenes[0].path;

        // 프로젝트에 포함된 에셋(AssetDatabase)에서 SceneAsset을 로드함.
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(firstScenePath);

        // 시작씬 설정
        EditorSceneManager.playModeStartScene = sceneAsset;
        EditorApplication.isPlaying = true;
    }

    [MenuItem("CustomEditor/PlayScene/현재 씬 플레이 %F5")] // 단축키 Ctrl + F5
    public static void PlayFromCurrentScene()
    {
        // 시작씬 설정
        EditorSceneManager.playModeStartScene = null;
        EditorApplication.isPlaying = true;
    }


}
