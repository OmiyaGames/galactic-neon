using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicEnemyInfo : IEnemyInfo
{
	public readonly IEnemy prefab;
	public readonly GameObject prefabGameObject;
	public readonly int life;
	public readonly Quaternion rotation;

	readonly Vector3 position;

	bool hasRigidBody = false;
	Vector2 velocity = Vector2.zero;
	float angularVelocity = 0;

	bool hasRenderer = false;
	Color color = Color.white;

	public Vector3 Position
	{
		get
		{
			return position;
		}
	}

	public void GatherRigidBodyInfo(Rigidbody2D body)
	{
		if(body != null)
		{
			hasRigidBody = true;
			velocity = body.velocity;
			angularVelocity = body.angularVelocity;

		}
		else
		{
			hasRigidBody = false;
		}
	}

	public void GatherRendererInfo(SpriteRenderer renderer)
	{
		if(renderer != null)
		{
			hasRenderer = true;
			color = renderer.color;
		}
		else
		{
			hasRenderer = false;
		}
	}

	public BasicEnemyInfo(IEnemy newPrefab, Vector3 newPosition, Quaternion newRotation) :
		this(newPrefab, newPrefab.maxLife, newPosition, newRotation) { }

	public BasicEnemyInfo(IEnemy newPrefab, int newLife, Vector3 newPosition, Quaternion newRotation)
	{
		if(newPrefab == null)
		{
			throw new System.ArgumentNullException("newPrefab");
		}
		else
		{
			prefab = newPrefab;
			prefabGameObject = prefab.gameObject;
			life = newLife;
			position = newPosition;
			rotation = newRotation;
		}
	}

	public IEnemy GenerateEnemy()
	{
		// Grab a pooled enemy
		GameObject clone = GlobalGameObject.Get<PoolingManager>().GetInstance(prefabGameObject);

        // Return the enemy script
        IEnemy returnEnemy = clone.GetComponent<IEnemy>();
        returnEnemy.Life = life;

        // Position the enemy
		Transform cloneTransform = clone.transform;
		cloneTransform.position = position;
		cloneTransform.rotation = rotation;

		// Check if this enemy is supposed to have a rigidbody
        if((hasRigidBody == true) && (returnEnemy.physicsBody != null))
		{
			// If so, update its rigidbody
            returnEnemy.physicsBody.velocity = velocity;
            returnEnemy.physicsBody.angularVelocity = angularVelocity;
		}

		// Check if this enemy is supposed to have a renderer
        if((hasRenderer == true) && (returnEnemy.mainRenderer != null))
		{
			// If so, update it's renderer
            returnEnemy.mainRenderer.color = color;
        }

		return returnEnemy;
	}
}
