using Godot;

public partial class ExplosionPowerup : GpuParticles2D
{
    public void DestroyParticle()
    {
        this.QueueFree();
    }
}
