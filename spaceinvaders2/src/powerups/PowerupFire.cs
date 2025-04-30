using Godot;

public partial class PowerupFire : Entity
{
    private Vector2 velocity = new Vector2(0, 200);
    public override Vector2 Velocity { get => velocity; set => velocity = value; }
    private AudioStreamPlayer2D PowerUpFly;

    public override void _Ready()
    {
        base._Ready();
        var node = this.GetNode<Area2D>("PowerupFire");
        node.AreaEntered += this.hit;
        PowerUpFly = GetNode<AudioStreamPlayer2D>("PowerUpFly");
        PowerUpFly.Play();
    }

    public override void hit(Area2D obj)
    {
        if (obj.Name.Equals("Area2D"))
        {
            this.QueueFree();
        }
    }
}
