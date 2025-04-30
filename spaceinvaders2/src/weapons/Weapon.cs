using Godot;
using SpaceInvaders;

public abstract partial class Weapon : Node
{
    private float currentReloadTime, reloadTime;

    //The weapon is reloading when the current reload time is larger than zero
    public bool Reloading => currentReloadTime > 0.0;

    public virtual bool Overheated
    {
        // A weapon by default will not overheat
        get => false;
    }

    protected abstract PackedScene BulletType { get; }

    protected abstract string ReloadTimerConfigurationKey { get; }

    protected abstract void WeaponFireEffect(Player Player, Vector2 gunPosition, Bullet bullet);

    public override void _Ready()
    {
        ConfigFile config = ConfigHelper.LoadConfigFile();
        reloadTime = (float)config.GetValue("Gameplay", ReloadTimerConfigurationKey);
        currentReloadTime = 0.0f;
    }

    public virtual void Fire(Player Player, Vector2 gunPosition)
    {
        //If the weapon is reloading or Overheated, the player cannot shoot this weapon
        if (Reloading || Overheated)
        {
            return;
        }
        Bullet bullet = CreateBullet(gunPosition, Player.PlayerNumber);

        this.WeaponFireEffect(Player, gunPosition, bullet);

        Shoot(Player, bullet);

        //The weapon now needs to reload
        currentReloadTime = reloadTime;
    }

    protected virtual Bullet CreateBullet(Vector2 gunPosition, int playerNumber)
    {
        Bullet bullet = (Bullet)BulletType.Instantiate();
        bullet.GlobalPosition = gunPosition;
        bullet.Shooter = playerNumber;
        return bullet;
    }

    protected void Shoot(Player player, Sprite2D bullet)
    {
        player.GetParent().AddChild(bullet);
        player.ProcessShot();
    }

    public virtual void ProcessWeapon(float delta, Player player)
    {
        currentReloadTime -= delta;
    }
}
