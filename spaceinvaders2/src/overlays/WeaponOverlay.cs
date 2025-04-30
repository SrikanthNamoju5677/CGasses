using Godot;

public partial class WeaponOverlay : CanvasLayer
{
    private ProgressBar heatingBar;
    private Sprite2D primaryBulletSprite;
    private bool IsOverheated = false;
    private AudioStreamPlayer2D OverheatedWarning;

    public override void _Ready()
    {
        Player player = (Player)this.GetParent();
        Weapon secondaryWeapon = player.WeaponCycle[1];

        player.MainWeaponEvent += this.HideHeatingBar;
        player.SprayWeaponEvent += this.HidePrimaryWeaponBar;

        (secondaryWeapon as SecondaryWeapon).SendHeatEvent += this.HookProgressBar;
        (secondaryWeapon as SecondaryWeapon).SetModulateOverheated += this.SetOverheated;
        (secondaryWeapon as SecondaryWeapon).SetModulateNotOverheated += this.SetNotOverheated;

        heatingBar = GetNode<ProgressBar>("HeatingBar");
        primaryBulletSprite = GetNode<Sprite2D>("PrimaryBulletSprite");

        OverheatedWarning = GetNode<AudioStreamPlayer2D>("OverheatedWarning");

        HideHeatingBar();
    }

    public void HideHeatingBar()
    {
        heatingBar.Hide();
        primaryBulletSprite.Show();
    }

    public void HidePrimaryWeaponBar()
    {
        heatingBar.Show();
        primaryBulletSprite.Hide();
    }

    public void HookProgressBar(float value)
    {
        heatingBar.Value = (int)value;
    }

    public void SetOverheated()
    {
        this.IsOverheated = true;
        heatingBar.Modulate = new Color((float)1.0, (float)0.0, (float)0.0, (float)1.0);
        if (OverheatedWarning!=null && !OverheatedWarning.Playing) OverheatedWarning.Play();
    }

    public void SetNotOverheated()
    {
        this.IsOverheated = false;
        heatingBar.Modulate = new Color((float)0.0, (float)0.5, (float)1.0, (float)1.0);
    }

    public void HideOverlay()
    {
        this.Hide();
    }
}
