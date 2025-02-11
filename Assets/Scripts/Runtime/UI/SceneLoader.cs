using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.NCG.Swagaria.Runtime.UI
{
    public class SceneLoader : MonoBehaviour
    {
        public void Play()
        {
            SceneManager.LoadScene("Game");
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}