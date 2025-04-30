using Godot;

public partial class SecondaryWeapon : Weapon
{
    [Signal]
    public delegate void SetModulateOverheatedEventHandler();
    [Signal]
    public delegate void SetModulateNotOverheatedEventHandler();
    [Signal]
    public delegate void OverheatEventEventHandler();
    [Signal]
    public delegate void SendHeatEventEventHandler(float value);
    [Signal]
    public delegate void FirePowerUpTimeoutEventHandler();

    //The second weapon can only shoot when it is not overheated
    private float heat = 0.0f;
    private bool isOverheated = false;

    //Timer that defines how long a fire power-up lasts
    private Timer powerUpTimer;

    //The secondary weapon can benefit from fire power-ups
    private bool poweredUp = false;

    public override bool Overheated
    {
        get => isOverheated;
    }

    protected override PackedScene BulletType
    {
        get => (PackedScene)GD.Load("res://src/weapons/SecondaryBullet.tscn");
    }

    protected override string ReloadTimerConfigurationKey
    {
        get => "SecondaryWeaponReloadTimer";
    }

    public override void _Ready()
    {
        base._Ready();

        powerUpTimer = new Godot.Timer();
        powerUpTimer.WaitTime = 6;
        this.AddChild(powerUpTimer);

        powerUpTimer.Timeout += this.PowerUpTimeout;
    }

    public void TriggerPowerupShooting()
    {
        poweredUp = true;
        powerUpTimer.Start();
        
        heat = 0;
        isOverheated = false;
    }

    protected override void WeaponFireEffect(Player Player, Vector2 gunPosition, Bullet bullet)
    {
        if (!poweredUp){
            //When the weapon is fired, the heat of the weapon increases
            heat += 25;
            bullet.Scale = new Vector2(0.5f, 0.5f);
            return;
        }
        bullet.Velocity = new Vector2(0, -1000);
        bullet.Texture = (Texture2D)GD.Load($"res://Laser Sprites/17.png");
        (bullet as SecondaryBullet).NewDamage = 4;
        bullet.Scale = new Vector2(0.6f, 0.6f); 
    }

    public void PowerUpTimeout()
    {
        this.EmitSignal(nameof(FirePowerUpTimeout));
        //Called when the powerup timer has a timeout
        poweredUp = false;
        powerUpTimer.Stop();
    }

    public override void ProcessWeapon(float delta, Player Player)
    {
        base.ProcessWeapon(delta, Player);

        heat -= delta * 50;

        //If the heat reaches 0, the weapon is not overheating anymore
        if (heat <= 0)
        {
            heat = 0;
            isOverheated = false;
            this.EmitSignal(nameof(SetModulateNotOverheated));
        }
        //If the heat of the weapon reaches 500, the weapon is overheated
        //Check isOverheated == false, so that we only send one overheat signal
        if (heat >= 500 && isOverheated == false)
        {
            isOverheated = true;
            this.EmitSignal(nameof(OverheatEvent));
        }
        if (this.isOverheated)
        {
            this.EmitSignal(nameof(SetModulateOverheated));
            heat -= delta * 20;
        }

        //Send a signal to the HUD, such that it always displays the proper 'heat'
        this.EmitSignal(nameof(SendHeatEvent), heat);
    }
}
