using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Zone : IComparable<Zone>
{
    public float min = 0;
    public float max = 0;
    public float height = 0;

    public Zone(float min, float max, float height)
    {
        this.min = min;
        this.max = max;
        this.height = height;
    }

    public int CompareTo(Zone other) => height.CompareTo(other.height);
}

public class NoGoZone : Zone
{
    public NoGoZone(float min, float max, float height) : base(min, max, height) { }

    public bool IsSafe(float position, float halfWidthPlayer)
    {
        return position < (this.min - halfWidthPlayer) || position > (this.max + halfWidthPlayer);
    }
}

public partial class AI : Player
{
    private const float SAFE_VERTICAL_DISTANCE_MAX = 800;
    private const float FIRE_TRIGGERING_DISTANCE_MAX = 300;
    private const float DANGER_HEIGHT_DIFFERENCE = 200;
    private const float DANGER_POSITION_DIFFERENCE = 100;
    private const float MINIMUM_DETECTION_DISTANCE = 800;
    private float HALF_WIDTH_PLAYER;
    private const float MAX_ASTEROID_WAVE_HEIGHT_GAP = 200;
    private const float ADDITIONAL_MARGIN = 1;
    private Dictionary<string, int> FIRE_TRIGGERING_CHANCE_DENOMINATOR = new Dictionary<string, int>(){{"Boss", 20}, {"Invaders", 10}}; // 1/FIRE_TRIGGERING_CHANCE_DENOMINATOR chance ai will fire

    private bool isBossLevel = false;
    private bool isFirePowerUpActive = false;
    private bool obstaclesExist = false;
    private bool isFollowingPowerUp = false;

    private Random randForFire = new Random();

    private MainGameOverlay overlay;
    private SecondaryWeapon secondaryWeapon;
    private int testField = 0;

    public override void _Ready()
    {
        // Call the _Ready() method from base Player class
        base._Ready();
        //this.Modulate = Colors.Yellow;
        BindToEvents();
        HALF_WIDTH_PLAYER = this.Texture.GetWidth() / 2;
    }

    private void BindToEvents()
    {
        var primaryWeapon = WeaponCycle.OfType<PrimaryWeapon>().First();
        primaryWeapon.FirePowerUpTimeout += ToggleFirePowerMode;
        this.FirePowerUp += FirePoweredUp;  // if the ai is powered up, switch to primary weapon (or keep using primary weapon)

        secondaryWeapon = WeaponCycle.OfType<SecondaryWeapon>().First();
        secondaryWeapon.OverheatEvent += SwitchWeapon;
        secondaryWeapon.SetModulateNotOverheated += SwitchWeapon;

        overlay = (MainGameOverlay)GetTree().CurrentScene.GetNode<CanvasLayer>("MainGame");
        overlay.BossLevelEntered += OnBossLevelEntered;
    }

    private void ToggleFirePowerMode()
    {
        if (!isFirePowerUpActive) // Fire power-up is not activated yet
        {
            if (CurrentWeapon is SecondaryWeapon)
                SwitchWeapon();

            isFirePowerUpActive = !isFirePowerUpActive;
        }
        else // Fire power-up is active
        {
            if (!secondaryWeapon.Overheated)
                SwitchWeapon();
            isFirePowerUpActive = !isFirePowerUpActive;
        }
    }

    private void FirePoweredUp()
    {
        isFirePowerUpActive = true;
        SwitchWeapon();
    }

    private void SwitchWeapon()
    {
        /***
        weapon switch rules:
        1. when the current weapon is primary weapon, switch to the secondary weapon 
           if it's save to switch to secondary weapon AND the fire power up is not activated;

        2. when the current weapon is secondary weapon, switch to the primary weapon
           if it's not safe to use secondary weapon OR the fire power up is activated.
        ***/

        bool isSafeToUseSecondary = !secondaryWeapon.Overheated; // Use second weapon unless overheated in no obstacle case

        if (CurrentWeapon is PrimaryWeapon)
        {
            if (!isFirePowerUpActive && isSafeToUseSecondary)
                SwitchWeapon("up");
            return;
        }

        else if (CurrentWeapon is SecondaryWeapon)
        {
            if (isFirePowerUpActive || !isSafeToUseSecondary)
                SwitchWeapon("up");
            return;
        }
        else GD.Print("Weapon undefined");
    }

