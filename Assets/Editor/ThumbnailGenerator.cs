using UnityEngine;
using UnityEditor;
using System.IO;

public class ThumbnailGenerator : EditorWindow
{
    [Header("Settings")]
    public string savePath = "Assets/Resources/Thumbnails";
    
    // Drag your 3D Model Prefabs here
    public GameObject[] prefabsToRender;

    [MenuItem("Tools/Generate Thumbnails")]
    public static void ShowWindow()
    {
        GetWindow<ThumbnailGenerator>("Thumbnail Gen");
    }

    void OnGUI()
    {
        GUILayout.Label("Batch Thumbnail Generator", EditorStyles.boldLabel);
        
        // Draw the list of prefabs
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty stringsProperty = so.FindProperty("prefabsToRender");

        EditorGUILayout.PropertyField(stringsProperty, true);
        so.ApplyModifiedProperties();

        GUILayout.Space(20);

        if (GUILayout.Button("GENERATE PNGs", GUILayout.Height(40)))
        {
            GenerateAll();
        }
    }

    void GenerateAll()
    {
        // Ensure directory exists
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        int count = 0;
        foreach (GameObject prefab in prefabsToRender)
        {
            if (prefab == null) continue;
            
            // Get the internal Unity Preview (what you see in the bottom right inspector)
            Texture2D preview = null;
            
            // Loop to force Unity to load the preview if it's not cached yet
            int retries = 0;
            while (preview == null && retries < 100)
            {
                preview = AssetPreview.GetAssetPreview(prefab);
                if (preview == null) System.Threading.Thread.Sleep(20); // Tiny wait
                retries++;
            }

            if (preview != null)
            {
                SaveTextureAsPNG(preview, prefab.name);
                count++;
            }
            else
            {
                Debug.LogError($"❌ Failed to get preview for {prefab.name}. Try clicking on the prefab once to load it into cache.");
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log($"✅ Generated {count} thumbnails in {savePath}!");
    }

    void SaveTextureAsPNG(Texture2D texture, string filename)
    {
        byte[] bytes = texture.EncodeToPNG();
        string fullPath = $"{savePath}/{filename}.png";
        File.WriteAllBytes(fullPath, bytes);
    }
}