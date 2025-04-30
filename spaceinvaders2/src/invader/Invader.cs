using Godot;
using PrototypeSpaceInvaders.Extensions;
using PrototypeSpaceInvaders.src.invader;
using SpaceInvaders;

public partial class Invader : SpaceObject
{
    [Signal]
    public delegate void OnHitEventHandler();

    private PackedScene packedScene = (PackedScene)GD.Load("res://src/weapons/EnemyLaser.tscn");
    private AudioStreamPlayer2D onHitSound;
    private bool invaderInScreen=false;
    private VisibleOnScreenNotifier2D InvaderInScreenNotifier;
	public void invaderEntered() => invaderInScreen=true;
	
    public override void _Process(double delta) => this.FlickerOnHit((float)delta);

    public override void _Ready()
    {
        Area2D node = GetNode<Area2D>("InvaderArea");
        node.AreaEntered += this.Hit;
        onHitSound = GetNode<AudioStreamPlayer2D>("onHit");
        InvaderInScreenNotifier = GetNode<VisibleOnScreenNotifier2D>("InvaderInScreenNotifier");
        InvaderInScreenNotifier.ScreenEntered += this.invaderEntered;
        lives = 2;
    }
    
    public override void Hit(Area2D obj)
    {
        GpuParticles2D explosion = (GpuParticles2D)this.packedSceneParticle.Instantiate();
        var name = obj.Name.ToString();

        if ((name.Contains("LaserArea") || name.Contains("RegularBulletArea2D")) &&
            !name.Contains("EnemyLaser") && !name.Contains("InvaderArea") && invaderInScreen)
        {
            dynamic bullet = obj.GetParent<Sprite2D>();
            this.lives -= bullet.Damage;
            this.EnemyHelper(packedSceneFloatingText, FloatingTextTypes.ENEMYHIT, (int)bullet.Damage, this.lives);
            this.EmitSignal(nameof(OnHit));

            if (this.lives <= 0)
            {
                onHitSound.Play();
                this.EnemyHelperPlayerAction(this.packedSceneFloatingText, FloatingTextTypes.POINTS, PointSystem.InvaderKillBonus, this.lives, (int)bullet.Shooter);
                ScoreHandler.InvaderKilled();

                explosion.GlobalPosition = this.Position;
                this.GetParent().AddChild(explosion);
                this.QueueFree();
            }
            else
            {
                this.startAnimationHit = true;
                onHitSound.Play();
            }
        }
    }
}
