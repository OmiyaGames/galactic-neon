using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class Hud : MonoBehaviour
{
    public enum Mode
    {
        HUD = 0,
        Pause,
        Shop,
        FinalScore
    }
	static Hud msInstance = null;

    public static Mode CurrentMode
    {
        get
        {
            Mode returnMode = Mode.HUD;
            if(msInstance != null)
            {
                returnMode = msInstance.state;
            }
            return returnMode;
        }
        set
        {
            if((msInstance != null) && (msInstance.state != value))
            {
                msInstance.state = value;
                msInstance.UpdateGui(value);
            }
        }
    }

    public string modeField = "State";
    [Header("HUD")]
    public Text hudScoreText;
    public Text hudMoneyText;
    public RectTransform hudLifeBar;
    [Header("Final Score")]
    public Text finalScoreText;
    public Text bestScoreText;

    Mode state = Mode.HUD;
    Animator animatorCache = null;

    void UpdateGui(Mode mode)
    {
        // Update the animation
        if(animatorCache == null)
        {
            animatorCache = GetComponent<Animator>();
        }
        animatorCache.SetInteger(modeField, (int)state);

        // Play sound
        if(mode != Mode.FinalScore)
        {
            ShipController.Instance.PlaySelectSound();
        }
    }

	void Start()
	{
		msInstance = this;

		// Setup events
		ShipController.Instance.deadEvent += OnDeath;
		ShipController.Instance.pauseEvent += OnPause;
		ShipController.Instance.endTutorialEvent += OnTutorialEnd;
	}

	void OnDestroy()
	{
		msInstance = null;
	}

	void OnDeath(ShipController instance)
	{
        CurrentMode = Mode.FinalScore;
		if(instance.ShipState == ShipController.State.Dead)
		{
            msInstance.finalScoreText.text = ShipController.Instance.Score.ToString();

			// Grab the best score
			int bestScore = PlayerPrefs.GetInt("bestScore", 0);
			if(ShipController.Instance.Score > bestScore)
			{
				bestScore = ShipController.Instance.Score;
				PlayerPrefs.SetInt("bestScore", bestScore);
			}
			msInstance.bestScoreText.text = bestScore.ToString();
		}
	}

	void OnPause(ShipController instance)
	{
        CurrentMode = Mode.Pause;
	}

	void OnTutorialEnd(ShipController instance)
	{
        CurrentMode = Mode.HUD;
    }

	void StartGame ()
	{
		ShipController.Instance.StartGame();
        CurrentMode = Mode.HUD;
    }

	void UnPauseGame ()
	{
        CurrentMode = Mode.HUD;
        ShipController.Instance.Unpause();
	}

	void QuitGame ()
	{
        CurrentMode = Mode.HUD;
        ShipController.Instance.Quit();
	}

	public static void SetLife(int life)
	{
		if(msInstance != null)
		{
			// Adjust life bar width
            Vector2 anchor = msInstance.hudLifeBar.anchorMax;
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
            anchor.x = lifeRatio;
            msInstance.hudLifeBar.anchorMax = anchor;


			// FIXME: Adjust life bar color
			if(life > ShipController.Instance.dangerWhenLifeBelow)
			{
				//msInstance.inGameHud.lifeBarForeground.color = msInstance.inGameHud.LifeBarOriginalColor;
			}
			else
			{
				//msInstance.inGameHud.lifeBarForeground.color = Color.red;
			}
		}
	}

	public static void SetScore(int score)
	{
		if(msInstance != null)
		{
			msInstance.hudScoreText.text = "Score: " + score.ToString();
		}
	}
}
