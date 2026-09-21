using NUnit.Framework;
using PolarityRunner.Utilities;
using UnityEngine;

namespace PolarityRunner.EditorTests
{
    public sealed class PolarityRunnerTests
    {
        private readonly float[] m_StageEnds = { 600f, 1200f, 1800f };

        [Test]
        public void MatchingPolaritiesActivateHazards()
        {
            Assert.That(PolarityRules.Matches(GameplayPolarity.Red, GameplayPolarity.Red), Is.True);
            Assert.That(PolarityRules.Matches(GameplayPolarity.Blue, GameplayPolarity.Blue), Is.True);
            Assert.That(PolarityRules.Matches(GameplayPolarity.Red, GameplayPolarity.Blue), Is.False);
            Assert.That(PolarityRules.Matches(GameplayPolarity.Default, GameplayPolarity.Red), Is.True);
            Assert.That(PolarityRules.Matches(GameplayPolarity.Default, GameplayPolarity.Blue), Is.True);
        }

        [Test]
        public void OppositeAlwaysChangesPolarity()
        {
            Assert.That(PolarityRules.Opposite(GameplayPolarity.Red), Is.EqualTo(GameplayPolarity.Blue));
            Assert.That(PolarityRules.Opposite(GameplayPolarity.Blue), Is.EqualTo(GameplayPolarity.Red));
        }

        [TestCase(0f, 1)]
        [TestCase(599.99f, 1)]
        [TestCase(600f, 2)]
        [TestCase(1199.99f, 2)]
        [TestCase(1200f, 3)]
        [TestCase(1800f, 3)]
        public void DistanceSelectsOneOfThreeStages(float distance, int expectedStage)
        {
            Assert.That(StageProgression.GetStage(distance, m_StageEnds), Is.EqualTo(expectedStage));
        }

        [Test]
        public void HazardColorsAlternateMoreFrequentlyInLaterStages()
        {
            Assert.That(PolarityRules.ForHazardPosition(Vector3.zero), Is.EqualTo(GameplayPolarity.Red));
            Assert.That(PolarityRules.ForHazardPosition(new Vector3(10.24f, 0f)), Is.EqualTo(GameplayPolarity.Blue));
            Assert.That(PolarityRules.ForHazardPosition(new Vector3(20.48f, 0f)), Is.EqualTo(GameplayPolarity.Default));
            Assert.That(PolarityRules.ForHazardPosition(new Vector3(1200f, 0f)), Is.EqualTo(GameplayPolarity.Red));
            Assert.That(PolarityRules.ForHazardPosition(new Vector3(1202.56f, 0f)), Is.EqualTo(GameplayPolarity.Blue));
            Assert.That(PolarityRules.ForHazardPosition(new Vector3(1205.12f, 0f)), Is.EqualTo(GameplayPolarity.Default));
        }

        [Test]
        public void DisplayMetersConvertToWorldDistance()
        {
            Assert.That(180f.ToWorldUnits(), Is.EqualTo(1800f));
            Assert.That(1800f.ToLength(), Is.EqualTo("180 m"));
        }

        [Test]
        public void PolarityShaderKeepsSpriteTintVisible()
        {
            Shader shader = Resources.Load<Shader>("PolarityTint");
            Assert.That(shader, Is.Not.Null);
            var material = new Material(shader);
            Assert.That(material.HasProperty("_TintNeutral"), Is.True);
            Color tint = material.GetColor("_Color");
            Assert.That(tint.r, Is.EqualTo(1f).Within(0.001f));
            Assert.That(tint.g, Is.EqualTo(1f).Within(0.001f));
            Assert.That(tint.b, Is.EqualTo(1f).Within(0.001f));
            Assert.That(tint.a, Is.EqualTo(1f).Within(0.001f));
            Object.DestroyImmediate(material);
        }

        [Test]
        public void TerrainCleanupOnlyRemovesBlocksFullyBehindThePlayer()
        {
            Assert.That(TerrainGeneration.TerrainGenerator.IsOutsideCleanupRange(250f, 100f, 100f), Is.True);
            Assert.That(TerrainGeneration.TerrainGenerator.IsOutsideCleanupRange(150f, 100f, 100f), Is.False);
            Assert.That(TerrainGeneration.TerrainGenerator.IsOutsideCleanupRange(50f, 100f, 100f), Is.False);
        }
    }
}
