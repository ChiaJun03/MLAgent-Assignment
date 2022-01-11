using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndMenuScript : MonoBehaviour
{
    public void PlayAgainButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void MainMenuButton()
    {   
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }
}
