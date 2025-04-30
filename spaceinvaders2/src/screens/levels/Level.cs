using Godot;
using SpaceInvaders;
using System;

public abstract partial class Level : CanvasLayer
{
    [Signal]
    // Signal that is sent to the 'Initial' class when the level is won
    public delegate void LevelIsWonEventHandler();

    [Signal]
    public delegate void SendMessageEventHandler(string message, bool returnSignal, int messagePriority);

    private PackedScene bonusScoreOverlayScene = (PackedScene)GD.Load("res://src/overlays/BonusScoreOverlay.tscn");

    private MessageWithTimer messageWithTimer;
    private BonusScoreOverlay bonusScoreOverlay;

    //Message that will be displayed at the start of the level
    public abstract string StartLevelMessage { get; }
    protected abstract int BonusScoreTimeoutSeconds { get; }
    private const int NoBonusScoreTimeout = 0;

    protected static ConfigFile configuration = ConfigHelper.LoadConfigFile();
    protected double powerupSpawnInterval;

    //Level creation, such as attaching nodes/scenes, connecting signals, setting timers, etc
    public abstract void Setup();
    //Level cleanup when the level is won
    public abstract void Cleanup();
    //Defines when the level is won
    public abstract bool CheckWin();


    public override void _Ready()
    {
        powerupSpawnInterval = (double)configuration.GetValue("Gameplay", "PowerupTimer");

        messageWithTimer = this.GetParent().GetNode<MessageWithTimer>("MessageWithTimer");
        this.SendMessage += messageWithTimer.ShowMessage;

        Setup();

        if (BonusScoreTimeoutSeconds == NoBonusScoreTimeout)
            return;
    }

    protected void SetupTimer(ref Timer timer, double waitTime, Action timeoutMethod)
    {
        timer = new Timer
        {
            WaitTime = waitTime
        };
        this.AddChild(timer);
        timer.Timeout += timeoutMethod;
        timer.Start();
    }

    protected void DisplayBonusScoreOverlay()
    {
        bonusScoreOverlay = (BonusScoreOverlay)bonusScoreOverlayScene.Instantiate();
        this.AddChild(bonusScoreOverlay);
        bonusScoreOverlay.Start(BonusScoreTimeoutSeconds);
    }

    public override void _Process(double delta)
    {
        //If the level is won, clean up and signal the 'Initial' class that the level is finished
        if (CheckWin())
        {
            Cleanup();
            this.EmitSignal(nameof(LevelIsWon));
        }
    }

    //Score the user gets when the level is completed
    public int GetScoreAtWin()
    {
        if (BonusScoreTimeoutSeconds == NoBonusScoreTimeout)
            return 0;

        return Mathf.RoundToInt(bonusScoreOverlay.BonusScoreTimerLeft * PointSystem.LevelPointsBonusMultiplier); ;
    }
}
