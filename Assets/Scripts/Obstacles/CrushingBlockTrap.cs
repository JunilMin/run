using System.Collections;
using UnityEngine;

namespace MindfulRun.Obstacles
{
    public sealed class CrushingBlockTrap : MonoBehaviour
    {
        [SerializeField] Rigidbody2D block;
        [SerializeField] SpriteRenderer warningShadow;
        [SerializeField] float warningDuration = .06f;
        [SerializeField] float requiredPlayerSpeed = 0f;
        bool triggered;
        void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<MindfulRun.Player.PlayerController2D>();
            if (!triggered && player && Mathf.Abs(other.attachedRigidbody.linearVelocity.x) >= requiredPlayerSpeed) StartCoroutine(Drop());
        }
        IEnumerator Drop()
        {
            triggered=true; if(warningShadow)warningShadow.enabled=true; yield return new WaitForSeconds(warningDuration); if(warningShadow)warningShadow.enabled=false; block.bodyType=RigidbodyType2D.Dynamic; block.gravityScale=8;
        }
    }
}
