using Godot;
using System;

public partial class BossCamera : Camera2D
{
	[Export] public float FollowSpeed = 6.0f;
	[Export] public float ReturnSpeed = 4.0f;
	[Export] public float BossIntroDuration = 4.0f;
	[Export] public float ShakeDuration = 0.5f;
	[Export] public float ShakeStrength = 4.0f;

	private Player _player;
	private Node2D _focusTarget;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	private bool _bossIntroActive = false;
	private float _bossIntroTimer = 0.0f;
	private float _shakeTimer = 0.0f;

	public override void _Ready()
	{
		AddToGroup("main_camera");
		_rng.Randomize();

		_player = GetTree().GetFirstNodeInGroup("player") as Player;

		if (_player == null)
			_player = GetTree().Root.FindChild("Player", true, false) as Player;
	}

	public override void _Process(double delta)
	{
		if (_player == null)
		{
			_player = GetTree().GetFirstNodeInGroup("player") as Player;

			if (_player == null)
				_player = GetTree().Root.FindChild("Player", true, false) as Player;
		}

		if (_bossIntroActive && _focusTarget != null)
		{
			GlobalPosition = _focusTarget.GlobalPosition;

			_bossIntroTimer -= (float)delta;
			if (_bossIntroTimer <= 0.0f)
			{
				_bossIntroActive = false;
				_focusTarget = null;
			}
		}
		else if (_player != null)
		{
			GlobalPosition = GlobalPosition.Lerp(_player.GlobalPosition, ReturnSpeed * (float)delta);
		}

		
		if (_shakeTimer > 0.0f)
		{
			_shakeTimer -= (float)delta;
			Offset = new Vector2(
				_rng.RandfRange(-ShakeStrength, ShakeStrength),
				_rng.RandfRange(-ShakeStrength, ShakeStrength)
			);
		}
		else
		{
			Offset = Vector2.Zero;
		}
	}

	public void ShowBossIntro(Node2D boss)
	{
		if (boss == null)
			return;

		_focusTarget = boss;
		_bossIntroActive = true;
		_bossIntroTimer = BossIntroDuration;
		_shakeTimer = ShakeDuration;

		GlobalPosition = boss.GlobalPosition;
	}
}
