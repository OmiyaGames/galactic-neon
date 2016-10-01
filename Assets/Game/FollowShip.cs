using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animation))]
public class FollowShip : MonoBehaviour
{
	public float lerpValue = 7f;

	private static FollowShip msInstance = null;
	private Vector3 targetPosition;

	void Start()
	{
		targetPosition = transform.position;
		msInstance = this;
	}

	void OnDestroy()
	{
		msInstance = null;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if(ShipController.Instance != null)
		{
			targetPosition.x = ShipController.Instance.transform.position.x;
			targetPosition.y = ShipController.Instance.transform.position.y;
			transform.position = Vector3.Lerp(transform.position, targetPosition, (lerpValue * Time.deltaTime));
		}
	}

	public static void ShakeCamera()
	{
		if(msInstance != null)
		{
			msInstance.GetComponent<Animation>().Stop();
			msInstance.GetComponent<Animation>().Play();
		}
	}
}
