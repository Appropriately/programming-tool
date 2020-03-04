public class Move : DraggableNode
{
    public override Node NodeFunction()
    {
        controller.player.MoveForward();
        return child;
    }
}
