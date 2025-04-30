using Godot;

namespace PrototypeSpaceInvaders.src.interfaces
{
    public interface IAsteroidManager
    {
        void SpawnAsteroids(bool isRandomAsteroidScale, bool isHigherAmountAsteroids);
        int GenerateAsteroidValue(bool isHigherAmountAsteroids);
        PackedScene AsteroidScene { get; }
    }
}
