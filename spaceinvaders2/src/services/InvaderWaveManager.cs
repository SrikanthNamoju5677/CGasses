using Godot;
using PrototypeSpaceInvaders.src.interfaces;
using System.Collections.Generic;

namespace PrototypeSpaceInvaders.src.services
{
    internal class InvaderWaveManager : IInvaderWaveManager
    {

        public PackedScene InvadersScene => (PackedScene)GD.Load("res://src/invader/Invaders.tscn");
        private Level level;

        public InvaderWaveManager(Level level)
        {
            this.level = level;
        }

        public void ClearAllInvaders(Invaders invaders)
        {
            invaders.IsConnected("InvadersWon", new Callable(level.GetParent(), "GameOver"));
        }

        public void ClearAllInvaders(List<Invaders> invadersCollection)
        {
            foreach (Invaders invaders in invadersCollection)
            {
                invaders.IsConnected("InvadersWon", new Callable(level.GetParent(), "GameOver"));
            }
        }

        public Invaders SpawnInvaders()
        {
            Invaders invaderWave = (Invaders)InvadersScene.Instantiate();
            invaderWave.direction = level is FirstLevel ? new Vector2(300, 0) : new Vector2(500, 0);

            var grid = level is FirstLevel ? new Vector2(8, 4) : new Vector2(12, 6);
            invaderWave.AddAsGrid(grid);
            invaderWave.Position = level is FirstLevel ? new Vector2(0, -250) : new Vector2(0, -400);
            level.AddChild(invaderWave);

            //When the invaderWave have won (i.e. have reached the bottom of the screen), we call 'gameover' 
            invaderWave.InvadersWon += () => (level.GetParent() as MainGameOverlay).GameOver(false);
            invaderWave.OnHit += () => (level.GetParent() as MainGameOverlay).TargetOnHit();

            return invaderWave;
        }
    }
}
