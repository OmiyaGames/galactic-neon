using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IEnemyInfo
{
	Vector3 Position
	{
		get;
	}
	IEnemy GenerateEnemy();
}
