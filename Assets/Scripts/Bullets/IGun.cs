using UnityEngine;

public interface IGun
{
	float GapBetweenFire
	{
		get;
	}
	Color GunColor
	{
		get;
	}
	bool ShowReticle
	{
		get;
	}
	void Fire(Transform gun);
}
