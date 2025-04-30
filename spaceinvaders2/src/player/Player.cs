using Godot;
using PrototypeSpaceInvaders.Extensions;
using SpaceInvaders;

public partial class Player : Sprite2D
{
    [Signal]
    public delegate void DiedEventHandler();
    [Signal]
    public delegate void MainWeaponEventEventHandler();
    [Signal]
    public delegate void SprayWeaponEventEventHandler();
    [Signal]
    public delegate void LifeHookEventHandler(int value);
    [Signal]
    public delegate void ShieldHookEventHandler(int value);
    [Signal]
    public delegate void FirePowerUpEventHandler();
    [Signal]
    public delegate void ProcessFireEventHandler();

    private PackedScene packedSceneParticle = (PackedScene)GD.Load("res://src/explosion/Explosion.tscn");
    private PackedScene packedSceneParticlePowerup = (PackedScene)GD.Load("res://src/explosion/ExplosionPowerup.tscn");
    private PackedScene packedSceneShieldsPowerup = (PackedScene)GD.Load("res://src/explosion/ExplosionPowerupShields.tscn");
    private PackedScene packedSceneFloatingText = (PackedScene)GD.Load("res://src/messages/FloatingText.tscn");
    private PackedScene weaponOverlayScene = (PackedScene)GD.Load("res://src/overlays/WeaponOverlay.tscn");
    private PackedScene weaponOverlayRightScene = (PackedScene)GD.Load("res://src/overlays/WeaponOverlayRight.tscn");
    private PackedScene healthOverlayScene = (PackedScene)GD.Load("res://src/overlays/HealthOverlay.tscn");
    private PackedScene healthOverlayRightScene = (PackedScene)GD.Load("res://src/overlays/HealthOverlayRight.tscn");

    private AudioStreamPlayer2D PowerUpSound;

    protected Vector2 MOVE_UNITS_LEFT { get; set; }
    protected Vector2 MOVE_UNITS_RIGHT { get; set; }
    protected Vector2 MOVE_UNITS_LEFT_FASTER { get; set; }
    protected Vector2 MOVE_UNITS_RIGHT_FASTER { get; set; }

    private Weapon[] weaponsCycle;
    private Node2D gunPosition;

    protected float MovementSpeed { get; set; }
    protected float leftBorder = (float)0.0;
    protected float rightBorder = 0.0f;

    public bool IsHit { get; set; }
    public float changeWeaponTime = 0.1f;
    public float changingWeapon = 0.0f;
    public int shields, Lives;
    public int indexCounterWeaponsCycle = 0;
    public int PlayerNumber { get; private set; }
    public Weapon[] WeaponCycle { get => weaponsCycle; }
    public Weapon CurrentWeapon;

    public void Init(int PlayerNumber) => this.PlayerNumber = PlayerNumber;

    public override void _Ready()
    {
        SetGameSettingsFromConfiguration();

        this.gunPosition = GetNode<Node2D>("GunPosition");

        //NOTE: TriggerPowerupShooting is a method of PrimaryWeapon but NOT of Weapon. Either instantiate primaryWeapon as a PrimaryWeapon or do a (somewhat ugly) typecast
        Weapon primaryWeapon = new PrimaryWeapon();
        this.AddChild(primaryWeapon);
        this.FirePowerUp += (primaryWeapon as PrimaryWeapon).TriggerPowerupShooting;

        Weapon secondaryWeapon = new SecondaryWeapon();
        this.AddChild(secondaryWeapon);
        this.FirePowerUp += (secondaryWeapon as SecondaryWeapon).TriggerPowerupShooting;
        weaponsCycle = new Weapon[] { primaryWeapon, secondaryWeapon };
        this.CurrentWeapon = this.weaponsCycle[0];

        this.Lives = 2;
        this.shields = 1;

        Area2D node = GetNode<Area2D>("Area2D");
        node.AreaEntered += this.Hit;
        float width = this.GetViewport().GetVisibleRect().Size.X;

        Vector2 halfSize = (this.Texture.GetSize() * Scale) / 2;
        leftBorder = halfSize.X;
        rightBorder = width - halfSize.X;

        Node weaponOverlay = PlayerNumber > 1 ? weaponOverlayRightScene.Instantiate() : weaponOverlayScene.Instantiate();
        this.AddChild(weaponOverlay);

        Node healthOverlay = PlayerNumber > 1 ? healthOverlayRightScene.Instantiate() : healthOverlayScene.Instantiate();
        this.AddChild(healthOverlay);

        this.EmitSignal(nameof(LifeHook), this.Lives);
        this.EmitSignal(nameof(ShieldHook), this.shields);

        PowerUpSound = GetNode<AudioStreamPlayer2D>("PowerUpSound");
        
    }

