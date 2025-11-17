using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
   public void PlayGame()
    {
        Debug.Log($"[Menu] PlayGame - Loading scene 1 at {System.DateTime.Now:HH:mm:ss}");
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        Debug.Log($"[Menu] QuitGame at {System.DateTime.Now:HH:mm:ss}");
        Application.Quit();
    }

    public void Home()
    {
        Debug.Log($"[Menu] Home - Loading scene 0 at {System.DateTime.Now:HH:mm:ss}");
        SceneManager.LoadSceneAsync(0);
    }
}
