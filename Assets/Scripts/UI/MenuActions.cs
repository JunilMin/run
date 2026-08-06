using UnityEngine;
using UnityEngine.SceneManagement;
namespace MindfulRun.UI
{
    public sealed class MenuActions : MonoBehaviour
    {
        public void Play() => SceneManager.LoadScene("Stage_01");
        public void Title() => SceneManager.LoadScene("MainMenu");
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
