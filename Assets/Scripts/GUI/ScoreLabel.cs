using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animation))]
public class ScoreLabel : IPooledObject
{
	public const float saturation = 1f;
	public const float brightness = 1f;

	public TextMesh label;

	private float hue = 0;
	private Color spriteColor = Color.red;

	public void UpdateText(string text)
	{
		label.text = text;
	}

	// Use this for initialization
	public override void Start ()
	{
		// Play animation
		animation.Play();

		// Get a random hue
		hue = Random.value;
		IPowerUp.FromHSV(hue, saturation, brightness, ref spriteColor);
		label.color = spriteColor;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(animation.isPlaying == true)
		{
			hue += IPowerUp.hueChangeSpeed * Time.deltaTime;
			if(hue > 1)
			{
				hue = 0;
			}
			IPowerUp.FromHSV(hue, saturation, brightness, ref spriteColor);
			label.color = spriteColor;
		}
		else
		{
			gameObject.SetActive(false);
		}
	}
}
