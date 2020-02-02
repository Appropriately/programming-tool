using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class DraggableNode : Node
{
    public bool draggable = true;

    /// <summary>
    /// Checks if the node is draggable and whether the game state is corrected.
    /// </summary>
    /// <returns>Whether the particular node is draggable</returns>
    public bool IsDraggable() => draggable && controller.IsInEditor();

    /// <summary>
    /// Handle moving the node about.
    /// </summary>
    public void OnMouseDrag() {
        if (IsDraggable() && !controller.IsRunning()) {
            if (IsAttached()) Disconnect();

            float distance_to_screen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen);
            transform.position = Camera.main.ScreenToWorldPoint(mousePosition);
        }
    }

    /// <summary>
    /// Handle connecting a node once it is no longer being dragged about.
    /// </summary>
    public void OnMouseUp() {
        if (IsDraggable() && !controller.IsRunning()) {
            if (controller.ValidLocation(transform.position) is false) controller.RemoveNode(gameObject);

            Collider[] closeColliders = Physics.OverlapSphere(gameObject.transform.position, 1.0f);
            foreach (Collider collider in closeColliders) {
                if (!collider.transform.position.Equals(transform.position)) {
                    if (PerformSnap(collider.gameObject.GetComponent<Node>(), this)) return;
                }
            }
        }
    }

    private void Disconnect() {
        Node previousChild = child;
        Node previousParent = parent;

        if (parent) parent = parent.child = null;
        if (child) child = child.parent = null;
        if (previousChild && previousParent) PerformSnap(previousParent, previousChild);

        controller?.NodeCheck();
    }

    private bool PerformSnap(Node top, Node bottom) {
        if (!top || !bottom || top.child) return false;

        float xCoordinate = top.gameObject.transform.position.x;
        float yCoordinate = top.gameObject.transform.position.y - gameObject.transform.localScale.y;
        Vector3 newPosition = new Vector3(xCoordinate, yCoordinate);
        bottom.transform.position = newPosition;

        top.child = bottom;
        bottom.parent = top;

        if (bottom.child) {
            Node nodeOne = bottom;
            Node nodeTwo = bottom.child;
            bottom.child = bottom.child.parent = null;
            return PerformSnap(nodeOne, nodeTwo);
        }

        return true;
    }
}