using UnityEngine;
using System.Collections;

public class Hud : MonoBehaviour
{
	private static Hud msInstance = null;

	public GUISkin buttonSkin;
	[System.Serializable]
	public class StartGameHud
	{
		public GUIText[] labels;
		public Vector2 startButtonDimensions = new Vector2(360, 180);
		public float margin;
		public void Show()
		{
			for(int index = 0; index < labels.Length; ++index)
			{
				if(labels[index] != null)
				{
					labels[index].enabled = true;
				}
			}
		}
		public void Hide()
		{
			for(int index = 0; index < labels.Length; ++index)
			{
				if(labels[index] != null)
				{
					labels[index].enabled = false;
				}
			}
		}
	}
	public StartGameHud startGameHud;
	[System.Serializable]
	public class TutorialHud
	{
		public GUIText[] labels;
		public void Show()
		{
			for(int index = 0; index < labels.Length; ++index)
			{
				if(labels[index] != null)
				{
					labels[index].enabled = true;
				}
			}
		}
		public void Hide()
		{
			for(int index = 0; index < labels.Length; ++index)
			{
				if(labels[index] != null)
				{
					labels[index].enabled = false;
				}
			}
		}
	}
	public TutorialHud tutorialHud;
	[System.Serializable]
	public class InGameHud
	{
		public GUIText lifeLabel;
		public GUITexture lifeBarForeground;
		public GUITexture lifeBarBackground;
		public GUIText scoreLabel;
		public float fullLifePixelWidth = 200;

		public Color LifeBarOriginalColor
		{
			get
			{
				return normalColor.GetValueOrDefault();
			}
		}
		private Color? normalColor = null;

		public void Show()
		{
			lifeLabel.enabled = true;
			scoreLabel.enabled = true;
			lifeBarForeground.enabled = true;
			lifeBarBackground.enabled = true;
			if(normalColor == null)
			{
				normalColor = lifeBarForeground.color;
			}
		}
		public void Hide()
		{
			lifeLabel.enabled = false;
			scoreLabel.enabled = false;
			lifeBarForeground.enabled = false;
			lifeBarBackground.enabled = false;
			if(normalColor == null)
			{
				normalColor = lifeBarForeground.color;
			}
		}
	}
	public InGameHud inGameHud;
	[System.Serializable]
	public class PauseGameHud
	{
		public GUIText titleLabel;
		public Vector2 buttonDimensions = new Vector2(360, 180);
		public float centerMargin = 4f;
		public void Show()
		{
			titleLabel.enabled = true;
		}
		public void Hide()
		{
			titleLabel.enabled = false;
		}
	}
	public PauseGameHud pauseGameHud;
	[System.Serializable]
	public class EndGameHud
	{
		public GUIText titleLabel;
		public GUIText scoreLabel;
		public GUIText bestLabel;
		public Vector2 restartButtonDimensions = new Vector2(360, 180);
		public float margin = 10f;

		public void Show()
		{
			titleLabel.enabled = true;
			scoreLabel.enabled = true;
			bestLabel.enabled = true;
		}
		public void Hide()
		{
			titleLabel.enabled = false;
			scoreLabel.enabled = false;
			bestLabel.enabled = false;
		}
	}
	public EndGameHud endGameHud;

	private Rect startButton;
	private Rect tutorialButton;
	private Rect exitButton;
	private Rect restartButton;
	private Rect backButton;
	private Rect resumeButton;
	private Rect quitButton;
	private bool dontTriggerOnConfirm = false;
	private bool resumeOnUnpause = false;

