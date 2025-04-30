using Godot;
using SpaceInvaders;
using System.Collections.Generic;
using System.Linq;

public partial class LeaderBordEntry
{
    public LeaderBordEntry(string name, int score)
    {
        Name = name;
        Score = score;
    }

    public string Name { get; private set; }
    public int Score { get; private set; }
}

public static class LeaderboardParser
{
    private const string LeaderboardFile = "Leaderboard.txt";

    public static List<LeaderBordEntry> LoadLeaderboard(GameMode mode)
    {
        var leaders = new List<LeaderBordEntry>();

        if (!FileAccess.FileExists(LeaderboardFile))
            return leaders;

        using var file = FileAccess.Open(LeaderboardFile, FileAccess.ModeFlags.Read);
        leaders = file.GetAsText()
                      .Split("\n")
                      .Where(s => !string.IsNullOrEmpty(s) && s.Split(",").Length == 3 && s.Split(",")[2] == mode.ToString())
                      .Select(l => new LeaderBordEntry(l.Split(",")[0], int.Parse(l.Split(",")[1])))
                      .ToList();

        return leaders;
    }

    public static void AddNewLeader(string name, string score, GameMode mode)
    {
        var modeFlag = FileAccess.FileExists(LeaderboardFile) ? FileAccess.ModeFlags.ReadWrite : FileAccess.ModeFlags.Write;
        using var file = FileAccess.Open(LeaderboardFile, modeFlag);
        file.SeekEnd();
        name = name.Replace(',', ' ');
        file.StoreLine(name + "," + score + "," + mode.ToString());
    }
}
