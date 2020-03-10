using System;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class LocalisationTests
    {
        [SetUp]
        public void Setup()
        {
            Localisation.Clear();
            Localisation.Initialize(SystemLanguage.English);
        }

        // -------------------------------------------------------------------------------------------------------- //
        // Initialization tests
        // -------------------------------------------------------------------------------------------------------- //

        [Test, Description("Test initialization was successful")]
        public void CallIsInitialized_ReturnsTrue() => Assert.IsTrue(Localisation.IsInitialized);

        [Test, Description("Clearing the dictionaries results in IsInitialized returning false")]
        public void CallIsInitializedAfterClear_ReturnsFalse()
        {
            Localisation.Clear();
            Assert.IsFalse(Localisation.IsInitialized);
        }

        [Test, Description("Initializing without clearing throws an ArgumentException")]
        public void CallInitializeWithoutClearing_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Localisation.Initialize(SystemLanguage.Japanese));
        }

        // -------------------------------------------------------------------------------------------------------- //
        // Test translation of tokens
        // -------------------------------------------------------------------------------------------------------- //

        [Test, Description("Test translating an empty string token")]
        public void TranslatingEmptyToken_ReturnsEmptyString() => Assert.IsEmpty(Localisation.Translate(string.Empty));

        [Test, Description("Test translating a known non-existent token")]
        public void TranslatingUnknownToken_ReturnsEmptyString() => Assert.IsEmpty(Localisation.Translate("not_real"));

        [Test, Description("Test accessing a known token for English")]
        public void TranslatingToken_ReturnsEnglish() => Assert.IsTrue(Localisation.Translate("language") == "English");

        [Test, Description("Translate the language token after clearing and initializing as Japanese")]
        public void TranslatingLanguageAfterJapaneseInitialization_Returns日本語()
        {
            Localisation.Clear();
            Localisation.Initialize(SystemLanguage.Japanese);
            Assert.IsTrue(Localisation.Translate("language") == "日本語");
        }

        [Test, Description("Translating language for an unknown language returns English")]
        public void TranslatingLanguageForUnknownLanguage_ReturnsEnglush()
        {
            Localisation.Clear();
            Localisation.Initialize(SystemLanguage.Afrikaans);
            Assert.IsTrue(Localisation.Translate("language") == "English");
        }
    }
}
