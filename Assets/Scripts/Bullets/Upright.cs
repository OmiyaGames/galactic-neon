using UnityEngine;
using System.Collections;

public class Upright : MonoBehaviour
{
	// Update is called once per frame
	private static readonly Quaternion upright = Quaternion.identity;
	void Update ()
	{
		transform.rotation = upright;
	}
}
