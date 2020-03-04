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
    public Sprite binOpen, binClosed;

    public GameObject nodeButtonTemplate;

    [Header("Other variables")]
    public GameObject alert;
    public GameObject score;

    public bool isDragging = false;

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

            nodes = new List<GameObject>{ startNode.gameObject };

            template = GenerateTemplateNode();
            Vector3 position = nodeButtonTemplate.GetComponent<RectTransform>().position;
            List<GameObject> list = new List<GameObject>();
            foreach (Block block in LevelManager.GetBlocks()) {
                list.Add(CreateNodeButton(block, position));
                position -= new Vector3(0, nodeButtonTemplate.GetComponent<RectTransform>().sizeDelta.y * 1.2f);
            }
            nodeButtons = list.ToArray();

            SetupPlayButton();
            UpdateScoreIndicator();
            home.onClick.AddListener(() => LevelManager.GoToMainMenu());
            editButton.onClick.AddListener(ToggleMode);
        } catch (System.Exception e) {
            #if UNITY_EDITOR
                Debug.LogError($"{e} Exception caught.");
            #endif
            LevelManager.GoToMainMenu("There was an issue generating the map");
        }
    }

    public void Update() {
        float lerpTime = Time.deltaTime * 3.0f;
        Vector3 target = IsInEditor ? editorCamera : playCamera;
        Quaternion rotation = IsInEditor ? editorRotation : playRotation;
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, target, lerpTime);
        Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, rotation, lerpTime);
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
    public bool ValidLocation(Vector3 location) {
        return editorBounds.Contains(location);
    }

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
    /// Update's the score display, determining whether it should be shown and what value to show.
    /// </summary>
    private void UpdateScoreIndicator()
    {
        int value = PlayerPrefs.GetInt(LevelManager.GetName(), 0);
        if (value > 0) {
            score.GetComponentInChildren<Text>().text = value.ToString();
            score.SetActive(true);
        } else {
            score.SetActive(false);
        }
    }

    /// <summary>
    /// Creates a button responsible for generating new nodes for the given <c>Block</c>.
    /// Sets up the appropriate events that will need to fire.
    /// </summary>
    /// <param name="block">The <c>Block</c> that the button will be expected to create</param>
    /// <param name="position">The <c>Vector3</c> position of the new button</param>
    /// <returns>The button as a <c>GameObject</c></returns>
    private GameObject CreateNodeButton(Block block, Vector3 position)
    {
        GameObject button = Instantiate(nodeButtonTemplate, position, Quaternion.identity);
        button.name = block.ToString();
        button.GetComponentInChildren<Text>().text = block.ToString();

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
            runButton.gameObject.SetActive(false);
            score.SetActive(false);
            editButton.GetComponent<Image>().sprite = test;
            foreach (GameObject button in nodeButtons) button.SetActive(true);
            state = State.Editor;
            home.gameObject.SetActive(false);
        } else if (IsInEditor) {
            runButton.gameObject.SetActive(true);
            UpdateScoreIndicator();
            editButton.GetComponent<Image>().sprite = edit;
            foreach (GameObject button in nodeButtons) button.SetActive(false);
            state = State.Stopped;
            home.gameObject.SetActive(true);
        }
    }

    private GameObject CreateCodeBlock(Block block)
    {
        GameObject node = Instantiate(template);
        node.name = block.ToString();
        node.GetComponentInChildren<TextMesh>().text = block.ToString().ToLower();

        switch (block)
        {
            case Block.Move:
                node.AddComponent<Move>();
                break;
            case Block.RotateRight:
                node.AddComponent<RotateRight>();
                break;
            case Block.RotateLeft:
                node.AddComponent<RotateLeft>();
                break;
            case Block.Speak:
                node.AddComponent<Speak>();
                break;
            case Block.IfSpaceIsTraversable:
                node.AddComponent<IfSpaceIsTraversable>();
                node.transform.localScale += new Vector3(node.transform.localScale.x, 0);
                break;
            case Block.IfSpaceIsActivatable:
                node.AddComponent<IfSpaceIsActivatable>();
                node.transform.localScale += new Vector3(node.transform.localScale.x, 0);
                break;
            case Block.WhileNotAtExit:
                node.AddComponent<WhileNotAtExit>();
                break;
            case Block.WhileTraversable:
                node.AddComponent<WhileTraversable>();
                break;
            case Block.Interact:
                node.AddComponent<Interact>();
                break;
        }

        return node;
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

        SetupStopButton();

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
        player.Reset();
        map.Reset();
        editButton.gameObject.SetActive(true);
        home.gameObject.SetActive(true);
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
