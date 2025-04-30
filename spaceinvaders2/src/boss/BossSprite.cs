using Godot;
using PrototypeSpaceInvaders.Extensions;
using PrototypeSpaceInvaders.src.invader;
using SpaceInvaders;
using System;

public partial class BossSprite : SpaceObject
{
    [Signal]
    public delegate void BossVictoryEventHandler();
    [Signal]
    public delegate void BurstWarningEventHandler();
    [Signal]
    public delegate void RechargingEventHandler();
    [Signal]
    public delegate void OnHitEventHandler();

    private PackedScene packedSceneBossBullet = (PackedScene)GD.Load("res://src/weapons/BossBullet.tscn");

    private AudioStreamPlayer2D bossSound;

    private const float DefaultReloadTime = 0.7f;
    private float leftBorder = 0.0f;
    private float rightBorder = 0.0f;
    private float screenHeight;

    private int normalAttackBulletCount = 5;
    private int normalAttackBulletSpeed = 400;

    private bool isSpreadingAttackTriggered = false;
    private bool secondPhase = false;
    private bool stopAttack = false;

    public float reloadTime;
    public float reloading = 0.0f;

    public bool bossHasLost = false;

    private AttackType pickedWeapon = AttackType.Normal;
    private Vector2 oldVelocity;
    public Vector2 velocity = new Vector2(100, 10);

    private enum AttackType
    {
        Normal,
        SpreadLeftRight,
        BurstAttack,
        Recharging
    }

    private readonly AttackType[] weaponsCycle = { AttackType.SpreadLeftRight, AttackType.BurstAttack, AttackType.Recharging };

    public override void _Ready()
    {
        packedSceneParticle = (PackedScene)GD.Load("res://src/explosion/BossExplosion.tscn");
        var healthBar = GetNode<ProgressBar>("ProgressBar");
        healthBar.MaxValue = 100;
        this.lives = 100;
        healthBar.Value = this.lives;

        Area2D node = GetNode<Area2D>("BossArea2D");
        node.AreaEntered += this.Hit;

        var screenSize = this.GetViewport().GetVisibleRect().Size;
        var screenWidth = screenSize.X;
        this.screenHeight = screenSize.Y;

        this.leftBorder = this.Texture.GetWidth() / 4;
        this.rightBorder = screenWidth - this.Texture.GetWidth() / 4;
        reloadTime = DefaultReloadTime;
        bossSound = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        bossSound.Play();
    }

    public void CheckForWin()
    {
        if (this.GlobalPosition.Y >= this.screenHeight)
        {
            this.EmitSignal("BossVictory", false);
        }
        if (!this.secondPhase && this.lives <= 50)
        {
            SwitchToSecondPhase();
        }
    }

    private void SwitchToSecondPhase()
    {
        this.secondPhase = true;
        this.normalAttackBulletCount++;
        this.normalAttackBulletSpeed *= 2;
    }

    public bool HitLeftBorder() => this.velocity.X < 0.0 && this.Position.X <= leftBorder;

    public bool HitRightBorder() => this.velocity.X > 0.0 && this.Position.X >= rightBorder;

    public void CheckBorderReached()
    {
        if (this.HitLeftBorder() || this.HitRightBorder())
        {
            this.velocity.X = -(this.velocity.X);
        }
    }

    public void RestoreOldVelocity() => this.velocity = this.oldVelocity;

    public async void OnTriggerBossTimerTimeout()
    {
        GetNode<Timer>("TriggerBossTimer").Stop();
        GetNode<Timer>("TriggerBossTimer").Start();
        GetNode<Timer>("TriggerBossTimer").Autostart = false;

        this.oldVelocity = this.velocity;
        this.velocity = Vector2.Zero;
        this.StopAttackTrigger();
        await ToSignal(GetTree().CreateTimer(5), "timeout");
        RestoreOldVelocity();
        this.stopAttack = false;
        this.pickedWeapon = AttackType.Normal;
    }

    public void Shoot()
    {
        var bullet = (BossBullet)packedSceneBossBullet.Instantiate();

        if (reloading > 0.0f)
            return;

        this.GetParent().AddChild(bullet);
        bullet.GlobalPosition = new Vector2(this.GlobalPosition.X, this.GlobalPosition.Y);

        bullet.Velocity = new Vector2(0, 400);
        this.reloadTime = 0.1f;
        this.reloading = reloadTime;
    }

