using SpaceInvaders;
using System;
using System.Collections.Generic;

static class SceneTransitions
{

    //Dictionary that contains the scene transitions based on what button was pressed
    static Dictionary<string, Tuple<string, List<object>>> _dict = new Dictionary<string, Tuple<string, List<object>>>
    {
        {"HomeScreen", new Tuple<string, List<object>> ("res://src/screens/home/HomeScreen.tscn",new List<object>() {})},
        {"SinglePlayerGame", new Tuple<string, List<object>> ("res://src/screens/levels/MainGameOverlay.tscn",new List<object>() {GameMode.Singleplayer})},
        {"MultiplayerGame", new Tuple<string, List<object>> ("res://src/screens/levels/MainGameOverlay.tscn",new List<object>() {GameMode.Multiplayer})},
        {"CooperativeAIGame", new Tuple<string, List<object>> ("res://src/screens/levels/MainGameOverlay.tscn",new List<object>() {GameMode.CooperativeAI})},
        {"AIDemoGame", new Tuple<string, List<object>> ("res://src/screens/levels/MainGameOverlay.tscn",new List<object>() {GameMode.AIDemoGame})},
        {"SurvivalModeGame", new Tuple<string, List<object>> ("res://src/screens/levels/MainGameOverlay.tscn",new List<object>() {GameMode.SurvivalMode})},
        {"SinglePlayerLeaderBoard", new Tuple<string, List<object>> ("res://src/screens/leaderboard/Leaderboard.tscn",new List<object>() {GameMode.Singleplayer, false})},
        {"MultiplayerLeaderBoard", new Tuple<string, List<object>> ("res://src/screens/leaderboard/Leaderboard.tscn",new List<object>() {GameMode.Multiplayer, false})},
        {"CooperativeAILeaderBoard", new Tuple<string, List<object>> ("res://src/screens/leaderboard/Leaderboard.tscn",new List<object>() {GameMode.CooperativeAI, false})},
        {"SurvivalModeLeaderBoard", new Tuple<string, List<object>> ("res://src/screens/leaderboard/Leaderboard.tscn",new List<object>() {GameMode.SurvivalMode, false})},
        {"CredentialsSingleplayer", new Tuple<string, List<object>> ("res://src/screens/credentials/CredentialScreen.tscn",new List<object>() {})},
        {"CredentialsMultiplayer", new Tuple<string, List<object>> ("res://src/screens/credentials/CredentialScreenMultiplayer.tscn",new List<object>() {})},
        {"CredentialsCooperativeAI", new Tuple<string, List<object>> ("res://src/screens/credentials/CredentialScreen.tscn",new List<object>() {})},
        {"CredentialsSurvivalMode", new Tuple<string, List<object>> ("res://src/screens/credentials/CredentialScreen.tscn",new List<object>() {})},
        {"CredentialsEnteredSingleplayer", new Tuple<string, List<object>> ("res://src/screens/leaderboard/Leaderboard.tscn",new List<object>() {GameMode.Singleplayer, true})},
        {"CredentialsEnteredMultiplayer", new Tuple<string, List<object>> ("res://src/screens/leaderboard/Leaderboard.tscn",new List<object>() {GameMode.Multiplayer, true})},
        {"CredentialsEnteredCooperativeAI", new Tuple<string, List<object>> ("res://src/screens/leaderboard/Leaderboard.tscn",new List<object>() {GameMode.CooperativeAI, true})},
        {"CredentialsEnteredSurvivalMode", new Tuple<string, List<object>> ("res://src/screens/leaderboard/Leaderboard.tscn",new List<object>() {GameMode.SurvivalMode, true})},
        {"GameModeMenu", new Tuple<string, List<object>> ("res://src/screens/home/GameModeMenu.tscn",new List<object>() {})},
        {"LeaderBoardMenu", new Tuple<string, List<object>> ("res://src/screens/home/LeaderBoardMenu.tscn",new List<object>() {})}
    };

    public static Tuple<string, List<object>> GetTransition(string word)
    {
        Tuple<string, List<object>> result;
        if (_dict.TryGetValue(word, out result))
        {
            return result;
        }
        else
        {
            return null;
        }
    }
}
