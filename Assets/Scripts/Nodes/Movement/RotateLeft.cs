using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLeft : DraggableNode
{
    public override Node NodeFunction()
    {
        controller.player.RotateLeft();
        return child;
    }
}
