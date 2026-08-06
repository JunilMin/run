using UnityEngine;
namespace MindfulRun.Obstacles { public sealed class MovingPlatformPlaceholder : MonoBehaviour { [SerializeField] Vector2 offset = new(3,0); [SerializeField] float speed = 1; Vector3 start; void Start()=>start=transform.position; void Update()=>transform.position=start+(Vector3)(offset*(.5f+.5f*Mathf.Sin(Time.time*speed))); } }
