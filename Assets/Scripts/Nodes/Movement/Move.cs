using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : DraggableNode
{
    public override Node NodeFunction()
    {
        controller.player.MoveForward();
        return child;
    }
}
