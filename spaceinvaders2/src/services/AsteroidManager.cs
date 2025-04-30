using Godot;
using PrototypeSpaceInvaders.src.interfaces;
using System;
using System.Collections.Generic;

namespace PrototypeSpaceInvaders.src.services
{
    internal class AsteroidManager : IAsteroidManager
    {
        private Level level;

        public PackedScene AsteroidScene => (PackedScene)GD.Load("res://src/weapons/EnemyLaser.tscn");

        protected static Random rnd = new Random();

        protected static List<string> Icons = new() { "warning", "error"};

        public AsteroidManager(Level level)
        {
            this.level = level;
        }

        public int GenerateAsteroidValue(bool isHigherAmountAsteroid)
        {
            int randomFrom = isHigherAmountAsteroid ? rnd.Next(1, 3) : rnd.Next(1, 5);
            int randomTo = isHigherAmountAsteroid ? rnd.Next(7, 18) : rnd.Next(5, 15);
            int enemyNumbers = randomTo - randomFrom;
            return enemyNumbers;
        }

        public void SpawnAsteroids(bool isRandomAsteroidScale, bool isHigherAmountAsteroid)
        {
            //Add a random number of projectiles at random positions to the level
            int velocity = rnd.Next(700, 750);
            int asteroidNumber = GenerateAsteroidValue(isHigherAmountAsteroid);

            for (int i = 0; i < asteroidNumber; i++)
            {
                int randomPositionX = rnd.Next(0, 1920);
                int randomPositionY = rnd.Next(0, 50);
                int asteroidSelector = i % 2;
                string Icon = Icons[asteroidSelector];
                int velocityBoost = asteroidSelector * 300;         // if the asteroid is error, 100 velocity boost; if it is warning, no boost.
                Vector2 randomAsteroidPosition = new Vector2(randomPositionX, randomPositionY);

                EnemyLaser nextAsteroid = (EnemyLaser)this.AsteroidScene.Instantiate();
                nextAsteroid.Position = randomAsteroidPosition;
                nextAsteroid.velocity = new Vector2(0, -velocity-velocityBoost);

                string AsteroidTexturePath = $"res://assets/asteroids/small/{Icon}.svg";
                nextAsteroid.Texture = (Texture2D)GD.Load(AsteroidTexturePath);
                nextAsteroid.SetLives(6);

                if (isRandomAsteroidScale)
                {
                    double randomScale = rnd.NextDouble() * (0.9) + 0.6;
                    nextAsteroid.Scale = new Vector2((float)randomScale, (float)randomScale);

                    int lives = (int)(randomScale * 10);
                    nextAsteroid.SetLives(lives);
                }

                level.AddChild(nextAsteroid);
            }
        }
    }
}
