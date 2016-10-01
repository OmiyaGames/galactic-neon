using UnityEngine;
using System.Collections;

public abstract class IDestructable : MonoBehaviour
{
	public abstract void Destroy(MonoBehaviour controller);
	public abstract void Preload(bool state);
}
