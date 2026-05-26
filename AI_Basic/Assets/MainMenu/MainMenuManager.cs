using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    public void Play()
    {
        StageFlow.LoadFirstStage();
    }

    // Update is called once per frame
    public void Exit()
    {
        Application.Quit();
    }
}
