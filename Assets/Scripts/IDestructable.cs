using UnityEngine;
using System.Collections;

public abstract class IDestructable : IPooledObject
{
	public abstract void Destroy(MonoBehaviour controller);
}
