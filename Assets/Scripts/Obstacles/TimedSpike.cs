using System.Collections;
using UnityEngine;
namespace MindfulRun.Obstacles
{
    public sealed class TimedSpike : MonoBehaviour
    {
        [SerializeField] float safeDuration = 1.5f, warningDuration = .6f, activeDuration = 1f;
        [SerializeField] SpriteRenderer visual; [SerializeField] Collider2D lethalCollider;
        IEnumerator Start() { if (!visual) visual = GetComponent<SpriteRenderer>(); if (!lethalCollider) lethalCollider = GetComponent<Collider2D>(); while (true) { Set(false, Color.gray); yield return new WaitForSeconds(safeDuration); Set(false, new Color(1,.55f,.1f)); yield return new WaitForSeconds(warningDuration); Set(true, Color.red); yield return new WaitForSeconds(activeDuration); } }
        void Set(bool active, Color color) { if (visual) visual.color = color; if (lethalCollider) lethalCollider.enabled = active; }
    }
}
