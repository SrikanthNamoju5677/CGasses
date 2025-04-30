using System.Collections.Generic;

namespace PrototypeSpaceInvaders.src.interfaces
{
    public interface IInvaderWaveManager
    {
        Invaders SpawnInvaders();
        void ClearAllInvaders(Invaders invadersCollection);
        void ClearAllInvaders(List<Invaders> invadersCollection);
        Godot.PackedScene InvadersScene { get; }

    }
}
