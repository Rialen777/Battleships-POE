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
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }
    
    public void Multiplayer()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Multiplayer");
    }

   // public void LoadScene()
  //  {
   //     SceneManager.LoadScene(whichScene);
   // }
}
