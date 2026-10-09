using MotionCore2D.Configuration;
using NUnit.Framework;
using UnityEngine;

namespace MotionCore2D.Tests.EditMode
{
    /// <summary>Unit tests for <see cref="MovementProfile"/> defaults and validation.</summary>
    public sealed class MovementProfileTests
    {
        [Test]
        public void Defaults_AreWithinSupportedRanges()
        {
            var profile = ScriptableObject.CreateInstance<MovementProfile>();

            Assert.GreaterOrEqual(profile.MaxSpeed, 0f);
            Assert.That(profile.AirControlFactor, Is.InRange(0f, 1f));
            Assert.That(profile.CrouchSpeedMultiplier, Is.InRange(0f, 1f));
            Assert.That(profile.CrouchHeightFactor, Is.InRange(0.1f, 1f));
            Assert.GreaterOrEqual(profile.FallGravityMultiplier, 1f);
            Assert.That(profile.MaxSlopeAngle, Is.InRange(0f, 89f));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void Validate_ClampsOutOfRangeValues()
        {
            var profile = ScriptableObject.CreateInstance<MovementProfile>();
            JsonUtility.FromJsonOverwrite(
                "{\"_maxSpeed\":-5,\"_airControlFactor\":3,\"_crouchSpeedMultiplier\":3,\"_crouchHeightFactor\":5,\"_fallGravityMultiplier\":0,\"_maxSlopeAngle\":120}",
                profile);

            profile.Validate();

            Assert.GreaterOrEqual(profile.MaxSpeed, 0f);
            Assert.LessOrEqual(profile.AirControlFactor, 1f);
            Assert.That(profile.CrouchSpeedMultiplier, Is.InRange(0f, 1f));
            Assert.That(profile.CrouchHeightFactor, Is.InRange(0.1f, 1f));
            Assert.GreaterOrEqual(profile.FallGravityMultiplier, 1f);
            Assert.LessOrEqual(profile.MaxSlopeAngle, 89f);

            Object.DestroyImmediate(profile);
        }
    }
}
