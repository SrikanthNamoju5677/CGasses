using Godot;
using SpaceInvaders;
using System.Collections.Generic;

public partial class PlayerHandler : Node
{
    [Signal]
    public delegate void PlayersKilledEventHandler(bool playerVictory);

    private PackedScene playerScene = (PackedScene)GD.Load("res://src/player/Player.tscn");
    private PackedScene artificialScene = (PackedScene)GD.Load("res://src/player/AI.tscn");
    private List<Player> players = new List<Player>();
    private static GameMode gameMode = GameMode.Singleplayer;
    private int deadPlayerCount = 0;

    public List<Player> Players
    {
        get { return players; }
    }

    public static GameMode GameMode
    {
        get { return gameMode; }
    }

    public override void _Ready()
    {
        this.Name = "PlayerHandler";

        MainGameOverlay mainGame = (MainGameOverlay)this.GetParent();
        mainGame.GameStarted += PreparePlayers;
        mainGame.GameEnded += GameOver;
        gameMode = mainGame.GameMode;
    }

    public void PreparePlayers()
    {
        if (gameMode == GameMode.Multiplayer)
        {
            PrepareMultiplayer();
        }
        else if (gameMode == GameMode.CooperativeAI)
        {
            PrepareCooperative();
        }
        else if (gameMode == GameMode.AIDemoGame)
        {
            PreparePlayer(new Vector2(800, 900), 1, true);
        }
        else
        {
            PrepareSinglePlayer();
        }
    }



    public void PrepareSinglePlayer()
    {
        PreparePlayer(new Vector2(800, 900));
    }

    public void PrepareMultiplayer()
    {
        PreparePlayer(new Vector2(800, 900));
        PreparePlayer(new Vector2(1200, 900), 2);
    }

    private void PrepareCooperative()
    {
        PreparePlayer(new Vector2(800, 900));
        PreparePlayer(new Vector2(1200, 900), 2, true);
    }

    public void PreparePlayer(Vector2 position, int playerNumber = 1, bool isArtificial = false)
    {
        Player player = isArtificial ? (AI)artificialScene.Instantiate() : (Player)playerScene.Instantiate();
        player.Position = position;
        player.Init(playerNumber);
        player.ProcessFire += () => (this.GetParent() as MainGameOverlay).PlayerFired();

        this.AddChild(player, true);
        players.Add(player);
        player.Died += () => { this.OnPlayerDied(player); };

        if (!isArtificial)
            player.WeaponCycle[1].Connect("OverheatEvent", new Callable(this.GetParent(), "ShowMessageOnOverheatAsync"));

        if (playerNumber == 2)
            ChangeColor(player);
    }

    public void OnPlayerDied(Player player)
    {
        deadPlayerCount++;
        if (deadPlayerCount >= players.Count)
        {
            this.EmitSignal(nameof(PlayersKilled), false);
        }
        else
        {
            foreach (Player p in players)
            {
                if (!ReferenceEquals(player, p) && p is AI)
                {
                    p.Hide();
                    this.EmitSignal(nameof(PlayersKilled), false);
                }
            }
        }
    }

    public void GameOver()
    {
        foreach (Player player in players)
        {
            player.Hide();
        }
    }
    public void ChangeColor(Player player)
    {
        player.Modulate = Colors.Yellow;
    }
}
