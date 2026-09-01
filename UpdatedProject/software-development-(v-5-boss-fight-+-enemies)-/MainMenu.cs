using Godot;
using System;

public partial class MainMenu : Control
{
	public void _on_start_pressed()
	{
GetTree().ChangeSceneToFile("res://Levels/levelone.tscn");	}

	public void _on_exit_pressed()
	{
		GetTree().Quit();
	}

	public void _on_options_pressed()
	{
		GD.Print("Options not ready yet");
	}
}
