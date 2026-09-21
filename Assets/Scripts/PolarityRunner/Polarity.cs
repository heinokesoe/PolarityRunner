using UnityEngine;
using PolarityRunner.Utilities;

namespace PolarityRunner
{
    public enum GameplayPolarity
    {
        Red,
        Blue,
        Default
    }

    public static class PolarityRules
    {
        public static readonly Color RedColor = new Color(0.95f, 0.08f, 0.12f, 1f);
        public static readonly Color BlueColor = new Color(0.08f, 0.42f, 1f, 1f);

        public static bool Matches(GameplayPolarity first, GameplayPolarity second)
        {
            return first == GameplayPolarity.Default ||
                   second == GameplayPolarity.Default ||
                   first == second;
        }

        public static GameplayPolarity Opposite(GameplayPolarity polarity)
        {
            if (polarity == GameplayPolarity.Red)
            {
                return GameplayPolarity.Blue;
            }

            if (polarity == GameplayPolarity.Blue)
            {
                return GameplayPolarity.Red;
            }

            return GameplayPolarity.Default;
        }

        public static GameplayPolarity ForHazardPosition(Vector3 position)
        {
            float distance = Mathf.Max(0f, position.x);
            float stageTwoStart = 60f.ToWorldUnits();
            float stageThreeStart = 120f.ToWorldUnits();
            float colorSectionWidth = distance < stageTwoStart ? 10.24f : distance < stageThreeStart ? 5.12f : 2.56f;
            int section = Mathf.FloorToInt(distance / colorSectionWidth);
            switch (section % 3)
            {
                case 0:
                    return GameplayPolarity.Red;
                case 1:
                    return GameplayPolarity.Blue;
                default:
                    return GameplayPolarity.Default;
            }
        }

        public static Color GetColor(GameplayPolarity polarity)
        {
            if (polarity == GameplayPolarity.Red)
            {
                return RedColor;
            }

            return polarity == GameplayPolarity.Blue ? BlueColor : Color.white;
        }
    }

    public static class PolarityVisuals
    {
        private static readonly int PolarityColorId = Shader.PropertyToID("_PolarityColor");
        private static readonly int TintNeutralId = Shader.PropertyToID("_TintNeutral");
        private static Material s_RedMaterial;
        private static Material s_BlueMaterial;
        private static Material s_RedHazardMaterial;
        private static Material s_BlueHazardMaterial;

        public static void Apply(GameObject root, GameplayPolarity polarity, bool tintNeutral = false)
        {
            // Default hazards retain their authored sprite colors and remain active
            // against either player polarity through PolarityRules.Matches.
            if (polarity == GameplayPolarity.Default)
            {
                return;
            }

            var renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
            Material material = GetMaterial(polarity, tintNeutral);
            Color color = PolarityRules.GetColor(polarity);

            foreach (var spriteRenderer in renderers)
            {
                if (material != null)
                {
                    spriteRenderer.sharedMaterial = material;
                    spriteRenderer.color = Color.white;
                }
                else
                {
                    spriteRenderer.color = color;
                }
            }
        }

        private static Material GetMaterial(GameplayPolarity polarity, bool tintNeutral)
        {
            Material cachedMaterial = polarity == GameplayPolarity.Red
                ? (tintNeutral ? s_RedHazardMaterial : s_RedMaterial)
                : (tintNeutral ? s_BlueHazardMaterial : s_BlueMaterial);

            if (cachedMaterial != null)
            {
                return cachedMaterial;
            }

            Shader shader = Resources.Load<Shader>("PolarityTint");
            if (shader == null)
            {
                return null;
            }

            var material = new Material(shader)
            {
                name = $"Polarity {polarity}",
                hideFlags = HideFlags.HideAndDontSave
            };
            material.SetColor(PolarityColorId, PolarityRules.GetColor(polarity));
            material.SetFloat(TintNeutralId, tintNeutral ? 1f : 0f);

            if (polarity == GameplayPolarity.Red)
            {
                if (tintNeutral)
                {
                    s_RedHazardMaterial = material;
                }
                else
                {
                    s_RedMaterial = material;
                }
            }
            else
            {
                if (tintNeutral)
                {
                    s_BlueHazardMaterial = material;
                }
                else
                {
                    s_BlueMaterial = material;
                }
            }

            return material;
        }
    }
}
