using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PowerUpMachineGun : IPowerUp, IGun
{
	public IDestructable bullet = null;
	public float gapBetweenFire = 0.05f;
	public float spreadAngle = 10f;
	public Color gunColor = Color.cyan;

	private Vector3 fireAngle;

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
		// Calculate the other 2 angles
		fireAngle = gun.rotation.eulerAngles;
		fireAngle.z += Random.Range(-spreadAngle, spreadAngle);
		Instantiate(bullet.gameObject, gun.position, Quaternion.Euler(fireAngle));
	}

	protected override string Trigger(ShipController instance)
	{
		instance.GunType = this;
		return "Machine Gun";
	}

	public override void Preload(bool state)
	{
		if(state == false)
		{
			GameObject clone = (GameObject)Instantiate(bullet.gameObject, transform.position, Quaternion.identity);
			clone.transform.parent = transform;
		}
		else
		{
			Destroy(gameObject);
		}
	}
}
