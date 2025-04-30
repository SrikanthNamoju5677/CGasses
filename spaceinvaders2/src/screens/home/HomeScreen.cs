using Godot;
using System.Threading.Tasks;

public partial class HomeScreen : Screen
{
    private AudioStreamPlayer2D selectSound;
    public override void _Ready()
    {
        selectSound = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        var buttonPlayGame = GetNode<Button>("PlayGame");
        buttonPlayGame.Pressed += this.DisplayGameModes;

        var buttonLeaderBoard = GetNode<Button>("LeaderBoard");
        buttonLeaderBoard.Pressed += this.DisplayLeaderBoards;

        var musicPlayer = GetNode<MusicManager>("/root/MusicManager");
        musicPlayer.StartMusic("home_bgm");

        Button exit = GetNode<Button>("Exit");
        exit.Pressed += ExitGame;
    }

    private async void DisplayGameModes()
    {
        selectSound.Play();
        while (selectSound.Playing)
        {
            await Task.Delay(1);
        }
        this.EmitSignal(nameof(DoChangeScene), "GameModeMenu");
    }

    private async void DisplayLeaderBoards()
    {
        selectSound.Play();
        while (selectSound.Playing)
        {
            await Task.Delay(1);
        }
        this.EmitSignal(nameof(DoChangeScene), "LeaderBoardMenu");
    }

    private async void ExitGame()
    {
        selectSound.Play();
        while (selectSound.Playing)
        {
            await Task.Delay(1);
        }
        GetTree().Quit();
    }
}
