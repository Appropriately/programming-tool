using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateRight : DraggableNode
{
    public override IEnumerator Run()
    {
        #if UNITY_EDITOR
            Debug.Log(DisplayName());
        #endif

        controller.player.RotateRight();

        yield return new WaitForSeconds(SECONDS_PAUSE);

        if (child) {
            yield return StartCoroutine(child.Run());
        } else {
            HandleEnd();
        }
    }
}
