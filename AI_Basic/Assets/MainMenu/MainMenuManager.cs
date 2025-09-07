using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    public void Play()
    {
        SceneManager.LoadScene("Gameplay");
    }

    // Update is called once per frame
    public void Exit()
    {
        Application.Quit();
    }
}
