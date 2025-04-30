using Godot;
using PrototypeSpaceInvaders.src.interfaces;
using PrototypeSpaceInvaders.src.services;
using System.Collections.Generic;

public partial class SurvivalModeLevel : Level
{
    private const string startLevelMessage = "Welcome to survival mode, kill to live!";
    private const double invaderWaveInterval = 25;
    private const double asteroidSpawnInterval = 0.9;

    private Timer asteroidTimer, powerupTimer, invaderWaveTimer;
    private List<Invaders> invaders = new List<Invaders>();
    public List<Invaders> Invaders => invaders;

    private IPowerUpManager powerupManager;
    private IAsteroidManager asteroidManager;
    private IInvaderWaveManager invaderWaveManager;

    public override string StartLevelMessage => startLevelMessage;
    protected override int BonusScoreTimeoutSeconds => 70;

    public override void Setup()
    {
        powerupManager = new PowerUpManager(this);
        asteroidManager = new AsteroidManager(this);
        invaderWaveManager = new InvaderWaveManager(this);

        var musicPlayer = GetNode<MusicManager>("/root/MusicManager");
        musicPlayer.StartMusic("levels_bgm");
        
        SpawnInvaders();
        InitializeTimers();
    }

    private void InitializeTimers()
    {
        SetupTimer(ref asteroidTimer, asteroidSpawnInterval, SpawnAsteroids);
        SetupTimer(ref invaderWaveTimer, invaderWaveInterval, SpawnInvaders);
        SetupTimer(ref powerupTimer, powerupSpawnInterval, SpawnPowerups);
    }

    private void SpawnAsteroids()
    {
        asteroidManager.SpawnAsteroids(true, true);
    }

    private void SpawnInvaders()
    {
        Invaders invaderWave = invaderWaveManager.SpawnInvaders();
        Invaders.Add(invaderWave);
    }
    private void SpawnPowerups()
    {
        powerupManager.SpawnPowerups();
    }

    public override bool CheckWin()
    {
        //Since it is a continuous level, there is no win
        return false;
    }

    public override void Cleanup()
    {
        StopAllTimers();
        ClearAllInvaders();
    }
    private void StopAllTimers()
    {
        asteroidTimer.Stop();
        powerupTimer.Stop();
        invaderWaveTimer.Stop();
    }

    public void ClearAllInvaders()
    {
        invaderWaveManager.ClearAllInvaders(Invaders);
    }
}
