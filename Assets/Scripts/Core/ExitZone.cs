using UnityEngine;
namespace MindfulRun.Core { public sealed class ExitZone : MonoBehaviour { void OnTriggerEnter2D(Collider2D other) { if (other.GetComponent<MindfulRun.Player.PlayerController2D>()) FindFirstObjectByType<StageManager>()?.Complete(); } } }
