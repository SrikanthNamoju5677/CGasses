using Godot;

namespace SpaceInvaders
{
    public static class ConfigHelper
    {

        private static string filePath = "res://Config/GameConfig.cfg";

        public static ConfigFile LoadConfigFile()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            ConfigFile config = new ConfigFile();
            Error err = config.Load(ConfigHelper.filePath);
            // When config cannot be found create a new file with default values.
            if (err.Equals(Error.FileNotFound))
            {
                SetupConfig(config);
            }

            return config;
        }

        private static void SetupConfig(ConfigFile config)
        {
            config.SetValue("Gameplay", "MovementSpeed", "0");
            config.SetValue("Gameplay", "PrimaryWeaponReloadTimer", "0");
            config.SetValue("Gameplay", "SecondaryWeaponReloadTimer", "0");
            config.SetValue("Gameplay", "PowerupTimer", "0");
            config.Save(ConfigHelper.filePath);
        }
    }
}
