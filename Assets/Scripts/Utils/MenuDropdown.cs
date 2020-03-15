using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper class for a dropdown on the main menu.
/// Handles setting up the listener event '<c>onValueChanged</c>' and checking if there is an existing value.
/// </summary>
public class MenuDropdown : MonoBehaviour
{
    public Text label;
    public Dropdown dropdown;

    private string token;

    /// <summary>
    /// Populates and updates the <c>Dropdown</c>, using the player's saved preferences if they exist.
    /// </summary>
    /// <param name="token">The token used for the label and saving/loading the preference</param>
    /// <param name="options">A <c>string</c> <c>List</c> of possible dropdown options</param>
    public void Initialize(string token, List<string> options)
    {
        this.token = token;
        label.text = Localisation.Translate(token, true);

        dropdown.options.Clear();
        foreach (string option in options)
            dropdown.options.Add (new Dropdown.OptionData() { text = Localisation.Translate(option, true) });

        dropdown.value = GetInitialIndex;
        dropdown.onValueChanged.AddListener((int value) => SettingsManager.Set(token, value));
    }

    /// <summary>
    /// Finds the player's saved preference from within the <c>dropdown</c>'s possible options.
    /// </summary>
    /// <returns>The index of the user's preference or <c>0</c></returns>
    private int GetInitialIndex => PlayerPrefs.GetInt(token, 0);
}
