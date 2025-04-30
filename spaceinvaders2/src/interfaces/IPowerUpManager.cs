using Godot;

namespace PrototypeSpaceInvaders.src.interfaces
{
    public interface IPowerUpManager
    {
        PackedScene FirePowerUpScene { get; }
        PackedScene HealthPowerUpScene { get; }
        public void SpawnPowerups();

    }
}
