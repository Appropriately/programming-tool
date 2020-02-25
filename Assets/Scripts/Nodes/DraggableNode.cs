using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// A particular type of Node that allows it to be dragged on <c>OnMouseDown</c>,
/// </summary>
[System.Serializable]
public abstract class DraggableNode : Node
{
    /// <summary>
    /// Whether the current node is being dragged or not.
    /// </summary>
    public bool dragging = false;

    /// <summary>
    /// Checks if the node is draggable and whether the game state is corrected.
    /// </summary>
    /// <returns>Whether the particular node is draggable</returns>
    public bool IsDraggable() => controller.IsInEditor() && (controller.IsRunning() is false);

    public void Update() {
        if (dragging) {
            if (IsDraggable() && Input.GetMouseButton(0)) {
                controller.CheckAndUpdateBinIcon(transform.position);
                controller.bin.gameObject.SetActive(true);
                MoveToMouse();
            } else {
                controller.bin.gameObject.SetActive(false);
                if (controller.ValidLocation(transform.position) is false)
                    controller.RemoveNode(gameObject);

                Collider[] colliders = Physics.OverlapSphere(transform.position, 0.5f);
                foreach (Collider collider in colliders) {
                    if (collider.transform.position.Equals(transform.position) is false) {
                        if (collider.gameObject.GetComponent<Node>().Snap(this)) break;
                    }
                }

                SetDragging(false);
            }

        }
    }

    public void OnMouseDown() {
        if (IsDraggable()) {
            if (IsAttached()) Disconnect();
            SetDragging(true);
        }
    }

    /// <summary>
    /// Disconnect the node from its parent and child.
    /// </summary>
    public virtual void Disconnect() {
        Node previousChild = child;
        Node previousParent = parent;

        if (parent) {
            parent = parent.child = null;
            previousParent.HandleMissingNodes();
        }
        if (child) {
            child = child.parent = null;
            previousChild.HandleMissingNodes();
        }

        if (previousChild && previousParent) previousParent.Snap(previousChild);
    }

    /// <summary>
    /// Sets the current node's dragging value, while also handling the game controller's variable.
    /// </summary>
    /// <param name="state">Whether dragging should be true or false</param>
    public void SetDragging(bool state)
    {
        dragging = state;
        controller.isDragging = state;
    }

    /// <summary>
    /// Change the position of the Node to the mouse's X and Y position, while keeping the Z coordinate the same.
    /// </summary>
    public void MoveToMouse()
    {
        float distance_to_screen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen);
        transform.position = Camera.main.ScreenToWorldPoint(mousePosition);
    }
}