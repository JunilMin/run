using UnityEngine;
using UnityEngine.SceneManagement;
namespace MindfulRun.Core
{
    public sealed class PauseMenu : MonoBehaviour
    {
        static PauseMenu instance; [SerializeField] GameObject panel;
        void Awake(){instance=this; panel?.SetActive(false);}
        public static void Toggle(){if(instance)instance.SetPaused(!instance.panel.activeSelf);}
        public void SetPaused(bool value){panel.SetActive(value);Time.timeScale=value?0:1;}
        public void Resume(){SetPaused(false);}
        public void Restart(){Time.timeScale=1;SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);}
        public void QuitToTitle(){Time.timeScale=1;SceneManager.LoadScene("MainMenu");}
    }
}
