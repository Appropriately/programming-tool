using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager for handling and abstracting the settings logic.
/// </summary>
public static class SettingsManager
{
    private const int INVALID_VALUE = -1;

    /// <summary>
    /// Sets up some default values for the <c>SettingsManager</c>.
    /// </summary>
    static SettingsManager()
    {
        #if UNITY_EDITOR
            Clear();
            Debug.Log("All values have been cleared.");
        #endif

        if (GetInt("default_game_speed", defaultValue: INVALID_VALUE) is INVALID_VALUE)
            Set("default_game_speed", 1);
    }

    /// <summary>
    /// A <c>Dictionary</c> of all of the available settings.
    /// </summary>
    public static Dictionary<string, List<string>> settings = new Dictionary<string, List<string>>()
    {
        { "default_game_speed", new List<string>(){ "slow", "normal", "fast" } },
        { "icon_colours", new List<string>() { "colourful", "black_and_white" } }
    };

    /// <summary>
    /// Returns the string representation for the given settings <c>token</c>.
    /// If there is no setting for the given <c>token</c>, it returns <c>string.Empty</c>.
    /// </summary>
    /// <param name="token">The setting to look for</param>
    /// <returns>The <c>string</c> representation of the setting in question</returns>
    public static string GetString(string token)
    {
        if (settings.ContainsKey(token) is false)
            return string.Empty;

        List<string> settingValues = settings[token];
        return settingValues[GetInt(token)];
    }

    /// <summary>
    /// Returns the index into the options array for the given <c>token</c>.
    /// </summary>
    /// <param name="token">The setting to look for</param>
    /// <returns>An index into the options array</returns>
    public static int GetInt(string token, int defaultValue = 0) => PlayerPrefs.GetInt(token, defaultValue);

    /// <summary>
    /// Handles saving a particular setting's value.
    /// </summary>
    /// <param name="token">The setting to look for</param>
    /// <param name="value">The <c>int</c> value to set</param>
    public static void Set(string token, int value) => PlayerPrefs.SetInt(token, value);

    /// <summary>
    /// Removes all the setting's saved values from <c>PlayerPrefs</c>.
    /// </summary>
    public static void Clear()
    {
        foreach (KeyValuePair<string, List<string>> setting in settings)
            PlayerPrefs.DeleteKey(setting.Key);
    }
}
