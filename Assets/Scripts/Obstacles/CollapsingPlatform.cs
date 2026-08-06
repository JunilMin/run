using System.Collections;
using UnityEngine;

namespace MindfulRun.Obstacles
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class CollapsingPlatform : MonoBehaviour
    {
        [SerializeField] float warningDuration = .55f;
        [SerializeField] float respawnDelay = 2.5f;
        [SerializeField] float fallGravity = 3f;
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
            while (elapsed < warningDuration) { elapsed += Time.deltaTime; visual.color = Mathf.FloorToInt(elapsed * 14) % 2 == 0 ? Color.red : new Color(.08f,.08f,.09f); transform.position = startPosition + Vector3.right * Mathf.Sin(elapsed * 70) * .035f; yield return null; }
            body.bodyType = RigidbodyType2D.Dynamic; body.gravityScale = fallGravity; platformCollider.enabled = false;
            yield return new WaitForSeconds(respawnDelay); body.bodyType = RigidbodyType2D.Kinematic; body.linearVelocity = Vector2.zero; transform.position = startPosition; transform.rotation = Quaternion.identity; platformCollider.enabled = true; visual.color = new Color(.08f,.08f,.09f); triggered = false;
        }
    }
}
