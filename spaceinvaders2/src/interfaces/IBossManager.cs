using Godot;

namespace PrototypeSpaceInvaders.src.interfaces
{
    public interface IBossManager
    {
        PackedScene BossPackedScene { get; }
        void ShowBurstWarningMessage();
        void ShowBossRechargingMessage();
        void SpawnBoss(ref BossSprite boss);
    }
}
