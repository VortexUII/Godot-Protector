using Godot;
using System;

public partial class EnemyCounter : Label
{
	[Export] public float WinDelay = 10.0f;
	[Export] public float DeathDelay = 5.0f;
	[Export] public PackedScene SoldierScene;
	[Export] public NodePath BossSpawnPointPath;
	[Export] public NodePath VictoryMusicPath;
	[Export] public NodePath BackgroundMusicPath;

	private bool bossSpawned = false;
	private bool bossAnnouncementActive = false;
	private bool levelComplete = false;
	private bool playerDead = false;
	private bool victoryPlayed = false;

	private float countdownTimer = 0f;
	private float bossAnnouncementTimer = 0f;

	private Player _player;
	private Node2D _bossSpawnPoint;
	private BossCamera _camera;

	private AudioStreamPlayer _victoryMusic;
	private AudioStreamPlayer _bgMusic;

	public override void _Ready()
	{
		_player = GetTree().GetFirstNodeInGroup("player") as Player;

		if (_player == null)
			_player = GetTree().Root.FindChild("Player", true, false) as Player;

		if (BossSpawnPointPath != null && !BossSpawnPointPath.IsEmpty)
			_bossSpawnPoint = GetNode<Node2D>(BossSpawnPointPath);

		_camera = GetTree().GetFirstNodeInGroup("main_camera") as BossCamera;

		if (_camera == null)
			_camera = GetTree().Root.FindChild("Camera2D", true, false) as BossCamera;

		if (VictoryMusicPath != null && !VictoryMusicPath.IsEmpty)
			_victoryMusic = GetNodeOrNull<AudioStreamPlayer>(VictoryMusicPath);

		if (BackgroundMusicPath != null && !BackgroundMusicPath.IsEmpty)
			_bgMusic = GetNodeOrNull<AudioStreamPlayer>(BackgroundMusicPath);

		if (_victoryMusic == null)
			GD.Print("VictoryMusic not assigned in EnemyCounter");

		if (_bgMusic == null)
			GD.Print("BackgroundMusic not assigned in EnemyCounter");
	}

	public override void _Process(double delta)
	{
		if (_player == null)
		{
			_player = GetTree().GetFirstNodeInGroup("player") as Player;

			if (_player == null)
				_player = GetTree().Root.FindChild("Player", true, false) as Player;
		}

		if (_player != null && !_player.IsAlive() && !playerDead)
		{
			playerDead = true;
			countdownTimer = DeathDelay;
		}

		if (playerDead)
		{
			countdownTimer -= (float)delta;
			int secondsLeft = Mathf.Max(0, Mathf.CeilToInt(countdownTimer));

			Text = "You Died!\nReturning to menu in " + secondsLeft + "...";

			if (countdownTimer <= 0)
				GetTree().ChangeSceneToFile("res://MainMenu/main_menu.tscn");

			return;
		}

		int totalEnemiesAlive = CountLivingEnemiesInGroup("enemies");
		int bossAlive = CountLivingEnemiesInGroup("boss");
		int regularEnemiesAlive = totalEnemiesAlive - bossAlive;

		if (regularEnemiesAlive <= 0 && !bossSpawned)
		{
			SpawnBoss();
			bossSpawned = true;
			bossAnnouncementActive = true;
			bossAnnouncementTimer = 4.0f;
			return;
		}

		if (bossAnnouncementActive)
		{
			bossAnnouncementTimer -= (float)delta;
			Text = "A Soldier has appeared!";

			if (bossAnnouncementTimer <= 0)
				bossAnnouncementActive = false;

			return;
		}

		if (regularEnemiesAlive > 0)
		{
			Text = "Enemies Remaining: " + regularEnemiesAlive;
			return;
		}

		if (bossAlive > 0)
		{
			Text = "Boss Fight: Soldier";
			return;
		}

		if (!levelComplete)
		{
			levelComplete = true;
			countdownTimer = WinDelay;

			if (!victoryPlayed)
			{
				victoryPlayed = true;

				if (_bgMusic != null && _bgMusic.Playing)
					_bgMusic.Stop();

				if (_victoryMusic != null)
				{
					_victoryMusic.Stop();
					_victoryMusic.Play();
					GD.Print("✅ Playing victory music");
				}
				else
				{
					GD.Print("❌ Victory music node not found");
				}
			}
		}

		countdownTimer -= (float)delta;
		int winSecondsLeft = Mathf.Max(0, Mathf.CeilToInt(countdownTimer));

		Text = "Town has been Saved!\nReturning to menu in " + winSecondsLeft + "...";

		if (countdownTimer <= 0)
			GetTree().ChangeSceneToFile("res://MainMenu/main_menu.tscn");
	}

	private int CountLivingEnemiesInGroup(string groupName)
	{
		var nodes = GetTree().GetNodesInGroup(groupName);
		int count = 0;

		foreach (Node node in nodes)
		{
			if (!IsInstanceValid(node))
				continue;

			if (node.HasMethod("IsAliveEnemy"))
			{
				bool alive = (bool)node.Call("IsAliveEnemy");
				if (alive)
					count++;
			}
			else
			{
				count++;
			}
		}

		return count;
	}

	private void SpawnBoss()
	{
		if (SoldierScene == null)
		{
			GD.Print("❌ SoldierScene NOT assigned!");
			Text = "Boss scene missing!";
			return;
		}

		Node2D soldier = SoldierScene.Instantiate<Node2D>();

		if (_bossSpawnPoint != null)
			soldier.GlobalPosition = _bossSpawnPoint.GlobalPosition;
		else
			soldier.GlobalPosition = new Vector2(0, 0);

		GetTree().CurrentScene.AddChild(soldier);

		if (_camera != null)
			_camera.ShowBossIntro(soldier);
	}
}
