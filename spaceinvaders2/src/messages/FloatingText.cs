using Godot;
using SpaceInvaders;
using System;

public partial class FloatingText : Marker2D
{
    public Vector2 Velocity { get; set; }
    public int LabelDamage { get; set; } = 0;
    public string EnemyDeathString { get; set; }
    public int Points { get; set; }
    public FloatingTextTypes FTextTypes { get; set; }
    public bool EnemyIsDead { get; set; }
    public float Xmovement { get; set; }
    public float Ymovement { get; set; }
    public EntityType Entity { get; set; }

    public void SetHitObjectType(EntityType type, Label label, Label labelPoints)
    {
        Random rnd = new Random();
        if (type == EntityType.PLAYER)
        {
            this.Xmovement = rnd.Next(-40, 41);
            this.Ymovement = rnd.Next(50, 100);
            switch (FTextTypes)
            {
                case FloatingTextTypes.HITLIFE:
                    label.Text = "-" + LabelDamage.ToString();
                    label.Modulate = new Color((float)1.0, (float)0.0, (float)0.0, (float)1.0);
                    break;
                case FloatingTextTypes.HEALING:
                    label.Modulate = new Color((float)0.119141, (float)1, (float)0.0);
                    labelPoints.Text = $"+ {this.Points} points";
                    label.Text = "+" + LabelDamage.ToString();
                    break;
                case FloatingTextTypes.HITSHIELD:
                    label.Text = "-" + LabelDamage.ToString();
                    label.Modulate = new Color((float)0.0, (float)0.87, (float)0.96, (float)1.0);
                    break;
                case FloatingTextTypes.POWERUPSHIELD:
                    label.Modulate = new Color((float)0.119141, (float)1, (float)0.0);
                    labelPoints.Text = $"+ €{this.Points}";
                    break;
                case FloatingTextTypes.POWERUPFIRE:
                    label.Modulate = new Color((float)0.119141, (float)1, (float)0.0);
                    labelPoints.Text = $"+ €{this.Points}";
                    break;
                case FloatingTextTypes.POINTS:
                    label.Modulate = new Color((float)0.119141, (float)1, (float)0.0);
                    labelPoints.Text = $"+ €{this.Points}";
                    break;
            }
        }
        else
        {
            this.Ymovement = rnd.Next(0, 100);
            this.Xmovement = rnd.Next(50, 100);
            switch (FTextTypes)
            {

                case FloatingTextTypes.ENEMYHIT:
                    if (EnemyIsDead)
                    {

                    }
                    label.Text = "-" + LabelDamage.ToString();

                    label.Modulate = new Color((float)1.0, (float)0.0, (float)0.0, (float)1.0);
                    break;


            }
        }
    }

    public override void _Ready()
    {
        Velocity = new Vector2(0.0f, 0.0f);
        var label = GetNode<Label>("Damage");
        var labelPoints = GetNode<Label>("PointsText");
        Velocity = new Vector2(this.Xmovement, this.Ymovement);
        this.SetHitObjectType(Entity, label, labelPoints);
        var tween = CreateTween();
        tween.TweenProperty(this, "scale", new Vector2(1, 1), 0.2f).SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(this, "scale", new Vector2(0.1f, 0.1f), 0.7f).SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In).SetDelay(0.3f);
        tween.TweenCallback(Callable.From(() => this.QueueFree()));
    }

    public override void _Process(double delta)
    {
        this.Position -= Velocity * (float)delta;
    }
}
