using UnityEngine;
using System.Collections;

/// <summary>
/// Interface with methods called based on certain GlobalGameObject events.
/// </summary>
public interface IGlobalScript
{
	/// <summary>
	/// Called when the first scene is loaded.
	/// </summary>
	void FirstSceneAwake(GlobalGameObject globalGameObject);
	/// <summary>
	/// Called when any scene after the first one is loaded.
	/// </summary>
	void SubsequentSceneAwake(GlobalGameObject globalGameObject);
}