    public void SpreadShoot(int bulletCount, int bulletSpread, int speed)
    {
        if (this.reloading > 0.0f)
            return;

        int position = -50;
        int bulletVelocity = -100;
        for (int i = 0; i < bulletCount; i++)
        {
            var bullet = (BossBullet)packedSceneBossBullet.Instantiate();
            bulletVelocity += bulletSpread;
            position += 5;
            this.GetParent().AddChild(bullet);
            float xPosition = this.pickedWeapon == AttackType.BurstAttack && this.pickedWeapon != AttackType.SpreadLeftRight && this.stopAttack ? this.GlobalPosition.X - 75 : this.GlobalPosition.X;
            float yPosition = this.pickedWeapon == AttackType.BurstAttack && this.stopAttack && this.pickedWeapon != AttackType.SpreadLeftRight ? this.GlobalPosition.Y - 20 : this.GlobalPosition.Y + 20;
            bullet.GlobalPosition = new Vector2(xPosition, yPosition);
            bullet.Velocity = new Vector2(bulletVelocity, speed);
            this.reloadTime = DefaultReloadTime;
        }

        reloading = reloadTime;
    }

    public void Move(float delta)
    {
        this.Position += this.velocity * delta;
    }

    public override void Hit(Area2D obj)
    {
        var name = obj.Name.ToString();

        if (!name.Contains("LaserArea") && !name.Contains("RegularBulletArea2D") ||
            name.Contains("EnemyLaser") || name.Contains("InvaderArea") || name.Contains("BossArea2D"))
            return;

        EmitSignal(nameof(OnHit));
        dynamic bulletDamage = obj.GetParent<Sprite2D>();
        this.lives -= bulletDamage.Damage;
        this.HookBossLives((int)bulletDamage.Damage);
        this.EnemyBossHelper(packedSceneFloatingText, FloatingTextTypes.ENEMYHIT, (int)bulletDamage.Damage, this.lives);

        if (this.lives <= 0)
        {
            if (bossHasLost)
                return;

            this.EnemyPlayerBossHelper(packedSceneFloatingText, FloatingTextTypes.POINTS, PointSystem.BossKillBonus, this.lives);
            bossHasLost = true;
            var explosion = (BossExplosion)this.packedSceneParticle.Instantiate();
            explosion.GlobalPosition = this.Position;
            ScoreHandler.BossKilled();
            this.GetParent().GetParent().AddChild(explosion);
        }
        else
        {
            this.startAnimationHit = true;
        }
    }

    protected override void FlickerOnHit(float delta)
    {
        if (!this.startAnimationHit)
            return;

        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(1f, 0, 0, 0.4f), 0.3f);
        tween.TweenProperty(this, "modulate", Colors.White, 0.15f);
        tween.TweenCallback(Callable.From(() => startAnimationHit = false));
    }

    public override void _Process(double delta)
    {
        this.reloading -= (float)delta;
        this.ActionsPerAttackType();
        this.CheckForWin();
        this.FlickerOnHit((float)delta);
        this.Move((float)delta);
        this.CheckBorderReached();
    }

    public void HookBossLives(int value)
    {
        ProgressBar healthBar = GetNode<ProgressBar>("ProgressBar");
        healthBar.Value -= value;
    }

    private void ActionsPerAttackType()
    {
        if (pickedWeapon != AttackType.SpreadLeftRight)
        {
            isSpreadingAttackTriggered = false;
        }

        switch (pickedWeapon)
        {
            case AttackType.Normal:
                SpreadShoot(normalAttackBulletCount, 50, normalAttackBulletSpeed);
                break;
            case AttackType.BurstAttack:
                SpreadShoot(25, 25, 500);
                break;
            case AttackType.Recharging:
                EmitSignal(nameof(Recharging));
                break;
            case AttackType.SpreadLeftRight:
                if (!isSpreadingAttackTriggered)
                {
                    EmitSignal(nameof(BurstWarning)); //emit event to inform user to stay away, will be called from main and it will call method from HUD 
                    isSpreadingAttackTriggered = true;
                    StartMovingRandomDirection();
                }
                this.Shoot();
                break;
        }
    }

    private void StartMovingRandomDirection()
    {
        var rnd = new Random();
        var randomBool = rnd.Next(2) == 1;
        this.velocity = new Vector2(randomBool ? -600 : 600, 2);
    }

    private void StopAttackTrigger()
    {
        this.stopAttack = true;
        Random rnd = new Random();
        int pickedWeaponIndex = rnd.Next(0, weaponsCycle.Length);
        this.pickedWeapon = weaponsCycle[pickedWeaponIndex];
    }
}
