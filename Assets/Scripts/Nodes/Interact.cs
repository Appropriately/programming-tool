using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact : DraggableNode
{
    public override Node NodeFunction()
    {
        controller.player.Interact();
        return child;
    }
}
