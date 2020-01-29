using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public enum Block
{
    Move,
    RotateRight,
    RotateLeft
}
public abstract class Node : MonoBehaviour
{
    public GameController controller;
    public Node child, parent;

    public const float SECONDS_PAUSE = 1.0f;

    /// <summary>
    /// Performs the Node specific functionality.
    /// </summary>
    public abstract IEnumerator Run();

    /// <summary>
    /// Returns a friendlier name for the particular node.
    /// </summary>
    /// <returns>A String representation of the particular Node</returns>
    public abstract string DisplayName();

    public virtual void Start() {
        if (controller is null) {
            controller = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<GameController>();
        }
    }

    public void HandleEnd() => controller.WinConditionHandling();
    public bool IsAttached() => child || parent;
}