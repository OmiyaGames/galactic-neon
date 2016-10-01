using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FastBloom))]
public class SynchBloomToMusic : MonoBehaviour
{
	[System.Serializable]
	public class SampleRange
	{
		public int minimumIndex = 0;
		public int maximumIndex = 500;
	}
	public AudioSource backgroundMusic;
	public int channel = 0;
	public int numberOfSamples = 1024;
	public SampleRange samplesToAverage;
	public Vector2 bloomIntensityRange = new Vector2(0.5f, 1.5f);
	public float lerpFactor = 10f;
	public float maxFrequency = 2f;

	private float[] allSamples = null;
	private FastBloom bloomEffect = null;
	private float targetIntensity = 0f;

	// Use this for initialization
	void Start ()
	{
		bloomEffect = GetComponent<FastBloom>();
		allSamples = new float[numberOfSamples];
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Grab all samples
		backgroundMusic.GetOutputData(allSamples, channel);

		// Check how much the samples average
		float sampleAverage = 0;
		for(int index = samplesToAverage.minimumIndex; index < samplesToAverage.maximumIndex; ++index)
		{
			sampleAverage += allSamples[index];
		}
		sampleAverage /= (samplesToAverage.maximumIndex - samplesToAverage.minimumIndex);
		if(sampleAverage < 0)
		{
			sampleAverage *= -1f;
		}
		if(sampleAverage > maxFrequency)
		{
			sampleAverage = maxFrequency;
		}
		sampleAverage /= maxFrequency;

		// Update target intensity
		targetIntensity = (sampleAverage * (bloomIntensityRange.y - bloomIntensityRange.x)) + bloomIntensityRange.x;
		bloomEffect.intensity = Mathf.Lerp(bloomEffect.intensity, targetIntensity, (Time.deltaTime * lerpFactor));
	}
}
