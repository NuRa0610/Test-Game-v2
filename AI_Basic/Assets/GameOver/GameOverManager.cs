using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    // Start is called before the first frame update
    public void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void Retry()
    {
        SceneManager.LoadScene("Gameplay");
    }

    // Update is called once per frame
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
