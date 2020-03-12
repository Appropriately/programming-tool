using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tool for building and managing menu buttons.
/// </summary>
public class MenuButton : MonoBehaviour
{
    public GameObject title, difficulty, score;

    public void Initialize(int id)
    {
        title.GetComponent<Text>().text = Localisation.Translate(name, true);
        GetComponent<Button>().onClick.AddListener(delegate { LevelManager.Load(id); });

        int scoreValue = LevelManager.GetScoreForID(id);
        if (scoreValue > 0) {
            score.GetComponentInChildren<Text>().text = scoreValue.ToString();
            score.SetActive(true);
        }
    }
}
