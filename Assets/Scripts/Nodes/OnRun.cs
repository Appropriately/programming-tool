using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnRun : Node
{  
    public override string DisplayName() => "Start";

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
