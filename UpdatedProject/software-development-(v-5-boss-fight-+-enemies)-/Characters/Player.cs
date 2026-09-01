using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float Speed = 100.0f;
	public const float JumpVelocity = -150.0f;

	private AnimatedSprite2D _animatedSprite;
	private WeaponHitbox _hitbox;
	private CollisionShape2D _hitboxShape;
	private ProgressBar _healthBar;

	private AudioStreamPlayer2D _attackSound;
	private AudioStreamPlayer2D _hurtSound;
	private AudioStreamPlayer2D _deathSound;
	private AudioStreamPlayer2D _blockSound;

	private int jumpCount = 0;
	private int maxJumps = 2;

	private int health = 100;
	private bool isDead = false;
	private bool isHurting = false;

	private bool isBlocking = false;
	private bool perfectBlock = false;
	private float perfectBlockTimer = 0.0f;

	private float knockbackForce = 120f;

	public override void _Ready()
	{
		AddToGroup("player");

		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_hitbox = GetNode<WeaponHitbox>("WeaponHitbox");
		_hitboxShape = _hitbox.GetNode<CollisionShape2D>("CollisionShape2D");
		_healthBar = GetNode<ProgressBar>("../CanvasLayer2/HealthBar");

		_attackSound = GetNode<AudioStreamPlayer2D>("AttackSound");
		_hurtSound = GetNode<AudioStreamPlayer2D>("HurtSound");
		_deathSound = GetNode<AudioStreamPlayer2D>("DeathSound");
		_blockSound = GetNode<AudioStreamPlayer2D>("BlockSound");

		_hitboxShape.SetDeferred("disabled", true);

		_healthBar.MinValue = 0;
		_healthBar.MaxValue = 100;
		_healthBar.Value = health;

		_animatedSprite.AnimationFinished += OnAnimationFinished;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (isDead)
		{
			Velocity = Vector2.Zero;
			MoveAndSlide();
			return;
		}

		if (isHurting)
		{
			Vector2 v = Velocity;

			if (!IsOnFloor())
				v += GetGravity() * (float)delta;

			Velocity = v;
			MoveAndSlide();
			return;
		}

		Vector2 velocity = Velocity;

		if (IsOnFloor())
			jumpCount = 0;

		if (!IsOnFloor())
			velocity += GetGravity() * (float)delta;

		if (Input.IsActionJustPressed("jump") && jumpCount < maxJumps)
		{
			velocity.Y = JumpVelocity;
			jumpCount++;
		}

		Vector2 direction = Input.GetVector("left", "right", "up", "down");

		if (direction.X > 0)
		{
			velocity.X = Speed;
			_animatedSprite.FlipH = false;
		}
		else if (direction.X < 0)
		{
			velocity.X = -Speed;
			_animatedSprite.FlipH = true;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		if (Input.IsActionJustPressed("block"))
			perfectBlockTimer = 0.2f;

		if (perfectBlockTimer > 0.0f)
			perfectBlockTimer -= (float)delta;

		isBlocking = Input.IsActionPressed("block");
		perfectBlock = perfectBlockTimer > 0.0f;

		bool isAttacking = Input.IsActionPressed("attack");
		bool isCharging = Input.IsActionPressed("charged");

		if (isAttacking)
		{
			_animatedSprite.Play("attack");
			_hitbox.Damage = 5;
			_hitboxShape.SetDeferred("disabled", false);

			_attackSound.Stop();
			_attackSound.Play();

			_hitbox.Position = _animatedSprite.FlipH
				? new Vector2(-22, 0)
				: new Vector2(22, 0);
		}
		else if (isCharging)
		{
			_animatedSprite.Play("charged");
			_hitbox.Damage = 10;
			_hitboxShape.SetDeferred("disabled", false);

			_attackSound.Stop();
			_attackSound.Play();

			_hitbox.Position = _animatedSprite.FlipH
				? new Vector2(-22, 0)
				: new Vector2(22, 0);
		}
		else
		{
			_hitboxShape.SetDeferred("disabled", true);

			if (isBlocking)
				_animatedSprite.Play("block");
			else if (!IsOnFloor())
				_animatedSprite.Play("jump");
			else if (direction.X > 0)
				_animatedSprite.Play("right");
			else if (direction.X < 0)
				_animatedSprite.Play("left");
			else
				_animatedSprite.Play("idle");
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public async void TakeDamage(int damage)
	{
		if (isDead)
			return;

		if (perfectBlock)
		{
			_blockSound.Stop();
			_blockSound.Play();

			GD.Print("Perfect block!");
			return;
		}

		if (isBlocking)
		{
			int reduced = damage / 2;
			health -= reduced;

			_blockSound.Stop();
			_blockSound.Play();
		}
		else
		{
			health -= damage;

			_hurtSound.Stop();
			_hurtSound.Play();
		}

		if (health < 0)
			health = 0;

		_healthBar.Value = health;

		if (_animatedSprite.FlipH)
			Velocity = new Vector2(knockbackForce, -50);
		else
			Velocity = new Vector2(-knockbackForce, -50);

		if (health > 0)
		{
			isHurting = true;
			_hitboxShape.SetDeferred("disabled", true);
			_animatedSprite.Play("hurt");

			await ToSignal(GetTree().CreateTimer(0.25f), "timeout");
			isHurting = false;
		}
		else
		{
			Die();
		}
	}

	private void Die()
	{
		if (isDead)
			return;

		isDead = true;
		Velocity = Vector2.Zero;
		_hitboxShape.SetDeferred("disabled", true);
		_healthBar.Value = 0;

		if (!_deathSound.Playing)
			_deathSound.Play();

		_animatedSprite.Play("death");
	}

	private void OnAnimationFinished()
	{
		if (_animatedSprite.Animation == "death")
		{
			_animatedSprite.Stop();
			_animatedSprite.Frame =
				_animatedSprite.SpriteFrames.GetFrameCount("death") - 1;
		}
	}

	public bool IsAlive()
	{
		return !isDead;
	}
}
