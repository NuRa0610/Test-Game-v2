using UnityEngine.SceneManagement;

public static class StageFlow
{
    private const string WinScreenSceneName = "WinScreen";
    private const string LoseScreenSceneName = "LoseScreen";
    private const string MainMenuSceneName = "Mainmenu";

    private static readonly string[] StageSceneNames =
    {
        "Stage 1",
        "Stage 2",
        "Stage 3"
    };

    private static string _currentStageSceneName = StageSceneNames[0];

    public static bool IsFinalStage
    {
        get { return CurrentStageIndex >= StageSceneNames.Length - 1; }
    }

    public static int CurrentStageIndex
    {
        get { return GetCurrentStageIndex(); }
    }

    public static string WinPrimaryButtonText
    {
        get { return IsFinalStage ? "Play Again" : "Next Stage"; }
    }

    public static void LoadFirstStage()
    {
        LoadStage(StageSceneNames[0]);
    }

    public static void LoadWinScreen()
    {
        RememberCurrentStage();
        SceneManager.LoadScene(WinScreenSceneName);
    }

    public static void LoadLoseScreen()
    {
        RememberCurrentStage();
        SceneManager.LoadScene(LoseScreenSceneName);
    }

    public static void LoadRetryStage()
    {
        LoadStage(_currentStageSceneName);
    }

    public static void LoadNextStageOrRestart()
    {
        int currentStageIndex = GetCurrentStageIndex();
        if (currentStageIndex < StageSceneNames.Length - 1)
        {
            LoadStage(StageSceneNames[currentStageIndex + 1]);
            return;
        }

        LoadFirstStage();
    }

    public static void LoadMainMenu()
    {
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private static void RememberCurrentStage()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (IsStageScene(activeScene.name))
        {
            _currentStageSceneName = activeScene.name;
        }
    }

    private static void LoadStage(string sceneName)
    {
        _currentStageSceneName = sceneName;
        SceneManager.LoadScene(sceneName);
    }

    private static int GetCurrentStageIndex()
    {
        for (int i = 0; i < StageSceneNames.Length; i++)
        {
            if (StageSceneNames[i] == _currentStageSceneName)
            {
                return i;
            }
        }

        _currentStageSceneName = StageSceneNames[0];
        return 0;
    }

    private static bool IsStageScene(string sceneName)
    {
        for (int i = 0; i < StageSceneNames.Length; i++)
        {
            if (StageSceneNames[i] == sceneName)
            {
                return true;
            }
        }

        return false;
    }
}
