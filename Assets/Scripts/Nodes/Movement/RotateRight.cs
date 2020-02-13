using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateRight : DraggableNode
{
    public override Node NodeFunction()
    {
        controller.player.RotateRight();
        return child;
    }
}
