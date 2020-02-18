using System.Collections;
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
    public Direction direction;
    public int coordinateX, coordinateY;

    private GameObject startTile;
    private Vector3 targetPosition;

    public void Start() {
        transform.localScale *= (MapController.Scale() * 0.75f);
    }

    public void Setup() {
        if (controller is null) {
            controller = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<GameController>();
        }
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
        int targetX = coordinateX;
        int targetY = coordinateY;
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
        if (controller.map.ValidatePosition(targetX, targetY)) {
            targetPosition = targetPosition + (DirectionToVector(direction) * MapController.Scale());
            coordinateX = targetX;
            coordinateY = targetY;
        } else {
            #if UNITY_EDITOR
                Debug.Log($"Position [{targetX},{targetY}] is not a valid position");
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
    /// Figure out the char that the player is currently at.
    /// </summary>
    /// <returns>The char representation of the tile</returns>
    public char Tile() => controller.map.map[coordinateX, coordinateY];

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
        Debug.Log($"{controller}, {coordinateX}, {coordinateY}");
        if (controller.map.ValidatePosition(coordinateX + 1, coordinateY)) {
            return Direction.Right;
        } else if (controller.map.ValidatePosition(coordinateX - 1, coordinateY)) {
            return Direction.Left;
        } else if (controller.map.ValidatePosition(coordinateX, coordinateY - 1)) {
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
}
