using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Comment : DraggableNode
{
    public string comment = "This will not do anything";

    public override string DisplayName() => $"Comment: {comment}";

    public override IEnumerator Run()
    {
        #if UNITY_EDITOR
            Debug.Log(DisplayName());
        #endif

        yield return new WaitForSeconds(SECONDS_PAUSE);

        if (child) {
            yield return StartCoroutine(child.Run());
        } else {
            HandleEnd();
        }  
    }
}
