using Godot;
using System;

public partial class HealthBar : ProgressBar
{
	// Node references
	private Timer _timer;
	private ProgressBar _damageBar;

	private int _health = 0;

	// Property to handle health updates and trigger the logic
	public int Health
	{
		get => _health;
		set => SetHealth(value);
	}

	public override void _Ready()
	{
		// These names must match the node names in your Godot Scene tree exactly
		_timer = GetNode<Timer>("Timer");
		_damageBar = GetNode<ProgressBar>("DamageBar");
	}

	// Called every frame
	public override void _Process(double delta)
	{
		// If the background damage bar is still higher than the actual health bar
		if (_damageBar.Value > Value)
		{
			// Smoothly interpolate the damage bar down to the current value
			// 0.1f is the speed; increase it for a faster catch-up
			_damageBar.Value = Mathf.Lerp(_damageBar.Value, Value, 0.1f);

			// Stop the lerp once it gets close enough to prevent tiny calculations
			if (Mathf.Abs(_damageBar.Value - Value) < 0.1)
			{
				_damageBar.Value = Value;
			}
		}
	}

	// Call this when the character first spawns to sync all bars
	public void InitHealth(int initialHealth)
	{
		_health = initialHealth;
		MaxValue = _health;
		Value = _health;

		_damageBar.MaxValue = _health;
		_damageBar.Value = _health;
	}

	private void SetHealth(int newHealth)
	{
		int prevHealth = _health;
		
		// Clamp health so it doesn't exceed the max bar size
		_health = (int)Mathf.Min(MaxValue, newHealth);
		Value = _health;

		// Logic for taking damage
		if (_health < prevHealth)
		{
			_timer.Start();
		}
		// Logic for healing (instantly update damage bar)
		else
		{
			_damageBar.Value = _health;
		}

		// Handle death
		if (_health <= 0)
		{
			// Note: This deletes the health bar. 
			// If you want a death animation first, handle QueueFree() elsewhere.
			QueueFree();
			_damageBar.Value = _health;
		}
	}
}
