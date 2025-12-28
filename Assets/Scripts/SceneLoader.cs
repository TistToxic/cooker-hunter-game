using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("aoygameplayuer");
        // OR: SceneManager.LoadScene(1); // using build index
    }
}
