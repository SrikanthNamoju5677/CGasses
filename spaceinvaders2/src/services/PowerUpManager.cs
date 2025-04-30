using Godot;
using PrototypeSpaceInvaders.src.interfaces;
using System;

namespace PrototypeSpaceInvaders.src.services
{
    internal class PowerUpManager : IPowerUpManager
    {
        private Level level;

        public PackedScene FirePowerUpScene => (PackedScene)GD.Load("res://src/powerups/PowerupFire.tscn");
        public PackedScene HealthPowerUpScene => (PackedScene)GD.Load("res://src/powerups/PowerupHealth.tscn");

        public PowerUpManager(Level level)
        {
            this.level = level;
        }

        public void SpawnPowerups()
        {
            {
                Random rnd = new Random();
                int randomPowerup = rnd.Next(1, 3);
                switch (randomPowerup)
                {
                    //Spawn a fire powerup
                    case 1:
                        int randomValX = rnd.Next(0, 1900);
                        int randomValPositionY = rnd.Next(10, 50);

                        PowerupFire powerup = (PowerupFire)FirePowerUpScene.Instantiate();
                        powerup.Position = new Vector2(randomValX, randomValPositionY);
                        level.AddChild(powerup);
                        break;
                    //Spawn a shield powerup
                    case 2:
                        int randomValXHealthPowerup = rnd.Next(0, 1900);
                        int randomValPositionYHealthPowerup = rnd.Next(10, 50);

                        PowerupHealth powerupHealth = (PowerupHealth)HealthPowerUpScene.Instantiate();
                        powerupHealth.Position = new Vector2(randomValXHealthPowerup, randomValPositionYHealthPowerup);
                        level.AddChild(powerupHealth);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
