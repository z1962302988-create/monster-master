using UnityEngine;
using UnityEngine.UI;

namespace MonsterMaster.UI
{
    /// <summary>
    /// Creates a private material for the ward background so its animated lighting
    /// can be tuned at runtime without mutating a shared project material.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    public sealed class WardRealtimeLighting : MonoBehaviour
    {
        [SerializeField] private Shader lightingShader;
        [Header("Palette")]
        [SerializeField] private Color nightColor = new Color(0.16f, 0.31f, 0.42f, 1f);
        [SerializeField] private Color moonColor = new Color(0.54f, 0.82f, 1f, 1f);
        [SerializeField] private Color warmColor = new Color(1f, 0.66f, 0.28f, 1f);
        [Header("Intensity")]
        [Range(0f, 1f)] [SerializeField] private float nightAmount = 0.48f;
        [Range(0f, 2f)] [SerializeField] private float moonIntensity = 0.44f;
        [Range(0f, 2f)] [SerializeField] private float warmIntensity = 0.24f;
        [Range(0f, 1f)] [SerializeField] private float shadowStrength = 0.32f;
        [Range(0f, 2f)] [SerializeField] private float motionSpeed = 0.07f;

        private static readonly int NightColorId = Shader.PropertyToID("_NightColor");
        private static readonly int MoonColorId = Shader.PropertyToID("_MoonColor");
        private static readonly int WarmColorId = Shader.PropertyToID("_WarmColor");
        private static readonly int NightAmountId = Shader.PropertyToID("_NightAmount");
        private static readonly int MoonIntensityId = Shader.PropertyToID("_MoonIntensity");
        private static readonly int WarmIntensityId = Shader.PropertyToID("_WarmIntensity");
        private static readonly int ShadowStrengthId = Shader.PropertyToID("_ShadowStrength");
        private static readonly int MotionSpeedId = Shader.PropertyToID("_MotionSpeed");

        private Graphic targetGraphic;
        private Material runtimeMaterial;

        private void Awake()
        {
            targetGraphic = GetComponent<Graphic>();
            ApplySettings();
        }

        private void OnEnable()
        {
            if (targetGraphic != null)
                ApplySettings();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && runtimeMaterial != null)
                UpdateMaterialProperties();
        }

        private void OnDestroy()
        {
            if (runtimeMaterial == null)
                return;

            if (targetGraphic != null && targetGraphic.material == runtimeMaterial)
                targetGraphic.material = null;

            Destroy(runtimeMaterial);
        }

        private void ApplySettings()
        {
            if (lightingShader == null)
            {
                Debug.LogError("Ward realtime lighting shader is missing.", this);
                enabled = false;
                return;
            }

            if (runtimeMaterial == null)
            {
                runtimeMaterial = new Material(lightingShader)
                {
                    name = "Ward Moonlight (Runtime)",
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            UpdateMaterialProperties();
            targetGraphic.material = runtimeMaterial;
            targetGraphic.SetMaterialDirty();
        }

        private void UpdateMaterialProperties()
        {
            runtimeMaterial.SetColor(NightColorId, nightColor);
            runtimeMaterial.SetColor(MoonColorId, moonColor);
            runtimeMaterial.SetColor(WarmColorId, warmColor);
            runtimeMaterial.SetFloat(NightAmountId, nightAmount);
            runtimeMaterial.SetFloat(MoonIntensityId, moonIntensity);
            runtimeMaterial.SetFloat(WarmIntensityId, warmIntensity);
            runtimeMaterial.SetFloat(ShadowStrengthId, shadowStrength);
            runtimeMaterial.SetFloat(MotionSpeedId, motionSpeed);
        }
    }
}
