using Godot;

public partial class MessageWithTimer : CanvasLayer
{

    [Signal]
    public delegate void MessageHiddenEventHandler();
    Label messageNode;
    Timer messageTimerNode;
    private bool returnSignal = false;
    private int currentPriority = -1;

    public override void _Ready()
    {
        messageNode = GetNode<Label>("Message");
        messageTimerNode = GetNode<Timer>("MessageTimer");

        var overlay = GetParent<MainGameOverlay>();
        overlay.SendMessage += ShowMessage;
        messageTimerNode.Timeout += MessageTimeout;
    }

    public void ShowMessage(string message, bool returnSignal, int messagePriority)
    {
        if (messagePriority < currentPriority)
        {
            // if the message priority is lower than our current priority, we don't want the new mesasge to override the old one
            return;
        }

        currentPriority = messagePriority;
        this.returnSignal = returnSignal;

        messageNode.Text = message;
        messageNode.Show();

        messageTimerNode.Start();
    }
    public void ShowMessage(string message)
    {
        messageNode.Text = message;
        messageNode.Show();
    }

    public void MessageTimeout()
    {
        messageNode.Hide();

        if (returnSignal)
        {
            this.EmitSignal(nameof(MessageHidden));
        }

        currentPriority = -1;
    }

}
