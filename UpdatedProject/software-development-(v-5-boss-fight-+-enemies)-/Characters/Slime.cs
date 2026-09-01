using Godot;
using System;

public partial class Slime : CharacterBody2D
{
	public const float Speed = 45.0f;
	public const float GravityAmount = 900.0f;

	private AnimatedSprite2D _animatedSprite;
	private WeaponHitbox _hitbox;
	private CollisionShape2D _hitboxShape;
	private CollisionShape2D _bodyShape;
	private Player _player;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	private AudioStreamPlayer2D _hurtSound;
	private AudioStreamPlayer2D _deathSound;
	private AudioStreamPlayer2D _attackSound;

	private int health = 25;
	private bool isDead = false;
	private bool isAttacking = false;
	private bool canAttack = true;
	private bool isHurting = false;

	private float patrolCenterX;
	private int patrolDirection = 1;

	private const float PatrolDistance = 40.0f;
	private const float ChaseRange = 140.0f;
	private const float AttackRange = 26.0f;

	public override void _Ready()
	{
		AddToGroup("enemies");

		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_hitbox = GetNode<WeaponHitbox>("WeaponHitbox");
		_hitboxShape = _hitbox.GetNode<CollisionShape2D>("CollisionShape2D");
		_bodyShape = GetNode<CollisionShape2D>("CollisionShape2D");

		_hurtSound = GetNode<AudioStreamPlayer2D>("Slime_Hurt");
		_deathSound = GetNode<AudioStreamPlayer2D>("Slime_Death");
		_attackSound = GetNode<AudioStreamPlayer2D>("Slime_Attack");

		_player = GetParent().GetNodeOrNull<Player>("Player");

		_hitboxShape.SetDeferred("disabled", true);
		patrolCenterX = GlobalPosition.X;

		_rng.Randomize();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (isDead)
		{
			Velocity = Vector2.Zero;
			MoveAndSlide();
			return;
		}

		if (_player == null || !_player.IsAlive())
		{
			isAttacking = false;
			_hitboxShape.SetDeferred("disabled", true);

			Vector2 idleVelocity = Velocity;

			if (!IsOnFloor())
				idleVelocity.Y += GravityAmount * (float)delta;
			else
			{
				idleVelocity.Y = 0;
				idleVelocity.X = 0;
			}

			_animatedSprite.Play("slime_idle");
			Velocity = idleVelocity;
			MoveAndSlide();
			return;
		}

		if (isHurting)
		{
			Vector2 hurtVelocity = Velocity;

			if (!IsOnFloor())
				hurtVelocity.Y += GravityAmount * (float)delta;

			Velocity = hurtVelocity;
			MoveAndSlide();
			return;
		}

		Vector2 velocity = Velocity;

		if (!IsOnFloor())
			velocity.Y += GravityAmount * (float)delta;
		else
			velocity.Y = 0;

		float distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);

		if (isAttacking)
		{
			velocity.X = 0;
		}
		else if (distanceToPlayer <= AttackRange)
		{
			velocity.X = 0;
			TryAttack();
		}
		else if (distanceToPlayer <= ChaseRange)
		{
			ChasePlayer(ref velocity);
		}
		else
		{
			Patrol(ref velocity);
		}

		if (!isAttacking && !isHurting)
			UpdateAnimation(velocity.X);

		Velocity = velocity;
		MoveAndSlide();
	}

	private void Patrol(ref Vector2 velocity)
	{
		float leftLimit = patrolCenterX - PatrolDistance;
		float rightLimit = patrolCenterX + PatrolDistance;

		if (GlobalPosition.X <= leftLimit)
			patrolDirection = 1;
		else if (GlobalPosition.X >= rightLimit)
			patrolDirection = -1;

		velocity.X = patrolDirection * Speed;
		_animatedSprite.FlipH = patrolDirection < 0;
	}

	private void ChasePlayer(ref Vector2 velocity)
	{
		if (_player.GlobalPosition.X > GlobalPosition.X)
		{
			velocity.X = Speed;
			_animatedSprite.FlipH = false;
		}
		else
		{
			velocity.X = -Speed;
			_animatedSprite.FlipH = true;
		}
	}

	private void UpdateAnimation(float moveX)
	{
		if (!IsOnFloor())
		{
			_animatedSprite.Play("slime_jump");
			return;
		}

		if (Mathf.Abs(moveX) > 0.1f)
			_animatedSprite.Play("slime_walk");
		else
			_animatedSprite.Play("slime_idle");
	}

	private async void TryAttack()
	{
		if (!canAttack || isAttacking || isDead || isHurting)
			return;

		if (_player == null || !_player.IsAlive())
			return;

		canAttack = false;
		isAttacking = true;

		_attackSound.Stop();
		_attackSound.Play();

		bool heavyAttack = _rng.RandiRange(0, 1) == 1;

		if (heavyAttack)
		{
			_animatedSprite.Play("slime_charged");
			_hitbox.Damage = 8;
		}
		else
		{
			_animatedSprite.Play("slime_attack");
			_hitbox.Damage = 5;
		}

		_hitbox.Position = _animatedSprite.FlipH ? new Vector2(-22, 0) : new Vector2(22, 0);

		await ToSignal(GetTree().CreateTimer(0.15f), "timeout");

		if (_player == null || !_player.IsAlive() || isDead)
		{
			_hitboxShape.SetDeferred("disabled", true);
			isAttacking = false;
			canAttack = true;
			return;
		}

		_hitboxShape.SetDeferred("disabled", false);

		await ToSignal(GetTree().CreateTimer(0.18f), "timeout");
		_hitboxShape.SetDeferred("disabled", true);

		isAttacking = false;

		await ToSignal(GetTree().CreateTimer(0.6f), "timeout");
		canAttack = true;
	}

	public async void TakeDamage(int damage)
	{
		if (isDead)
			return;

		health -= damage;

		if (_animatedSprite.FlipH)
			Velocity = new Vector2(90, -45);
		else
			Velocity = new Vector2(-90, -45);

		if (health > 0)
		{
			_hurtSound.Stop();
			_hurtSound.Play();

			isHurting = true;
			isAttacking = false;
			_hitboxShape.SetDeferred("disabled", true);
			_animatedSprite.Play("slime_hurt");

			await ToSignal(GetTree().CreateTimer(0.25f), "timeout");
			isHurting = false;
		}
		else
		{
			Die();
		}
	}

	private async void Die()
	{
		if (isDead)
			return;

		isDead = true;
		isAttacking = false;
		isHurting = false;
		canAttack = false;

		Velocity = Vector2.Zero;
		_hitboxShape.SetDeferred("disabled", true);
		_bodyShape.SetDeferred("disabled", true);

		if (!_deathSound.Playing)
			_deathSound.Play();

		_animatedSprite.Play("slime_death");

		await ToSignal(GetTree().CreateTimer(0.8f), "timeout");
		QueueFree();
	}

	public bool IsAliveEnemy()
	{
		return !isDead;
	}
}
