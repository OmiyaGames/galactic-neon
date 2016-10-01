using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// A script that makes only one instance of a game object persist through each scene.
/// </summary>
public class GlobalGameObject : MonoBehaviour
{
	struct CachedGameObject
	{
		public readonly GameObject gameObject;
		public readonly Dictionary<Type, Component> cachedComponents;

		public CachedGameObject(GameObject theObject)
		{
			gameObject = theObject;
			if(theObject != null)
			{
				cachedComponents = new Dictionary<Type, Component>();
			}
			else
			{
				cachedComponents = null;
			}
		}
	}

	static GlobalGameObject instance = null;

	GameObject cachedGameObject = null;
	Transform cachedtransform = null;
	readonly Dictionary<Type, Component> cachedComponents = new Dictionary<Type, Component>();
	readonly Dictionary<string, CachedGameObject> cachedNameAndComponents = new Dictionary<string, CachedGameObject>();

	void Awake()
	{
		// Check if there's an instance of this script
		if(instance == null)
		{
			// Set the instance
			instance = this;
			cachedGameObject = gameObject;
			cachedtransform = transform;

			// Mark the game object to not destroy itself when switching scenes
			DontDestroyOnLoad(cachedGameObject);

			// Go through all the script components, check if they implement IGlobalScript, and call FirstSceneAwake
			MonoBehaviour[] allScripts = cachedGameObject.GetComponentsInChildren<MonoBehaviour>();
			for(int index = 0; index < allScripts.Length; ++index)
			{
				if(allScripts[index] is IGlobalScript)
				{
					((IGlobalScript)allScripts[index]).FirstSceneAwake(this);
				}
			}
		}
		else
		{
			// Go through all the script components in instance,  check if they implement IGlobalScript, and call SubsequentSceneAwake
			MonoBehaviour[] allScripts = instance.cachedGameObject.GetComponentsInChildren<MonoBehaviour>();
			for(int index = 0; index < allScripts.Length; ++index)
			{
				if(allScripts[index] is IGlobalScript)
				{
					((IGlobalScript)allScripts[index]).FirstSceneAwake(instance);
				}
			}

			// Destroy this game object
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// Gets a component on either this script's GameObject, or its children.
	/// If there are multiple instances of the same component, a random one will be returned.
	/// </summary>
	/// <param name="gameObjectName">
	/// If null, searches all components in the global GameObject.
	/// If not, searched for a child GameObject with the same name first, before searching components in that GameObject.
	/// If there are multiple GameObjects with the same name, a random one will be selected.
	/// </param>
	/// <param name="overwriteCache">
	/// If set to <c>true</c> overwrite the cached value, if there is any.
	/// </param>
	/// <typeparam name="T">
	/// The type of component you are retrieving.
	/// </typeparam>
	public static T Get<T>(string gameObjectName = null, bool overwriteCache = false) where T : Component
	{
		// By default, return null
		T returnComponent = null;

		// Make sure there's an instance of this script
		if(instance != null)
		{
			// Grab the default dictionary and GameObject
			Dictionary<Type, Component> cacheDictionary = instance.cachedComponents;
			GameObject getComponentFrom = instance.cachedGameObject;

			// Check if a name for a GameObject is provided
			if(gameObjectName != null)
			{
				// Check if a dictionary already exists for this game object's name
				if((overwriteCache == false) && (instance.cachedNameAndComponents.ContainsKey(gameObjectName) == true))
				{
					// If so, use the cached information as reference
					getComponentFrom = instance.cachedNameAndComponents[gameObjectName].gameObject;
					cacheDictionary = instance.cachedNameAndComponents[gameObjectName].cachedComponents;
				}
				else
				{
					// If not, search for a GameObject
					Transform foundObject = instance.cachedtransform.Find(gameObjectName);

					// Check if an object was found
					CachedGameObject newCacheInfo = new CachedGameObject(null);
					if(foundObject != null)
					{
						newCacheInfo = new CachedGameObject(foundObject.gameObject);
					}

					// Add or update the entry
					if(instance.cachedNameAndComponents.ContainsKey(gameObjectName) == false)
					{
						instance.cachedNameAndComponents.Add(gameObjectName, newCacheInfo);
					}
					else
					{
						instance.cachedNameAndComponents[gameObjectName] = newCacheInfo;
					}

					// Update reference
					getComponentFrom = newCacheInfo.gameObject;
					cacheDictionary = newCacheInfo.cachedComponents;
				}
			}

			// Check if we have all the necessary information
			if((getComponentFrom != null) && (cacheDictionary != null))
			{
				// Check if the dictionary already contains the type
				if((overwriteCache == false) && (cacheDictionary.ContainsKey(typeof(T)) == true))
				{
					// If so, return the cached component
					returnComponent = (T) cacheDictionary[typeof(T)];
				}
				else
				{
					// If not (or the cache should be overwritten), check if a name was provided
					if(gameObjectName == null)
					{
						// Retrieve the component in the cached game object or its children.
						returnComponent = getComponentFrom.GetComponentInChildren<T>();
					}
					else
					{
						// Retrieve the component in the filtered game object.
						returnComponent = getComponentFrom.GetComponent<T>();
					}

					// Add or overwrite the component into the dictionary
					if(cacheDictionary.ContainsKey(typeof(T)) == true)
					{
						cacheDictionary[typeof(T)] = returnComponent;
					}
					else
					{
						cacheDictionary.Add(typeof(T), returnComponent);
					}
				}
			}
		}
		return returnComponent;
	}
}
