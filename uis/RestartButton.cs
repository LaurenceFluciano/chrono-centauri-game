using Godot;
using System;

public partial class RestartButton : Button
{
	[Export] PackedScene LevelScene;
	public override void _Ready()
    {
        Pressed += OnButtonPressed;
    }

    private void OnButtonPressed()
    {
        GetTree().ChangeSceneToPacked(LevelScene);
    }
}
