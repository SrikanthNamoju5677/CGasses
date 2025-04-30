using Godot;
using SpaceInvaders;
using System.Collections.Generic;
using System.Linq;

public partial class Leaderboard : Screen
{
    private const int NrOfLeaders = 5;
    private const int RowNonLeader = NrOfLeaders + 1;
    private Color HighlightColor = Colors.Yellow;
    GridContainer board;

    private GameMode leaderboardType;
    private bool publishLastScore;

    public override void init(List<object> arguments)
    {
        leaderboardType = (GameMode)arguments[0];
        publishLastScore = (bool)arguments[1];
    }

    public override void _Ready()
    {
        board = GetNode<GridContainer>("BoardContainer");

        LoadLeaderboard();

        Button backButton = GetNode<Button>("Back");
        backButton.Pressed += this.BackButtonPressed;
        backButton.GrabFocus();
    }

    public void LoadLeaderboard()
    {
        var leaders = LeaderboardParser.LoadLeaderboard(leaderboardType);
        var leadersOrdered = leaders.OrderByDescending(l => l.Score).ToList();

        int i = 0;
        foreach (var l in leadersOrdered)
        {
            if (i < NrOfLeaders)
            {
                i++;
                FillLeaderboardRow(l, i, i);
            }
            else break;
        }

        if (publishLastScore)
        {
            var lastScore = leaders[leaders.Count() - 1];
            var postition = leadersOrdered.IndexOf(lastScore) + 1;

            if (postition > NrOfLeaders)
            {
                FillLeaderboardRow(lastScore, postition);
                HighlightScore();
            }
            else
            {
                HighlightScore(postition);
            }
        }

        board.Show();
    }

    public void FillLeaderboardRow(LeaderBordEntry entry, int pos, int row = RowNonLeader)
    {
        GetLabel("Row", row).Text = pos.ToString() + ".";
        GetLabel("Name", row).Text = entry.Name;
        GetLabel("Score", row).Text = $"€{entry.Score}";
    }

    public void HighlightScore(int row = RowNonLeader)
    {
        GetLabel("Row", row).Modulate = HighlightColor;
        GetLabel("Name", row).Modulate = HighlightColor;
        GetLabel("Score", row).Modulate = HighlightColor;
    }

    private Label GetLabel(string labelPrefix, int row)
    {
        return board.GetNode<Label>(labelPrefix + row.ToString());
    }

    public void BackButtonPressed()
    {
        this.EmitSignal(nameof(DoChangeScene), "LeaderBoardMenu");
    }
}
