using UnityEngine;
using System.Collections;

public class BackgroundManager : MonoBehaviour
{
	private static BackgroundManager msInstance = null;

	public BackgroundGenerator[] allGenerators;

	private int index = 0;

	public static BackgroundManager Instance
	{
		get
		{
			return msInstance;
		}
	}

	public void ApplyExplosionForce(Vector3 position)
	{
		for(index = 0; index < allGenerators.Length; ++index)
		{
			if(allGenerators[index] != null)
			{
				allGenerators[index].ApplyExplosionForce(position);
			}
		}
	}

	// Use this for initialization
	void Awake()
	{
		msInstance = this;
	}
	
	// Update is called once per frame
	void OnDestroy()
	{
		msInstance = null;
	}
}
