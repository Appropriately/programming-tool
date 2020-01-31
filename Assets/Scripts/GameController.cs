using System;
using System.Collections.Generic;
using UnityEngine;
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

    public Button nodeButton;
    public GameObject alert;

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
            Vector3 position = nodeButton.gameObject.GetComponent<RectTransform>().position;
            List<GameObject> list = new List<GameObject>();
            foreach (Block block in LevelManager.GetBlocks())
            {
                list.Add(CreateNodeButton(block, position));
                position -= new Vector3(0, nodeButton.gameObject.GetComponent<RectTransform>().sizeDelta.y * 1.2f);
            }
            nodeButtons = list.ToArray();

            playButton();
            editButton.onClick.AddListener(ToggleMode);
        }
        catch (Exception e)
        {
            #if UNITY_EDITOR
                Debug.LogError($"{e} Exception caught.");
            #endif
            LevelManager.ErrorToMainMenu("There was an issue generating the map");
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
    /// Do a pass of all nodes and ensure that there are no unnecessary connections between nodes.
    /// </summary>
    public void NodeCheck() {
        foreach (GameObject node in nodes) {
            Node script = node.GetComponent<Node>();
            if (script) {
                if (script.parent && !script.parent.child) {
                    script.parent = null;
                } else if (script.child && !script.child.parent) {
                    script.child = null;
                }
            }
        }
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
        GameObject button = Instantiate(nodeButton.gameObject, position, Quaternion.identity);
        button.name = block.ToString();
        button.GetComponentInChildren<Text>().text = block.ToString();

        button.GetComponent<Button>().onClick.AddListener(delegate { DrawCodeBlock(block); });

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
        Vector3 originalPosition = Camera.main.transform.position;
        editorCamera = originalPosition + new Vector3(0, Camera.main.orthographicSize * 2.0f);
        editorRotation = Camera.main.transform.rotation;

        playCamera = Camera.main.transform.position + new Vector3(8.0f, -8.0f);
        Camera.main.transform.position = playCamera;
        Camera.main.transform.LookAt(Vector3.zero);
        playRotation = Camera.main.transform.rotation;
        Debug.Log(playRotation);

        float xOffset = Camera.main.aspect * Camera.main.orthographicSize * 0.8f;
        float yOffset = Camera.main.orthographicSize * 0.8f;
        editorBounds = Rect.MinMaxRect(
            editorCamera.x - xOffset, editorCamera.y - yOffset, editorCamera.x + xOffset, editorCamera.y + yOffset
        );
        xOffset *= 0.9f;
        yOffset *= 0.9f;
        startNode.transform.position = new Vector2(editorCamera.x - xOffset * 0.9f, editorCamera.y + yOffset);

        if (Camera.main.aspect < 1.0f) Camera.main.orthographicSize = 10.0f;
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

    private void DrawCodeBlock(Block block) {
        GameObject node = Instantiate(template);
        node.name = block.ToString();
        node.GetComponentInChildren<TextMesh>().text = block.ToString().ToLower();
        node.SetActive(true);

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
        }

        nodes.Add(node);
    }

    private void StartRun() {
        runButton.onClick.RemoveAllListeners();
        editButton.gameObject.SetActive(false);
        NodeCheck();

        stopButton();

        SetRunning(true);
        StartCoroutine(startNode.Run());
    }

    private void stopRun() {
        StopAllCoroutines();

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
