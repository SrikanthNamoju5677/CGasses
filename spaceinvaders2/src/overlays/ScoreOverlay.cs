using Godot;

public partial class ScoreOverlay : CanvasLayer
{
    private bool gameIsStarted;

    public void NewGame()
    {
        ScoreHandler.NewGame();
        gameIsStarted = true;
        this.Show();
    }

    public void GameOver()
    {
        gameIsStarted = false;
        this.Hide();
    }

    public override void _Process(double delta)
    {
        if (gameIsStarted)
        {
            UpdateScore();
        }
    }

    public void UpdateScore()
    {
        GetNode<Label>("Score").Text = ScoreHandler.Score.ToString();
    }
}
