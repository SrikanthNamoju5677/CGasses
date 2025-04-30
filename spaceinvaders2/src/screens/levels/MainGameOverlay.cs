using Godot;
using SpaceInvaders;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class MainGameOverlay : Screen
{
    private PackedScene scoreOverlayScene = (PackedScene)GD.Load("res://src/overlays/ScoreOverlay.tscn");

    [Signal]
    public delegate void SendMessageEventHandler(string message, bool returnSignal, int messagePriority);
    [Signal]
    public delegate void GameStartedEventHandler();
    [Signal]
    public delegate void BossDefeatedEventHandler();
    [Signal]
    public delegate void GameEndedEventHandler();
    [Signal]
    public delegate void BossLevelEnteredEventHandler();

    private const int LevelCountTutorial = 0;
    private int targetHitCounter, fireCounter = 0;

    private List<Level> levels;
    public int levelcounter = LevelCountTutorial;
    private Level currentLevel;

    private ScoreOverlay scoreOverlay;
    private PlayerHandler playerHandler; 
    public GameMode GameMode { get; private set; }

    public override void init(List<object> arguments)
    {
        GameMode = (GameMode)arguments[0];
    }

    public override void _Ready()
    {
        playerHandler = new PlayerHandler();
        this.AddChild(playerHandler);
        playerHandler.PlayersKilled += this.GameOver;

        scoreOverlay = (ScoreOverlay)scoreOverlayScene.Instantiate();
        scoreOverlay.NewGame();
        this.AddChild(scoreOverlay);

        ConfigureTimedMessages();

        StartGame();
    }

    public void TargetOnHit() => targetHitCounter++;

    public void PlayerFired() => fireCounter++;

    public static void ResetPlayerHits(List<Player> players) => players.ForEach(x => x.IsHit = false);

    public void ConfigureScoreCounter()
    {
        this.BossDefeated += () => scoreOverlay.GameOver();
    }

    public void ConfigureTimedMessages()
    {
        var messageWithTimerNode = GetNode<MessageWithTimer>("MessageWithTimer");
        this.BossDefeated += () => messageWithTimerNode.MessageTimeout();
    }

    public void StartGame()
    {
        ConfigureScoreCounter();
        var musicPlayer = GetNode<MusicManager>("/root/MusicManager");
        //Define which levels the game consists of, and in what order
        levels = new List<Level> { new TutorialLevel(), new FirstLevel(GameMode), new SecondLevel(), new BossLevel() };
        if (GameMode == GameMode.AIDemoGame)
            levels.RemoveAt(0);
        if (GameMode == GameMode.SurvivalMode)
            levels = new List<Level> { new TutorialLevel(), new SurvivalModeLevel() };
        if (GameMode != GameMode.AIDemoGame)
            musicPlayer.StartMusic("error");

        levelcounter = LevelCountTutorial;
        this.EmitSignal(nameof(GameStarted));

        PrepareNextLevel();
    }

    async public void PrepareNextLevel()
    {
        //Retrieve the current level and 
        currentLevel = levels[levelcounter];
        ResetPlayerHits(playerHandler.Players);

        if (currentLevel is BossLevel)
            EmitSignal(nameof(BossLevelEntered));

        //Display the StartLevelMessage of this level
        this.EmitSignal(nameof(SendMessage), currentLevel.StartLevelMessage, true, (int)MessagePriority.GAMEFLOW);
        var messageWithTimerNode = (MessageWithTimer)GetNode<CanvasLayer>("MessageWithTimer");
        await ToSignal(messageWithTimerNode, "MessageHidden");
        if (GameMode == GameMode.AIDemoGame)
            messageWithTimerNode.MessageHidden += () => messageWithTimerNode.ShowMessage("Ready for a challenge?\n\nPress \"Enter\" to start the journey in space.");

        //When the level is won, we call 'CurrentLevelIsWon'
        this.AddChild(currentLevel, true);
        currentLevel.LevelIsWon += CurrentLevelIsWon;
    }
    public void CurrentLevelIsWon()
    {
        UpdateScoreForLevelWin();
        CleanUpCurrentLevel();
        CheckAndHandleTutorialEnd();
        PrepareForNextLevelOrEndGame();
    }

    private void UpdateScoreForLevelWin()
    {
        // Update score for level completion and accuracy bonus
        ScoreHandler.LevelCompleted(currentLevel.GetScoreAtWin());
        ScoreHandler.AddAccuracyBonus(currentLevel, ref fireCounter, ref targetHitCounter);

        // Award no-hit bonus for non-tutorial levels
        if (!(currentLevel is TutorialLevel))
        {
            AwardNoHitBonusToEligiblePlayers();
        }
    }

    private void AwardNoHitBonusToEligiblePlayers()
    {
        foreach (var player in playerHandler.Players)
        {
            if (!player.IsHit)
                ScoreHandler.AddNoHitBonus();
        }
    }

    private void CleanUpCurrentLevel()
    {
        // Disconnect signals and remove the level node
        currentLevel.LevelIsWon -= CurrentLevelIsWon;
        this.RemoveChild(currentLevel);
    }

    private void CheckAndHandleTutorialEnd()
    {
        // Check if tutorial ended and handle accordingly
        if (levelcounter == LevelCountTutorial)
        {
            scoreOverlay.NewGame();
        }
    }

    private void PrepareForNextLevelOrEndGame()
    {
        levelcounter++;

        if (levelcounter < levels.Count)
        {
            // Prepare the next level if available
            PrepareNextLevel();
        }
        else
        {
            // End the game if all levels are completed
            GameOver(true);
        }
    }

    /*
	Called when the game is over, because the player has won
	Or because the player is dead/defeated
	*/
    public async void GameOver(bool playerVictory)
    {
        this.EmitSignal(nameof(BossDefeated));

        //If the player has lost, the current level node was not yet deleted 
        if (!playerVictory)
        {
            currentLevel.LevelIsWon -= CurrentLevelIsWon;
            this.RemoveChild(currentLevel);
        }

        //Depending on whether the player has won or lost, display a message
        if (playerVictory)
        {
            await WaitForBossExplosionFinished();
            await ShowVictoryMessage();
        }
        else
        {
            await ShowGameOverAsync();
        }

        this.EmitSignal(nameof(GameEnded));

        string wantedScene = "Credentials" + PlayerHandler.GameMode.ToString();
        if (PlayerHandler.GameMode == GameMode.AIDemoGame)
            wantedScene = "AIDemoGame";

        this.EmitSignal(nameof(DoChangeScene), wantedScene);
    }

    public void ShowMessageOnOverheatAsync()
    {
        this.EmitSignal(nameof(SendMessage), "Take caution, your weapon is overheated...", false, (int)MessagePriority.WARNING);
    }

    public async Task ShowGameOverAsync()
    {
        this.EmitSignal(nameof(SendMessage), "Game over..." + "your salary is: €" + ScoreHandler.Score, true, (int)MessagePriority.GAMEFLOW);

        Node messageWithTimerNode = GetNode<CanvasLayer>("MessageWithTimer");
        await ToSignal(messageWithTimerNode, "MessageHidden");
    }

    public async Task ShowVictoryMessage()
    {
        this.EmitSignal(nameof(SendMessage), $"Well done... your salary is €{ScoreHandler.Score}.\n You saved your software code from \n the evil bugs.", true, (int)MessagePriority.GAMEFLOW);

        Node messageWithTimerNode = GetNode<CanvasLayer>("MessageWithTimer");
        await ToSignal(messageWithTimerNode, "MessageHidden");
    }

    public async Task WaitForBossExplosionFinished()
    {
        Node bossExplosion = GetNode<BossExplosion>("BossExplosion");

        await ToSignal(bossExplosion, "ExplosionFinished");
    }
}
