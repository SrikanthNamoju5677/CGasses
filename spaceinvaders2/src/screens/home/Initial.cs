using Godot;
using SpaceInvaders;
using System.Collections.Generic;
using System.Globalization;

public partial class Initial : Node
{
    private Screen currentScreen;

    private List<string> screensaver_list = new List<string>{"HomeScreen", "GameModeMenu", "LeaderBoardMenu"};

    public override void _Ready()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        ConfigFile configuration = ConfigHelper.LoadConfigFile();
        var starsBackground = GetNode<ParallaxBackground>("StarsBackground");
        starsBackground.ScrollSpeed = (float)configuration.GetValue("Gameplay", "ScrollSpeed");

        LoadNextScreen("HomeScreen");
    }

    private void LoadNextScreen(string screen)
    {
        if (currentScreen != null)
        {
            this.RemoveChild(currentScreen);
        }

        var result = SceneTransitions.GetTransition(screen);
        var nextScreen = result.Item1;
        currentScreen = (Screen)GD.Load<PackedScene>(nextScreen).Instantiate();

        var arguments = result.Item2;

        if (arguments.Count > 0)
        {
            currentScreen.init(arguments);
        }
        currentScreen.DoChangeScene += this.LoadNextScreen;
        this.AddChild(currentScreen);
    }

    public override void _Input(InputEvent @event)
    {
        Timer timerNode = GetNode<Timer>("TriggerScreensaver");
        timerNode.Start();

        if ((currentScreen as MainGameOverlay)?.GameMode == GameMode.AIDemoGame)
            LoadNextScreen("HomeScreen");
    }

    public async void OnTriggerScreensaverTimeout()
    {
        GetNode<Timer>("TriggerScreensaver").Stop();

        if (screensaver_list.Contains(currentScreen.Name)) LoadNextScreen("AIDemoGame");
    }
}
