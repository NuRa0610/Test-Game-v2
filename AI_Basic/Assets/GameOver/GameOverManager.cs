using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _primaryButtonText;
    [SerializeField]
    private Image _backgroundImage;
    [SerializeField]
    private Sprite[] _winBackgrounds;
    [SerializeField]
    private Sprite[] _loseBackgrounds;

    // Start is called before the first frame update
    public void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UpdatePrimaryButtonText();
        UpdateBackground();
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

    private void UpdateBackground()
    {
        Image backgroundImage = _backgroundImage != null
            ? _backgroundImage
            : FindBackgroundImage();

        if (backgroundImage == null)
        {
            return;
        }

        Sprite stageBackground = GetStageBackground();
        if (stageBackground != null)
        {
            backgroundImage.sprite = stageBackground;
        }
    }

    private Sprite GetStageBackground()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        Sprite[] backgrounds = null;

        if (activeSceneName == "WinScreen")
        {
            backgrounds = _winBackgrounds;
        }
        else if (activeSceneName == "LoseScreen")
        {
            backgrounds = _loseBackgrounds;
        }

        if (backgrounds == null)
        {
            return null;
        }

        int stageIndex = StageFlow.CurrentStageIndex;
        if (stageIndex < 0 || stageIndex >= backgrounds.Length)
        {
            return null;
        }

        return backgrounds[stageIndex];
    }

    private Image FindBackgroundImage()
    {
        GameObject background = GameObject.Find("Background");
        if (background == null)
        {
            return null;
        }

        return background.GetComponent<Image>();
    }
}
