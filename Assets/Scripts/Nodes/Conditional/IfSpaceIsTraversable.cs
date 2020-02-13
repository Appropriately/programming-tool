public class IfSpaceIsTraversable : Conditional
{

    public override Node NodeFunction()
    {
        int targetX = controller.player.coordinateX;
        int targetY = controller.player.coordinateY;
        switch (controller.player.direction)
        {
            case Direction.Up:
                targetY++;
                break;
            case Direction.Left:
                targetX--;
                break;
            case Direction.Down:
                targetY--;
                break;
            case Direction.Right:
                targetX++;
                break;
        }

        return controller.map.ValidatePosition(targetX, targetY) ? success : failure;
    }
}
