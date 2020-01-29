using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private const float MOVEMENT_SPEED = 3.0f;

    public enum Direction {
        Up,
        Left,
        Down,
        Right
    }

    public GameController controller;
    public Direction direction = Direction.Up;
    public int coordinateX, coordinateY;
    
    private GameObject startTile;
    private Vector3 targetPosition;

    public void Start() {
        transform.localScale *= (MapController.Scale() * 0.75f);
    }

    public void Setup() {
        startTile = GameObject.FindGameObjectWithTag("Respawn");
        Reset();
        if (controller is null) {
            controller = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<GameController>();
        }
    }

    public void Update() {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * MOVEMENT_SPEED);
    }

    public void Reset() {
        direction = Direction.Up;
        targetPosition = startTile.transform.position - Vector3.forward;
        gameObject.transform.position = targetPosition;
    }

    public void SetPosition(Vector3 position) => targetPosition = position;

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
