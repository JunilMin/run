using System.Collections;
using MindfulRun.Analytics;
using MindfulRun.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MindfulRun.Core
{
    public sealed class StageManager : MonoBehaviour
    {
        [SerializeField] int stageNumber = 1;
        [SerializeField] string nextSceneName;
        [SerializeField] PlayerController2D player;
        [SerializeField] PlayerBehaviorTracker tracker;
        bool transitioning;
        public int StageNumber => stageNumber;
        void Start() { if (!player) player = FindFirstObjectByType<PlayerController2D>(); if (!tracker) tracker = FindFirstObjectByType<PlayerBehaviorTracker>(); tracker?.Initialize(stageNumber); }
        void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current?.rKey.wasPressedThisFrame == true) Restart(false);
            if (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true) PauseMenu.Toggle();
        }
        public void KillPlayer()
        {
            if (transitioning) return; transitioning = true; tracker?.RecordDeath(); player?.PlayDeath(); StartCoroutine(RestartAfterDeath());
        }
        IEnumerator RestartAfterDeath() { Time.timeScale = .12f; yield return new WaitForSecondsRealtime(.16f); Time.timeScale = 1f; Restart(true); }
        public void Restart(bool afterDeath) { Time.timeScale = 1f; tracker?.RecordRestart(); SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
        public void Complete()
        {
            if (transitioning) return; transitioning = true; player?.SetControlsEnabled(false); tracker?.CompleteStage(); StartCoroutine(LoadNext());
        }
        IEnumerator LoadNext() { yield return new WaitForSecondsRealtime(.35f); SceneManager.LoadScene(nextSceneName); }
    }
}
