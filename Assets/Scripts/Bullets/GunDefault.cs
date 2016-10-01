using UnityEngine;
using System.Collections;

public class GunDefault : MonoBehaviour, IGun
{
	public IDestructable bullet = null;
	public float gapBetweenFire = 0.2f;
	public Color gunColor = Color.yellow;

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
		GlobalGameObject.Get<PoolingManager>().GetInstance(bullet.gameObject, gun.position, gun.rotation);
	}
}
