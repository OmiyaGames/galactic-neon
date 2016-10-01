using UnityEngine;
using System.Collections.Generic;

public abstract class IGameManager : MonoBehaviour
{
	public const float OriginalScreenRatio = 920f / 575f;

	private static IGameManager currentManager;

	public static IGameManager Instance
	{
		get
		{
			return currentManager;
		}
	}

	public static float ScreenRatioMultiplier
	{
		get
		{
			float returnValue = Screen.width;
			returnValue /= Screen.height;
			returnValue /= OriginalScreenRatio;
			return returnValue;
		}
	}

	public static float DistanceSquared(Vector3 from, Vector3 to)
	{
		float xDiff = from.x - to.x;
		xDiff *= xDiff;
		float yDiff = from.y - to.y;
		yDiff *= yDiff;
		return xDiff + yDiff;
	}
	
	public abstract int NumDestructables
	{
		get;
	}
	public abstract IEnumerable<IDestructable> AllDestructables();
	public abstract void AddEnemy(IEnemy destructable);
	public abstract void AddDestructable(IDestructable destructable);

	public virtual void Awake()
	{
		currentManager = this;
	}

	public virtual void OnDestroy()
	{
		currentManager = null;
	}
}
