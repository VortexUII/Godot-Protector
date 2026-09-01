using Godot;
using System;

public partial class Pause : Control
{
	private VBoxContainer _menuBox;
	private Node2D _player;

	public override void _Ready()
	{
		Hide();
		ProcessMode = ProcessModeEnum.Always;

		_menuBox = GetNode<VBoxContainer>("VBoxContainer");
		_player = GetNode<Node2D>("../../Player");
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("pause"))
		{
			TogglePause();
		}
	}

	private void TogglePause()
	{
		if (GetTree().Paused)
		{
			GetTree().Paused = false;
			Hide();
		}
		else
		{
			FollowPlayer();
			GetTree().Paused = true;
			Show();
		}
	}

	private void FollowPlayer()
	{
		if (_player == null || _menuBox == null)
			return;

		Vector2 menuSize = _menuBox.Size;

		Vector2 screenPos = GetViewport().GetCanvasTransform() * _player.GlobalPosition;

		GlobalPosition = screenPos - new Vector2(menuSize.X / 2f, menuSize.Y + 20f);
	}

	public void _on_button_pressed()
	{
		GetTree().Paused = false;
		Hide();
	}

	public void _on_button_2_pressed()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://MainMenu/main_menu.tscn");
	}
}
