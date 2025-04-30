using System;
using System.Text.RegularExpressions;

public class Credential
{
    private string name;
    private string emailAddress;
    private string playerNumber;
    public bool isAllowedToStore;

    public Credential(string name, string emailAddress, bool isAllowedToStore, string playerNumber)
    {
        this.playerNumber = playerNumber;
        this.Name = name;
        this.EmailAddress = emailAddress;
        this.isAllowedToStore = isAllowedToStore;
    }

    public string EmailAddress
    {
        get { return emailAddress; }

        set
        {
            bool isValidEmail = Regex.Match(value, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$").Success;
            if (!isValidEmail)
                throw new ArgumentException($"Player {playerNumber}: Invalid email address.");

            emailAddress = value;
        }
    }

    public string Name
    {
        get { return name; }

        set
        {
            ValidateName(value, playerNumber);
            name = value.Trim();
        }
    }

    private void ValidateName(string value, string playerNumber)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new Exception($"Player {playerNumber}: Name cannot be empty.");

        if (value.Length > 35)
            throw new ArgumentException($"Player {playerNumber}: Name cannot be longer than 35 characters.");

        if (Regex.IsMatch(value, "[^a-zA-Z0-9 -]"))
            throw new Exception($"Player {playerNumber}: Name field can contain only letters, digits and '-'");
    }

    public override string ToString()
    {
        return $"{this.Name}, {this.EmailAddress}";
    }
}
