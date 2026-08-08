using System.Collections;
using UnityEngine;
namespace MindfulRun.Obstacles
{
    public sealed class FallingBlock : MonoBehaviour
    {
        [SerializeField] Rigidbody2D blockBody; [SerializeField] SpriteRenderer warning;
        [SerializeField] float warningDuration = .06f, triggerSpeedThreshold = 4f;
        bool triggered;
        void OnTriggerEnter2D(Collider2D other) { var p = other.GetComponent<MindfulRun.Player.PlayerController2D>(); if (!triggered && p && Mathf.Abs(other.attachedRigidbody.linearVelocity.x) >= triggerSpeedThreshold) StartCoroutine(Drop()); }
        IEnumerator Drop() { triggered = true; if (warning) warning.enabled = true; yield return new WaitForSeconds(warningDuration); if (warning) warning.enabled = false; blockBody.gravityScale = 6f; blockBody.bodyType = RigidbodyType2D.Dynamic; }
    }
}
