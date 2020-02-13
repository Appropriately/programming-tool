using UnityEngine;

[System.Serializable]
public abstract class Conditional : DraggableNode
{
    public Node success, failure;

    private enum Condition
    {
        Success,
        Failure
    }

    public override bool Snap(Node node)
    {
        #if UNITY_EDITOR
            Debug.Log($"Trying to connect {node.DisplayName()} to {DisplayName()}");
        #endif

        Condition condition = GetConditionFromPosition(node);
        if (node is null || node.parent || child) return false;

        if (condition == Condition.Success && success) return false;
        if (condition == Condition.Failure && failure) return false;

        float offset = transform.localScale.x * 0.25f;
        float x = transform.position.x + (condition == Condition.Success ? -offset : offset);
        float y = transform.position.y - transform.localScale.y;
        node.transform.position = new Vector3(x, y);

        node.parent = this;
        if (condition == Condition.Success) {
            success = node;
        } else {
            failure = node;
        }

        if (node.child) {
            Node nodeChild = node.child;
            node.child = node.child.parent = null;
            return node.Snap(nodeChild);
        }

        return true;
    }

    public override void Disconnect()
    {
        Node previousSuccess = success;
        Node previousFailure = failure;
        Node previousParent = parent;

        if (parent) {
            parent = parent.child = null;
            previousParent.HandleMissingNodes();
        }
        if (success) {
            success = success.parent = null;
            previousSuccess.HandleMissingNodes();
        }
        if (failure) {
            failure = failure.parent = null;
            previousFailure.HandleMissingNodes();
        }

        if (previousSuccess && previousParent) previousParent.Snap(previousSuccess);
        if (previousFailure && previousParent) previousParent.Snap(previousFailure);
    }

    public override Node GetLowestNode()
    {
        Node lowestSuccess = success?.GetLowestNode();
        Node lowestFailure = failure?.GetLowestNode();

        if (lowestSuccess is null && lowestFailure is null) {
            return null;
        } else if (lowestSuccess is null) {
            return failure;
        } else if (lowestFailure is null) {
            return success;
        } else {
            return lowestSuccess.transform.position.y <= lowestFailure.transform.position.y ? success : failure;
        }
    }

    public override void HandleMissingNodes()
    {
        if (success && success.parent is null) success = null;
        if (failure && failure.parent is null) failure = null;
    }

    /// <summary>
    /// Given some node, figure out based on it's position whether it is a success or failure node.
    /// </summary>
    /// <param name="node">The node that will be compared against.</param>
    /// <returns>The appropriate Condition</returns>
    private Condition GetConditionFromPosition(Node node)
    {
        return transform.position.x - node.transform.position.x >= 0 ? Condition.Success : Condition.Failure;
    }
}