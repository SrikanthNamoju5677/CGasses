using Godot;
using System;
using System.Collections.Generic;

public partial class Screen : CanvasLayer
{
    [Signal]
    public delegate void DoChangeSceneEventHandler(String screenName);

    public virtual void init(List<object> arguments)
    {
        return;
    }
}
