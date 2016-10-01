using UnityEngine;
using System.Collections;

public class TitleMenu : MonoBehaviour
{
    public GameObject loadingPanel;
    public UnityEngine.UI.Button[] allButtons;

    public void OnModeClicked(string sceneName)
    {
        if(loadingPanel.activeSelf == false)
        {
            // Show loading
            loadingPanel.SetActive(true);

            // Disable all buttons
            for(int index = 0; index < allButtons.Length; ++index)
            {
                allButtons[index].interactable = false;
            }

            // Load the next scene
            Application.LoadLevelAsync(sceneName);
        }
    }
}