    public override void RestoreColor() => this.Modulate = Colors.Yellow;

    private void OnBossLevelEntered() => isBossLevel = true;

    private bool SafeLocation(List<NoGoZone> nogo_zone, float position) //returns if the postion is safe (i.e. if there is no astriod above the location)
    {
        var closestObstacle = nogo_zone.OrderBy(o => Math.Abs(o.height - this.Position.Y)).FirstOrDefault();

        if ((this.Position.Y - closestObstacle.height > MINIMUM_DETECTION_DISTANCE) && (!isBossLevel)) // Danger is not in the distance to detect
            return true;

        return UnderAsteroid(nogo_zone, position) == null;
    }

    private NoGoZone UnderAsteroid(List<NoGoZone> nogo_zone, float position) //returns the NoGoZone (i.e. min and max) that the position is in
    {
        foreach (NoGoZone item in nogo_zone)
        {
            if (!item.IsSafe(position, HALF_WIDTH_PLAYER))
            {
                return item;
            }
        }

        return null;
    }

    private float ClosestSafeLocation(List<NoGoZone> nogo_zone, float position, Direction direction) //returns the closest safe location to the postion in the direction (left or right)
    {
        while (!SafeLocation(nogo_zone, position))
        {
            NoGoZone asteroid = UnderAsteroid(nogo_zone, position);

            // Calculate the agent's position, adjusting for its half width,and an additional margin to avoid collision with the asteroid.			
            if (direction == Direction.Left)
                position = asteroid.min - HALF_WIDTH_PLAYER - ADDITIONAL_MARGIN; // Move left of the asteroid
            else
                position = asteroid.max + HALF_WIDTH_PLAYER + ADDITIONAL_MARGIN; // Move right of the asteroid
        }

        return position;
    }

    public override void CheckUserInput(double delta)
    {
        // AI agent is dead
        if (!this.Visible)
            return;

        float player_x = this.GlobalPosition.X;
        var obstacles = NoGoZones();
        obstaclesExist = obstacles.Any(); //obstacles has elements

        if (!obstaclesExist || SafeLocation(obstacles, player_x))
        {
            var groupName = isBossLevel ? "Boss" : "Invaders";
            var targets = GetTree().GetNodesInGroup(groupName).ToList();

            if (targets.Count != 0)
            {
                var closestTargetHeight = targets.Cast<Node>().OfType<Node2D>().Max(node => node.GlobalPosition.Y); // get the horizontal position of the invaders or boss, in order to check if the enemies are in the screen

                TriggerFire(targets, closestTargetHeight, groupName);

                if (!IsDangerNearby(obstacles))
                {
                    PickClosestPowerUp(obstacles);
                    if (!isFollowingPowerUp)
                    {
                        MoveAgentTowardsTarget(targets, closestTargetHeight);
                    }
                }
            }
        }
        else
        {
            AvoidObstacles(obstacles, player_x);
        }
    }

    private void TriggerFire(List<Godot.Node> targets, float closestTargetHeight, string groupName) // Trigger fire only if the invaders or boss are in the screen and close to the AI ship horizontally.
    {

        if (closestTargetHeight < 0 || closestTargetHeight > this.GlobalPosition.Y) return; // The targets have not appeared on the screen yet.

        var targetPositionX = targets.Where(t => t is Node2D)
                            .OrderBy(t => ((Node2D)t).GlobalPosition.X)
                            .ElementAt(targets.Count() / 2);
        bool isWithinHorizontalDistance = (Math.Abs(((Node2D)targetPositionX).GlobalPosition.X - this.GlobalPosition.X) < FIRE_TRIGGERING_DISTANCE_MAX);
        bool allowFire = randForFire.Next(FIRE_TRIGGERING_CHANCE_DENOMINATOR[groupName]) == 0;
        if (isWithinHorizontalDistance && allowFire)
        {
            Fire();
        }
    }


