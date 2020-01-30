using System;
using System.Collections;
using UnityEngine;

public class Speak : DraggableNode
{
    private static readonly string[] comments = {
        "hint_1", "hint_2", "comment_1"
    };

    public override IEnumerator Run()
    {
        System.Random rand = new System.Random();
        string comment = comments[rand.Next(comments.Length)];

        #if UNITY_EDITOR
            Debug.Log($"{DisplayName()}, comment = {comment}");
        #endif

        controller.player.Speak(comment);

        yield return new WaitForSeconds(SECONDS_PAUSE);

        if (child) {
            yield return StartCoroutine(child.Run());
        } else {
            HandleEnd();
        }
    }
}
