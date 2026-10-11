using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingContinueButton : MonoBehaviour
{

    public void Continue()
    {
        if (SceneManager.GetActiveScene().name == "Ending")
        {
            SceneManager.LoadScene("Credits");
        }
        if (SceneManager.GetActiveScene().name == "Credits")
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
