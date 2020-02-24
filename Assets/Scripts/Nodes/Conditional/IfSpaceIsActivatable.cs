using System;

public class IfSpaceIsActivatable : Conditional
{

    public override Node NodeFunction()
    {
        var (x, y) = controller.player.FrontCoordinates();
        return controller.map.IsButton(x, y) ? success : failure;
    }
}
