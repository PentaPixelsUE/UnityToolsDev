using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;





public class Material_Creator : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/Material Creator Tool")]
    public static void ShowExample()
    {
        Material_Creator wnd = GetWindow<Material_Creator>();
        wnd.titleContent = new GUIContent("Material Creator Tool");
    }

    public void CreateGUI()
    {   
        // Instantiate UXML
        VisualElement root = rootVisualElement;
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
        
        // Get the button elements from the UXML by name
        Button Generate = root.Q<Button>("GenerateMat");
        Button Assign = root.Q<Button>("AssignMat");
       
        // Add an event handler to the "Generate" button
        Generate.clicked += () =>
        {
            
            string parentFolder = "Assets/Resources"; // Change this to your desired parent folder path
            string newFolderName = "Materials"; // Change this to the desired folder name
            string newFolderPath = parentFolder + "/" + newFolderName;
                        

            // Check if the "Materials" folder already exists
            if (!AssetDatabase.IsValidFolder(newFolderPath))
            {
                // If the folder doesn't exist, create it
                string newFolderGUID = AssetDatabase.CreateFolder(parentFolder, newFolderName);
                newFolderPath = AssetDatabase.GUIDToAssetPath(newFolderGUID);
                AssetDatabase.Refresh(); // Refresh the asset database
                
                // Check if the folder was created successfully
                 Dictionary<string, List<string>> AssetData = new Dictionary<string, List<string>>();
                AssetData = ImageUtility.ImageProcessor();
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
            }
            else
            {
                Debug.Log("Materials folder already exists at: " + newFolderPath);
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
                        int texturecounter = 0;
                        string searchPattern = $"{objectName}_{map}.*";

                        // Search for textures in subdirectories
                        string[] texturePaths = Directory.GetFiles("Assets/Scenes/ToolsDev/Textures", searchPattern, SearchOption.AllDirectories);

                        if (texturecounter < texturePaths.Length)
                        {
                            string texturePath = texturePaths[texturecounter];

                            // Try loading the texture
                            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

                            if (texture != null)
                            {
                                // Set the texture as a shader property
                                materialVariant.SetTexture("_" + map, texture);
                                Debug.Log($"{map} texture loaded for {objectName}");
                            }
                            else
                            {
                                Debug.LogError($"Texture not found for {map} of {objectName}");
                            }
                        }

                        ++texturecounter;
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
        Material[] materials = Resources.LoadAll<Material>("Materials");
        var prefab = Resources.Load<GameObject>("Meshes/village house kit");

        if (prefab != null)
        {
            foreach (Material mat in materials)
            {
                Transform[] children = prefab.GetComponentsInChildren<Transform>();

                foreach (Transform child in children)
                {
                    string[] parts = mat.name.Split('_');
                    string materialName = parts[0];

                    if (child.name == materialName)
                    {
                        Debug.Log($"Child {child.name}_{materialName}");
                    }

                    // Create an array of materials for the child
                    Material[] childMaterials = child.GetComponentInChildren<MeshRenderer>().sharedMaterials;

                    bool assigned = false;

                    for (int i = 0; i < childMaterials.Length; i++)
                    {
                        if (i == 0 && childMaterials[i].name == materialName)
                        {
                            childMaterials[i] = mat;
                            assigned = true;
                            Debug.Log($"Assigned material {mat.name} to element {i} of {child.name}");
                        }
                        if (i == 1 && childMaterials[i].name == materialName)
                        {
                            childMaterials[i] = mat;
                            assigned = true;
                            Debug.Log($"Assigned material {mat.name} to element {i} of {child.name}");
                        }
                    }

                    if (assigned)
                    {
                        child.GetComponentInChildren<MeshRenderer>().sharedMaterials = childMaterials;
                    }
                }
            }
        }
        else
        {
            Debug.Log("ERROR: Prefab not found");
        }
    }
}

