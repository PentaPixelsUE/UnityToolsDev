using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Rendering;
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
        Button Generate = root.Q<Button>("GenerateMat");
        Button Assign = root.Q<Button>("AssignMat");
        // Add an event handler to the button

        Generate.clicked += () =>
        {
            string parentFolder = "Assets/Resources"; // Change this to your desired parent folder path
            string newFolderName = "Materials"; // Change this to the desired folder name

            // Create the new folder within the parent folder
            string newFolderGUID = AssetDatabase.CreateFolder(parentFolder, newFolderName);
            string newFolderPath = AssetDatabase.GUIDToAssetPath(newFolderGUID);
            AssetDatabase.Refresh(); // Refresh the asset database

            // Check if the folder was created successfully
            
    
            Dictionary<string, List<string>> AssetData = new Dictionary<string, List<string>>();
            AssetData=ImageUtility.ImageProcessor();
            
            ShaderGraphUtility.MaterialVariantProcessor(AssetData);
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
            
            };
            Assign.clicked += () =>
            {
            MaterialAssignUtility.AssignMaterials();
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
            
           // Iterate through AssetData to create variants
            foreach (var kvp in AssetData)
            {
                string objectName = kvp.Key;
                List<string> maps = kvp.Value;
                
                // Create a new Material variant based on the parent
                Material materialVariant = new Material(shader);
                
                
                // Customize the variant properties (e.g., map settings)
                foreach (string map in maps)
                {
                    // Load the texture based on the map name (assuming textures are in the same folder)
                    string texturePath = $"Assets/Scenes/ToolsDev/Textures/{objectName}/{objectName}_{map}";

                    // Try loading a PNG texture
                    
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{texturePath}.jpg");
                    Debug.LogWarning($"{texturePath} loaded");
                   
                    if (texture == null)
                    {
                        // If PNG texture is not found, try loading a JPG texture
                        texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{texturePath}.png");
                        
                    }
                   

                    if (texture != null)
                    {
                        // Set the texture as a shader property
                        materialVariant.SetTexture("_"+map, texture);
                    
                    }

                    else
                    {
                        Debug.LogError($"Texture not found for {map} of {objectName}");
                    }
                   
                }

                string variantPath = $"Assets/Resources/Materials/{objectName}_Variant.mat";
                AssetDatabase.CreateAsset(materialVariant, variantPath);
                

            // Refresh the asset database
            AssetDatabase.Refresh();
    
            }
        }
        
    
        else
        {
            Debug.LogError("Shader not found at " + shaderGraphPath);
        }
    
    }
}






public class MaterialAssignUtility
{
     public static void AssignMaterials()
     {
        
        // Load the prefab from the Resources folder
        //string[] guids = AssetDatabase.FindAssets("t:material", new[] { "Assets/Scenes/ToolsDev/Materials" });
        Material[]materials = Resources.LoadAll<Material>("Materials");

        foreach(var mat in materials){
        

        var prefab = Resources.Load<GameObject>("Meshes/village house kit");
        
        if (prefab != null)
        {
            // Access all children of the prefab
            Transform[] children = prefab.GetComponentsInChildren<Transform>();

            foreach (Transform child in children)
            {
                
                // Do something with each child
                MeshRenderer[] meshRenderers = child.GetComponentsInChildren<MeshRenderer>();
                int elements= meshRenderers.Length;

                Debug.LogWarning($"{mat} has {elements}componenets");
                //child.gameObject.GetComponent<MeshRenderer>()=
                string[] parts=mat.name.Split('_');
                string materialname= parts[0];
                if(child.name == materialname)
                {
                child.GetComponent<MeshRenderer>().material = mat;
                
                        // Log the child name and material name once
                        Debug.Log($"Child name:   {child.name} {materialname} ");
                }
            }
        }
    
        else
        {
            Debug.Log("ERROR: Prefab not found");
        }
    }
    }
    
}

