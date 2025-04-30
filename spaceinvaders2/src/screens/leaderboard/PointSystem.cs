namespace SpaceInvaders
{
    public static class PointSystem
    {
        public static int InvaderHitBonus { get; } = 300;
        public static int InvaderKillBonus { get; } = 10;
        public static int BossHitBonus { get; } = 300;
        public static int BossKillBonus { get; } = 200;
        public static int PowerUpPointBonus { get; set; } = 20;
        public static int PointsInitial { get; set; } = 0;
        public static int LevelPointsBonusMultiplier { get; set; } = 10;
        public static int NoHitBonus { get; } = 500;
    }
}
