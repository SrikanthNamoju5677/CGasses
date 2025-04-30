using Godot;

public abstract partial class Bullet : Entity
{
    private Vector2 velocity = new Vector2(0, -300);
    private const int defaultDamage = 2;
    public override Vector2 Velocity { get => velocity; set => velocity = value; }
    public virtual int Damage { get; set;} = defaultDamage;
    public int Shooter { get; set; }

    protected abstract NodePath BulletType { get; }

    public void UpdateTexture(char letter)
    {
        string letterTexturePath = $"res://assets/Letters/{letter}.png";
        this.Texture = (Texture2D)GD.Load(letterTexturePath);
        // ScaleDown();
    }

    private void ScaleDown()
    {
        this.Scale = this.Scale * 0.8f;
    }

    public override void _Ready()
    {
        base._Ready();
        Area2D node = GetNode<Area2D>(BulletType);
        node.AreaEntered += this.hit;
        var bulletSound = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        bulletSound.Play();
    }
}
