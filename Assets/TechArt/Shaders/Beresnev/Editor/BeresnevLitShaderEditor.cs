using UnityEngine;
using UnityEditor;

public class BeresnevLitShaderEditor : ShaderGUI
{
    private MaterialEditor materialEditor;
    private MaterialProperty[] properties;

    private MaterialProperty _BaseMap;
    private MaterialProperty _BaseColor;
    private MaterialProperty _AdditionalMap;
    private MaterialProperty _UsingNormalMap;
    private MaterialProperty _NormalMapScale;
    private MaterialProperty _Metallic;
    private MaterialProperty _Smoothness;
    private MaterialProperty _Brightness;
    private MaterialProperty _UseAlphaClip;
    private MaterialProperty _AlphaClip;
    private MaterialProperty _Cull;
    private MaterialProperty _Blend1;
    private MaterialProperty _Blend2;
    private MaterialProperty _ZWrite;

    // Enum declarations
    public enum CullEnum
    {
        Off = 0,
        Front = 1,
        Back = 2
    }

    public enum BlendModes
    {
        Opaque = 0,
        Transparent = 1,
        Fade = 2,
    }

    public enum BlendModeEnum
    {
        Zero = 0,
        One = 1,
        DstColor = 2,
        SrcColor = 3,
        OneMinusDstColor = 4,
        SrcAlpha = 5,
        OneMinusSrcColor = 6,
        DstAlpha = 7,
        OneMinusDstAlpha = 8,
        SrcAlphaSaturate = 9,
        OneMinusSrcAlpha = 10
    }

    public enum ZWriteEnum
    {
        On = 1,
        Off = 0
    }

    private BlendModes _blendMode;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        this.materialEditor = materialEditor;
        this.properties = properties;

        FindProperties();

        EditorGUI.BeginChangeCheck();
        {
            materialEditor.TextureProperty(_BaseMap, "Albedo");
            materialEditor.ColorProperty(_BaseColor, "Color");
            materialEditor.TextureProperty(_AdditionalMap, "Additional Map");
            if (_AdditionalMap.textureValue != null)
            {
                materialEditor.ShaderProperty(_UsingNormalMap, "Use Normal Map");
                materialEditor.RangeProperty(_NormalMapScale, "Normal Map Scale");
            }

            EditorGUILayout.Space();
            materialEditor.RangeProperty(_Metallic, "Metallic");
            materialEditor.RangeProperty(_Smoothness, "Smoothness");
            EditorGUILayout.Space();

            materialEditor.RangeProperty(_Brightness, "Brightness");
            EditorGUILayout.Space(20);
            materialEditor.ShaderProperty( _UseAlphaClip, "Use Alpha Clip" );
            if ( _UseAlphaClip.floatValue == 1 )
            {
                materialEditor.RangeProperty( _AlphaClip, "Alpha Clip Threshold" );
            }
            foreach (var obj in materialEditor.targets)
            {
                Material material = obj as Material;
                SetKeyword(material, "_USEALPHACLIP", _UseAlphaClip.floatValue==1);
            }
            
            _Cull.floatValue = (float) (CullEnum) EditorGUILayout.EnumPopup("Cull", (CullEnum) _Cull.floatValue);
            EditorGUILayout.Space();

            _blendMode = ( BlendModes )EditorGUILayout.EnumPopup( "Blend Mode", _blendMode );
           // 

            switch ( _blendMode )
            {
               
                case BlendModes.Opaque: 
                    _Blend1.floatValue = ( int )BlendModeEnum.One;
                    _Blend2.floatValue = (int ) BlendModeEnum.Zero;
                    _ZWrite.floatValue = ( int )ZWriteEnum.On;

                    break;
                case BlendModes.Transparent:
                    _Blend1.floatValue = (int)BlendModeEnum.SrcAlpha;
                    _Blend2.floatValue = (int)BlendModeEnum.OneMinusSrcAlpha;
                    _ZWrite.floatValue = ( int )ZWriteEnum.Off;

                    break;
                case BlendModes.Fade:
                    _Blend1.floatValue = (int)BlendModeEnum.SrcAlpha;
                    _Blend2.floatValue = (int)BlendModeEnum.OneMinusSrcAlpha;
                    _ZWrite.floatValue = ( int )ZWriteEnum.On;

                    break;
            }
            
            materialEditor.RenderQueueField();

            foreach (var obj in materialEditor.targets)
            {
                Material material = obj as Material;
                SetKeyword(material, "_NORMALMAP", _UsingNormalMap.floatValue == 1 && _AdditionalMap.textureValue != null);
                SetKeyword(material, "_ADDITIONALMAP", _AdditionalMap.textureValue != null);
            }
           
        }
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var obj in materialEditor.targets)
            {
                Material material = obj as Material;
                SetKeyword(material, "_NORMALMAP", _UsingNormalMap.floatValue == 1 && _AdditionalMap.textureValue != null);
                SetKeyword(material, "_ADDITIONALMAP", _AdditionalMap.textureValue != null);
            }
        }
    }
    
    private BlendModes GetBlendModeFromMaterialProperties()
    {
       
        if (_Blend1.floatValue == (int)BlendModeEnum.SrcAlpha && _Blend2.floatValue == (int)BlendModeEnum.OneMinusSrcAlpha)
        {
            if (_ZWrite.floatValue == (int)ZWriteEnum.On)
                return BlendModes.Fade;
            else
                return BlendModes.Transparent;
        }
        return BlendModes.Opaque;
    }
    

    private void SetKeyword(Material material, string keyword, bool enabled)
    {
        if (enabled)
        {
            material.EnableKeyword(keyword);
        }
        else
        {
            material.DisableKeyword(keyword);
        }
    }

    private void FindProperties()
    {
        _BaseMap = FindProperty("_BaseMap");
        _BaseColor = FindProperty("_BaseColor");
        _AdditionalMap = FindProperty("_AdditionalMap");
        _UsingNormalMap = FindProperty("_UsingNormalMap");
        _NormalMapScale = FindProperty("_NormalMapScale");
        _UseAlphaClip = FindProperty( "_UseAlphaClip" );
        _AlphaClip = FindProperty( "_AlphaClip" );
        _Metallic = FindProperty("_Metallic");
        _Smoothness = FindProperty("_Smoothness");
        _Brightness = FindProperty("_Brightness");
        _Cull = FindProperty("_Cull");
        _Blend1 = FindProperty("_Blend1");
        _Blend2 = FindProperty("_Blend2");
        _ZWrite = FindProperty("_ZWrite");
        _blendMode = _blendMode = GetBlendModeFromMaterialProperties();
    }

    private MaterialProperty FindProperty(string propertyName)
    {
        return FindProperty(propertyName, properties);
    }
}