using UnityEngine;
using System.Collections;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;

public class ThumbnailGenerator : EditorWindow
{
    // ✅ FIX: Added 'readonly' to satisfy the IDE warning
    private readonly string modelPath = "Assets/Resources/Models";
    private readonly string savePath = "Assets/Resources/Thumbnails";

    [MenuItem("Tools/Generate Thumbnails")]
    public static void ShowWindow()
    {
        GetWindow<ThumbnailGenerator>("Thumbnail Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Bulk Thumbnail Generator", EditorStyles.boldLabel);
        
        GUILayout.Space(10);
        GUILayout.Label($"Source: {modelPath}");
        GUILayout.Label($"Output: {savePath}");
        GUILayout.Space(10);

        if (GUILayout.Button("Force Generate All"))
        {
            ProcessAllModels();
        }
    }

    void ProcessAllModels()
    {
        // 1. Ensure output folder exists
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        // 2. Get ALL files in the directory directly
        DirectoryInfo dir = new(modelPath); 
        FileInfo[] files = dir.GetFiles("*.*"); 

        int processedCount = 0;

        foreach (FileInfo file in files)
        {
            // Filter for Model extensions only
            string ext = file.Extension.ToLower();
            if (ext is ".glb" or ".gltf" or ".fbx" or ".obj") 
            {
                // Convert file path to Unity Asset Path
                string relativePath = $"{modelPath}/{file.Name}";
                GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);

                if (modelPrefab != null)
                {
                    // Naming Logic: M_Basilica -> T_Basilica
                    string modelName = modelPrefab.name;
                    string thumbName = modelName.Replace("M_", "T_");
                    
                    if (!thumbName.StartsWith("T_")) thumbName = "T_" + modelName;

                    Debug.Log($"📸 Capturing: {modelName} -> {thumbName}");
                    CaptureThumbnail(modelPrefab, thumbName);
                    processedCount++;
                }
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Complete", $"Generated {processedCount} thumbnails.", "OK");
    }

    void CaptureThumbnail(GameObject prefab, string fileName)
    {
        // 1. Setup a temporary scene stage
        GameObject stage = new("ThumbnailStage");
        stage.transform.position = new Vector3(1000, 1000, 1000); 
        
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, stage.transform);
        instance.transform.localPosition = Vector3.zero;

        // Auto-Scale Logic
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        Bounds bounds = new(instance.transform.position, Vector3.zero);
        foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);
        
        // 2. Setup Camera
        GameObject camObj = new("RenderCam");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0); 
        
        // Position camera
        float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float distance = maxSize * 2.0f; 
        if (distance < 2f) distance = 2f; 

        cam.transform.position = bounds.center + new Vector3(0, distance * 0.5f, -distance);
        cam.transform.LookAt(bounds.center);

        // 3. Setup Lighting
        GameObject lightObj = new("Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.5f;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);

        // 4. Capture
        RenderTexture rt = new(512, 512, 24);
        cam.targetTexture = rt;
        cam.Render();

        // 5. Save
        RenderTexture.active = rt;
        Texture2D tex = new(512, 512, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        string fullPath = $"{savePath}/{fileName}.png";
        File.WriteAllBytes(fullPath, bytes);

        // 6. Cleanup
        RenderTexture.active = null;
        cam.targetTexture = null;
        DestroyImmediate(stage);
        DestroyImmediate(camObj);
        DestroyImmediate(lightObj);
        DestroyImmediate(rt);
    }
}
#endif