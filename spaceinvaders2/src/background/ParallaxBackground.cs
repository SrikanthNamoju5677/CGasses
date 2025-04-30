using Godot;

public partial class ParallaxBackground : Godot.ParallaxBackground
{
    private float scrollSpeed;
    public float ScrollSpeed
    {
        get { return scrollSpeed; }
        set { scrollSpeed = value; }
    }
    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
        var velocity = new Vector2(0, 1);
        ScrollOffset += velocity * ScrollSpeed;
    }
}
