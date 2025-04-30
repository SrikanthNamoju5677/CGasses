using SpaceInvaders;

public static class ScoreHandler
{
    private static int score;
    public static int Score { get => score; }

    public static void NewGame()
    {
        score = PointSystem.PointsInitial;
    }

    public static void LevelCompleted(int amount)
    {
        score += amount;
    }

    public static void InvaderKilled()
    {
        score += PointSystem.InvaderKillBonus;
    }

    public static void BossKilled()
    {
        score += PointSystem.BossKillBonus;
    }

    public static void OnScoreTimerTimeout()
    {
        score++;
    }

    public static void OnPlayerShieldPowerUp()
    {
        score += PointSystem.PowerUpPointBonus;
    }

    public static void OnPlayerFirePowerUp()
    {
        score += PointSystem.PowerUpPointBonus;
    }
    public static void AddNoHitBonus()
    {
        score += PointSystem.NoHitBonus;
    }

    public static void AddAccuracyBonus(Level currentLevel, ref int fireCount, ref int targetHitCount)
    {
        if (currentLevel is TutorialLevel)
            return;

        var fullAccuracyBonus = currentLevel is BossLevel ? PointSystem.BossHitBonus : PointSystem.InvaderHitBonus;
        var totalAccuracyBonus = fullAccuracyBonus * (targetHitCount / fireCount);
        score += totalAccuracyBonus;
        fireCount = 0;
        targetHitCount = 0;
    }
}
