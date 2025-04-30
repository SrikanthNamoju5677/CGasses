using Godot;

public partial class TutorialLevel : Level
{
    public PackedScene packedSceneTutorialLevel = (PackedScene)GD.Load("res://src/overlays/TutorialOverlay.tscn");
    private CanvasLayer tutorial;
    private bool TutorialFinished = false;

    public override string StartLevelMessage { get => "Your software code is being attacked by evil bugs!\n Kill these invaders with your programming skills and earn your salary as a developer!"; }

    protected override int BonusScoreTimeoutSeconds => 0;

    public override bool CheckWin()
    {
        //the level is won when the tutorial has finished
        return TutorialFinished;
    }

    public override void Cleanup()
    {
        return;
    }

    public override void Setup()
    {
        this.tutorial = (CanvasLayer)this.packedSceneTutorialLevel.Instantiate();
        this.AddChild(this.tutorial);
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_skip"))
        {
            TutorialFinished = true;
        }
        base._Process(delta);
    }
}
