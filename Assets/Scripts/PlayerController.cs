using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Direction {
    Up,
    Left,
    Down,
    Right
}

public class PlayerController : MonoBehaviour
{
    private const float MOVEMENT_SPEED = 3.0f;

    public GameController controller;
    public MapController mapController;
    public Direction direction;
    public int coordinateX, coordinateY;

    private GameObject startTile;
    private Vector3 targetPosition;

    private Dictionary<char, char> activateChar = new Dictionary<char, char>() { {'1','A'}, {'2','B'} };

    public void Start() => transform.localScale *= (MapController.Scale() * 0.75f);

    public void Setup() {
        if (controller is null) {
            controller = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<GameController>();
        }
        if (mapController is null) mapController = controller.gameObject.GetComponent<MapController>();

        startTile = GameObject.FindGameObjectWithTag("Respawn");
        Reset();
    }

    public void Update() {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * MOVEMENT_SPEED);
    }

    public void Reset() {
        targetPosition = startTile.transform.position - (Vector3.forward * 0.2f);
        gameObject.transform.position = targetPosition;
        direction = StartDirection();
    }

    public void MoveForward() {
        var (x, y) = FrontCoordinates();
        if (mapController.IsTraversable(x, y)) {
            targetPosition = targetPosition + (DirectionToVector(direction) * MapController.Scale());
            coordinateX = x;
            coordinateY = y;
        } else {
            #if UNITY_EDITOR
                Debug.Log($"Position [{x},{y}] is not a valid position");
            #endif
        }
    }

    public void RotateRight() {
        switch(direction)
        {
            case Direction.Up:
                direction = Direction.Right;
                break;
            case Direction.Left:
                direction = Direction.Up;
                break;
            case Direction.Down:
                direction = Direction.Left;
                break;
            case Direction.Right:
                direction = Direction.Down;
                break;
        }
    }

    public void RotateLeft() {
        switch(direction)
        {
            case Direction.Up:
                direction = Direction.Left;
                break;
            case Direction.Left:
                direction = Direction.Down;
                break;
            case Direction.Down:
                direction = Direction.Right;
                break;
            case Direction.Right:
                direction = Direction.Up;
                break;
        }
    }

    /// <summary>
    /// Causes the player to 'speak', used for giving hints and providing information.
    /// </summary>
    /// <param name="text">What the player will say</param>
    /// <param name="duration">How long (in seconds) the message will last for</param>
    public void Speak(string text, int duration = 5) => StartCoroutine(SpeakCoroutine(text, duration));

    /// <summary>
    /// "Interacts" with the space in front of the player.
    /// If the space contains a button, the corresponding tiles are made traversable.
    /// </summary>
    public void Interact()
    {
        var (x, y) = FrontCoordinates();
        if (mapController.IsButton(x, y)) {
            mapController.activated.Add(activateChar[mapController.map[x,y]]);
            foreach (GameObject obj in mapController.activatable) {
                if (mapController.activated.Contains(obj.GetComponent<Activatable>().type)) obj.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Returns the <c>char</c> at the position the player is currently registered at.
    /// </summary>
    /// <returns>The <c>char</c> representation of the tile</returns>
    public char Tile() => mapController.map[coordinateX, coordinateY];

    /// <summary>
    /// Returns the X and Y coordinates ahead of the player's current position
    /// </summary>
    /// <returns>The (x, y) coordinates</returns>
    public (int, int) FrontCoordinates() => CoordinatesAhead(coordinateX, coordinateY);

    private IEnumerator SpeakCoroutine(string text, int duration)
    {
        #if UNITY_EDITOR
            Debug.Log($"Player said {text}");
        #endif

        controller.alert.GetComponentInChildren<Text>().text = text;
        controller.alert.SetActive(true);

        yield return new WaitForSeconds(duration);

        controller.alert.SetActive(false);
    }

    private Direction StartDirection()
    {
        if (mapController.IsTraversable(coordinateX + 1, coordinateY)) {
            return Direction.Right;
        } else if (mapController.IsTraversable(coordinateX - 1, coordinateY)) {
            return Direction.Left;
        } else if (mapController.IsTraversable(coordinateX, coordinateY - 1)) {
            return Direction.Down;
        } else {
            return Direction.Up;
        }
    }

    private Vector3 DirectionToVector(Direction direction) {
        switch (direction)
        {
            case Direction.Up:
                return Vector3.up;
            case Direction.Left:
                return Vector3.left;
            case Direction.Down:
                return Vector3.down;
            case Direction.Right:
                return Vector3.right;
            default:
                return Vector3.zero;
        }
    }

    private (int, int) CoordinatesAhead(int x, int y)
    {
        int targetX = x;
        int targetY = y;
        switch (direction)
        {
            case Direction.Up:
                targetY++;
                break;
            case Direction.Left:
                targetX--;
                break;
            case Direction.Down:
                targetY--;
                break;
            case Direction.Right:
                targetX++;
                break;
        }
        return (targetX, targetY);
    }
}
