﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]
public class BulletControllerShield : BulletController
{
	private Vector3? localPosition;
	private Quaternion? localRotation;

	public void Activate()
	{
		// Retrieve info
		if(localPosition == null)
		{
			localPosition = transform.localPosition;
		}
		else
		{
			transform.localPosition = localPosition.GetValueOrDefault();
		}
		if(localRotation == null)
		{
			localRotation = transform.localRotation;
		}
		else
		{
			transform.localRotation = localRotation.GetValueOrDefault();
		}

		// Activate
		BulletObject.SetActive(true);
	}

	public override void Destroy(MonoBehaviour controller)
	{
        BulletObject.SetActive(false);
	}

	public override void Start()
	{
		collider2D.isTrigger = true;
	}

	protected override void FixedUpdate ()
	{
		// Do nothing
		if(localPosition != null)
		{
			transform.localPosition = localPosition.GetValueOrDefault();
		}
		if(localRotation != null)
		{
			transform.localRotation = localRotation.GetValueOrDefault();
		}
	}
}