	void Start()
	{
		msInstance = this;

		// Setup HUD
		inGameHud.Hide();
		endGameHud.Hide();
		pauseGameHud.Hide();
		tutorialHud.Hide();

		// Setup buttons
		startButton.width = startGameHud.startButtonDimensions.x;
		startButton.height = startGameHud.startButtonDimensions.y;
		tutorialButton.width = startGameHud.startButtonDimensions.x;
		tutorialButton.height = startGameHud.startButtonDimensions.y;
		exitButton.width = startGameHud.startButtonDimensions.x;
		exitButton.height = startGameHud.startButtonDimensions.y;
		restartButton.width = endGameHud.restartButtonDimensions.x;
		restartButton.height = endGameHud.restartButtonDimensions.y;
		backButton.width = endGameHud.restartButtonDimensions.x;
		backButton.height = endGameHud.restartButtonDimensions.y;
		resumeButton.width = pauseGameHud.buttonDimensions.x;
		resumeButton.height = pauseGameHud.buttonDimensions.y;
		quitButton.width = pauseGameHud.buttonDimensions.x;
		quitButton.height = pauseGameHud.buttonDimensions.y;

		// Setup events
		ShipController.Instance.deadEvent += OnDeath;
		ShipController.Instance.pauseEvent += OnPause;
		ShipController.Instance.endTutorialEvent += OnTutorialEnd;
	}

	void OnDestroy()
	{
		msInstance = null;
	}

	void OnGUI()
	{
		GUI.skin = buttonSkin;
		switch(ShipController.Instance.ShipState)
		{
			case ShipController.State.Start:
				// Setup buttons
				startButton.center = new Vector2((Screen.width / 2f), (Screen.height / 2));
				tutorialButton.x = startButton.x;
				tutorialButton.y = startButton.yMax + startGameHud.margin;
				exitButton.x = startButton.x;
				exitButton.y = tutorialButton.yMax + startGameHud.margin;
				if(GUI.Button(startButton, "Start") == true)
				{
					StartGame();
					ShipController.Instance.ControlScheme = ShipController.Control.MouseAndKeyboard;
				}
				else if(GUI.Button(tutorialButton, "Controls") == true)
				{
					// Switch to tutorial scene
					ShipController.Instance.StartTutorial();
					startGameHud.Hide();
					tutorialHud.Show();
					ShipController.Instance.PlaySelectSound();
					ShipController.Instance.ControlScheme = ShipController.Control.MouseAndKeyboard;
				}
				else if(GUI.Button(exitButton, "Exit Game") == true)
				{
					// Quit application
					Application.Quit();
				}
				else if(Mathf.Approximately(Input.GetAxis("OK"), 0) == false)
				{
					if(dontTriggerOnConfirm == false)
					{
						StartGame();
						ShipController.Instance.ControlScheme = ShipController.Control.Xbox360Controller;
					}
				}
				else
				{
					dontTriggerOnConfirm = false;
				}
				break;
			case ShipController.State.Dead:
				// Setup buttons
				restartButton.center = new Vector2((Screen.width / 2f), (Screen.height / 2));
				backButton.x = restartButton.x;
				backButton.y = restartButton.yMax + endGameHud.margin;
				if(GUI.Button(restartButton, "Play Again") == true)
				{
					StartGame();
					ShipController.Instance.ControlScheme = ShipController.Control.MouseAndKeyboard;
				}
				else if(GUI.Button(backButton, "Menu") == true)
				{
					// Switch back to start screen
					endGameHud.Hide();
					startGameHud.Show();
					ShipController.Instance.Menu();
					ShipController.Instance.PlaySelectSound();
					ShipController.Instance.ControlScheme = ShipController.Control.MouseAndKeyboard;
				}
				else if(Mathf.Approximately(Input.GetAxis("OK"), 0) == false)
				{
					if(dontTriggerOnConfirm == false)
					{
						StartGame();
						ShipController.Instance.ControlScheme = ShipController.Control.Xbox360Controller;
					}
				}
				else
				{
					dontTriggerOnConfirm = false;
				}
				break;
			case ShipController.State.Paused:
				// Setup buttons
				resumeButton.center = new Vector2((Screen.width / 2f), (Screen.height / 2));
				quitButton.x = resumeButton.x;
				quitButton.y = resumeButton.yMax + pauseGameHud.centerMargin;
				if(resumeOnUnpause == true)
				{
					GUI.FocusControl("Resume");
				}
				else
				{
					GUI.FocusControl("Quit");
				}
				if(GUI.Button(resumeButton, "Resume") == true)
				{
					UnPauseGame ();
					ShipController.Instance.ControlScheme = ShipController.Control.MouseAndKeyboard;
				}
				else if(GUI.Button(quitButton, "Quit") == true)
				{
					dontTriggerOnConfirm = true;
					QuitGame ();
					ShipController.Instance.ControlScheme = ShipController.Control.MouseAndKeyboard;
				}
				else if(Mathf.Approximately(Input.GetAxis("OK"), 0) == false)
				{
					if(dontTriggerOnConfirm == false)
					{
						if(resumeOnUnpause == true)
						{
							UnPauseGame();
						}
						else
						{
							QuitGame();
						}
						ShipController.Instance.ControlScheme = ShipController.Control.Xbox360Controller;
					}
				}
				else
				{
					dontTriggerOnConfirm = false;
					float input = Input.GetAxis("XboxVertical");
					if(input > 0.5)
					{
						resumeOnUnpause = true;
					}
					else if(input < -0.5)
					{
						resumeOnUnpause = false;
					}
				}
				break;
		}
	}

