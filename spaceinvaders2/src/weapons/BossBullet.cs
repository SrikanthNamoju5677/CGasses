using Godot;

public partial class BossBullet : Bullet
{
    protected override NodePath BulletType => "BossBulletArea";

    public override void hit(Area2D obj)
    {
        if (obj.Name.Equals("Area2D"))
        {
            this.QueueFree();
        }
    }
}
