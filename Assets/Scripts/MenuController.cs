using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject button;

    public void Start()
    {
        LevelManager.Seed();
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
        newButton.GetComponentInChildren<Text>().text = $"{name} - {ComplexityAsString(complexity)}";

        newButton.GetComponent<Button>().onClick.AddListener(delegate { LevelManager.Load(id); });

        newButton.transform.SetParent(button.transform.parent, false);
        newButton.GetComponent<RectTransform>().position = position;

        newButton.SetActive(true);
    }

    private string ComplexityAsString(float complexity)
    {
        if (complexity < 0.3f) {
            return "Easy";
        } else if (complexity > 0.7f) {
            return "Hard";
        } else {
            return "Intermediate";
        }
    }
}
