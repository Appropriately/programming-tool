using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLeft : DraggableNode
{
    public override string DisplayName() => "Rotate Left";

    public override IEnumerator Run()
    {
        #if UNITY_EDITOR
            Debug.Log(DisplayName());
        #endif

        controller.player.RotateLeft();

        yield return new WaitForSeconds(SECONDS_PAUSE);

        if (child) {
            yield return StartCoroutine(child.Run());
        } else {
            HandleEnd();
        }
    }
}
