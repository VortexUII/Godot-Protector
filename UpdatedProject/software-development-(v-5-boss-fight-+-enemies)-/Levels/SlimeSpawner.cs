using Godot;
using System;
using System.Collections.Generic;

public partial class SlimeSpawner : Node2D
{
	private PackedScene SlimeScene = GD.Load<PackedScene>("res://Characters/slime.tscn");

	[Export] public int SpawnCount = 6;

	// Spread area
	[Export] public float XRange = 250f;
	[Export] public float YRange = 40f;

	// Prevent stacking
	[Export] public float MinDistance = 70f;

	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	public override void _Ready()
	{
		_rng.Randomize();
		CallDeferred(nameof(SpawnSlimes));
	}

	private void SpawnSlimes()
	{
		List<Vector2> usedPositions = new List<Vector2>();

		for (int i = 0; i < SpawnCount; i++)
		{
			Vector2 pos = GetValidPosition(usedPositions);
			usedPositions.Add(pos);

			Node2D slime = SlimeScene.Instantiate<Node2D>();
			slime.GlobalPosition = pos;

			GetParent().AddChild(slime);
		}
	}

	private Vector2 GetValidPosition(List<Vector2> used)
	{
		for (int attempt = 0; attempt < 40; attempt++)
		{
			Vector2 pos = new Vector2(
				GlobalPosition.X + _rng.RandfRange(-XRange, XRange),
				GlobalPosition.Y + _rng.RandfRange(-YRange, YRange)
			);

			bool valid = true;

			foreach (var other in used)
			{
				if (pos.DistanceTo(other) < MinDistance)
				{
					valid = false;
					break;
				}
			}

			if (valid)
				return pos;
		}

		return GlobalPosition;
	}
}
