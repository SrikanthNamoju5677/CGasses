using Godot;
using SpaceInvaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


public partial class CredentialScreen : Screen
{
    private const string CredentialsFile = "Credentials.csv";
    private Label errorLabel, errorLabel2;

    public override void _Ready()
    {
        Button submitButton = GetNode<Button>("Submit");
        submitButton.Pressed += SubmitCredentials;
        errorLabel = GetNode<Label>("LabelError");
        errorLabel2 = GetNode<Label>("LabelError2");

        this.errorLabel.Hide();
        if (errorLabel2 != null)
            this.errorLabel2.Hide();
        var musicPlayer = GetNode<MusicManager>("/root/MusicManager");
        musicPlayer.StartMusic("you_are_winner");
    }

    public void SubmitCredentials()
    {
        if (IsAnyErrorLabelVisible())
            HideErrorLabels();
        List<Credential> credentials = new List<Credential>();
        bool isSuccess = true;

        // Process first credential
        isSuccess &= ProcessCredential("1", credentials);

        // Process second credential for multiplayer mode
        if (PlayerHandler.GameMode == GameMode.Multiplayer)
        {
            isSuccess &= ProcessCredential("2", credentials);
        }

        // Continue only if both credentials were processed successfully
        if (isSuccess)
        {
            // Other processing
            ProcessLeaderboardAndSave(credentials);

            // Change scene
            ChangeScene();
        }
    }

    private bool ProcessCredential(string id, List<Credential> credentials)
    {
        try
        {
            credentials.Add(GetCredential(id));
            return true;
        }
        catch (Exception e)
        {
            DisplayError(e, id);
            return false;
        }
    }

    private void DisplayError(Exception e, string credentialId)
    {
        Label labelToDisplay = (credentialId == "1") ? errorLabel : errorLabel2;
        UpdateAndShowErrorLabel(e.Message, labelToDisplay);
    }

    private void ProcessLeaderboardAndSave(List<Credential> credentials)
    {
        string name = string.Join(" & ", credentials.Select(c => c.Name));
        LeaderboardParser.AddNewLeader(name, ScoreHandler.Score.ToString(), PlayerHandler.GameMode);

        Credential[] allowedCredentials = credentials.Where(c => c.isAllowedToStore).ToArray();
        SaveCredentialsToDB(allowedCredentials);
    }

    private void ChangeScene()
    {
        string wantedScene = "CredentialsEntered" + PlayerHandler.GameMode.ToString();
        EmitSignal(nameof(DoChangeScene), wantedScene);
        errorLabel.Hide();
    }

    private void UpdateAndShowErrorLabel(string message, Label labelError)
    {
        labelError.Text = message;
        labelError.Show();
    }

    private Credential GetCredential(string playerNumber)
    {
        LineEdit inputName = GetNode<LineEdit>("InputName" + playerNumber);
        LineEdit inputEmail = GetNode<LineEdit>("InputEmail" + playerNumber);
        CheckBox checkBox = GetNode<CheckBox>("CheckBox" + playerNumber);

        return new Credential(inputName.Text, inputEmail.Text, checkBox.ButtonPressed, playerNumber);
    }

    private void SaveCredentialsToDB(Credential[] credentials)
    {
        using (var writer = new StreamWriter(CredentialsFile, true))
        {
            foreach (Credential credential in credentials)
            {
                writer.WriteLine(credential.ToString());
            }
        }
    }

    private bool IsAnyErrorLabelVisible()
    {
        if (errorLabel != null && errorLabel.Visible)
        {
            return true;
        }

        if (errorLabel2 != null && errorLabel2.Visible)
        {
            return true;
        }

        return false;
    }

    private void HideErrorLabels()
    {
        this.errorLabel.Hide();
        if (errorLabel2 != null)
            this.errorLabel2.Hide();
    }
}
