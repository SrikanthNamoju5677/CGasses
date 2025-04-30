using Godot;

public partial class SecondaryBullet : Bullet
{
    public override int Damage {get; set;} = 1;
    protected override NodePath BulletType => "LaserArea";
    public int NewDamage{
        get {return Damage;}
        set {Damage = value;}
    }
}
