using System.Collections;
using UnityEngine;

namespace MindfulRun.Obstacles
{
    public sealed class RisingSpikeTrap : MonoBehaviour
    {
        [SerializeField] Transform spikes;
        [SerializeField] SpriteRenderer warning;
        [SerializeField] float warningDuration = .3f;
        [SerializeField] float riseDistance = 1.6f;
        [SerializeField] float riseSpeed = 13f;
        [SerializeField] bool triggerOnJump = true;
        bool triggered; Vector3 hiddenPosition;
        void Awake() { hiddenPosition = spikes.localPosition; if (warning) warning.enabled = false; }
        void OnTriggerStay2D(Collider2D other)
        {
            var player = other.GetComponent<MindfulRun.Player.PlayerController2D>();
            if (!triggered && player && (!triggerOnJump || other.attachedRigidbody.linearVelocity.y > 1f)) StartCoroutine(Activate());
        }
        IEnumerator Activate()
        {
            triggered = true; if (warning) warning.enabled = true; yield return new WaitForSeconds(warningDuration); if (warning) warning.enabled = false;
            Vector3 target = hiddenPosition + Vector3.up * riseDistance;
            while (Vector3.Distance(spikes.localPosition,target) > .02f) { spikes.localPosition = Vector3.MoveTowards(spikes.localPosition,target,riseSpeed*Time.deltaTime); yield return null; }
            yield return new WaitForSeconds(.7f);
            while (Vector3.Distance(spikes.localPosition,hiddenPosition) > .02f) { spikes.localPosition = Vector3.MoveTowards(spikes.localPosition,hiddenPosition,riseSpeed*.45f*Time.deltaTime); yield return null; }
            triggered = false;
        }
    }
}
