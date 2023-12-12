using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace BeresnevGames.Graphics.Lighting
{
    [CreateAssetMenu(fileName = "NewLightingSettings", menuName = "ScriptableObjects/Create new Lighting Settings")]
    public class LightingSettingsConfig : ScriptableObject
    {
        [Header("Lighting")]
        public Color AmbientSkyColor;
        public Color AmbientEquatorColor;
        public Color AmbientGroundColor;

        [NonSerialized]
        public readonly AmbientMode AmbientMode = AmbientMode.Trilight; //захардкоден Gradient Mode
        public Cubemap ReflectionCubeMap;

        [Space(10)]
        [Header("Shadow")]
        public Color RealtimeShadowColor = new Color(0.09f, 0.09f, 0.31f);

        [Space(10)]
        [Header("Fog")]
        public bool FogEnabled;
        public Color FogColor;
        public float FogStartDistance;
        public float FogEndDistance;

        [NonSerialized]
        public readonly FogMode FogMode = FogMode.Linear; //захардкоден linear maode
        //public float FogDensity; // на будущее
    }
}