    private void SetTemporarilyColorLoseHP()
    {
        this.Modulate = Colors.Red;
        RestoreColorAfterOneSecond();
    }

    private void SetTemporarilyColorLoseShield()
    {
        this.Modulate = Colors.Aqua;
        RestoreColorAfterOneSecond();
    }

    private void RestoreColorAfterOneSecond()
    {
        this.GetTree().CreateTimer(1).Timeout += RestoreColor;
    }

    public virtual void RestoreColor() => this.Modulate = Colors.White;

    public void Hit(Area2D obj)
    {
        Explosions? particleEffect;
        switch (obj.Name)
        {
            case "InvaderArea":
            case "BossArea2D":
                particleEffect = HandleHitByInvaderOrBoss();
                IsHit = true;
                break;
            case "EnemyLaserArea":
            case "BossBulletArea":
                particleEffect = HandleHitByBulletOrLaser();
                IsHit = true;
                break;
            case "PowerupFire":
                particleEffect = HandlePowerUpWeapon();
                PowerUpSound.Play();
                break;
            case "PowerupHealth":
                particleEffect = HandlePowerUpShield();
                PowerUpSound.Play();
                break;
            default:
                return;
        }

        if (particleEffect.HasValue)
        {
            this.AddParticleScene(particleEffect.Value);
        }

        if (this.Lives <= 0)
        {
            OnDied();
        }
    }

    private Explosions HandleHitByInvaderOrBoss()
    {
        Lives = 0;
        shields = 0;
        this.EmitSignal(nameof(LifeHook), this.Lives);
        this.EmitSignal(nameof(ShieldHook), this.shields);

        return Explosions.Hit;
    }

    private Explosions HandleHitByBulletOrLaser()
    {
        if (this.shields > 0)
        {
            this.PlayerHelper(packedSceneFloatingText, FloatingTextTypes.HITSHIELD);
            this.SetTemporarilyColorLoseShield();
            this.shields--;
            this.EmitSignal(nameof(ShieldHook), this.shields);
        }
        else
        {
            this.PlayerHelper(packedSceneFloatingText, FloatingTextTypes.HITLIFE);
            this.SetTemporarilyColorLoseHP();
            this.Lives--;
            this.EmitSignal(nameof(LifeHook), this.Lives);
        }

        return Explosions.Hit;
    }

    private Explosions HandlePowerUpShield()
    {
        ScoreHandler.OnPlayerShieldPowerUp();
        this.shields++;
        this.EmitSignal(nameof(ShieldHook), this.shields);
        this.PlayerHelper(packedSceneFloatingText, FloatingTextTypes.POWERUPSHIELD);

        return Explosions.PowerupShields;
    }

    private Explosions HandlePowerUpWeapon()
    {
        ScoreHandler.OnPlayerFirePowerUp();
        this.EmitSignal(nameof(FirePowerUp));
        this.PlayerHelper(packedSceneFloatingText, FloatingTextTypes.POWERUPFIRE);

        return Explosions.PowerupWeapon;
    }

    private void OnDied()
    {
        WeaponOverlay weaponOverlay = (WeaponOverlay)this.GetNode("WeaponOverlay");
        weaponOverlay.HideOverlay();

        this.CallDeferred("remove_child", this.GetNode<Area2D>("Area2D"));
        this.EmitSignal(nameof(Died));
        this.Hide();
    }

    public void AddParticleScene(Explosions explosion)
    {
        GpuParticles2D particles;
        switch (explosion)
        {
            case Explosions.Hit:
                particles = (GpuParticles2D)packedSceneParticle.Instantiate();
                break;
            case Explosions.PowerupShields:
                particles = (GpuParticles2D)packedSceneShieldsPowerup.Instantiate();
                break;
            case Explosions.PowerupWeapon:
                particles = (GpuParticles2D)packedSceneParticlePowerup.Instantiate();
                break;
            default:
                particles = null;
                break;
        }
        this.GetParent().AddChild(particles);
        particles.GlobalPosition = this.GlobalPosition;
    }

