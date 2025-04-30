using Godot;

namespace PrototypeSpaceInvaders.src.invader
{
    public abstract partial class SpaceObject : Sprite2D
    {
        protected PackedScene packedSceneParticle = (PackedScene)GD.Load("res://src/explosion/Explosion.tscn");
        protected PackedScene packedSceneFloatingText = (PackedScene)GD.Load("res://src/messages/FloatingText.tscn");

        protected int lives;
        protected bool startAnimationHit = false;
        protected float startDecreaseColor = 1;

        public abstract void Hit(Area2D obj);

        protected virtual void FlickerOnHit(float delta)
        {
            if (this.startAnimationHit)
            {
                startDecreaseColor -= delta;

                if (startDecreaseColor <= 0.40f)
                {
                    startDecreaseColor = 1.0f;
                    startAnimationHit = false;
                }

                this.SelfModulate = new Color(1.16f, 0.0f, 0.0f, startDecreaseColor);
            }
        }

    }
}
