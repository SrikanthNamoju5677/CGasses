namespace SpaceInvaders
{
    //All messages displayed have a certain priority over one another
    public enum MessagePriority
    {
        //messages regarding warnings, such as overheating of weapon
        WARNING = 0,

        //messages regarding warnings during levels, such as warnings during the boss level
        lEVELWARNING = 1,

        //messages regarding gameflow: start of levels, victory, game over, etc
        GAMEFLOW = 2
    }
}