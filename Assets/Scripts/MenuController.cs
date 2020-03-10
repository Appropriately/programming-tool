using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject button;

    public void Start()
    {
        LevelManager.Seed();
        if (Localisation.IsInitialized is false)
        {
            #if UNITY_EDITOR
                Localisation.Initialize(SystemLanguage.Japanese);
            #else
                Localisation.Initialize(Application.systemLanguage);
            #endif
        }

        Vector3 position = button.GetComponent<RectTransform>().position;
        float height = button.GetComponent<RectTransform>().sizeDelta.y;
        foreach (KeyValuePair<int, string> level in LevelManager.GetLevels())
        {
            CreateButtonForScene(level.Key, level.Value, position);
            position -= new Vector3(0, height * 1.2f);
        }
    }

    private void CreateButtonForScene(int id, string name, Vector3 position)
    {
        GameObject newButton = Instantiate(button, position, Quaternion.identity);
        newButton.name = name;

        float complexity = LevelManager.GetComplexityForID(id);
        string text = $"{Localisation.Translate(name, true)} - {ComplexityAsString(complexity)}";
        newButton.GetComponentInChildren<Text>().text = text;

        newButton.GetComponent<Button>().onClick.AddListener(delegate { LevelManager.Load(id); });

        newButton.transform.SetParent(button.transform.parent, false);
        newButton.GetComponent<RectTransform>().position = position;

        newButton.SetActive(true);
    }

    private string ComplexityAsString(float complexity)
    {
        if (complexity < 0.3f) {
            return Localisation.Translate("easy");
        } else if (complexity > 0.7f) {
            return Localisation.Translate("hard");
        } else {
            return Localisation.Translate("intermediate");
        }
    }
}
