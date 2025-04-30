using Godot;
using System.Linq;

public partial class Invaders : Node2D
{
    [Signal]
    public delegate void InvadersWonEventHandler();
    [Signal]
    public delegate void OnHitEventHandler();

    private PackedScene packedScene = (PackedScene)GD.Load("res://src/invader/Invader.tscn");
    private PackedScene packedSceneFloatingText = (PackedScene)GD.Load("res://src/messages/FloatingText.tscn");

    private Vector2 screenSize;
    private float screenWidth, screenHeight;

    public bool isSecondWave = false;
    public bool didWin = false;
    public Vector2 direction, position;

    private AudioStreamPlayer2D invadersSound;

    public override void _Ready()
    {
        this.screenSize = this.GetViewport().GetVisibleRect().Size;
        this.screenWidth = this.screenSize.X;
        this.screenHeight = this.screenSize.Y;
        invadersSound = GetNode<AudioStreamPlayer2D>("invadersSound");
    }

    public void AddAsGrid(Vector2 size)
    {
        this.isSecondWave = false;
        var position = new Vector2();

        for (int i = 0; i < size.Y; i++)
        {
            position.X = 0;
            position.Y = i * 70;

            for (int x = 0; x < size.X; x++)
            {
                position.X = x * 70;
                Invader invader = (Invader)this.packedScene.Instantiate();
                invader.Position = position;
                invader.OnHit += OnHitHandler;
                this.AddChild(invader);
            }
        }
    }

    private void OnHitHandler() => EmitSignal(nameof(OnHit));

    public void PlayInvadersSound()
    {
        if (!invadersSound.Playing)
        {
            invadersSound.Play();
        }
    }

    public void AddAsGridSecondWave(Vector2 size)
    {
        this.isSecondWave = true;
        var position = new Vector2(0, 0);

        for (int i = 0; i < size.Y; i++)
        {
            position.X = 0;
            position.Y = i * 70;

            for (int x = 0; x < size.X; x++)
            {
                position.X = x * 90;
                Invader invader = (Invader)this.packedScene.Instantiate();
                invader.Position = position;
                invader.OnHit += OnHitHandler;
                this.Position = new Vector2(0, -450);
                this.AddChild(invader);
            }
        }
    }

    public void CheckForWin()
    {
        foreach (Invader item in this.GetTree().GetNodesInGroup("Invaders"))
        {
            if (item.GlobalPosition.Y >= this.screenHeight && !this.didWin)
            {
                this.didWin = true;
                this.EmitSignal(SignalName.InvadersWon);
            }
        }
    }

    public bool HitBottomBorder(Invader invader)
    {
        return invader.GlobalPosition.Y >= this.screenHeight;
    }

    public void MoveFormation(float delta)
    {
        this.Position += this.direction * delta;
    }

    public void CheckBorderReached()
    {
        foreach (var item in this.GetChildren().OfType<Invader>())
        {
            if ((this.HitLeftBorder(item) || this.HitRightBorder(item)) && !item.Name.ToString().Contains("EnemyLaser"))
            {
                this.direction.X = -this.direction.X;

                if (this.isSecondWave)
                {
                    this.direction.Y = 13.0f;
                }
                else
                {
                    this.direction.Y = 20.0f;
                }
                break;
            }
        }
    }

    public void InvadersInScreen()
    {

        var targets = GetTree().GetNodesInGroup("Invaders").ToList();

        if (targets.Count != 0)
        {
            var closestTargetHeight = targets.Cast<Node>().OfType<Node2D>().Max(node => node.GlobalPosition.Y);
            if (closestTargetHeight > -5)
            {
                this.PlayInvadersSound();
            }
        }
    }

    public bool HitLeftBorder(Invader invader)
    {
        return direction.X < 0.0 && !invader.Name.ToString().Contains("EnemyLaser") && invader.GlobalPosition.X < 0;
    }

    public bool HitRightBorder(Invader invader)
    {
        return direction.X > 0.0 && !invader.Name.ToString().Contains("EnemyLaser") && invader.GlobalPosition.X > this.screenWidth;
    }

    public override void _Process(double delta)
    {
        this.MoveFormation((float)delta);
        this.CheckBorderReached();
        this.InvadersInScreen();
        this.CheckForWin();
    }
}
