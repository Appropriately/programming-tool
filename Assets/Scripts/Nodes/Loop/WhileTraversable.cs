using UnityEngine;

public class WhileTraversable : Loop
{
    public override Node NodeFunction() {
        var (x, y) = controller.player.FrontCoordinates();
        if (controller.map.IsTraversable(x, y)) {
            return (loop is null) ? this : loop;
        } else {
            return child;
        }
    }
}
