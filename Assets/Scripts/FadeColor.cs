using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class FadeColor : MonoBehaviour
{
	public Color startColor;
	public Color endColor;
	public float lerpValue;

	private SpriteRenderer sprite;

	// Use this for initialization
	void Start ()
	{
		sprite = GetComponent<SpriteRenderer>();
		sprite.color = startColor;
	}
	
	// Update is called once per frame
	void Update ()
	{
		sprite.color = Color.Lerp(sprite.color, endColor, (Time.deltaTime * lerpValue));
	}
}
