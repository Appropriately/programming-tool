using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// An object that can have a child and/or a parent, that allows other <c>Node</c>s to <c>Snap</c> to it.
/// </summary>
[System.Serializable]
public abstract class Node : MonoBehaviour
{
    public GameController controller;
    public Node child, parent;

    /// <summary>
    /// Performs the Node specific functionality, calling upon the NodeFunction function which has specific
    /// functionality depending on the node.
    /// </summary>
    public IEnumerator Run()
    {
        #if UNITY_EDITOR
            Debug.Log($"{DisplayName}; ID = {gameObject.GetInstanceID()}");
        #endif

        Node node = NodeFunction();
        yield return new WaitForSeconds(0.5f);

        if (node is null) {
            node = IsInLoop();
            if (node is null) {
                controller.WinConditionHandling();
            } else {
                yield return StartCoroutine(node.Run());
            }
        } else {
            yield return StartCoroutine(node.Run());
        }
    }

    /// <summary>
    /// Functionality unique to a particular node, that returns the next node to progress to.
    /// </summary>
    /// <returns>The next node that will be run</returns>
    public virtual Node NodeFunction() => child;

    /// <summary>
    /// Returns a friendlier name for the particular node, based the object's <c>Type</c>.
    /// </summary>
    /// <returns>
    /// A String representation of the particular Node
    /// </returns>
    /// <example>
    /// <code>
    /// Node node = this;
    /// node.DisplayName(); // "Node"
    /// </code>
    /// </example>
    public string DisplayName => Regex.Replace(GetType().Name, "([a-z])([A-Z])", "$1 $2");

    public virtual void Start() {
        if (controller is null) {
            controller = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<GameController>();
        }
    }

    /// <summary>
    /// <c>Snap</c>s the given node to this node as a child.
    /// Handles any instances where the node should not be snapping due to pre-existing connections.
    /// </summary>
    /// <param name="node">The node that will be the new child</param>
    /// <returns>Whether the process was successful or not</returns>
    public virtual bool Snap(Node node)
    {
        #if UNITY_EDITOR
            Debug.Log($"Trying to connect {node.DisplayName} to {DisplayName}");
        #endif

        if (node is null || node.parent || child) return false;

        float y = gameObject.transform.position.y - gameObject.transform.localScale.y;
        node.gameObject.transform.position = new Vector3(gameObject.transform.position.x, y);

        node.parent = this;
        child = node;

        if (node.child) {
            Node originalChild = node.child;
            node.child = node.child.parent = null;
            return node.Snap(originalChild);
        }

        return true;
    }

    /// <summary>
    /// Returns the 'lowest' node.
    /// </summary>
    /// <returns>The node which is at the lowest physical point</returns>
    public virtual Node GetLowestNode() => child is null ? this : child.GetLowestNode();

    /// <summary>
    /// Checks to see if there are any disconnected nodes that need their relationships severed.
    /// </summary>
    public virtual void HandleMissingNodes()
    {
        if (child && child.parent is null) child = null;
    }

    /// <summary>
    /// Returns the count, starting from this <c>Node</c>.
    /// </summary>
    public virtual int Count => 1 + (child ? child.Count : 0);

    public bool IsAttached => child || parent;

    /// <summary>
    /// Determines whether the current node is indented in some form of <c>Loop</c>.
    /// </summary>
    /// <returns>
    /// The <c>Loop</c> node or <c>null</c>.
    /// </returns>
    public Loop IsInLoop()
    {
        if (parent) {
            if (parent.GetType().IsSubclassOf(typeof(Loop))) {
                Loop cast = (Loop) parent;
                return GetInstanceID() == cast.loop.GetInstanceID() ? cast : cast.IsInLoop();
            } else {
                return parent.IsInLoop();
            }
        }
        return null;
    }
}