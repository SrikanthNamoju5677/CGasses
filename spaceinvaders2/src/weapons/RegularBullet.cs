using Godot;

public partial class RegularBullet : Bullet
{
    protected override NodePath BulletType => "RegularBulletArea2D";
}
