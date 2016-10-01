using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class ShipController : MonoBehaviour
{
	public enum Control
	{
		MouseAndKeyboard,
		Xbox360Controller
	}
	public enum State
	{
		Start,
		Tutorial,
		Playing,
		Paused,
        Shop,
		Dead
	}
	public const float ForceThresholdSquared = 0.05f;
	private static ShipController msInstance = null;

	public int maxLife = 100;
	public float maxVelocity = 0.5f;
	public float lerpValue = 5f;
	public SpriteRenderer gun = null;
	public ParticleSystem jetParticles = null;
	public ParticleSystem gunParticles = null;
	public ParticleSystem appearParticles = null;
	public ParticleSystem hurtParticles = null;
	public ParticleSystem deathParticles = null;
	public Renderer[] allRenderers;
	public BulletControllerShield[] allShields;
	public Animation textAnimation;
	public TextMesh textLabel;
	public AudioSource jetSound;
	public AudioClip powerUpSound;
	public int dangerWhenLifeBelow = 50;
	public AudioClip harmSound;
	public AudioClip dangerSound;
	public AudioClip deathSound;
	public AudioClip selectSound;
	public HomingTrigger homingTrigger;
	public Animation comboTextAnimation;
	public TextMesh comboTextLabel;
	public float comboDuration = 0.5f;
	public float comboMultiplier = 2;
	public int comboScoreLimit = 50;

	public event System.Action<ShipController> startEvent;
	public event System.Action<ShipController> pauseEvent;
	public event System.Action<ShipController> deadEvent;
	public event System.Action<ShipController> endTutorialEvent;

	private Vector2 shipVelocity;
	private bool isShipMoving = false;
	private Vector2 gunDirection;
	private Vector3 mouseRealWorldPosition;
	private Ray cameraRay;
	private float cameraToGunDistance;
	private float angle = 0f;
	private Vector2 checkControlScheme;
	private bool isFiring = false;
	private float lastFired = 0f;
	private int score = 0;
	private int currentLife = 0;
	private State shipState = State.Start;
	private Control controlScheme = Control.MouseAndKeyboard;
	private bool dontPauseOnPrompt = true;
	private IGun gunType = null;
	private float hue = 0;
	private Color spriteColor = Color.red;
	private float comboHue = 0;
	private Color comboSpriteColor = Color.red;
	private Color originalGunColor = Color.yellow;
	private float timeComboStarted = -1f;
	private int comboCounter = 0;
	private Transform shipTransform = null;
    private bool[] wasShieldActive = null;

	#region Properties

	public static ShipController Instance
	{
		get
		{
			return msInstance;
		}
	}

	public static Vector3 Position
	{
		get
		{
			Vector3 returnVector = Vector3.zero;
			if(Instance != null)
			{
				if(Instance.shipTransform == null)
				{
					Instance.shipTransform = Instance.transform;
				}
				if(Instance.shipTransform != null)
				{
					returnVector = Instance.shipTransform.position;
				}
			}
			return returnVector;
		}
        set
        {
            if(Instance != null)
            {
                if(Instance.shipTransform == null)
                {
                    Instance.shipTransform = Instance.transform;
                }
                if(Instance.shipTransform != null)
                {
                    Instance.shipTransform.position = value;
                }
            }
        }
	}

	public int Score
	{
		get
		{
			return score;
		}
	}

	public int Life
	{
		get
		{
			return currentLife;
		}
		set
		{
			if(currentLife != value)
			{
				if(value < currentLife)
				{
					if(value > 0)
					{
						hurtParticles.Stop();
						hurtParticles.Play();
						if(value > dangerWhenLifeBelow)
						{
							audio.PlayOneShot(harmSound);
						}
						else
						{
							audio.PlayOneShot(dangerSound);
							ShipController.Instance.TextEffect("Danger!!!", "textAttention");
						}
					}
					else
					{
						audio.PlayOneShot(deathSound);
						deathParticles.Play();
					}
				}

				currentLife = value;

				if(currentLife < 0)
				{
					currentLife = 0;
				}
				else if(currentLife > maxLife)
				{
					currentLife = maxLife;
				}

				if(currentLife == 0)
				{
					Death();
				}
				Hud.SetLife(currentLife);
			}
		}
	}

	public State ShipState
	{
		get
		{
			return shipState;
		}
	}

	public Control ControlScheme
	{
		get
		{
			return controlScheme;
		}
		set
		{
			if(controlScheme != value)
			{
				controlScheme = value;
				if(controlScheme == Control.MouseAndKeyboard)
				{
					Screen.showCursor = true;
				}
				else
				{
					Screen.showCursor = false;
				}
			}
		}
	}

	public IGun GunType
	{
		set
		{
			gunType = value;
			gun.color = gunType.GunColor;
			homingTrigger.gameObject.SetActive(gunType.ShowReticle);
		}
	}

	#endregion

    #region Public Methods

	public void Menu()
	{
		shipState = State.Start;
	}

	public void StartTutorial()
	{
		// Reset everything
		ResetScore();
		Life = maxLife;
		shipState = State.Tutorial;
		gunType = GetComponent<GunDefault>();
        gun.color = originalGunColor;
        SetShieldEnabled(false);
        
        // Disable homing trigger
        homingTrigger.Reset();
        homingTrigger.gameObject.SetActive(false);

        // Show the ship
        ShowShip(null);
	}

	public void StartGame()
	{
		StartTutorial();
		shipState = State.Playing;

		// Run the start event
		if(startEvent != null)
		{
			startEvent(this);
		}
	}

	public void Unpause()
	{
		// Switch the states
		Time.timeScale = 1;
		shipState = State.Playing;
	}

	public void Quit()
	{
		// Switch the states
		Time.timeScale = 1;
		Life = 0;
	}

	public void PowerUpEffect(string text)
	{
		// Play the power-up particle effect
		appearParticles.Stop();
		appearParticles.Play();

		// Play sound effect
		audio.PlayOneShot(powerUpSound);

		// Play test effect
		TextEffect(text, null);
	}

	public void TextEffect(string text, string animationName)
	{
		// Play the text animation
		textAnimation.Stop();
		if(string.IsNullOrEmpty(animationName) == true)
		{
			textAnimation.Play();
		}
		else
		{
			textAnimation.Play(animationName);
		}
		
		// Update the label
		textLabel.text = text;
		textLabel.renderer.enabled = true;
		
		// Get a random hue
		hue = Random.value;
		IPowerUp.FromHSV(hue, ScoreLabel.saturation, ScoreLabel.brightness, ref spriteColor);
		textLabel.color = spriteColor;
	}

	private void ComboEffect(string text)
	{
		// Play the text animation
		comboTextAnimation.Stop();
		comboTextAnimation.Play();

		// Update the label
		comboTextLabel.text = text;
		comboTextLabel.renderer.enabled = true;
		
		// Get a random hue
		comboHue = Random.value;
		IPowerUp.FromHSV(comboHue, ScoreLabel.saturation, ScoreLabel.brightness, ref comboSpriteColor);
		comboTextLabel.color = comboSpriteColor;
	}

	public void SetShieldEnabled(bool enable)
	{
        if(wasShieldActive == null)
        {
            wasShieldActive = new bool[allShields.Length];
        }
		for (int index = 0; index < allShields.Length; ++index)
		{
			if (allShields[index] != null)
			{
                wasShieldActive[index] = allShields[index].BulletObject.activeSelf;
				if(enable == true)
				{
					allShields[index].Activate();
				}
				else
				{
					allShields[index].Destroy(null);
				}
			}
		}
	}

	public void PlaySelectSound()
	{
		audio.PlayOneShot(selectSound);
	}

	public void IncrementScore(ScoreLabel label, int incrementBy)
	{
		int difference = IncrementScore(incrementBy) - incrementBy;
		if(difference > 0)
		{
			label.UpdateText(incrementBy.ToString() + '+' + difference);
		}
		else
		{
			label.UpdateText(incrementBy.ToString());
		}
	}

	public int IncrementScore(int incrementBy)
	{
		if(incrementBy > 0)
		{
			if((Time.time - timeComboStarted) > comboDuration)
			{
				comboCounter = 1;
			}
			else
			{
				// Increment score (before incrementing combo)
				if(comboCounter < comboScoreLimit)
				{
					incrementBy += Mathf.FloorToInt(comboCounter * comboMultiplier);
				}
				else
				{
					incrementBy += Mathf.FloorToInt(comboScoreLimit * comboMultiplier);
				}
				
				// Display the combo
				++comboCounter;
				ComboEffect("Combo x" + comboCounter);
			}
			timeComboStarted = Time.time;
			score += incrementBy;
			Hud.SetScore(score);
		}
		else
		{
			incrementBy = 0;
		}
		return incrementBy;
	}

	public void ResetScore()
	{
		score = 0;
		Hud.SetScore(0);
	}

    public void UpdateShop(bool enterShop)
    {
        if(enterShop == true)
        {
            // Update state
            shipState = State.Shop;

            // Hide the ship
            HideShip();
        }
        else
        {
            // Update state
            shipState = State.Playing;

            // Show the ship
            ShowShip(wasShieldActive);
        }
    }

    #endregion

	void EndTutorial()
	{
		// Stop tutorial
		shipState = State.Start;
		
        // Hide the ship
        HideShip();

		// Run the end tutorial event
		if(endTutorialEvent != null)
		{
			endTutorialEvent(this);
		}
	}

	void Death()
	{
		// Indicate you're dead
		shipState = State.Dead;

		// Hide all renderers
		for (int index = 0; index < allRenderers.Length; ++index)
		{
			if (allRenderers[index] != null)
			{
				allRenderers[index].enabled = false;
			}
		}
		SetShieldEnabled(false);
		
		// Stop the jet particle
		jetParticles.Stop();
		
		// Show the cursor
		ControlScheme = Control.MouseAndKeyboard;
		
		// Hide the text animation
		textAnimation.Stop();
		textLabel.renderer.enabled = false;
		comboTextAnimation.Stop();
		comboTextLabel.renderer.enabled = false;

		// Run the death event
		if(deadEvent != null)
		{
			deadEvent(this);
		}
	}

	void Awake()
	{
		// Setup variables
		msInstance = this;
		cameraToGunDistance = Mathf.Abs(gun.transform.position.z - Camera.main.transform.position.z);

        if(enabled == true)
        {
            // Hide all renderers
            for (int index = 0; index < allRenderers.Length; ++index)
            {
                if (allRenderers[index] != null)
                {
                    allRenderers[index].enabled = false;
                }
            }
        }

		// Get gun color
		originalGunColor = gun.color;
	}

	void OnDestroy()
	{
		msInstance = null;
	}

	void Update()
	{
        // FIXME: use InControl this time, since the control stuff I have is too buggy.
		if((ShipState == State.Playing) || (ShipState == State.Tutorial))
		{

			if(Mathf.Approximately(Input.GetAxis("Pause"), 0) == false)
			{
				if(dontPauseOnPrompt == false)
				{
					if(ShipState == State.Tutorial)
					{
						EndTutorial();
					}
					else
					{
						dontPauseOnPrompt = true;
						Time.timeScale = 0;
						shipState = State.Paused;
						if(pauseEvent != null)
						{
							pauseEvent(this);
						}
					}
				}
				else
				{
					UpdateControlScheme();
					ControlShip();
					ControlGun();
				}
			}
			else
			{
				UpdateControlScheme();
				ControlShip();
				ControlGun();
				dontPauseOnPrompt = false;
			}

			// Check if the text is visible
			if(textLabel.renderer.enabled == true)
			{
				// Check if it's still animating
				if(textAnimation.isPlaying == true)
				{
					// Animate its color
					hue += IPowerUp.hueChangeSpeed * Time.deltaTime;
					if(hue > 1)
					{
						hue = 0;
					}
					IPowerUp.FromHSV(hue, ScoreLabel.saturation, ScoreLabel.brightness, ref spriteColor);
					textLabel.color = spriteColor;
				}
				else
				{
					// Hide the text label when it's done animating
					textLabel.renderer.enabled = false;
				}
			}
			if(comboTextLabel.renderer.enabled == true)
			{
				// Check if it's still animating
				if(comboTextAnimation.isPlaying == true)
				{
					// Animate its color
					comboHue += IPowerUp.hueChangeSpeed * Time.deltaTime;
					if(comboHue > 1)
					{
						comboHue = 0;
					}
					IPowerUp.FromHSV(comboHue, ScoreLabel.saturation, ScoreLabel.brightness, ref comboSpriteColor);
					comboTextLabel.color = comboSpriteColor;
				}
				else
				{
					// Hide the text label when it's done animating
					comboTextLabel.renderer.enabled = false;
				}
			}
		}
	}

	void FixedUpdate()
	{
		// Check if there's any input
		if(((ShipState == State.Playing) || (ShipState == State.Tutorial)) && (isShipMoving == true))
		{
			// Adjust the ship's velocity
			rigidbody2D.velocity = Vector2.Lerp(rigidbody2D.velocity, shipVelocity, (lerpValue * Time.deltaTime));
		}
	}

	void UpdateControlScheme()
	{
		// Check if the keyboard was pressed
		checkControlScheme.x = Input.GetAxis("KeyHorizontal");
		checkControlScheme.y = Input.GetAxis("KeyVertical");
		if(checkControlScheme.sqrMagnitude > ForceThresholdSquared)
		{
			// Indicate the controls should switch to mouse and keyboard
			ControlScheme = Control.MouseAndKeyboard;
			return;
		}

		// Check if the mouse has moved
		checkControlScheme.x = Input.GetAxis("MouseHorizontal");
		checkControlScheme.y = Input.GetAxis("MouseVertical");
		if(checkControlScheme.sqrMagnitude > ForceThresholdSquared)
		{
			// Indicate the controls should switch to mouse and keyboard
			ControlScheme = Control.MouseAndKeyboard;
			return;
		}

		// Check if the left-stick was tilted
		checkControlScheme.x = Input.GetAxis("XboxHorizontal");
		checkControlScheme.y = Input.GetAxis("XboxVertical");
		if(checkControlScheme.sqrMagnitude > ForceThresholdSquared)
		{
			// Indicate the controls should switch to Xbox 360 controllers
			ControlScheme = Control.Xbox360Controller;
			return;
		}

		// Check if the right-stick was tilted
		checkControlScheme.x = Input.GetAxis("FireHorizontal");
		checkControlScheme.y = Input.GetAxis("FireVertical");
		if(checkControlScheme.sqrMagnitude > ForceThresholdSquared)
		{
			// Indicate the controls should switch to Xbox 360 controllers
			ControlScheme = Control.Xbox360Controller;
			return;
		}
	}

	void ControlShip()
	{
		// Get the controls
		switch(ControlScheme)
		{
		case Control.MouseAndKeyboard:
			shipVelocity.x = Input.GetAxis("KeyHorizontal");
			shipVelocity.y = Input.GetAxis("KeyVertical");
			break;
		case Control.Xbox360Controller:
			shipVelocity.x = Input.GetAxis("XboxHorizontal");
			shipVelocity.y = Input.GetAxis("XboxVertical");
			break;
		}

		// Check if the ship is moving
		isShipMoving = false;
		if (shipVelocity.sqrMagnitude > ForceThresholdSquared)
		{
			// Indicate it's moving
			isShipMoving = true;

			// Normalize the velocity
			shipVelocity.Normalize ();

			// Turn the ship around
			angle = Vector2.Angle (Vector2.up, shipVelocity);
			if (shipVelocity.x > 0)
			{
				angle = 360f - angle;
			}
			transform.rotation = Quaternion.Euler (0f, 0f, angle);

			// Multiply the velocity
			shipVelocity *= maxVelocity;
		}

		// Emit jet particles if it isn't already
		if(jetParticles.enableEmission != isShipMoving)
		{
			jetParticles.enableEmission = isShipMoving;
			if(isShipMoving == true)
			{
				jetSound.Play();
			}
			else
			{
				jetSound.Stop();
			}
		}
	}

	void ControlGun()
	{
		if (gun != null)
		{
			switch(ControlScheme)
			{
			case Control.MouseAndKeyboard:
				ControlGunWithMouse();
				break;
			case Control.Xbox360Controller:
				ControlGunWithXbox360();
				break;
			}
		}
	}

	void ControlGunWithMouse()
	{
		// Get the mouse position
		cameraRay = Camera.main.ScreenPointToRay (Input.mousePosition);
		mouseRealWorldPosition = cameraRay.GetPoint (cameraToGunDistance);

		// Find the direction from mouse to gun
		gunDirection.x = mouseRealWorldPosition.x - gun.transform.position.x;
		gunDirection.y = mouseRealWorldPosition.y - gun.transform.position.y;

		// Turn the gun around
		gun.transform.rotation = LookRotation2D(gunDirection);

		// Check if we want to fire a bullet
		if (Input.GetMouseButtonDown (0) == true)
		{
			isFiring = true;
			gunType.Fire(gun.transform);
			gunParticles.Emit (50);
			lastFired = Time.time;
		}
		else if (isFiring == true)
		{
			if (Input.GetMouseButtonUp (0) == true)
			{
				isFiring = false;
			}
			else if ((Time.time - lastFired) > gunType.GapBetweenFire)
			{
				gunType.Fire(gun.transform);
				gunParticles.Emit (50);
				lastFired = Time.time;
			}
		}
	}

	void ControlGunWithXbox360()
	{
		// Find the direction from right joystick
		gunDirection.x = Input.GetAxis("FireHorizontal");
		gunDirection.y = Input.GetAxis("FireVertical");

		// Make sure it indicates a direction
		if(gunDirection.sqrMagnitude > ForceThresholdSquared)
		{
			// Turn the gun around
			gun.transform.rotation = LookRotation2D(gunDirection);;
			
			// Check if we want to fire a bullet
			if((Time.time - lastFired) > gunType.GapBetweenFire)
			{
				isFiring = true;
				gunType.Fire(gun.transform);
				gunParticles.Emit (50);
				lastFired = Time.time;
			}
		}
		else
		{
			isFiring = false;
		}
	}

	void OnCollisionEnter2D(Collision2D info)
	{
		if(info.gameObject.CompareTag("Enemy") == true)
		{
			// Damage the ship
			IEnemy enemy = info.gameObject.GetComponent<IEnemy>();
			Life -= enemy.Damage(this);
			FollowShip.ShakeCamera();

			// Destroy the enemy
			enemy.Destroy(this);
		}
	}

    void HideShip()
    {
        // Hide all renderers
        for (int index = 0; index < allRenderers.Length; ++index)
        {
            if (allRenderers[index] != null)
            {
                allRenderers[index].enabled = false;
            }
        }
        SetShieldEnabled(false);

        // Stop the jet particle
        jetParticles.Stop();

        // Stop the jet sound
        jetSound.Stop();

        // Show the cursor
        ControlScheme = Control.MouseAndKeyboard;

        // Hide the text animation
        textAnimation.Stop();
        textLabel.renderer.enabled = false;
    }

    void ShowShip(bool[] activateShields)
    {
        // Reset combo meter
        dontPauseOnPrompt = true;
        timeComboStarted = -1f;
        comboCounter = 0;

        // Show all renderers
        for (int index = 0; index < allRenderers.Length; ++index)
        {
            if (allRenderers[index] != null)
            {
                allRenderers[index].enabled = true;
            }
        }

        // Check if we need to show shields
        if(activateShields == null)
        {
            SetShieldEnabled(false);
        }
        else
        {
            for (int index = 0; index < allShields.Length; ++index)
            {
                if (allShields[index] != null)
                {
                    if(activateShields[index] == true)
                    {
                        allShields[index].Activate();
                    }
                    else
                    {
                        allShields[index].Destroy(null);
                    }
                }
            }
        }

        // Make a puff
        appearParticles.Play();
        jetParticles.Play();
    }

	public static float Angle(Vector2 direction)
	{
		float angle = 0f;
		if(direction.sqrMagnitude > 0)
		{
			// Normalize the direction
			direction.Normalize();
			
			// Calculate angle
			angle = Vector2.Angle (Vector2.up, direction);
			if (direction.x > 0)
			{
				angle = 360f - angle;
			}
		}
		return angle;
	}

	public static Quaternion LookRotation2D(Vector2 direction)
	{
		// Create Quaternion
		return Quaternion.Euler(0f, 0f, Angle(direction));
	}
}
