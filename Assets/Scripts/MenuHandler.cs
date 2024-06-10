using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*Allows buttons to fire various functions, like QuitGame and LoadScene*/

public class MenuHandler : MonoBehaviour {

	//[SerializeField] private string whichScene;

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SinglePlayer()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void Multiplayer()
    {
        SceneManager.LoadScene("Multiplayer");
    }
}
