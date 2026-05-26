using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _primaryButtonText;

    // Start is called before the first frame update
    public void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UpdatePrimaryButtonText();
    }
    
    public void Retry()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName == "WinScreen")
        {
            StageFlow.LoadNextStageOrRestart();
            return;
        }

        StageFlow.LoadRetryStage();
    }

    // Update is called once per frame
    public void LoadMainMenu()
    {
        StageFlow.LoadMainMenu();
    }

    private void UpdatePrimaryButtonText()
    {
        if (SceneManager.GetActiveScene().name != "WinScreen")
        {
            return;
        }

        TMP_Text buttonText = _primaryButtonText != null
            ? _primaryButtonText
            : FindPrimaryButtonText();

        if (buttonText != null)
        {
            buttonText.text = StageFlow.WinPrimaryButtonText;
        }
    }

    private TMP_Text FindPrimaryButtonText()
    {
        GameObject retryButton = GameObject.Find("RetryButton");
        if (retryButton == null)
        {
            return null;
        }

        return retryButton.GetComponentInChildren<TMP_Text>();
    }
}
