using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

            Image binImage = controller.bin.GetComponent<Image>();
            binImage.sprite = controller.ValidLocation(transform.position) ? controller.binClosed : controller.binOpen;
            controller.bin.gameObject.SetActive(true);

            float distance_to_screen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen);
            transform.position = Camera.main.ScreenToWorldPoint(mousePosition);
        }
    }

    /// <summary>
    /// Handle connecting a node once it is no longer being dragged about.
    /// </summary>
    public void OnMouseUp() {
        controller.bin.gameObject.SetActive(false);
        if (IsDraggable() && !controller.IsRunning()) {
            if (controller.ValidLocation(transform.position) is false) controller.RemoveNode(gameObject);

            Collider[] closeColliders = Physics.OverlapSphere(gameObject.transform.position, 1.0f);
            foreach (Collider collider in closeColliders) {
                if (!collider.transform.position.Equals(transform.position)) {
                    if (collider.gameObject.GetComponent<Node>().Snap(this)) return;
                }
            }
        }
    }

    /// <summary>
    /// Disconnect the node from its parent and child.
    /// </summary>
    public virtual void Disconnect() {
        Node previousChild = child;
        Node previousParent = parent;

        if (parent) parent = parent.child = null;
        if (child) child = child.parent = null;
        if (previousChild && previousParent) previousParent.Snap(previousChild);

        controller?.NodeCheck();
    }
}