    private void MoveAgentTowardsTarget(float targetPositionX)
    {
        Direction moveDirection = targetPositionX > this.GlobalPosition.X ? Direction.Right : Direction.Left;
        Move(moveDirection);
    }

    private void MoveAgentTowardsTarget(List<Godot.Node> targets, float closestTargetHeight)
    {

        if (closestTargetHeight < 0 || closestTargetHeight > this.GlobalPosition.Y)
            return; // The targets have not appeared on the screen yet.

        var middleTarget = targets.Where(t => t is Node2D)
                                    .OrderBy(t => ((Node2D)t).GlobalPosition.X)
                                    .ElementAt(targets.Count() / 2);

        MoveAgentTowardsTarget(((Node2D)middleTarget).GlobalPosition.X);
    }

    /// <summary>
    /// Determines the closest power-up and guides the AI Agent towards it if it's safe (i.e., no obstacles nearby).
    /// </summary>
    /// <param name="obstacles">List of nearby obstacles to consider when moving towards the power-up.</param>
    private void PickClosestPowerUp(List<NoGoZone> obstacles)
    {
        var powerUpZones = PowerUpZones(); // Retrieve all available power-up zones.

        if (!powerUpZones.Any()) // No power-ups on screen OR there are dangers near the AI Agent.
            return;

        var closestPowerUp = powerUpZones.Last(); // by Height

        // Check if the AI Agent's current position is outside the horizontal boundaries of the closest power-up.
        bool isOutsidePowerUpBounds = closestPowerUp.max <= this.GlobalPosition.X || closestPowerUp.min >= this.GlobalPosition.X;
        bool isWithinVerticalDistance = (Math.Abs(closestPowerUp.height - this.GlobalPosition.Y) < SAFE_VERTICAL_DISTANCE_MAX);

        if (isOutsidePowerUpBounds && isWithinVerticalDistance)
        {
            Direction moveDirection = closestPowerUp.max > this.GlobalPosition.X ? Direction.Right : Direction.Left;
            this.Move(moveDirection); // Guide the AI Agent towards the power-up.
            isFollowingPowerUp = true;
        }
        else
        {
            if (isOutsidePowerUpBounds)
                isFollowingPowerUp = false;
        }
    }

    private bool IsDangerNearby(List<NoGoZone> obstacles)
    {
        float playerHeight = this.GlobalPosition.Y;
        float playerPosX = this.GlobalPosition.X;

        return obstacles.Any(obstacle => Math.Abs(obstacle.height - playerHeight) < DANGER_HEIGHT_DIFFERENCE && Math.Abs(((obstacle.max + obstacle.min) / 2) - playerPosX) < DANGER_POSITION_DIFFERENCE);
    }

    /// <summary>
    /// Calculate the zones where power-ups exist in the scene.
    /// </summary>
    /// <returns>A list of NoGoZone items, sorted by height.</returns>
    private List<Zone> PowerUpZones() => CalculateZones("PowerUps");

