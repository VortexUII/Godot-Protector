using Godot;
using System;

public partial class WeaponHitbox : Area2D
{
	public int Damage = 0;
	private Node _owner;

	public override void _Ready()
	{
		_owner = GetParent();
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node body)
	{
		if (body == _owner)
			return;

		if (body.HasMethod("TakeDamage"))
		{
			body.Call("TakeDamage", Damage);
		}
	}
}