    public void Fire() => CurrentWeapon.Fire(this, this.gunPosition.GlobalPosition);

    public void ProcessShot() => EmitSignal(nameof(ProcessFire));

    protected void Move(Vector2 adjustment)
    {
        var newX = this.Position.X + adjustment.X;

        if (newX <= leftBorder)
        {
            newX = leftBorder;
        }
        else if (newX >= rightBorder)
        {
            newX = rightBorder;
        }
        Position = new Vector2(newX, Position.Y);
    }

    protected enum Direction
    {
        Left,
        Right,
        FasterLeft,
        FasterRight
    }

    protected void Move(Direction direction)
    {
        switch (direction)
        {
            case Direction.Left:
                Move(MOVE_UNITS_LEFT);
                break;
            case Direction.Right:
                Move(MOVE_UNITS_RIGHT);
                break;
            case Direction.FasterLeft:
                Move(MOVE_UNITS_LEFT_FASTER);
                break;
            case Direction.FasterRight:
                Move(MOVE_UNITS_RIGHT_FASTER);
                break;
        }
    }

    public void SwitchWeapon(string direction)
    {
        int maxIterations = this.weaponsCycle.Length - 1;
        int minIterations = 0;

        if (changingWeapon <= 0)
        {
            //if the button is clicked I want the counter to increase, and to set the weapon string to main weapon 
            if (direction == "up")
            {
                this.indexCounterWeaponsCycle++;

                if (this.indexCounterWeaponsCycle > maxIterations)
                {
                    this.indexCounterWeaponsCycle = 0;
                }

                this.CurrentWeapon = this.weaponsCycle[indexCounterWeaponsCycle];
                changingWeapon = changeWeaponTime;
            }

            if (direction == "down")
            {
                this.indexCounterWeaponsCycle--;

                if (this.indexCounterWeaponsCycle < minIterations)
                {
                    this.indexCounterWeaponsCycle = 1;
                }

                this.CurrentWeapon = this.weaponsCycle[indexCounterWeaponsCycle];
                changingWeapon = changeWeaponTime;
            }

            if (this.CurrentWeapon == this.weaponsCycle[0])
            {
                this.EmitSignal(nameof(MainWeaponEvent));
            }
            else
            {
                this.EmitSignal(nameof(SprayWeaponEvent));
            }
        }
    }

    public virtual void CheckUserInput(double delta)
    {
        if (this.Visible)
        {
            if (Input.IsActionPressed("move_left_" + PlayerNumber.ToString()))
            {
                Move(MOVE_UNITS_LEFT);
            }
            if (Input.IsActionPressed("move_right_" + PlayerNumber.ToString()))
            {
                Move(MOVE_UNITS_RIGHT);
            }
            if (Input.IsActionPressed("fire_" + PlayerNumber.ToString()))
            {
                Fire();
            }
            if (Input.IsActionJustPressed("SwitchMainWeapon_" + PlayerNumber.ToString()))
            {
                SwitchWeapon("down");
            }
            if (Input.IsActionJustPressed("SwitchSprayWeapon_" + PlayerNumber.ToString()))
            {
                SwitchWeapon("up");
            }
        }
    }

    public override void _Process(double delta)
    {
        foreach (var w in weaponsCycle)
        {
            w.ProcessWeapon((float)delta, this);
        }

        this.changingWeapon -= (float)delta;
        CheckUserInput(delta);
    }

    private void SetGameSettingsFromConfiguration()
    {
        //Retrieve settings from the GameConfig configuration file
        ConfigFile configuration = ConfigHelper.LoadConfigFile();

        this.MovementSpeed = (float)configuration.GetValue("Gameplay", "MovementSpeed");
        this.MOVE_UNITS_LEFT = new Vector2(-this.MovementSpeed, 0);
        this.MOVE_UNITS_RIGHT = new Vector2(this.MovementSpeed, 0);
        this.MOVE_UNITS_LEFT_FASTER = new Vector2((-this.MovementSpeed * 3), 0);
        this.MOVE_UNITS_RIGHT_FASTER = new Vector2((this.MovementSpeed * 3), 0);
    }
}
