using Godot;

public partial class PauseScreen : Screen
{
    public override void _Ready()
    {
        var resumeButton = GetNode<Button>("ResumeButton");
        resumeButton.Pressed += SetPaused;

        var homeButton = GetNode<Button>("HomeButton");
        homeButton.Pressed += ReturnToHome;
    }

    private void SetPaused()
    {
        GetTree().Paused = !GetTree().Paused;
        this.Visible = !this.Visible;

        MessageWithTimer messageWithtimer = this.GetParent().GetNode<MessageWithTimer>("MessageWithTimer");
        messageWithtimer.Visible = !messageWithtimer.Visible;
    }

    private void ReturnToHome()
    {
        SetPaused();
        this.GetParent().EmitSignal(nameof(DoChangeScene), "HomeScreen");
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Pause"))
        {
            SetPaused();
        }
    }
}
