using System.Collections.Generic;
using NUnit.Framework;

namespace Tests
{
    public class LevelManagerTests
    {
        [SetUp]
        public void Setup() => LevelManager.Clear();

        // -------------------------------------------------------------------------------------------------------- //
        // Test accessing level information
        // -------------------------------------------------------------------------------------------------------- //

        [Test, Description("A request to an empty map should throw a KeyNotFoundException")]
        public void LevelDetailsAccessBeforeSeeding_ThrowsKeyNotFoundException() {
            Assert.Throws<KeyNotFoundException>(() => LevelManager.GetNameForID(1));
        }

        [Test, Description("After seeding, the level data structures should contain information after seeding")]
        public void LevelDetailsAccessAfterSeedingValidID_ReturnData() {
            LevelManager.Seed();
            Assert.IsNotNull(LevelManager.GetNameForID(1));
        }

        [Test, Description("An invalid ID should throw a KeyNotFoundException")]
        public void LevelDetailsAccessAfterSeedingInvalidID_ThrowsKeyNotFoundException() {
            LevelManager.Seed();
            Assert.Throws<KeyNotFoundException>(() => LevelManager.GetNameForID(999));
        }
    }
}
