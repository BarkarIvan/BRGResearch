Shader "Cosmocompost/UI_Map"
{
    Properties
    {
       [PerRendererData] _MainTex("MainTex", 2D) = "white"{}
        _SoilColor("SoilColor", Color) = (1,1,1,1)
        _PlantColor("PlantColor", Color) = (1,1,1,1)
        _GroundColor("GroundColor", Color) = (1,1,1,1)
        _ContourColor("ContourColor", Color) = (1,1,1,1)
       
        _Color ("Tint", Color) = (1,1,1,1)
        _BorderWidth ("BorderWidth", float) = 1
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color: COLOR;
                half2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                half2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
                float4 worldPosition : TEXCOORD1;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID

            };

            sampler2D _MainTex;
            half4 _Color;
            half4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            half4 _SoilColor;
            half4 _PlantColor;
            half4 _GroundColor;
            half4 _ContourColor;
            half _BorderWidth;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = IN.positionOS;
                OUT.positionHCS = UnityObjectToClipPos(OUT.worldPosition);

                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

                OUT.color = IN.color * _Color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half edgeDistX = min(IN.uv.x, 1.0 - IN.uv.x);
                half edgeDistY = min(IN.uv.y, 1.0 - IN.uv.y);
                half square_edge = min(edgeDistX, edgeDistY);

                square_edge = step(_BorderWidth * 0.01, square_edge);
                
                half4 result;
                half4 col = tex2D(_MainTex, IN.uv) * IN.color;
                result = lerp(1.0, _GroundColor, 1.0 - col.g);
                result = lerp(result, _SoilColor,  col.b);
                result = lerp(result, _PlantColor, col.r);
                result = lerp(result, _ContourColor,  1.0 - square_edge);

                #ifdef UNITY_UI_CLIP_RECT
                result.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (result.a - 0.001);
                #endif
                return result;
            }
            ENDHLSL
        }
    }
}