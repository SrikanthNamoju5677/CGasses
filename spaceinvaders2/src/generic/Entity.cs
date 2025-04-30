using Godot;

public abstract partial class Entity : Sprite2D
{
    [Export]
    public abstract Vector2 Velocity { get; set; }
    private Vector2 screenSize;

    public override void _Ready()
    {
        this.screenSize = this.GetViewport().GetVisibleRect().Size;
    }

    public virtual void hit(Area2D obj)
    {
        if (obj.Name.Equals("InvaderArea") || obj.Name.Equals("BossArea2D") || obj.Name.Equals("EnemyLaserArea"))
        {
            this.QueueFree();
        }
    }

    public void Move(double delta)
    {
        this.GlobalPosition += this.Velocity * (float)delta;
    }

    public void RemoveFromScreen()
    {
        if (this.GlobalPosition.Y < 0 || this.GlobalPosition.Y > screenSize.Y)
        {
            this.QueueFree();
        }
    }

    public override void _Process(double delta)
    {
        this.Move(delta);
        this.RemoveFromScreen();
    }
}
