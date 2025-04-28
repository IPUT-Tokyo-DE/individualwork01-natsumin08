using UnityEngine;
using UnityEngine.SceneManagement;

public class Retry : MonoBehaviour
{
    public void First_button()
    {
        SceneManager.LoadScene("1stStage");
    }

    public void Second_button()
    {
        SceneManager.LoadScene("2ndStage");
    }

    public void ExitGameButton()
    {
        Application.Quit();
    }
}
