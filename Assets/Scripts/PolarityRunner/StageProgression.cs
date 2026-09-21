using System;

namespace PolarityRunner
{
    public static class StageProgression
    {
        public const int StageCount = 3;

        public static int GetStage(float distance, float[] stageEndDistances)
        {
            if (stageEndDistances == null || stageEndDistances.Length == 0)
            {
                return 1;
            }

            for (int index = 0; index < stageEndDistances.Length; index++)
            {
                if (distance < stageEndDistances[index])
                {
                    return index + 1;
                }
            }

            return stageEndDistances.Length;
        }

        public static float GetStageProgress(float distance, int stage, float[] stageEndDistances)
        {
            if (stageEndDistances == null || stageEndDistances.Length == 0)
            {
                return 0f;
            }

            int safeStage = Math.Max(1, Math.Min(stage, stageEndDistances.Length));
            float start = safeStage == 1 ? 0f : stageEndDistances[safeStage - 2];
            float end = stageEndDistances[safeStage - 1];
            return end <= start ? 1f : UnityEngine.Mathf.Clamp01((distance - start) / (end - start));
        }
    }
}
