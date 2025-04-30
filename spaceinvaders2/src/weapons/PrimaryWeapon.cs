using Godot;
using SpaceInvaders;
using System.Collections.Generic;

public partial class PrimaryWeapon : Weapon
{
    [Signal]
    public delegate void FirePowerUpTimeoutEventHandler();
    private int nrPoweredUpBullets = 2;

    private float timeToNextWord, reloadTime;
    private float timeSinceLastShot = 0f;


    //The primary weapon can benefit from fire power-ups
    private bool poweredUp = false;
    private bool isWordComplete = false;
    private bool reloadingNextWord => timeToNextWord > 0;

    private List<string> words = new List<string> { "ALTEN", "NEDERLAND" };
    private string currentWord = "ALTEN";
    private int currentLetterIndex = 0;
    private Bullet currentLetterBullet;

    //Timer that defines how long a fire power-up lasts
    private Timer powerUpTimer;

    protected override PackedScene BulletType
    {
        get => (PackedScene)GD.Load("res://src/weapons/RegularBullet.tscn");
    }

    protected override string ReloadTimerConfigurationKey
    {
        get => "PrimaryWeaponReloadTimer";
    }

    public override void _Ready()
    {
        base._Ready();

        ConfigFile config = ConfigHelper.LoadConfigFile();
        reloadTime = (float)config.GetValue("Gameplay", "WordIntervalTime");

        powerUpTimer = new Godot.Timer();
        powerUpTimer.WaitTime = 6;
        this.AddChild(powerUpTimer);

        powerUpTimer.Timeout += this.PowerUpTimeout;
    }

    public void PowerUpTimeout()
    {
        this.EmitSignal(nameof(FirePowerUpTimeout));
        //Called when the powerup timer has a timeout
        poweredUp = false;
        powerUpTimer.Stop();
    }

    public void TriggerPowerupShooting()
    {
        poweredUp = true;
        powerUpTimer.Start();
    }

    protected override void WeaponFireEffect(Player Player, Vector2 gunPosition, Bullet centerBullet=null)
    {
        if (!poweredUp)
            return;

        // If the weapon is powered up the weapon shoots more bullets at once in several directions
        //TODO: better code?
        int bulletVelocity = -100;
        
        for (int i = 0; i < nrPoweredUpBullets; i++)
        {
            RegularBullet bullet = (RegularBullet)BulletType.Instantiate();
            bullet.Texture = currentLetterBullet.Texture;
            bullet.GlobalPosition = gunPosition;
            bullet.Shooter = Player.PlayerNumber;
            bullet.Velocity = new Vector2(bulletVelocity, bullet.Velocity.Y);
            bullet.RotationDegrees = 0;

            Shoot(Player, bullet);

            bulletVelocity += 200;
        }
    }

    public override void Fire(Player Player, Vector2 gunPosition)
    {
        if (reloadingNextWord)
            return;

        // If more than 1 second since last shot, start a new word
        if (timeSinceLastShot > 1f)
        {
            DetermineCurrentWordAndLetter();
        }

        base.Fire(Player, gunPosition);
        timeSinceLastShot = 0f;

        // Determine the current word and letter if the previous word was completed
        if (isWordComplete)
        {
            DetermineCurrentWordAndLetter();
            timeToNextWord = reloadTime;
        }
    }

    public override void ProcessWeapon(float delta, Player player)
    {
        base.ProcessWeapon(delta, player);
        timeToNextWord -= delta;
        timeSinceLastShot += delta;
    }

    protected override Bullet CreateBullet(Vector2 gunPosition, int playerNumber)
    {
        char currentLetter = currentWord[currentLetterIndex];

        Bullet bullet = base.CreateBullet(gunPosition, playerNumber);

        bullet.UpdateTexture(currentLetter);
        bullet.Rotation = 0;
        currentLetterBullet = bullet;

        // Update current letter index for next fire
        currentLetterIndex = (currentLetterIndex + 1) % currentWord.Length;

        isWordComplete = (currentLetterIndex == 0);  // Set to true if wrapped around, indicating word is complete

        return bullet;
    }

    private void DetermineCurrentWordAndLetter()
    {
        // Transition to the next word and reset the letter index
        int wordIndex = (words.IndexOf(currentWord) + 1) % words.Count;
        currentWord = words[wordIndex];
        currentLetterIndex = 0;  // Reset letter index for the new word
    }
}
