using System.Collections;
using System.Collections.Generic;
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

    public override void HandleMissingConnections()
    {
        if (parent && parent.child is null) parent = null;
        if (success && success.parent is null) child = null;
        if (failure && failure.parent is null) child = null;
    }

    public override bool Snap(Node node)
    {
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

        if (parent) parent = parent.child = null;
        if (success) success = success.parent = null;
        if (failure) failure = failure.parent = null;
        if (previousSuccess && previousParent) previousParent.Snap(previousSuccess);
        if (previousFailure && previousParent) previousParent.Snap(previousFailure);

        controller?.NodeCheck();
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