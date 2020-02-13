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
        public void LevelDetailsAccessBeforeSeeding_ThrowsKeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(() => LevelManager.GetNameForID(1));
        }

        [Test, Description("After seeding, the level data structures should contain information after seeding")]
        public void LevelDetailsAccessAfterSeedingValidID_ReturnData()
        {
            LevelManager.Seed();
            Assert.IsNotNull(LevelManager.GetNameForID(1));
        }

        [Test, Description("An invalid ID should throw a KeyNotFoundException")]
        public void LevelDetailsAccessAfterSeedingInvalidID_ThrowsKeyNotFoundException()
        {
            LevelManager.Seed();
            Assert.Throws<KeyNotFoundException>(() => LevelManager.GetNameForID(999));
        }

        [Test, Description("The returned item from the level name array should be of type String")]
        public void AccessingNameOfALevel_ReturnString()
        {
            LevelManager.Seed();
            Assert.IsInstanceOf<string>(LevelManager.GetNameForID(1));
        }

        [Test, Description("The returned item from the level name array should be a string")]
        public void AccessingMapForAValidLevel_ReturnBlockArray()
        {
            LevelManager.Seed();
            Assert.IsInstanceOf<string>(LevelManager.GetMapForID(1));
        }

        [Test, Description("The returned item from the level map array should be a Block array")]
        public void AccessingBlocksForAValidLevel_ReturnBlockArray()
        {
            LevelManager.Seed();
            Assert.IsInstanceOf<Block[]>(LevelManager.GetBlocksForID(1));
        }

        [Test, Description("Accessing the current map should fail as a map hasn't been loaded through the manager")]
        public void AccessingCurrentMapWhenMapIsNotLoaded_ThrowsKeyNotFoundException()
        {
            LevelManager.Seed();
            Assert.Throws<KeyNotFoundException>(() => LevelManager.GetName());
        }
    }
}