    /// <summary>
    /// Directs the AI agent to move in a way that avoids nearby obstacles. The decision on which direction to move is based on:
    /// - The proximity of safe locations on the AI agent's left and right.
    /// - The current game level state:
    ///   - For boss levels, the AI agent may move directly to the left or right.
    ///   - For other levels, the AI agent is directed towards the center if nearing the screen borders.
    /// The method determines the closest safe location in both directions and then decides the optimal move direction.
    /// </summary>
    /// <param name="obstacles">A list of NoGoZone objects representing the obstacles to avoid.</param>
    /// <param name="player_x">The current x-coordinate of the AI agent.</param>
    private void AvoidObstacles(List<NoGoZone> obstacles, float player_x)
    {
        float ClosestSafeLocationLeft = ClosestSafeLocation(obstacles, player_x, Direction.Left);
        float distanceToClosestLeftSafeLocation = player_x - ClosestSafeLocationLeft;
        float ClosestSafeLocationRight = ClosestSafeLocation(obstacles, player_x, Direction.Right);
        float distanceToClosestRightSafeLocation = ClosestSafeLocationRight - player_x;

        // Check for boundary conditions
        if (ClosestSafeLocationLeft <= leftBorder)
        {
            Move(isBossLevel ? Direction.FasterRight : Direction.Right);
            return;
        }
        if (rightBorder <= ClosestSafeLocationRight)
        {
            Move(isBossLevel ? Direction.FasterLeft : Direction.Left);
            return;
        }

        // Choose closest direction based on player's current position
        Direction closestDirection = (distanceToClosestLeftSafeLocation <= distanceToClosestRightSafeLocation) ? Direction.Left : Direction.Right;
        Move(closestDirection);
    }

    /// <summary>
    /// Calculate the zones where obstacles (asteroids or boss bullets) exist in the scene.
    /// Depending on the level, it either fetches zones for asteroids or boss bullets.
    /// </summary>
    /// <returns>A list of NoGoZone items, sorted by height. If it's not boss level, it returns the closest asteroids.</returns>
    private List<NoGoZone> NoGoZones()
    {
        // Determine the group name based on the level type.
        var groupName = isBossLevel ? "BossBullets" : "Asteroids";

        // Use the helper method to get zones with an optional safe width of 120.
        var zones = CalculateZones(groupName);

        var noGoZones = ConvertZonesToNoGoZones(zones);

        // If it's a boss level, return all zones. Otherwise, return only the closest asteroids.
        return isBossLevel ? noGoZones : GetClosestAsteroids(noGoZones);
    }

    public static List<NoGoZone> ConvertZonesToNoGoZones(List<Zone> zones) => zones.Select(z => new NoGoZone(z.min, z.max, z.height)).ToList();

    /// <summary>
    /// Calculate the zones based on items from a specific group in the scene.
    /// </summary>
    /// <param name="groupName">The name of the group to fetch items from.</param>
    /// <returns>A list of NoGoZone items, sorted by height.</returns>
    private List<Zone> CalculateZones(string groupName)
    {
        List<Zone> zones = new List<Zone>();
        var items = GetTree().GetNodesInGroup(groupName).OfType<Sprite2D>();

        if (items == null || !items.Any())
            return zones;

        foreach (var item in items)
        {
            float itemWidth = item.Texture.GetWidth();
            float scale = item.Scale[0]; // Calculate the boundaries of the zone.
            zones.Add(new Zone(item.GlobalPosition.X - scale * itemWidth / 2,
                                item.GlobalPosition.X + scale * itemWidth / 2,
                                item.GlobalPosition.Y));
        }

        // Sort zones by their height.
        zones.Sort();

        return zones;
    }

    /// <summary>
    /// Filters the list of asteroids to consider only those that are close (within a distance of 50 units) 
    /// to the asteroid with the maximum height value (i.e., the lowest on the screen).
    /// </summary>
    /// <param name="asteroidsOnMap">List of asteroids on the map.</param>
    /// <returns>Filtered list of asteroids.</returns>
    private List<NoGoZone> GetClosestAsteroids(List<NoGoZone> asteroidsOnMap)
    {
        if (asteroidsOnMap.Count == 0)
            return asteroidsOnMap;

        // Only consider the lowest wave
        var lastAsteroidHeight = asteroidsOnMap.Last().height;

        return asteroidsOnMap.Where(asteroid => !(lastAsteroidHeight - asteroid.height > MAX_ASTEROID_WAVE_HEIGHT_GAP)).ToList();
    }
}
