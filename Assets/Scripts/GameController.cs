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
    public Sprite binOpen, binClosed;

    public GameObject nodeButtonTemplate;

    [Header("Other variables")]
    public GameObject alert;
    public bool isDragging = false;

    private GameObject[] nodeButtons;
    private List<GameObject> nodes;
    private Rect editorBounds;

    private GameObject template;
    public PlayerController player;
    public MapController map;

    private Vector3 playCamera, editorCamera;
    private Quaternion playRotation, editorRotation;

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

        try
        {
            map.Create(LevelManager.GetMap());
            map.Render(Vector3.zero);
            player.Setup();
            map.Reset();

            nodes = new List<GameObject>{ startNode.gameObject };

            template = GenerateTemplate(startNode.gameObject);
            Vector3 position = nodeButtonTemplate.GetComponent<RectTransform>().position;
            List<GameObject> list = new List<GameObject>();
            foreach (Block block in LevelManager.GetBlocks())
            {
                list.Add(CreateNodeButton(block, position));
                position -= new Vector3(0, nodeButtonTemplate.GetComponent<RectTransform>().sizeDelta.y * 1.2f);
            }
            nodeButtons = list.ToArray();

            playButton();
            editButton.onClick.AddListener(ToggleMode);
        }
        catch (System.Exception e)
        {
            #if UNITY_EDITOR
                Debug.LogError($"{e} Exception caught.");
            #endif
            LevelManager.GoToMainMenu("There was an issue generating the map");
        }
    }

    public void Update() {
        float lerpTime = Time.deltaTime * 3.0f;
        Vector3 target = IsInEditor() ? editorCamera : playCamera;
        Quaternion rotation = IsInEditor() ? editorRotation : playRotation;
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, target, lerpTime);
        Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, rotation, lerpTime);
    }

    public void SetRunning(bool value) => state = value ? State.Playing : State.Stopped;
    public bool IsRunning() => state == State.Playing;
    public bool IsStopped() => state == State.Stopped;
    public bool IsInEditor() => state == State.Editor;


    /// <summary>
    /// Checks whether the given location is within the bounds of the editor.
    /// </summary>
    /// <param name="location">The location that needs checking</param>
    /// <returns>Whether the location is within the editor's bounds</returns>
    public bool ValidLocation(Vector3 location) {
        return editorBounds.Contains(location);
    }

    /// <summary>
    /// Iterates through each player and determines whether they have reached an exit. Traditionally called by a Node
    /// when it has no child.
    /// </summary>
    public void WinConditionHandling() {
        bool hasWon = true;
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player")) {
            PlayerController component = player.GetComponent<PlayerController>();
            if (map.map[component.coordinateX, component.coordinateY] != MapController.END_TILE) hasWon = false;
        }

        if (hasWon) LevelManager.GoToMainMenu();
    }

    /// <summary>
    /// Handles node deletion and removal from the nodes array
    /// </summary>
    /// <param name="node">The node that needs to be removed</param>
    public void RemoveNode(GameObject node)
    {
        int index = nodes.FindIndex(i => node.GetInstanceID() == i.GetInstanceID());
        Destroy(node);
        if (index >= 0) nodes.RemoveAt(index);
    }

    private GameObject CreateNodeButton(Block block, Vector3 position) {
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

    private GameObject GenerateTemplate(GameObject node) {
        float xOffset = startNode.transform.localScale.x * 3f;
        GameObject temp = Instantiate(node, editorBounds.center, Quaternion.identity);

        temp.SetActive(false);
        temp.name = "template";

        DestroyImmediate(temp.GetComponent<OnRun>());

        return temp;
    }

    /// <summary>
    /// Adjusts the camera so the editor screen is appropriately setup and the camera is adjusted for portait displays.
    /// </summary>
    private void UpdateCamera() {
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
        xOffset *= 0.9f;
        yOffset *= 0.9f;
        startNode.transform.position = new Vector2(editorCamera.x - xOffset * 0.8f, editorCamera.y + yOffset);
    }

    private void ToggleMode() {
        if (IsStopped()) {
            runButton.gameObject.SetActive(false);
            editButton.GetComponent<Image>().sprite = test;
            foreach (GameObject button in nodeButtons) button.SetActive(true);
            state = State.Editor;
        } else if (IsInEditor()) {
            runButton.gameObject.SetActive(true);
            editButton.GetComponent<Image>().sprite = edit;
            foreach (GameObject button in nodeButtons) button.SetActive(false);
            state = State.Stopped;
        }
    }

    private GameObject CreateCodeBlock(Block block) {
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

    private void StartRun() {
        runButton.onClick.RemoveAllListeners();
        editButton.gameObject.SetActive(false);

        stopButton();

        SetRunning(true);
        StartCoroutine(startNode.Run());
    }

    private void stopRun() {
        StopAllCoroutines();
        foreach (GameObject node in nodes) node.GetComponent<Node>()?.StopAllCoroutines();

        runButton.onClick.RemoveAllListeners();
        playButton();

        SetRunning(false);
        player.Reset();
        map.Reset();
        editButton.gameObject.SetActive(true);
    }

    private void playButton()
    {
        runButton.onClick.AddListener(StartRun);
        Image image = runButton.GetComponent<Image>();
        image.color = playColour;
        image.sprite = play;
    }

    private void stopButton()
    {
        runButton.onClick.AddListener(stopRun);
        Image image = runButton.GetComponent<Image>();
        image.color = stopColour;
        image.sprite = stop;
    }
 }
