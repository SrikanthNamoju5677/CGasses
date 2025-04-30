using Godot;

namespace SpaceInvaders
{
    public partial class BonusScoreOverlay : CanvasLayer
    {
        private Timer bonusScoreTimer;
        private Label timeLeftLabel;

        public int BonusScoreTimerLeft { get; private set; }

        public override void _Ready()
        {
            bonusScoreTimer = GetNode<Timer>("BonusScoreTimer");
            timeLeftLabel = GetNode<Label>("BonusScoreTimeLeft");
        }

        public override void _Process(double delta)
        {
            if (bonusScoreTimer.IsStopped())
                return;

            UpdateTimeLeft();
        }

        public void UpdateTimeLeft()
        {
            BonusScoreTimerLeft = Mathf.RoundToInt(bonusScoreTimer.TimeLeft);
            timeLeftLabel.Text = BonusScoreTimerLeft.ToString();
        }

        public void Start(int bonusScoreTimeoutSeconds)
        {
            bonusScoreTimer.Start(bonusScoreTimeoutSeconds);
        }
    }
}
