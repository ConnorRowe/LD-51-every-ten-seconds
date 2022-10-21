using Godot;
using System;

namespace TenSecs
{
    public class MainMenu : Control
    {
        private static PackedScene arenaScene = GD.Load<PackedScene>("res://scenes/Arena.tscn");

        private VBoxContainer settings;
        private VBoxContainer main;

        public override void _Ready()
        {
            GetNode("Main/Play").Connect("pressed", this, nameof(Play));
            GetNode("Main/Settings").Connect("pressed", this, nameof(ShowSettings));
            GetNode("Settings/Back").Connect("pressed", this, nameof(ShowMain));

            settings = GetNode<VBoxContainer>("Settings");
            main = GetNode<VBoxContainer>("Main");

            GetNode<Label>("Highscore").Text = string.Format("Personal best: {0}", DateTime.FromBinary(599266080000000000).AddSeconds(SaveData.MaxTime).ToString("mm:ss"));

            Music.PlayMenuMusic();
        }

        private void Play()
        {
            GetTree().ChangeSceneTo(arenaScene);
            QueueFree();
        }

        private void ShowSettings()
        {
            settings.Visible = true;
            main.Visible = false;
        }

        private void ShowMain()
        {
            settings.Visible = false;
            main.Visible = true;
        }
    }
}