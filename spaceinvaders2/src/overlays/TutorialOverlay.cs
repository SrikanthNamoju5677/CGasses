using Godot;
using SpaceInvaders;

public partial class TutorialOverlay : CanvasLayer
{
    Color startColor = Color.Color8(255, 255, 255, 255);
    Color endColor = Color.Color8(255, 255, 255, 0);
    Tween startTween;

    public override void _Ready()
    {
        if (PlayerHandler.GameMode == GameMode.Multiplayer)
        {
            GetNode<Node2D>("FirstPlayerControls").MoveLocalX(-175);
            GetNode<Label>("FirstPlayerControls/Shoot").HorizontalAlignment = Godot.HorizontalAlignment.Center;
            GetNode<Label>("FirstPlayerControls/Move").HorizontalAlignment = Godot.HorizontalAlignment.Center;
            GetNode<Label>("FirstPlayerControls/Weapon").HorizontalAlignment = Godot.HorizontalAlignment.Center;
            GetNode<Node2D>("SecondPlayerControls").Show();
        }

        startTween = CreateTween();
        var node = GetNode<Node2D>("StartNode");
        startTween.TweenProperty(node, "modulate", endColor, 0.7f);
        startTween.TweenProperty(node, "modulate", startColor, 0.7f);
        startTween.SetLoops();
    }
}
