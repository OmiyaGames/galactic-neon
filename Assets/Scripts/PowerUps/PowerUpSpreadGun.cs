using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PowerUpSpreadGun : IPowerUp, IGun
{
	public IDestructable bullet = null;
	public float gapBetweenFire = 0.2f;
	public float spreadAngle = 10f;
	public Color gunColor = Color.yellow;
	
	private Vector3 mainAngle;
	private Vector3 otherAngle;

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
			return false;
		}
	}

	public void Fire(Transform gun)
	{
		// Shoot the main bullet
		GlobalGameObject.Get<PoolingManager>().GetInstance(bullet.gameObject, gun.position, gun.rotation);

		// Calculate the other 2 angles
		mainAngle = gun.rotation.eulerAngles;
		otherAngle = mainAngle;
		otherAngle.z -= spreadAngle;
		while(otherAngle.z < 0)
		{
			otherAngle.z += 360;
		}
		GlobalGameObject.Get<PoolingManager>().GetInstance(bullet.gameObject, gun.position, Quaternion.Euler(otherAngle));
		otherAngle = mainAngle;
		otherAngle.z += spreadAngle;
		while(otherAngle.z > 360)
		{
			otherAngle.z -= 360;
		}
		GlobalGameObject.Get<PoolingManager>().GetInstance(bullet.gameObject, gun.position, Quaternion.Euler(otherAngle));
	}

	protected override string Trigger(ShipController instance)
	{
		instance.GunType = this;
		return "Spread Gun";
	}
}
