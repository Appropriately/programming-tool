using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Node startNode;
    public Button startButton, editButton, nodeButton;

    private GameObject[] nodeButtons;
    private List<GameObject> nodes;
    private Rect editorBounds;

    private GameObject template;
    public PlayerController player;
    public MapController map;

    private Vector3 playerOriginalPosition;
    private Vector3 cameraPlayPosition;
    private Vector3 editorCamera;

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

            startButton.onClick.AddListener(StartRun);
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
        Vector3 target = IsInEditor() ? editorCamera : cameraPlayPosition;
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, target, Time.deltaTime * 3.0f);
    }

    public void SetRunning(bool value) => state = value ? State.Playing : State.Stopped;
    public bool IsRunning() => state == State.Playing;
    public bool IsStopped() => state == State.Stopped;
    public bool IsInEditor() => state == State.Editor;


    public Vector3 CodeBoundsCentre() {
        return editorBounds.center;
    }

    public bool ValidLocation(Vector3 location) {
        return editorBounds.Contains(location);
    }

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
        cameraPlayPosition = Camera.main.transform.position;
        editorCamera = cameraPlayPosition + new Vector3(0, Camera.main.orthographicSize * 2.0f);

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
            startButton.gameObject.SetActive(false);
            editButton.GetComponentInChildren<Text>().text = "Test";
            foreach (GameObject button in nodeButtons) button.SetActive(true);
            state = State.Editor;
        } else if (IsInEditor()) {
            startButton.gameObject.SetActive(true);
            editButton.GetComponentInChildren<Text>().text = "Edit";
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
        }

        nodes.Add(node);
    }

    private void StartRun() {
        startButton.onClick.RemoveAllListeners();
        editButton.gameObject.SetActive(false);
        NodeCheck();
        startButton.GetComponentInChildren<Text>().text = "Reset";
        startButton.onClick.AddListener(stopRun);
        SetRunning(true);
        StartCoroutine(startNode.Run());
    }

    public void stopRun() {
        StopAllCoroutines();
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(StartRun);
        startButton.GetComponentInChildren<Text>().text = "Play";
        SetRunning(false);
        player.Reset();
        map.Reset();
        editButton.gameObject.SetActive(true);
    }
 }
