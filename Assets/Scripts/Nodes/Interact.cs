public class Interact : DraggableNode
{
    public override Node NodeFunction()
    {
        controller.player.Interact();
        return child;
    }
}
