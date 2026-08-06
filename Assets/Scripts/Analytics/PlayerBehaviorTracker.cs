using System;
using MindfulRun.Player;
using UnityEngine;

namespace MindfulRun.Analytics
{
    [Serializable]
    public sealed class StageBehaviorData
    {
        public int stageNumber; public float stageStartTime; public float timeUntilFirstInput = -1;
        public float totalTimeMoving, totalTimeStandingStill; public int leftInputs, rightInputs, jumps, deaths, restarts;
        public int immediateRetries; public float lastDeathToRestart = -1, stageCompletionTime;
    }

    public sealed class PlayerBehaviorTracker : MonoBehaviour
    {
        [SerializeField] PlayerController2D player;
        public StageBehaviorData Data { get; private set; }
        static StageBehaviorData activeData; static int activeStage = -1; static float persistentStartedAt, deathAt = -1;
        float startedAt;
        public void Initialize(int stageNumber)
        {
            if (!player) player = FindFirstObjectByType<PlayerController2D>();
            if (activeData == null || activeStage != stageNumber)
            {
                activeData = new StageBehaviorData { stageNumber = stageNumber, stageStartTime = Time.time };
                activeStage = stageNumber; persistentStartedAt = Time.time;
            }
            Data = activeData; startedAt = persistentStartedAt;
            if (player) { player.MovementInput += OnMovement; player.Jumped += OnJump; }
        }
        void Update()
        {
            if (Data == null || !player || !player.ControlsEnabled) return;
            if (Mathf.Abs(player.HorizontalInput) > .01f) Data.totalTimeMoving += Time.unscaledDeltaTime; else Data.totalTimeStandingStill += Time.unscaledDeltaTime;
        }
        void OnMovement(float direction) { if (Data.timeUntilFirstInput < 0) Data.timeUntilFirstInput = Time.time - startedAt; if (direction < 0) Data.leftInputs++; else Data.rightInputs++; }
        void OnJump() { if (Data.timeUntilFirstInput < 0) Data.timeUntilFirstInput = Time.time - startedAt; Data.jumps++; }
        public void RecordDeath() { Data.deaths++; deathAt = Time.unscaledTime; }
        public void RecordRestart() { Data.restarts++; if (deathAt >= 0) { Data.lastDeathToRestart = Time.unscaledTime - deathAt; if (Data.lastDeathToRestart <= 1.5f) Data.immediateRetries++; } }
        public void CompleteStage() { Data.stageCompletionTime = Time.time - startedAt; Debug.Log("[Behavior] " + JsonUtility.ToJson(Data, true)); activeData = null; activeStage = -1; }
        void OnDestroy() { if (player) { player.MovementInput -= OnMovement; player.Jumped -= OnJump; } }
    }
}
