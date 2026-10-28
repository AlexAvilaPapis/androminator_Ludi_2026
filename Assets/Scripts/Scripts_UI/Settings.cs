using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
}
