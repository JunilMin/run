using UnityEngine;
namespace MindfulRun.Core { public sealed class Hazard : MonoBehaviour { void OnTriggerEnter2D(Collider2D other) { if (other.GetComponent<MindfulRun.Player.PlayerController2D>()) FindFirstObjectByType<StageManager>()?.KillPlayer(); } } }
