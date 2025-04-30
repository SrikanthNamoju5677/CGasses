using Godot;

public partial class LeaderBoardMenu : Screen
{
    public override void _Ready()
    {
        var btnSPLeaderBoard = GetNode<Button>("SinglePlayerLeaderBoard");
        btnSPLeaderBoard.Pressed += this.SinglePlayerLeaderBoard;

        var btnMPLeaderBoard = GetNode<Button>("MultiplayerLeaderBoard");
        btnMPLeaderBoard.Pressed += this.MultiplayerLeaderBoard;

        var btnCoOpAILeaderBoard = GetNode<Button>("CooperativeAILeaderBoard");
        btnCoOpAILeaderBoard.Pressed += this.CooperativeAILeaderBoard;

        var btnSurvivalModeLeaderBoard = GetNode<Button>("SurvivalModeLeaderBoard");
        btnSurvivalModeLeaderBoard.Pressed += this.SurvivalModeLeaderBoard;

        Button previousMenu = GetNode<Button>("BackLB");
        previousMenu.Pressed += PreviousMenu;

        var musicPlayer = GetNode<MusicManager>("/root/MusicManager");
        musicPlayer.StartMusic("home_bgm");
    }

    private void SinglePlayerLeaderBoard()
    {
        this.EmitSignal(nameof(DoChangeScene), "SinglePlayerLeaderBoard");
    }

    private void MultiplayerLeaderBoard()
    {
        this.EmitSignal(nameof(DoChangeScene), "MultiplayerLeaderBoard");
    }

    private void CooperativeAILeaderBoard()
    {
        this.EmitSignal(nameof(DoChangeScene), "CooperativeAILeaderBoard");
    }

    private void SurvivalModeLeaderBoard()
    {
        this.EmitSignal(nameof(DoChangeScene), "SurvivalModeLeaderBoard");
    }

    private void PreviousMenu()
    {
        this.EmitSignal(nameof(DoChangeScene), "HomeScreen");
    }
}
