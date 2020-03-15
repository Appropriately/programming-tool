using NUnit.Framework;

namespace Tests
{
    public class SettingsManagerTests
    {
        [SetUp]
        public void Setup() => SettingsManager.Clear();

        [Test, Description("Testing an invalid access")]
        public void CallingGetOnInvalidToken_ReturnStringEmpty()
        {
            Assert.AreEqual(SettingsManager.GetString("does_not_exist"), string.Empty);
        }

        [Test, Description("Testing a valid access")]
        public void CallingGetOnKnownToken_ReturnExpectedResult()
        {
            Assert.AreEqual(SettingsManager.GetString("default_game_speed"), "slow");
        }

        [Test, Description("Testing a setting save and retrieval")]
        public void SavingThenCallingKnownToken_ReturnExpectedResult()
        {
            SettingsManager.Set("default_game_speed", 2);
            Assert.AreEqual(SettingsManager.GetString("default_game_speed"), "fast");
        }
    }
}
