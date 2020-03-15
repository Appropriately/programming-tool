using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Parses localisation files for the user's locale and handles translating text.
/// The files are <c>csv</c> and are named <c>x.csv</c> (where x is the two character ISO country code).
/// Assumes the English language for unknown translations.
/// They have the format:
/// <code>
/// identifier_1;Translation of identifier_1
/// identifier_2;Translation of identifier_2
/// </code>
/// </summary>
public static class Localisation
{
    /// <summary>
    /// The location of the localisation <c>CSV</c> files
    /// </summary>
    private const string LOCALISATION_FOLDER = "Localisation/";

    private static Dictionary<string, string> english = new Dictionary<string, string>();
    private static Dictionary<string, string> userLanguage = new Dictionary<string, string>();

    /// <summary>
    /// Set up the localisation manager.
    /// Pulls the user's native locale and sets up the appropriate dictionary
    /// </summary>
    /// <param name="language">The <c>SystemLanguage</c> to be used for translation (default is English)</param>
    /// <example>
    /// <code>
    /// Localisation.Initialize(Application.systemLanguage);
    /// Localisation.Translate("test_string");
    /// </code>
    /// </example>
    public static void Initialize(SystemLanguage language = SystemLanguage.English)
    {
        if (language != SystemLanguage.English)
            Parse(userLanguage, language);

        Parse(english, SystemLanguage.English);
    }

    /// <summary>
    /// Clears variables associated to localisation management.
    /// </summary>
    public static void Clear()
    {
        english.Clear();
        userLanguage.Clear();
    }

    /// <summary>
    /// Given a token, finds the appropriate translation for the token.
    /// Initially searches the user's language dictionary, followed by the default "English" dictionary.
    /// </summary>
    /// <param name="token">The token that needs a suitable translation</param>
    /// <param name="handleDefault">Whether a default value or empty string should be returned on token miss</param>
    /// <returns>The translated string</returns>
    /// <example>
    /// <code>
    /// Localisation.Translate("test_string");
    /// </code>
    /// </example>
    public static string Translate(string token, bool handleDefault = false)
    {
        if (string.IsNullOrEmpty(token)) return string.Empty;
        if (userLanguage.ContainsKey(token)) return userLanguage[token];
        if (english.ContainsKey(token)) return english[token];

        if (handleDefault)
        {
            return Default(token);
        } else {
            #if UNITY_EDITR
                Debug.Log($"The token '{token}' is missing a translation")
            #endif
            return string.Empty;
        }
    }

    /// <summary>
    /// Checks whether the language dictionaries have been initialized.
    /// If any of them are initialized, the dictionaries should be cleared before being initialized again.!--
    /// </summary>
    /// <seealso cref="Localisation.Clear()"/>
    public static bool IsInitialized => english.Count + userLanguage.Count > 0;

    /// <summary>
    /// Finds and populates a given dictionary for a given <c>SystemLanguage</c>.
    /// </summary>
    /// <param name="dictionary">The <c>Dictionary</c> which will store the <c>token, translation</c> pairs</param>
    /// <param name="language">The <c>SystemLanguage</c> to be used for translation</param>
    private static void Parse(Dictionary<string, string> dictionary, SystemLanguage language)
    {
        TextAsset textFile = Resources.Load<TextAsset>($"{LOCALISATION_FOLDER}{SystemLanguageToIso(language)}");
        if (textFile == null)
            return;

        List<string> lines = new List<string>(textFile.text.Split(
            System.Environment.NewLine.ToCharArray(),
            System.StringSplitOptions.RemoveEmptyEntries)
        );

        foreach (string line in lines)
        {
            string[] values = line.Split(';');
            if (values.Length == 2 && dictionary.ContainsKey(values[0]) is false)
                dictionary.Add(values[0], CleanText(values[1]));
        }
    }

    /// <summary>
    /// Utility function for handline the text coming from the <c>CSV</c> read.
    /// </summary>
    /// <param name="text">The <c>string</c> that needs to be 'tidied'</param>
    /// <returns>A <c>string</c> ready to be inserted into the <c>Dictionary</c></returns>
    private static string CleanText(string text) => text.Replace("\\n","\n");

    /// <summary>
    /// Handles a "default" value for a given translation token.
    /// Used when no translation can be found for the given token.
    /// </summary>
    /// <param name="token">The translation token</param>
    /// <returns>A "default" value for the given token</returns>
    private static string Default(string token)
    {
        if (string.IsNullOrEmpty(token)) return string.Empty;

        char[] array = token.Replace('_', ' ').ToCharArray();
        array[0] = char.ToUpper(array[0]);
        return new string(array);
    }

    /// <summary>
    /// Converts the weird <c>SystemLanguage</c> representation to the ISO standard country code representation
    /// </summary>
    /// <param name="language">The <c>SystemLanguage</c> to convert</param>
    /// <returns>The two character ISO country code, defaulting to <c>"en"</c></returns>
    private static string SystemLanguageToIso(SystemLanguage language)
    {
		switch (language) {
			case SystemLanguage.Afrikaans: return "af";
			case SystemLanguage.Arabic: return "ar";
			case SystemLanguage.Basque: return "eu";
			case SystemLanguage.Belarusian: return "by";
			case SystemLanguage.Bulgarian: return "bg";
			case SystemLanguage.Catalan: return "ca";
			case SystemLanguage.Chinese: return "zh";
			case SystemLanguage.Czech: return "cs";
			case SystemLanguage.Danish: return "da";
			case SystemLanguage.Dutch: return "nl";
			case SystemLanguage.Estonian: return "et";
			case SystemLanguage.Faroese: return "fo";
			case SystemLanguage.Finnish: return "fo";
			case SystemLanguage.French: return "fr";
			case SystemLanguage.German: return "de";
			case SystemLanguage.Greek: return "el";
			case SystemLanguage.Hebrew: return "iw";
			case SystemLanguage.Hungarian: return "hu";
			case SystemLanguage.Icelandic: return "is";
			case SystemLanguage.Indonesian: return "in";
			case SystemLanguage.Italian: return "it";
			case SystemLanguage.Japanese: return "ja";
			case SystemLanguage.Korean: return "ko";
			case SystemLanguage.Latvian: return "lv";
			case SystemLanguage.Lithuanian: return "lt";
			case SystemLanguage.Norwegian: return "no";
			case SystemLanguage.Polish: return "pl";
			case SystemLanguage.Portuguese: return "pt";
			case SystemLanguage.Romanian: return "ro";
			case SystemLanguage.Russian: return "ru";
			case SystemLanguage.SerboCroatian: return "sh";
			case SystemLanguage.Slovak: return "sk";
			case SystemLanguage.Slovenian: return "sl";
			case SystemLanguage.Spanish: return "es";
			case SystemLanguage.Swedish: return "sv";
			case SystemLanguage.Thai: return "th";
			case SystemLanguage.Turkish: return "tr";
			case SystemLanguage.Ukrainian: return "uk";
			case SystemLanguage.Vietnamese: return "vi";
            default: return "en";
		}
    }
}
