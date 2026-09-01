using Godot;
using System;
using System.Collections.Generic;

public partial class Spawner : Node2D
{
	private PackedScene OrcScene = GD.Load<PackedScene>("res://Characters/enemy.tscn");

	[Export] public int SpawnCount = 7;
	[Export] public float XRange = 700f;
	[Export] public float YRange = 80f;
	[Export] public float MinDistance = 100f;

	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	public override void _Ready()
	{
		_rng.Randomize();
		CallDeferred(nameof(SpawnOrcs));
	}

	private void SpawnOrcs()
	{
		List<Vector2> usedPositions = new List<Vector2>();

		for (int i = 0; i < SpawnCount; i++)
		{
			Vector2 spawnPos = GetValidPosition(usedPositions);
			usedPositions.Add(spawnPos);

			Node2D orc = OrcScene.Instantiate<Node2D>();
			orc.GlobalPosition = spawnPos;

			GetParent().AddChild(orc);
		}
	}

	private Vector2 GetValidPosition(List<Vector2> usedPositions)
	{
		for (int attempt = 0; attempt < 50; attempt++)
		{
			Vector2 pos = new Vector2(
				GlobalPosition.X + _rng.RandfRange(-XRange, XRange),
				GlobalPosition.Y + _rng.RandfRange(-YRange, YRange)
			);

			bool tooClose = false;

			foreach (Vector2 usedPos in usedPositions)
			{
				if (pos.DistanceTo(usedPos) < MinDistance)
				{
					tooClose = true;
					break;
				}
			}

			if (!tooClose)
				return pos;
		}

		return new Vector2(
			GlobalPosition.X + _rng.RandfRange(-XRange, XRange),
			GlobalPosition.Y + _rng.RandfRange(-YRange, YRange)
		);
	}
}
