﻿public class Speak : DraggableNode
{
    private static readonly string[] comments = {
        "hint_1", "hint_2", "comment_1"
    };

    public override Node NodeFunction()
    {
        System.Random rand = new System.Random();
        string comment = comments[rand.Next(comments.Length)];
        controller.player.Speak(Localisation.Translate(comment, true));
        return child;
    }
}
