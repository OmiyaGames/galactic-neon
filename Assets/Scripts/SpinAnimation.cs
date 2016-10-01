using UnityEngine;
using System.Collections;

public class SpinAnimation : MonoBehaviour
{
	public float rotateSpeed = 90;
	public Vector3 localPosition = Vector3.zero;

	void Update ()
	{
		transform.localPosition = localPosition;
		transform.Rotate(0, 0, (rotateSpeed * Time.deltaTime));
	}
}
