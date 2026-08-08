using System.Collections;
using UnityEngine;

namespace MindfulRun.Obstacles
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class CollapsingPlatform : MonoBehaviour
    {
        [SerializeField] float warningDuration = .06f;
        [SerializeField] float respawnDelay = 2.5f;
        [SerializeField] float fallGravity = 6f;
        Rigidbody2D body; Collider2D platformCollider; SpriteRenderer visual;
        Vector3 startPosition; bool triggered;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>(); platformCollider = GetComponent<Collider2D>(); visual = GetComponent<SpriteRenderer>();
            startPosition = transform.position; body.bodyType = RigidbodyType2D.Kinematic;
        }
        void OnCollisionEnter2D(Collision2D collision)
        {
            if (!triggered && collision.collider.GetComponent<MindfulRun.Player.PlayerController2D>()) StartCoroutine(Collapse());
        }
        IEnumerator Collapse()
        {
            triggered = true; float elapsed = 0;
            while (elapsed < warningDuration) { elapsed += Time.deltaTime; visual.color = Mathf.FloorToInt(elapsed * 40) % 2 == 0 ? new Color(.96f,.16f,.24f) : new Color(.16f,.23f,.34f); transform.position = startPosition + Vector3.right * Mathf.Sin(elapsed * 90) * .035f; yield return null; }
            body.bodyType = RigidbodyType2D.Dynamic; body.gravityScale = fallGravity; platformCollider.enabled = false;
            yield return new WaitForSeconds(respawnDelay); body.bodyType = RigidbodyType2D.Kinematic; body.linearVelocity = Vector2.zero; transform.position = startPosition; transform.rotation = Quaternion.identity; platformCollider.enabled = true; visual.color = new Color(.16f,.23f,.34f); triggered = false;
        }
    }
}
