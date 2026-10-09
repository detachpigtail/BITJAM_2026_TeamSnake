using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuu : MonoBehaviour
{
    //[SerializeField] private string gameSceneName;

    public void Play()
    {
        SceneManager.LoadScene("FactoryScene01");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
