using System.Collections;
using UnityEngine;

public class IfSpaceIsTraversable : Conditional
{
    public override IEnumerator Run()
    {
        #if UNITY_EDITOR
            Debug.Log(DisplayName());
        #endif

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

        Node next = controller.map.ValidatePosition(targetX, targetY) ? success : failure;

        yield return new WaitForSeconds(SECONDS_PAUSE);

        if (next) {
            yield return StartCoroutine(next.Run());
        } else {
            HandleEnd();
        }
    }
}
