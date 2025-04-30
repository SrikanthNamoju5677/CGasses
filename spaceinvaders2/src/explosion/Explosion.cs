using Godot;

public partial class Explosion : GpuParticles2D
{
    public void DestroyParticle()
    {
        this.QueueFree();
    }
}
