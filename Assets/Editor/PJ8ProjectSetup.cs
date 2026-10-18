#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Creates the starter scene the first time the project is opened.</summary>
[InitializeOnLoad]
public static class PJ8ProjectSetup
{
    private const string ScenePath = "Assets/Scenes/PJ8Viewer.unity";

    static PJ8ProjectSetup()
    {
        EditorApplication.delayCall += EnsureStarterScene;
    }

    [MenuItem("PJ8/Create or repair starter scene")]
    private static void EnsureStarterScene()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null) return;

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0, 4.4f, -8.5f);
        cameraObject.transform.rotation = Quaternion.Euler(25f, 0f, 0f);
        cameraObject.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        cameraObject.GetComponent<Camera>().backgroundColor = new Color(0.22f, 0.42f, 0.57f);

        var lightObject = new GameObject("Directional Light", typeof(Light));
        var light = lightObject.GetComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightObject.transform.rotation = Quaternion.Euler(48f, -30f, 0f);

        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Preview Floor";
        floor.transform.position = new Vector3(0, -0.56f, 0);
        floor.transform.localScale = new Vector3(8, 1, 8);
        floor.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Standard")) { color = new Color(0.28f, 0.36f, 0.34f) };

        new GameObject("PJ8 Viewer", typeof(PJ8Viewer));
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("PJ8 starter scene created. Open Assets/Scenes/PJ8Viewer.unity and press Play.");
    }
}
#endif
