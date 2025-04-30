using Godot;

public partial class BossExplosion : AnimatedSprite2D
{
	[Signal]
	public delegate void ExplosionFinishedEventHandler();

	public override void _Ready()
	{
		var timerNode = GetNode<Timer>("ExplosionDurationTimer");
		timerNode.Timeout += Timeout;
	}

	public void Timeout()
	{
		this.EmitSignal(nameof(ExplosionFinished));

		this.QueueFree();
	}
}
