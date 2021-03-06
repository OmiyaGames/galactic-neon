﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PowerUpGun : IPowerUp, IGun
{
	public string gunName = "Normal Gun";
	public IDestructable bullet = null;
	public float gapBetweenFire = 0.2f;
	public Color gunColor = Color.white;
	public bool showReticle = false;

	public float GapBetweenFire
	{
		get
		{
			return gapBetweenFire;
		}
	}

	public Color GunColor
	{
		get
		{
			return gunColor;
		}
	}

	public bool ShowReticle
	{
		get
		{
			return showReticle;
		}
	}

	public void Fire(Transform gun)
	{
		GlobalGameObject.Get<PoolingManager>().GetInstance(bullet.gameObject, gun.position, gun.rotation);
	}

	protected override string Trigger(ShipController instance)
	{
		instance.GunType = this;
		return gunName;
	}
}
