using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles all of the main menu's functionality
/// </summary>
public class MenuController : MonoBehaviour
{
    /// <summary>
    /// The template <c>GameObject</c> that acts as the basis for other buttons.
    /// The first one will be initialized over this so it is expected to be inactive.
    /// </summary>
    public GameObject template;

    [Header("Content")]
    public GameObject scrollViewContent;
    public GameObject closeButton;
    public GameObject aboutButton;

    [Header("Groups")]
    public GameObject mainMenu;
    public GameObject aboutMenu;

    private enum State
    {
        LevelSelect,
        About
    }

    private State currentState = State.LevelSelect;

    /// <summary>
    /// Used to determine the spacing of menu buttons
    /// </summary>
    private const float HEIGHT_MULTIPLIER = 1.2f;

    public void Start()
    {
        LevelManager.Seed();
        if (Localisation.IsInitialized is false)
        {
            #if UNITY_EDITOR
                Localisation.Initialize(SystemLanguage.English);
            #else
                Localisation.Initialize(Application.systemLanguage);
            #endif
        }

        closeButton.GetComponent<Button>().onClick.AddListener(() => ChangeState(State.LevelSelect));
        aboutButton.GetComponent<Button>().onClick.AddListener(() => ChangeState(State.About));
        aboutMenu.GetComponentInChildren<Text>().text = Localisation.Translate("about_game");

        Vector3 position = template.GetComponent<RectTransform>().position;
        float height = template.GetComponent<RectTransform>().sizeDelta.y;
        Invoke("UpdateScrollViewSize", 0.02f);

        foreach (KeyValuePair<int, string> level in LevelManager.GetLevels())
        {
            CreateButtonForScene(level.Key, level.Value, position);
            position -= new Vector3(0, height * HEIGHT_MULTIPLIER);
        }

        if (Screen.width > 800)
            HandleDesktopView();
    }

    #if !UNITY_WEBGL
        private void Update()
        {
            if(Input.GetKeyDown("escape"))
                Application.Quit();
        }
    #endif

    /// <summary>
    /// Helper function for setting up a menu button.
    /// </summary>
    /// <param name="id">The level's ID</param>
    /// <param name="name">The name of the level</param>
    /// <param name="position">The position at which the button needs to be generated</param>
    private void CreateButtonForScene(int id, string name, Vector3 position)
    {
        GameObject button = Instantiate(template, position, Quaternion.identity);
        button.name = name;
        button.GetComponent<MenuButton>().Initialize(id);
        button.transform.SetParent(template.transform.parent, false);
        button.GetComponent<RectTransform>().position = position;
        button.SetActive(true);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="state"></param>
    private void ChangeState(State state)
    {
        bool isMainMenu = state is State.LevelSelect;
        mainMenu.SetActive(isMainMenu);
        closeButton.SetActive(!isMainMenu);

        aboutMenu.SetActive(state is State.About);
        currentState = state;
    }

    /// <summary>
    /// Function for converting the <c>float</c> representation of complexity into a human-readable one.
    /// <c>Localisation</c> is performed to get the text in a given language.
    /// </summary>
    /// <param name="complexity">The level 'complexity'</param>
    /// <returns>A text-representation of the complexity</returns>
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

    /// <summary>
    /// Utility function for adjusting the size of the scrollview for handling overflow.
    /// </summary>
    private void UpdateScrollViewSize()
    {
        RectTransform rectTransform = scrollViewContent.GetComponent<RectTransform>();

        int levelCount = LevelManager.GetLevels().Count;
        float height = template.GetComponent<RectTransform>().sizeDelta.y;
        float size = rectTransform.rect.height - (height * 0.5f) - (levelCount * height * HEIGHT_MULTIPLIER);

        if (size < 0)
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, size);
    }

    /// <summary>
    /// Utility function that adjusts the menu to reflect a 'desktop' view size.
    /// </summary>
    private void HandleDesktopView()
    {
        MapController map = GetComponent<MapController>();

        // Generate and render a map to the left of the screen
        float offset = Camera.main.aspect * Camera.main.orthographicSize * 0.25f;
        map.Create(LevelManager.GetMapForID(Random.Range(1, 4)));
        map.Render(new Vector3(-offset, -offset));
        map.GetMapObject.AddComponent<Spin>();

        // Offset the camera to improve the display of the rendered map
        Camera.main.transform.position += new Vector3(8.0f, -8.5f);
        Camera.main.transform.LookAt(Vector3.zero);
        Camera.main.transform.Rotate(0, 0, 50.0f);

        // Adjust the level select so it takes up the whole screen's height
        RectTransform rectangle = scrollViewContent.transform.parent.transform.parent.GetComponent<RectTransform>();
        rectangle.sizeDelta = new Vector2(rectangle.sizeDelta.x, Screen.height);
        rectangle.position = new Vector3(rectangle.position.x, Screen.height * 0.5f, rectangle.position.z);
    }
}
