using Godot;
using SpaceInvaders;

namespace PrototypeSpaceInvaders.Extensions
{
    public static class Extensions
    {
        public static Vector2 VectorHelper(this Vector2 vector2, float x, float y)
        {
            return new Vector2(x, y);
        }

        public static void PlayerHelper(this Player node, PackedScene scene, FloatingTextTypes type)
        {
            dynamic floatingText = (Marker2D)scene.Instantiate();
            floatingText.Entity = EntityType.PLAYER;
            switch (type)
            {
                case FloatingTextTypes.HITSHIELD:
                    floatingText.FTextTypes = FloatingTextTypes.HITSHIELD;
                    floatingText.LabelDamage = 1;
                    break;
                case FloatingTextTypes.HITLIFE:
                    floatingText.FTextTypes = FloatingTextTypes.HITLIFE;
                    floatingText.LabelDamage = 1;
                    break;
                case FloatingTextTypes.HEALING:
                    floatingText.FTextTypes = FloatingTextTypes.HEALING;
                    floatingText.LabelDamage = +1;
                    floatingText.Points = 10;
                    break;
                case FloatingTextTypes.POWERUPFIRE:
                    floatingText.FTextTypes = FloatingTextTypes.POWERUPFIRE;
                    floatingText.Points = PointSystem.PowerUpPointBonus;
                    break;
                case FloatingTextTypes.POWERUPSHIELD:
                    floatingText.FTextTypes = FloatingTextTypes.POWERUPSHIELD;
                    floatingText.Points = PointSystem.PowerUpPointBonus;
                    break;
            }
            floatingText.GlobalPosition = new Vector2(node.GlobalPosition.X, node.GlobalPosition.Y);
            node.GetParent().AddChild(floatingText);
        }

        public static void EnemyHelper(this Sprite2D node, PackedScene scene, FloatingTextTypes type, int bulletDmg, int lives)
        {
            dynamic floatingText = (Marker2D)scene.Instantiate();

            floatingText.Entity = EntityType.ENEMY;
            switch (type)
            {
                case FloatingTextTypes.ENEMYHIT:
                    floatingText.FTextTypes = FloatingTextTypes.ENEMYHIT;
                    floatingText.LabelDamage = bulletDmg;
                    floatingText.GlobalPosition = new Vector2(node.GlobalPosition.X, node.GlobalPosition.Y);
                    node.GetParent().GetParent().AddChild(floatingText);
                    break;
            }
        }

        public static void EnemyBossHelper(this BossSprite node, PackedScene scene, FloatingTextTypes type, int bulletDmg, int lives)
        {
            dynamic floatingText = (Marker2D)scene.Instantiate();

            floatingText.Entity = EntityType.ENEMY;
            switch (type)
            {
                case FloatingTextTypes.ENEMYHIT:
                    floatingText.FTextTypes = FloatingTextTypes.ENEMYHIT;
                    floatingText.LabelDamage = bulletDmg;
                    floatingText.GlobalPosition = new Vector2(node.GlobalPosition.X, node.GlobalPosition.Y);
                    node.GetParent().GetParent().AddChild(floatingText);
                    break;
            }
        }

        public static void EnemyHelperPlayerAction(this Sprite2D node, PackedScene scene, FloatingTextTypes type, int points, int lives, int player)
        {
            dynamic floatingText = (Marker2D)scene.Instantiate();
            string playerName = (player == 1) ? "Player" : "Player" + player.ToString();
            var playerNode = node.GetParent().GetParent().GetParent().GetNode("PlayerHandler").GetNode<Sprite2D>(playerName);

            floatingText.Entity = EntityType.PLAYER;
            floatingText.FTextTypes = type;
            floatingText.Points = points;
            floatingText.GlobalPosition = new Vector2(playerNode.GlobalPosition.X, playerNode.GlobalPosition.Y);

            node.GetParent().GetParent().AddChild(floatingText);
        }

        public static void EnemyPlayerBossHelper(this BossSprite node, PackedScene scene, FloatingTextTypes type, int points, int lives)
        {
            dynamic floatingText = (Marker2D)scene.Instantiate();

            floatingText.Entity = EntityType.PLAYER;
            switch (type)
            {
                case FloatingTextTypes.POINTS:
                    floatingText.FTextTypes = FloatingTextTypes.POINTS;
                    floatingText.Points = points;
                    floatingText.FTextTypes = FloatingTextTypes.POINTS;
                    var node_player = node.GetParent().GetParent().GetNode<Node>("PlayerHandler").GetNode<Sprite2D>("Player");
                    floatingText.GlobalPosition = new Vector2(node_player.GlobalPosition.X, node_player.GlobalPosition.Y);
                    node.GetParent().AddChild(floatingText);
                    break;
            }
        }
    }
}
