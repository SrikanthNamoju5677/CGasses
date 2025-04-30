using Godot;
using PrototypeSpaceInvaders.src.interfaces;
using PrototypeSpaceInvaders.src.services;
using SpaceInvaders;

public partial class BossLevel : Level
{
    private string startLevelMessage = "Well done...\nAre you ready for the next challenge,\n the ultimate threat to your software code?";
    private BossSprite boss;
    private Timer powerupTimer;

    private IPowerUpManager powerupManager;
    private IBossManager bossManager;

    protected override int BonusScoreTimeoutSeconds => 70;
    public override string StartLevelMessage { get => startLevelMessage; }
    public BossSprite Boss { get => boss; }

    //the level is won when the boss has lost, i.e. has no lives anymore
    public override bool CheckWin() => boss.bossHasLost;

    public override void Cleanup()
    {
        this.boss.BossVictory -= () => (this.GetParent() as MainGameOverlay).GameOver(false);
    }

    public override void Setup()
    {
        powerupManager = new PowerUpManager(this);
        bossManager = new BossManager(this);
        powerupSpawnInterval = (double)configuration.GetValue("Gameplay", "PowerupTimer");

        var musicPlayer = GetNode<MusicManager>("/root/MusicManager");
        musicPlayer.StartMusic("boss_level");
        SpawnBoss();
        InitializeTimers();
        DisplayBonusScoreOverlay();
    }

    private void InitializeTimers()
    {
        SetupTimer(ref powerupTimer, powerupSpawnInterval, SpawnPowerups);
    }

    public void SpawnBoss()
    {
        bossManager.SpawnBoss(ref boss);
    }

    private void SpawnPowerups()
    {
        powerupManager.SpawnPowerups();
    }

    public void ShowBurstWarningMessage()
    {
        this.EmitSignal(nameof(SendMessage), "The boss is about to shoot in a spread pattern,\n avoid the bullets!", false, (int)MessagePriority.lEVELWARNING);
    }

    public void ShowBossRechargingMessage()
    {
        this.EmitSignal(nameof(SendMessage), "The boss is recharging, shoot him down quickly!", false, (int)MessagePriority.lEVELWARNING);
    }
}
