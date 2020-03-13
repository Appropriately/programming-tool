using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Node startNode;

    [Header("Run/Stop button")]
    public Button runButton;
    public Sprite play, stop;
    private static readonly Color playColour = Color.green;
    private static readonly Color stopColour = Color.red;

    [Header("Edit/Test button")]
    public Button editButton;
    public Sprite edit, test;

    [Header("Buttons")]
    public Button bin;
    public Button home;
    public Button fastForward;
    public Sprite binOpen, binClosed;

    public GameObject nodeButtonTemplate;

    [Header("Other variables")]
    public GameObject alert;
    public GameObject score;

    [Header("Icons")]
    public Sprite cheese;
    public Sprite forward;
    public Sprite rotate;
    public Sprite speak;
    public Sprite interact;

    [Header("Other")]

    public bool isDragging = false;

    private bool isFastForwarded = false;

    private GameObject[] nodeButtons;
    private List<GameObject> nodes;
    private Rect editorBounds;

    private GameObject template;
    public PlayerController player;
    public MapController map;

    private Vector3 playCamera, editorCamera;
    private Quaternion playRotation, editorRotation;

    /// <summary>
    /// The state of the current game, either in editor or playing/stopped.
    /// </summary>
    enum State
    {
        Stopped,
        Editor,
        Playing
    }

    private State state = State.Stopped;

    public void Start() {
        if (player is null) player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
        if (map is null) map = GetComponent<MapController>();

        UpdateCamera();

        try {
            map.Create(LevelManager.GetMap());
            map.Render(Vector3.zero);
            player.Setup();
            map.Reset();

            startNode.GetComponentInChildren<TextMesh>().text = Localisation.Translate("on", true);
            nodes = new List<GameObject>{ startNode.gameObject };

            template = GenerateTemplateNode();
            SetUpButtons();

            SetupPlayButton();
            UpdateScoreIndicator();

            // Setup relevant onClick listeners
            home.onClick.AddListener(() => LevelManager.GoToMainMenu());
            fastForward.onClick.AddListener(FastForward);
            editButton.onClick.AddListener(ToggleMode);
        } catch (System.Exception e) {
            #if UNITY_EDITOR
                Debug.LogError($"{e} Exception caught.");
            #endif
            LevelManager.GoToMainMenu("There was an issue generating the map");
        }
    }

    private void Update() {
        float lerpTime = Time.deltaTime * 3.0f;
        Vector3 target = IsInEditor ? editorCamera : playCamera;
        Quaternion rotation = IsInEditor ? editorRotation : playRotation;
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, target, lerpTime);
        Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, rotation, lerpTime);

        if(Input.GetKeyDown("escape"))
            LevelManager.GoToMainMenu();
    }

    public void SetRunning(bool value) => state = value ? State.Playing : State.Stopped;
    public bool IsRunning => state == State.Playing;
    public bool IsStopped => state == State.Stopped;
    public bool IsInEditor => state == State.Editor;

    /// <summary>
    /// Checks whether the given location is within the bounds of the editor.
    /// </summary>
    /// <param name="location">The location that needs checking</param>
    /// <returns>Whether the location is within the editor's bounds</returns>
    public bool ValidLocation(Vector3 location) => editorBounds.Contains(location);

    /// <summary>
    /// Iterates through each "player" and determines whether they have reached an exit.
    /// Traditionally called by a <c>Node</c> when it has no child node left to determine if the player has 'won'.
    /// </summary>
    public void WinConditionHandling()
    {
        if (player.GetComponent<PlayerController>().Tile() != MapController.END_TILE) return;

        LevelManager.SetScore(startNode.child ? startNode.child.Count : 0);
        UpdateScoreIndicator();
        StopRun();
    }

    /// <summary>
    /// Returns the number of nodes in the current solution, including the <c>startNode</c>.
    /// </summary>
    public int NodeCount => nodes.Count;

    public bool IsFastForwarded { get => isFastForwarded; set => isFastForwarded = value; }

    /// <summary>
    /// Handles <c>Node</c> deletion and removal from the nodes array.
    /// </summary>
    /// <param name="node">The <c>Node</c> that needs to be removed</param>
    public void RemoveNode(GameObject node)
    {
        int index = nodes.FindIndex(i => node.GetInstanceID() == i.GetInstanceID());
        Destroy(node);
        if (index >= 0) nodes.RemoveAt(index);
    }

    /// <summary>
    /// Given a position, determines whether the bin icon should be shown open or closed.
    /// </summary>
    /// <param name="position">The position vector that should be checked</param>
    public void CheckAndUpdateBinIcon(Vector3 position)
    {
        Image binImage = bin.GetComponent<Image>();
        binImage.sprite = ValidLocation(position) ? binClosed : binOpen;
    }

    /// <summary>
    /// Returns whether the play mode is fast forwarded.
    /// </summary>
    public bool fastForwarded => state == State.Playing && isFastForwarded;

    /// <summary>
    /// Update's the score display, determining whether it should be shown and what value to show.
    /// </summary>
    private void UpdateScoreIndicator()
    {
        int value = LevelManager.GetScore();
        if (value > 0) {
            score.GetComponentInChildren<Text>().text = value.ToString();
            score.SetActive(true);
        } else {
            score.SetActive(false);
        }
    }

    /// <summary>
    /// Iterates over all the available blocks for the level and generates a unique button for each one.
    /// </summary>
    private void SetUpButtons()
    {
        Vector3 position = nodeButtonTemplate.GetComponent<RectTransform>().position;
        List<GameObject> list = new List<GameObject>();
        foreach (Block block in LevelManager.GetBlocks()) {
            GameObject button;

            switch (block) {
                case Block.RotateRight:
                    button = CreateNodeButton (
                        block, position, Localisation.Translate("rotate", true), rotate, Color.cyan
                    );
                    Image image = button.GetComponentsInChildren<Image>()[1];
                    image.transform.eulerAngles = image.transform.rotation.eulerAngles + 180f * Vector3.up;
                    break;
                case Block.RotateLeft:
                    button = CreateNodeButton (
                        block, position, Localisation.Translate("rotate", true), rotate, Color.cyan
                    );
                    break;
                case Block.Speak:
                    button = CreateNodeButton (
                        block, position, Localisation.Translate("speak", true), speak, Color.red
                    );
                    break;
                case Block.IfSpaceIsTraversable:
                    button = CreateNodeButton (
                        block, position, Localisation.Translate("if", true), forward, Color.cyan
                    );
                    break;
                case Block.IfSpaceIsActivatable:
                    button = CreateNodeButton (
                        block, position, Localisation.Translate("if", true), interact, Color.red
                    );
                    break;
                case Block.WhileNotAtExit:
                    button = CreateNodeButton (
                        block, position, Localisation.Translate("until", true), cheese, Color.white
                    );
                    break;
                case Block.WhileTraversable:
                    button = CreateNodeButton (
                        block, position, Localisation.Translate("while", true), forward, Color.cyan
                    );
                    break;
                case Block.Interact:
                    button = CreateNodeButton (
                        block, position, Localisation.Translate("interact", true), interact, Color.red
                    );
                    break;
                default:
                    button = CreateNodeButton (
                        block, position, Localisation.Translate("move", true), forward, Color.cyan
                    );
                    break;
            }

            list.Add(button);
            position -= new Vector3(0, nodeButtonTemplate.GetComponent<RectTransform>().sizeDelta.y * 1.05f);
        }
        nodeButtons = list.ToArray();
    }

    /// <summary>
    /// Creates a button responsible for generating new nodes for the given <c>Block</c>.
    /// Sets up the appropriate events that will need to fire.
    /// </summary>
    /// <param name="block">The <c>Block</c> that the button will be expected to create</param>
    /// <param name="position">The <c>Vector3</c> position of the new button</param>
    /// <param name="text">The content of the button's <c>Text</c> block</param>
    /// <param name="sprite">The <c>Sprite</c> to display on the button</param>
    /// <param name="colour">The colour of the <c>Sprite</c></param>
    /// <returns>The button as a <c>GameObject</c></returns>
    private GameObject CreateNodeButton(Block block, Vector3 position, string text, Sprite sprite, Color colour)
    {
        GameObject button = Instantiate(nodeButtonTemplate, position, Quaternion.identity);
        button.name = block.ToString();
        button.GetComponentInChildren<Text>().text = text;

        Image image = button.GetComponentsInChildren<Image>()[1];
        image.sprite = sprite;
        image.color = colour;

        EventTrigger trigger = button.GetComponent<EventTrigger>();
        EventTrigger.Entry drag = new EventTrigger.Entry();
        drag.eventID = EventTriggerType.Drag;
        drag.callback.AddListener( (eventData) => { AddNode(block, true); } );

        EventTrigger.Entry click = new EventTrigger.Entry();
        click.eventID = EventTriggerType.PointerClick;
        click.callback.AddListener( (eventData) => { AddNode(block, false); } );

        trigger.triggers.Add(drag);
        trigger.triggers.Add(click);

        button.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
        button.GetComponent<RectTransform>().position = position;
        return button;
    }

    private GameObject GenerateTemplateNode()
    {
        float xOffset = startNode.transform.localScale.x * 3f;
        GameObject node = Instantiate(startNode.gameObject, editorBounds.center, Quaternion.identity);

        node.SetActive(false);
        node.name = "template";

        DestroyImmediate(node.GetComponent<OnRun>());

        return node;
    }

    /// <summary>
    /// Adjusts the camera so the editor screen is appropriately setup and the camera is adjusted for portait displays.
    /// </summary>
    private void UpdateCamera()
    {
        if (Camera.main.aspect < 1.0f) Camera.main.orthographicSize = 10.0f;

        Vector3 originalPosition = Camera.main.transform.position;
        editorCamera = originalPosition + new Vector3(0, Camera.main.orthographicSize * 2.5f);
        editorRotation = Camera.main.transform.rotation;

        playCamera = Camera.main.transform.position + new Vector3(8.0f, -8.5f);
        Camera.main.transform.position = playCamera;
        Camera.main.transform.LookAt(Vector3.zero);
        Camera.main.transform.Rotate(0, 0, 50.0f);
        playRotation = Camera.main.transform.rotation;

        float xOffset = Camera.main.aspect * Camera.main.orthographicSize * 0.8f;
        float yOffset = Camera.main.orthographicSize * 0.8f;
        editorBounds = Rect.MinMaxRect(
            editorCamera.x - xOffset, editorCamera.y - yOffset, editorCamera.x + xOffset, editorCamera.y + yOffset
        );

        startNode.transform.position = new Vector2 (
            editorCamera.x - (xOffset * 0.5f), editorCamera.y + (yOffset * 0.9f)
        );
    }

    private void ToggleMode()
    {
        if (IsStopped) {
            // Disable buttons and UI elements
            runButton.gameObject.SetActive(false);
            home.gameObject.SetActive(false);
            score.SetActive(false);

            foreach (GameObject button in nodeButtons)
                button.SetActive(true);

            editButton.GetComponent<Image>().sprite = test;
            state = State.Editor;
        } else if (IsInEditor) {
            // Enable relevant buttons and UI elements
            runButton.gameObject.SetActive(true);
            home.gameObject.SetActive(true);

            foreach (GameObject button in nodeButtons)
                button.SetActive(false);

            UpdateScoreIndicator();
            editButton.GetComponent<Image>().sprite = edit;
            state = State.Stopped;
        }
    }

    private GameObject CreateCodeBlock(Block block)
    {
        GameObject node = Instantiate(template);
        float xScale = node.transform.localScale.x;
        switch (block)
        {
            case Block.Move:
                SetupNode(node, typeof(Move), 1.0f, Localisation.Translate("move", true), Color.cyan, forward);
                break;
            case Block.RotateRight:
                SetupNode (
                    node, typeof(RotateRight), 1.0f, Localisation.Translate("rotate", true), Color.cyan, rotate
                );
                node.GetComponentInChildren<SpriteRenderer>().flipX = true;
                break;
            case Block.RotateLeft:
                SetupNode (
                    node, typeof(RotateLeft), 1.0f, Localisation.Translate("rotate", true), Color.cyan, rotate
                );
                break;
            case Block.Speak:
                SetupNode (
                    node, typeof(Speak), 1.0f, Localisation.Translate("speak", true), Color.red, speak
                );
                break;
            case Block.IfSpaceIsTraversable:
                SetupNode (
                    node, typeof(IfSpaceIsTraversable), 2.0f, Localisation.Translate("if", true), Color.cyan, forward
                );
                break;
            case Block.IfSpaceIsActivatable:
                SetupNode (
                    node, typeof(IfSpaceIsActivatable), 2.0f, Localisation.Translate("if", true), Color.red, interact
                );
                break;
            case Block.WhileNotAtExit:
                SetupNode (
                    node, typeof(WhileNotAtExit), 1.0f, Localisation.Translate("until", true), Color.white, cheese
                );
                break;
            case Block.WhileTraversable:
                SetupNode (
                    node, typeof(WhileTraversable), 1.0f, Localisation.Translate("while", true), Color.cyan, forward
                );
                break;
            case Block.Interact:
                SetupNode(node, typeof(Interact), 1.0f, Localisation.Translate("interact", true), Color.red, interact);
                break;
        }

        return node;
    }

    /// <summary>
    /// Utility function for adjusting the <c>Node</c>'s appearance
    /// Adjusts the look of the <c>SpriteRenderer</c> as well as the included <c>TextMesh</c>
    /// </summary>
    /// <param name="node">The <c>Node</c> that needs adjusting</param>
    /// <param name="type">The particular <c>Node</c> implementation</param>
    /// <param name="scale">The scale of the particular node, along the x-axis</param>
    /// <param name="text">The text to display</param>
    /// <param name="colour">The colour of the <c>Sprite</c></param>
    /// <param name="sprite">The actual image to display for the <c>SpriteRenderer</c></param>
    private void SetupNode(GameObject node, System.Type type, float scale, string text, Color colour, Sprite sprite)
    {
        SpriteRenderer image = node.GetComponentInChildren<SpriteRenderer>();
        TextMesh textMesh = node.GetComponentInChildren<TextMesh>();

        node.AddComponent(type);
        textMesh.text = text;
        image.color = colour;
        image.sprite = sprite;

        if (scale > 1.0f) {
            Vector3 vectorScale = node.transform.localScale;
            node.transform.localScale = new Vector3(vectorScale.x * scale, vectorScale.y, vectorScale.z);
            vectorScale = image.transform.localScale;
            image.transform.localScale = new Vector3(vectorScale.x / scale, vectorScale.y, vectorScale.z);
            vectorScale = textMesh.transform.localScale;
            textMesh.transform.localScale = new Vector3(vectorScale.x / scale, vectorScale.y, vectorScale.z);
        }
    }

    private void AddNode(Block block, bool draggable)
    {
        if (isDragging is false) {
            GameObject obj = CreateCodeBlock(block);
            if (draggable) {
                Node node = obj.GetComponent<Node>();
                node.controller = this;
                if (node.GetType().IsSubclassOf(typeof(DraggableNode))) {
                    DraggableNode draggableNode = (DraggableNode) node;
                    draggableNode.SetDragging(true);
                    draggableNode.MoveToMouse();
                }
            }
            obj.SetActive(true);
            nodes.Add(obj);
        }
    }

    private void StartRun()
    {
        runButton.onClick.RemoveAllListeners();
        editButton.gameObject.SetActive(false);
        home.gameObject.SetActive(false);
        fastForward.gameObject.SetActive(true);

        SetupStopButton();

        IsFastForwarded = false;
        SetRunning(true);
        StartCoroutine(startNode.Run());
    }

    private void StopRun()
    {
        StopAllCoroutines();
        player.StopAllCoroutines();
        foreach (GameObject node in nodes) node.GetComponent<Node>()?.StopAllCoroutines();

        runButton.onClick.RemoveAllListeners();
        SetupPlayButton();

        SetRunning(false);
        map.Reset();
        player.Reset();
        editButton.gameObject.SetActive(true);
        home.gameObject.SetActive(true);
        fastForward.gameObject.SetActive(false);
    }

    /// <summary>
    /// Utility function for toggling a fast forward.
    /// </summary>
    private void FastForward()
    {
        isFastForwarded = true;
        fastForward.gameObject.SetActive(false);
    }

    private void SetupPlayButton()
    {
        runButton.onClick.AddListener(StartRun);
        Image image = runButton.GetComponent<Image>();
        image.color = playColour;
        image.sprite = play;
    }

    private void SetupStopButton()
    {
        runButton.onClick.AddListener(StopRun);
        Image image = runButton.GetComponent<Image>();
        image.color = stopColour;
        image.sprite = stop;
    }
 }
