using Godot;
using PrototypeSpaceInvaders.src.interfaces;
using PrototypeSpaceInvaders.src.services;

public partial class SecondLevel : Level
{
    public PackedScene InvadersScene => (PackedScene)GD.Load("res://src/invader/Invaders.tscn");
    public PackedScene AsteroidScene => (PackedScene)GD.Load("res://src/weapons/EnemyLaser.tscn");


    private string startLevelMessage = "The next wave is about to invade... brace yourself";
    public override string StartLevelMessage { get => startLevelMessage; }

    private Timer asteroidTimer, powerupTimer;
    private IPowerUpManager powerupManager;
    private IAsteroidManager asteroidManager;
    private IInvaderWaveManager invaderWaveManager;

    protected override int BonusScoreTimeoutSeconds => 70;
    public double AsteroidSpawnInterval => 0.9;

    private Invaders invaders;
    public Invaders Invaders { get => invaders; }


    public override void Setup()
    {
        powerupManager = new PowerUpManager(this);
        asteroidManager = new AsteroidManager(this);
        invaderWaveManager = new InvaderWaveManager(this);

        //Add a set of invaders to the level that need to be defeated
        SpawnInvaders();

        InitializeTimers();
        DisplayBonusScoreOverlay();
    }

    private void InitializeTimers()
    {
        //Set a asteroidTimer and add a wave of random projectiles every time it times out
        SetupTimer(ref asteroidTimer, AsteroidSpawnInterval, SpawnAsteroids);

        //Set a poewrupTimer that spawns a powerup every time it times out
        SetupTimer(ref powerupTimer, powerupSpawnInterval, SpawnPowerups);
    }

    public override bool CheckWin()
    {
        //The level is won when it contains no invaders anymore
        var invadersCounter = this.GetTree().GetNodesInGroup("Invaders");

        return invadersCounter.Count == 0;
    }

    public override void Cleanup()
    {
        asteroidTimer.Stop();
        powerupTimer.Stop();

        ClearAllInvaders();
    }

    private void SpawnInvaders()
    {
        invaders = invaderWaveManager.SpawnInvaders();
    }

    private void SpawnAsteroids()
    {
        asteroidManager.SpawnAsteroids(true, true);
    }

    private void SpawnPowerups()
    {
        powerupManager.SpawnPowerups();
    }

    private void ClearAllInvaders()
    {
        invaderWaveManager.ClearAllInvaders(invaders);
    }
}
