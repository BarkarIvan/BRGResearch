using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class AdditionalMapCombinerWindow : EditorWindow
{
    private ObjectField _normalMapField;
    private ObjectField _smoothnessMaskField;
    private ObjectField _metallicMaskField;
    private Button _combineButton;
    
    [MenuItem("Tools/AdditionalMapCombiner")]
    public static void ShowWindow()
    {
        GetWindow<AdditionalMapCombinerWindow>("Additional Map Combiner");
    }


    private void OnEnable()
    {
        var root = rootVisualElement;
         _normalMapField = new ObjectField("Normal Map") { objectType = typeof(Texture2D) };
         _smoothnessMaskField = new ObjectField("Smoothness Mask") { objectType = typeof(Texture2D) };
         _metallicMaskField = new ObjectField("Metallic Mask") { objectType = typeof(Texture2D) };

         _combineButton = new Button(CombineTextures) { text = "Combine to Additional Map" };
        
        root.Add(_normalMapField);
        root.Add(_smoothnessMaskField);
        root.Add(_metallicMaskField);
        root.Add(_combineButton);

    }
    
    private void MakeTextureReadable(Texture2D texture)
    {
        if (texture != null)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);

            if (importer != null)
            {
                importer.isReadable = true;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }

    private void CombineTextures()
    {
        var normalMap = _normalMapField.value as Texture2D;
        var smoothnessMask = _smoothnessMaskField.value as Texture2D;
        var metallicMask = _metallicMaskField.value as Texture2D;

        //TODO сделать Read/Write
        
        
        if (normalMap == null || smoothnessMask == null || metallicMask == null)
        {
            Debug.LogWarning("Пожалуйста, назначте все текстуры в поля");
            return;
        }

        if (!normalMap.isReadable)
        {
           MakeTextureReadable(normalMap);
        }

        if (!smoothnessMask.isReadable)
        {
            MakeTextureReadable(smoothnessMask);
        }

        if (!metallicMask.isReadable)
        {
            MakeTextureReadable(metallicMask);
        }

        Color[] normalMapData = normalMap.GetPixels();
        Color[] smoothnessData = smoothnessMask.GetPixels();
        Color[] metallicData = metallicMask.GetPixels();

        Color[] resultData = new Color[normalMapData.Length];

        for (int i = 0; i < normalMapData.Length; i++)
        {
            resultData[i] = new Color(
                normalMapData[i].r,
                normalMapData[i].g,
                smoothnessData[i].r,
                metallicData[i].r
            );
        }

        Texture2D resultTexture = new Texture2D(normalMap.width, normalMap.height);
        resultTexture.SetPixels(resultData);
        resultTexture.Apply();

        SaveTextureAsset(resultTexture);
    }

    private void SaveTextureAsset(Texture2D resultTexture)
    {
        string path = EditorUtility.SaveFilePanel(
            "Save Additional Map",
            "",
            "AdditionalMap.tga",
            "tga"
        );

        if (!string.IsNullOrEmpty(path))
        {
            byte[] textureBytes = resultTexture.EncodeToTGA();
            System.IO.File.WriteAllBytes(path, textureBytes);
            UnityEditor.AssetDatabase.Refresh();
        }
    }
}
