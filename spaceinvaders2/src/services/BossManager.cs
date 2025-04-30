using Godot;
using PrototypeSpaceInvaders.src.interfaces;
using SpaceInvaders;

namespace PrototypeSpaceInvaders.src.services
{
    internal class BossManager : IBossManager
    {
        public PackedScene BossPackedScene => (PackedScene)GD.Load("res://src/boss/BossSprite.tscn");
        private readonly Level level;

        public BossManager(Level level)
        {
            this.level = level;
        }

        public void ShowBurstWarningMessage()
        {
            level.EmitSignal(nameof(level.SendMessage), "The boss is about to shoot in a spread pattern,\n avoid the bullets!", false, (int)MessagePriority.lEVELWARNING);
        }

        public void ShowBossRechargingMessage()
        {
            level.EmitSignal(nameof(level.SendMessage), "The boss is recharging, shoot him down quickly!", false, (int)MessagePriority.lEVELWARNING);
        }

        public void SpawnBoss(ref BossSprite boss)
        {
            //Add a boss to the level and set its position
            boss = (BossSprite)BossPackedScene.Instantiate();
            level.AddChild(boss);
            boss.Position = new Vector2(800, 200);

            //Call gameOver when the boss has won 
            boss.BossVictory += () => (level.GetParent() as MainGameOverlay).GameOver(false);
            boss.BurstWarning += this.ShowBurstWarningMessage;
            boss.Recharging += this.ShowBossRechargingMessage;
            boss.OnHit += () => (level.GetParent() as MainGameOverlay).TargetOnHit();

        }
    }
}
