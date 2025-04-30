using Godot;
using PrototypeSpaceInvaders.Extensions;
using PrototypeSpaceInvaders.src.invader;
using SpaceInvaders;

public partial class EnemyLaser : SpaceObject
{
    public Vector2 velocity = new Vector2(0, -200);
    public float screenHeight;

    public override void _Ready()
    {
        Area2D node = this.GetNode<Area2D>("EnemyLaserArea");
        node.AreaEntered += this.Hit;
        this.screenHeight = this.GetViewport().GetVisibleRect().Size.Y;
    }

    public override void Hit(Area2D obj)
    {
        if (obj.Name.Equals("Area2D"))
        {
            this.QueueFree();
        }
        else if (obj.Name.Equals("RegularBulletArea2D") || obj.Name.Equals("LaserArea"))
        {
            GpuParticles2D explosion = (GpuParticles2D)this.packedSceneParticle.Instantiate();
            dynamic bullet = obj.GetParent<Sprite2D>();
            this.lives -= bullet.Damage;
            this.EnemyHelper(packedSceneFloatingText, FloatingTextTypes.ENEMYHIT, (int)bullet.Damage, lives);

            if (this.lives <= 0)
            {
                explosion.GlobalPosition = this.Position;
                this.GetParent().AddChild(explosion);
                this.QueueFree();
            }
            else
            {
                this.startAnimationHit = true;
            }
        }
    }

    public void CheckAndRemoveOffScreen()
    {
        if (this.GlobalPosition.Y > screenHeight)
        {
            this.QueueFree();
        }
    }

    public void Move(double delta)
    {
        this.GlobalPosition -= this.velocity * (float)delta;
    }

    public override void _Process(double delta)
    {
        this.Move(delta);
        this.CheckAndRemoveOffScreen();
        FlickerOnHit((float)delta);
    }

    public void SetLives(int lives)
    {
        this.lives = lives;
    }
}
