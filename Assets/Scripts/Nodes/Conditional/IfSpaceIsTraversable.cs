public class IfSpaceIsTraversable : Conditional
{

    public override Node NodeFunction()
    {
        var (x, y) = controller.player.FrontCoordinates();
        return controller.map.IsTraversable(x, y) ? success : failure;
    }
}
