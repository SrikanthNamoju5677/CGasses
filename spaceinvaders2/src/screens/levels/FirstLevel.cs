using Godot;
using PrototypeSpaceInvaders.src.interfaces;
using PrototypeSpaceInvaders.src.services;
using SpaceInvaders;
using System.Linq;

public partial class FirstLevel : Level
{

    private Invaders invaders;
    private Timer blasttTimer;
    private string startLevelMessage = "Prepare for the first wave of evil bugs!";
    public override string StartLevelMessage { get => startLevelMessage; }
    public Invaders Invaders { get => invaders; }

    private IAsteroidManager asteroidManager;
    private IInvaderWaveManager invaderWaveManager;

    protected override int BonusScoreTimeoutSeconds => 60;
    private const double asteroidSpawnInterval = 2;

    private GameMode GameMode;
    public FirstLevel() {}
    
    public FirstLevel(GameMode GameMode) {
        this.GameMode = GameMode;
    }

    public override void Setup()
    {
        asteroidManager = new AsteroidManager(this);
        invaderWaveManager = new InvaderWaveManager(this);

        SpawnInvaders();

        //Every time the blasttimer has a timeout, we add a new set of random asteroids to the level
        SetupTimer(ref blasttTimer, asteroidSpawnInterval, SpawnAsteroids);
        var musicPlayer = GetNode<MusicManager>("/root/MusicManager");
        if (this.GameMode != GameMode.AIDemoGame)
            musicPlayer.StartMusic("levels_bgm");
        DisplayBonusScoreOverlay();
    }

    public override void Cleanup()
    {
        blasttTimer.Stop();
        blasttTimer.Timeout -= SpawnAsteroids;
        ClearAllInvaders();
        //TODO
    }

    public override bool CheckWin()
    {
        //The level is won when the level does not contain any invaders anymore
        var invadersCounter = this.GetTree().GetNodesInGroup("Invaders");

        return invadersCounter.Count == 0;
    }

    /*
  Called at every asteroidTimer timeout
  Adds a wave of a random number of asteroids at random positions to the level
  */
    public void SpawnAsteroids()
    {
        asteroidManager.SpawnAsteroids(false, false);
    }

    public void SpawnInvaders()
    {
        //Add a set of invaders to the level
        this.invaders = invaderWaveManager.SpawnInvaders();
        this.AddChild(invaders);
    }

    public void ClearAllInvaders()
    {
        invaders.Disconnect("InvadersWon", new Callable(this.GetParent(), "GameOver"));
    }
}
