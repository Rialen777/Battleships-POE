using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayerButton : MonoBehaviour
{
    // Method to switch to a specific scene
    public void LoadScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
