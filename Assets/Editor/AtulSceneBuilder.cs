// Builds the starting scene for Abominable Atul from the imported room layout.
// Runs once automatically the first time the project is opened, and can be re-run
// from the menu: Abominable Atul > Build Layout Scene.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

[InitializeOnLoad]
public static class AtulSceneBuilder
{
    const string ModelPath = "Assets/Models/Layout/area_v2.fbx";
    const string ScenePath = "Assets/Scenes/Layout.unity";
    const string AutoBuiltKey = "AbominableAtul.LayoutSceneAutoBuilt";

    static AtulSceneBuilder()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorPrefs.GetBool(AutoBuiltKey + "." + Application.dataPath, false)) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null) return;
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath) == null) return; // still importing
            EditorPrefs.SetBool(AutoBuiltKey + "." + Application.dataPath, true);
            Build();
        };
    }

    [MenuItem("Abominable Atul/Build Layout Scene")]
    public static void Build()
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        if (model == null)
        {
            Debug.LogError($"[Abominable Atul] Layout model not found at {ModelPath}.");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // --- Room ---
        var room = (GameObject)PrefabUtility.InstantiatePrefab(model);
        room.name = "Layout";
        room.isStatic = true;
        foreach (var t in room.GetComponentsInChildren<Transform>(true))
            t.gameObject.isStatic = true;

        // Solid surfaces get colliders so the player can't walk through walls.
        // Glass, mirrors, lights and the fan are left without colliders on purpose.
        string[] noCollider = { "glass", "mirror", "light", "lamp", "chandelier", "fan" };
        foreach (var mf in room.GetComponentsInChildren<MeshFilter>(true))
        {
            string n = mf.gameObject.name.ToLowerInvariant();
            if (noCollider.Any(n.Contains)) continue;
            if (mf.GetComponent<Collider>() == null)
                mf.gameObject.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh;
        }

        // The fan should spin later, so it shouldn't be static.
        foreach (var t in room.GetComponentsInChildren<Transform>(true))
            if (t.name.ToLowerInvariant().Contains("fan") && !t.name.ToLowerInvariant().Contains("room") && !t.name.ToLowerInvariant().Contains("area"))
                t.gameObject.isStatic = false;

        Bounds bounds = ComputeBounds(room);

        // --- Horror lighting: dark ambient, a weak cold moonlight, fog ---
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.04f, 0.045f, 0.06f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.02f, 0.02f, 0.03f);
        RenderSettings.fogDensity = 0.04f;
        RenderSettings.skybox = null;

        var moon = new GameObject("Moonlight");
        var moonLight = moon.AddComponent<Light>();
        moonLight.type = LightType.Directional;
        moonLight.color = new Color(0.55f, 0.62f, 0.8f);
        moonLight.intensity = 0.15f;
        moonLight.shadows = LightShadows.Soft;
        moon.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // --- Player spawn on the floor near the middle of the room ---
        Vector3 spawn = FindFloor(bounds);
        var spawnGo = new GameObject("PlayerSpawn");
        spawnGo.transform.position = spawn;

        // Placeholder camera at head height. Replace with an XR Origin once the
        // XR packages are installed (see README).
        var camGo = new GameObject("Main Camera (placeholder for XR Origin)");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.nearClipPlane = 0.05f;
        camGo.AddComponent<AudioListener>();
        camGo.transform.SetParent(spawnGo.transform, false);
        camGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);

        // A dim torch-like light on the player so the room isn't pitch black.
        var torch = new GameObject("Player Light");
        var torchLight = torch.AddComponent<Light>();
        torchLight.type = LightType.Point;
        torchLight.range = 6f;
        torchLight.intensity = 0.8f;
        torchLight.color = new Color(1f, 0.85f, 0.65f);
        torch.transform.SetParent(camGo.transform, false);

        // Empty groups to fill in as the game takes shape.
        foreach (var group in new[] { "Clues", "Ghosts", "Interactables", "Audio" })
            new GameObject(group);

        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ScenePath));
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.Refresh();

        var scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s => s.path == ScenePath))
        {
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        Selection.activeGameObject = room;
        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.Frame(bounds, false);

        Debug.Log($"[Abominable Atul] Built {ScenePath}. Room size: {bounds.size.x:0.0} x {bounds.size.y:0.0} x {bounds.size.z:0.0} m, spawn at {spawn}.");
    }

    static Bounds ComputeBounds(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return new Bounds(root.transform.position, Vector3.one);
        Bounds b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }

    static Vector3 FindFloor(Bounds b)
    {
        Physics.SyncTransforms();
        // Start just under the lowest ceiling-ish height and look down for the floor.
        var origin = new Vector3(b.center.x, b.min.y + Mathf.Min(2.5f, b.size.y * 0.5f), b.center.z);
        if (Physics.Raycast(origin, Vector3.down, out var hit, b.size.y + 1f))
            return hit.point;
        return new Vector3(b.center.x, b.min.y, b.center.z);
    }
}
