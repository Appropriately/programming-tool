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
        foreach (KeyValuePair<int, string> level in LevelManager.GetLevels())
        {
            CreateButtonForScene(level.Key, level.Value, position);
            position -= new Vector3(0, button.GetComponent<RectTransform>().sizeDelta.y * 1.2f);
        }
    }

    private void CreateButtonForScene(int id, string name, Vector3 position)
    {
        GameObject newButton = Instantiate(button, position, Quaternion.identity);
        newButton.name = name;
        newButton.GetComponentInChildren<Text>().text = name;

        newButton.GetComponent<Button>().onClick.AddListener(delegate { LevelManager.Load(id); });

        newButton.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
        newButton.GetComponent<RectTransform>().position = position;

        newButton.SetActive(true);
    }
}
