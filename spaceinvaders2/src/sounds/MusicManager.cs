using Godot;
using System;
using System.Collections.Generic;

public partial class MusicManager : Node
{
    List<string> musicList = new List<string> {"home_bgm", "error", "levels_bgm", "boss_level", "you_are_winner"};

    public void StartMusic(string musicName)
    {
        var music = GetNode<AudioStreamPlayer2D>(musicName);
        if (!music.Playing)
            music.Play(); // Start playing the BGM
        foreach (string i in musicList){
            var music_in_list = GetNode<AudioStreamPlayer2D>(i);
            if (i!=musicName && music_in_list.Playing)
                music_in_list.Stop();
        }
    }
}
