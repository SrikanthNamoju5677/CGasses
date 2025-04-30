using Godot;

public partial class HealthOverlay : CanvasLayer
{
    private ProgressBar healthBar;
    private ProgressBar shieldBar;

    public override void _Ready()
    {
        Player player = (Player)this.GetParent();
        player.ShieldHook += HookShieldBar;
        player.LifeHook += HookHealthBar;


        healthBar = GetNode<ProgressBar>("HealthBar");
        shieldBar = GetNode<ProgressBar>("ShieldBar");

        InitialSettings();
    }

    public void HookHealthBar(int value)
    {
        Label labelNotifierHealth = healthBar.GetNode<Label>("HealthNotifier");
        healthBar.Value = value;
        labelNotifierHealth.Text = $"{healthBar.Value}/{healthBar.MaxValue}";
        if (healthBar.Value <= 1)
        {
            healthBar.Modulate = new Color((float)1.0, (float)0.0, (float)0.0, (float)1.0);
        }
        else
        {
            healthBar.Modulate = new Color((float)0.0, (float)0.5, (float)1.0);
        }
    }

    public void HookShieldBar(int playerShields)
    {
        shieldBar.Value = playerShields;
        Label labelNotifierShields = GetNode<ProgressBar>("ShieldBar").GetNode<Label>("ShieldNotifier");
        labelNotifierShields.Text = $"{shieldBar.Value}/{shieldBar.MaxValue}";

        if (playerShields > shieldBar.MaxValue)
        {
            SetMaxValueShield(playerShields);
        }
    }

    public void SetMaxValueShield(int value)
    {
        Label labelNotifierShields = shieldBar.GetNode<Label>("ShieldNotifier");
        shieldBar.MaxValue = value;
        labelNotifierShields.Text = $"{shieldBar.Value}/{shieldBar.MaxValue}";
    }

    public void InitialSettings()
    {
        this.SetMaxValueShield(1);

        healthBar.MaxValue = 2;
        HookHealthBar(2);

        shieldBar.MaxValue = 1;
        HookShieldBar(1);
    }
}
