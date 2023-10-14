using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering.Fullscreen.ShaderGraph;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;


public class Material_Creator : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/Material_Creator")]
    public static void ShowExample()
    {
        Material_Creator wnd = GetWindow<Material_Creator>();
        wnd.titleContent = new GUIContent("Material_Creator");
    }

 
    
    
    
    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        // Get the button element from the UXML by name
        Button button = root.Q<Button>("GenerateMat");

        // Add an event handler to the button
        button.clicked += () =>
        {
            string parentFolder = "Assets/Scenes/ToolsDev"; // Change this to your desired parent folder path
            string newFolderName = "Materials"; // Change this to the desired folder name

            // Create the new folder within the parent folder
            string newFolderGUID = AssetDatabase.CreateFolder(parentFolder, newFolderName);
            string newFolderPath = AssetDatabase.GUIDToAssetPath(newFolderGUID);
            AssetDatabase.Refresh(); // Refresh the asset database

            // Check if the folder was created successfully
            if (!string.IsNullOrEmpty(newFolderPath))
            {
                Debug.Log("Folder created at: " + newFolderPath);

                // Show a warning dialog
                bool result = EditorUtility.DisplayDialog("Warning", "Materials Created!", "OK");
                if (result)
                {
                    Debug.Log("Added " + newFolderPath);
                }
            }
            else
            {
                Debug.LogError("Failed to create the folder.");
            }
            //ImageUtility.ImageProcessor();
            Dictionary<string, List<string>> AssetData = new Dictionary<string, List<string>>();
            AssetData=ImageUtility.ImageProcessor();
            
            ShaderGraphUtility.MaterialVariantProcessor(AssetData);

        };
    }
}

public class ImageUtility
{
public static Dictionary<string, List<string>>   ImageProcessor()
{

    string[] guids = AssetDatabase.FindAssets("t:texture2D", new[] { "Assets/Scenes/ToolsDev/Textures" });
    Dictionary<string, List<string>> AssetData = new Dictionary<string, List<string>>();

    foreach (string guid in guids)
    {
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        string texName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        string[] nameParts = texName.Split('_');

        if (nameParts.Length >= 2)
        {
            string objectName = nameParts[0];
            string mapName = nameParts[1];

            if (!AssetData.ContainsKey(objectName))
            {
                AssetData[objectName] = new List<string>();
            }

            if (!AssetData[objectName].Contains(mapName))
            {
                AssetData[objectName].Add(mapName);
            }
        }
    }

    foreach (var kvp in AssetData)
    {
        string objectName = kvp.Key;
        List<string> maps = kvp.Value;
        Debug.Log($"{objectName} has -> {string.Join(", ", maps)}");
    }
    return AssetData;
}

}
public class ShaderGraphUtility
{
    public static void MaterialVariantProcessor(Dictionary<string, List<string>> AssetData)
    {
        
        // Set the path to your Shader Graph asset
        string shaderGraphPath = "Assets/Scenes/ToolsDev/ShaderGraphs/Master_SG.shadergraph";
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderGraphPath);

        if (shader != null)
        {
            // Load or create a parent material (create it only once)
            Material parentMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Scenes/ToolsDev/Materials/ParentMaterial.mat");
            if (parentMaterial == null)
            {
                parentMaterial = new Material(shader);
                AssetDatabase.CreateAsset(parentMaterial, "Assets/Scenes/ToolsDev/Materials/ParentMaterial.mat");
                AssetDatabase.Refresh();
            }

            // Iterate through AssetData to create variants
            foreach (var kvp in AssetData)
            {
                string objectName = kvp.Key;
                List<string> maps = kvp.Value;

                // Create a new Material variant based on the parent
                Material materialVariant = new Material(parentMaterial);

                // Customize the variant properties (e.g., map settings)
                foreach (string map in maps)
                {
                    // Set properties based on map or objectName
                    // Example: materialVariant.SetFloat(map, 1.0f);
                }

                // Save the variant Material
                string variantPath = $"Assets/Scenes/ToolsDev/Materials/{objectName}_Variant.mat";
                AssetDatabase.CreateAsset(materialVariant, variantPath);
            }

            // Refresh the asset database
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError("Shader not found at " + shaderGraphPath);
        }
    }
}

