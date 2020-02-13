using UnityEngine;

[System.Serializable]
public abstract class Loop : DraggableNode
{
    public Node loop;

    private enum Option
    {
        Child,
        Loop
    }

    public override bool Snap(Node node)
    {
        #if UNITY_EDITOR
            Debug.Log($"Trying to connect {node.DisplayName()} to {DisplayName()}");
        #endif

        if (node is null || node.parent) return false;

        Option option = GetOptionFromPosition(node);
        if (option == Option.Child && child) return false;
        if (option == Option.Loop && loop) return false;

        float offset = transform.localScale.x * 0.2f;
        float x = transform.position.x + (option == Option.Loop ? offset : 0);
        Debug.Log($"{loop?.GetLowestNode().DisplayName()}");
        float y = (loop is null) ? gameObject.transform.position.y : loop.GetLowestNode().transform.position.y;
        node.gameObject.transform.position = new Vector3(x, y - gameObject.transform.localScale.y);

        node.parent = this;
        if (option == Option.Loop) {
            loop = node;
        } else {
            child = node;
        }

        if (node.child) {
            Node originalChild = node.child;
            node.child = node.child.parent = null;
            return node.Snap(originalChild);
        }

        return true;
    }

    public override void Disconnect()
    {
        Node previousLoop = loop;
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
        if (loop) {
            loop = loop.parent = null;
            previousLoop.HandleMissingNodes();
        }

        if (previousChild && previousParent) previousParent.Snap(previousChild);
    }

    public override void HandleMissingNodes()
    {
        if (child && child.parent is null) child = null;
        if (loop && loop.parent is null) loop = null;
    }

    public void UpdateSize()
    {
        // TODO: Update the size of the node based on the number of items in the loop
    }

    private Option GetOptionFromPosition(Node node)
    {
        return transform.position.x - node.transform.position.x >= 0 ? Option.Child : Option.Loop;
    }
}