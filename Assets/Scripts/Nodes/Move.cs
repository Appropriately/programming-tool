using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : DraggableNode
{
    public override IEnumerator Run()
    {
        #if UNITY_EDITOR
            Debug.Log(DisplayName());
        #endif

        controller.player.MoveForward();

        yield return new WaitForSeconds(SECONDS_PAUSE);

        if (child) {
            yield return StartCoroutine(child.Run());
        } else {
            HandleEnd();
        }
    }
}
