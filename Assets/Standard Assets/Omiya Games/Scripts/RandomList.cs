using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RandomEntry
{
	/// <summary>
	/// The entry.
	/// </summary>
	public GameObject entry = null;
	/// <summary>
	/// The frequency the entry will appear.
	/// </summary>
	public int frequency = 1;
}

[System.Serializable]
public class RandomList
{
	public RandomEntry[] allEntries = new RandomEntry[] {};
	GameObject[] randomizedList = null;
	int index = 0;

	public GameObject RandomElement
	{
		get
		{
			GameObject returnElement = null;
			if(NumberOfEntries > 0)
			{
				// Check if I need to setup a list
				if(randomizedList == null)
				{
					index = 0;
					SetupList();
					ShuffleList();
				}

				// Grab the next element
				returnElement = randomizedList[index++];

				// Check if I need to shuffle the list
				if(index >= randomizedList.Length)
				{
					ShuffleList();
					index = 0;
				}
			}
			return returnElement;
		}
	}

	public int NumberOfEntries
	{
		get
		{
			int returnNum = 0;
			if(allEntries != null)
			{
				returnNum = allEntries.Length;
			}
			return returnNum;
		}
	}

	#region

	void SetupList()
	{
		// Sum all the frequencies
		int index = 0, totalFrequency = 0;
		for(; index < allEntries.Length; ++index)
		{
			// Make sure the frequency is positive
			if(allEntries[index].frequency > 0)
			{
				totalFrequency += allEntries[index].frequency;
			}
		}

		// Generate a new list, populated with entries based on frequency
		int innerIndex = 0, numElements = 0;
		randomizedList = new GameObject[totalFrequency];
		for(index = 0; index < allEntries.Length; ++index)
		{
			// Add the entry the number of times the frequency is set to
			for(innerIndex = 0; innerIndex < allEntries[index].frequency; ++innerIndex)
			{
				randomizedList[numElements] = allEntries[index].entry;
				++numElements;
			}
		}
	}

	void ShuffleList()
	{
		GameObject swapElement;
		int index = 0, randomIndex = 0;
		for(; index < randomizedList.Length; ++index)
		{
			// Swap a random element
			randomIndex = Random.Range(0, randomizedList.Length);
			if(index != randomIndex)
			{
				swapElement = randomizedList[index];
				randomizedList[index] = randomizedList[randomIndex];
				randomizedList[randomIndex] = swapElement;
			}
		}
	}

	#endregion
}