	void OnDeath(ShipController instance)
	{
		inGameHud.Hide();
		if(instance.ShipState == ShipController.State.Dead)
		{
			endGameHud.Show();
			msInstance.endGameHud.scoreLabel.text = "Final " + msInstance.inGameHud.scoreLabel.text;

			// Grab the best score
			int bestScore = PlayerPrefs.GetInt("bestScore", 0);
			if(ShipController.Instance.Score > bestScore)
			{
				bestScore = ShipController.Instance.Score;
				PlayerPrefs.SetInt("bestScore", bestScore);
			}
			msInstance.endGameHud.bestLabel.text = "Best Score: " + bestScore.ToString();
		}
		else
		{
			startGameHud.Show();
		}
	}

	void OnPause(ShipController instance)
	{
		pauseGameHud.Show();
		resumeOnUnpause = true;
		dontTriggerOnConfirm = true;
		ShipController.Instance.PlaySelectSound();
		ShipController.Instance.ControlScheme = ShipController.Control.MouseAndKeyboard;
	}

	void OnTutorialEnd(ShipController instance)
	{
		// Switch to tutorial scene
		startGameHud.Show();
		tutorialHud.Hide();
		ShipController.Instance.PlaySelectSound();
		ShipController.Instance.ControlScheme = ShipController.Control.MouseAndKeyboard;
	}

	void StartGame ()
	{
		inGameHud.Show ();
		startGameHud.Hide ();
		endGameHud.Hide();
		ShipController.Instance.StartGame ();
		ShipController.Instance.PlaySelectSound();
	}

	void UnPauseGame ()
	{
		pauseGameHud.Hide ();
		inGameHud.Show ();
		ShipController.Instance.Unpause ();
		ShipController.Instance.PlaySelectSound();
	}

	void QuitGame ()
	{
		pauseGameHud.Hide ();
		endGameHud.Show();
		ShipController.Instance.Quit ();
	}

	public static void SetLife(int life)
	{
		if(msInstance != null)
		{
			// Adjust life bar width
			Rect inset = msInstance.inGameHud.lifeBarForeground.pixelInset;
			float lifeRatio = life;
			lifeRatio /= ShipController.Instance.maxLife;
			if(lifeRatio > 1)
			{
				lifeRatio = 1;
			}
			else if(lifeRatio < 0)
			{
				lifeRatio = 0;
			}
			inset.width = msInstance.inGameHud.fullLifePixelWidth * lifeRatio;
			msInstance.inGameHud.lifeBarForeground.pixelInset = inset;

			// Adjust life bar color
			if(life > ShipController.Instance.dangerWhenLifeBelow)
			{
				msInstance.inGameHud.lifeBarForeground.color = msInstance.inGameHud.LifeBarOriginalColor;
			}
			else
			{
				msInstance.inGameHud.lifeBarForeground.color = Color.red;
			}
		}
	}

	public static void SetScore(int score)
	{
		if(msInstance != null)
		{
			msInstance.inGameHud.scoreLabel.text = "Score: " + score.ToString();
		}
	}
}
