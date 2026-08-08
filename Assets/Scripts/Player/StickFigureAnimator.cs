using UnityEngine;

namespace MindfulRun.Player
{
    public sealed class StickFigureAnimator : MonoBehaviour
    {
        [SerializeField] Transform leftArm, rightArm, leftLeg, rightLeg, torso;
        [SerializeField] SpriteRenderer[] renderers;
        float phase, landPulse; bool dead;
        public void SetMotion(Vector2 velocity, bool grounded, bool landed)
        {
            if (dead) return;
            if (Mathf.Abs(velocity.x) > .05f) transform.localScale = new Vector3(Mathf.Sign(velocity.x), 1, 1);
            if (landed) landPulse = .12f;
            phase += Time.deltaTime * (grounded && Mathf.Abs(velocity.x) > .2f ? 12f : 2f);
            float swing = grounded ? Mathf.Sin(phase) * Mathf.Min(38f, Mathf.Abs(velocity.x) * 5f) : 12f;
            SetZ(leftArm, swing); SetZ(rightArm, -swing); SetZ(leftLeg, -swing); SetZ(rightLeg, swing);
            if (!grounded) { SetZ(leftLeg, velocity.y > 0 ? -25 : 18); SetZ(rightLeg, velocity.y > 0 ? 25 : -18); }
            landPulse = Mathf.Max(0, landPulse - Time.deltaTime);
            torso.localScale = new Vector3(1, landPulse > 0 ? .85f : 1f, 1);
        }
        public void PlayDeath() { dead = true; transform.localRotation = Quaternion.Euler(0, 0, 82); foreach (var r in renderers) r.color = new Color(.96f, .16f, .24f); }
        static void SetZ(Transform t, float angle) { if (t) t.localRotation = Quaternion.Euler(0, 0, angle); }
    }
}
