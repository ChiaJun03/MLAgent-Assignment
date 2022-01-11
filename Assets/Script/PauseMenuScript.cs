using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfimaGames.LowPolyShooterPack
{
    public class PauseMenuScript : MonoBehaviour
    {
        public static bool GameIsPaused = false;
        public GameObject player;
        public GameObject pauseMenuUI;


        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (GameIsPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }

        public void Resume()
        {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            GameIsPaused = false;
        }

        public void Pause()
        {
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
            GameIsPaused = true;
        }

        public void ResumeByClick()
        {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            GameIsPaused = false;
            player.GetComponent<Character>().cursorLocked = !player.GetComponent<Character>().cursorLocked;
            player.GetComponent<Character>().UpdateCursorState();
        }

        public void LoadMenu()
        {   
            Time.timeScale = 1f;
            GameIsPaused = false;
            SceneManager.LoadScene("StartScene");

        }

        public void QuitGame()
        {
            Debug.Log("QUIT");
            Application.Quit();
        }
    }
}