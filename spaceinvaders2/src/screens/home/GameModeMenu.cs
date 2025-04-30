using System.Threading.Tasks;
using Godot;

public partial class GameModeMenu : Screen
{
    private AudioStreamPlayer2D selectSound;
    public override void _Ready()
    {
        selectSound = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");

        var buttonStartGame = GetNode<Button>("SinglePlayerGame");
        buttonStartGame.Pressed += this.SinglePlayerGame;

        var buttonStartMultiplayerGame = GetNode<Button>("MultiPlayerGame");
        buttonStartMultiplayerGame.Pressed += this.MultiplayerGame;

        var buttonStartCooperativeAIGame = GetNode<Button>("CooperativeAIGame");
        buttonStartCooperativeAIGame.Pressed += this.CooperativeAIGame;

        var buttoSurvivalModeGame = GetNode<Button>("SurvivalModeGame");
        buttoSurvivalModeGame.Pressed += this.SurvivalModeGame;

        Button back = GetNode<Button>("BackGM");
        back.Pressed += this.PreviousMenu;
    }

    private async void SinglePlayerGame()
    {
        selectSound.Play();
        while (selectSound.Playing)
        {
            await Task.Delay(1);
        }
        this.EmitSignal(nameof(DoChangeScene), "SinglePlayerGame");
    }

    private async void MultiplayerGame()
    {
        selectSound.Play();
        while (selectSound.Playing)
        {
            await Task.Delay(1);
        }
        this.EmitSignal(nameof(DoChangeScene), "MultiplayerGame");
    }

    private async void CooperativeAIGame()
    {
        selectSound.Play();
        while (selectSound.Playing)
        {
            await Task.Delay(1);
        }
        this.EmitSignal(nameof(DoChangeScene), "CooperativeAIGame");
    }

    private async void SurvivalModeGame()
    {
        selectSound.Play();
        while (selectSound.Playing)
        {
            await Task.Delay(1);
        }
        this.EmitSignal(nameof(DoChangeScene), "SurvivalModeGame");
    }

    private async void PreviousMenu()
    {
        selectSound.Play();
        while (selectSound.Playing)
        {
            await Task.Delay(1);
        }
        this.EmitSignal(nameof(DoChangeScene), "HomeScreen");
    }
}
