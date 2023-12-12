using Sirenix.OdinInspector;
using UnityEngine;

namespace BeresnevGames.Graphics.Lighting
{
    public class LevelLightingSettings : MonoBehaviour
    {
        [OnValueChanged("TryApplyLightingSettings", true)]
        [InlineEditor((InlineEditorObjectFieldModes.Boxed))]
        [SerializeField]
        private LightingSettingsConfig lightingSettingsConfig;

        private void Start()
        {
            TryApplyLightingSettings();
        }

        private void OnValidate()
        {
            TryApplyLightingSettings();
        }

        private void TryApplyLightingSettings()
        {
            if (lightingSettingsConfig == null) return;
            RenderSettings.ambientMode = lightingSettingsConfig.AmbientMode;
            RenderSettings.ambientSkyColor = lightingSettingsConfig.AmbientSkyColor;
            RenderSettings.ambientEquatorColor = lightingSettingsConfig.AmbientEquatorColor;
            RenderSettings.ambientGroundColor = lightingSettingsConfig.AmbientGroundColor;
            RenderSettings.customReflectionTexture = lightingSettingsConfig.ReflectionCubeMap;
            RenderSettings.fog = lightingSettingsConfig.FogEnabled;
            RenderSettings.fogMode = lightingSettingsConfig.FogMode;
            RenderSettings.fogColor = lightingSettingsConfig.FogColor;
            RenderSettings.fogStartDistance = lightingSettingsConfig.FogStartDistance;
            RenderSettings.fogEndDistance = lightingSettingsConfig.FogEndDistance;
            RenderSettings.subtractiveShadowColor = lightingSettingsConfig.RealtimeShadowColor;
        }
    }
}