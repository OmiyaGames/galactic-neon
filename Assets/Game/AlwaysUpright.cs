using UnityEngine;
using System.Collections;

public class AlwaysUpright : MonoBehaviour
{
	private Quaternion upright = Quaternion.identity;

	// Update is called once per frame
	void LateUpdate ()
	{
		transform.rotation = upright;
	}
}
