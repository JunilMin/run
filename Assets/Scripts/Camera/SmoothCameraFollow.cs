using UnityEngine;
namespace MindfulRun.CameraSystem
{
    public sealed class SmoothCameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target; [SerializeField] float smoothTime=.18f; [SerializeField] Vector2 xBounds=new(0,35); [SerializeField] float fixedY=2.5f; Vector3 velocity;
        void Start(){ if(!target) target=FindFirstObjectByType<MindfulRun.Player.PlayerController2D>()?.transform; }
        void LateUpdate(){ if(!target)return; var desired=new Vector3(Mathf.Clamp(target.position.x,xBounds.x,xBounds.y),fixedY,-10); transform.position=Vector3.SmoothDamp(transform.position,desired,ref velocity,smoothTime); transform.rotation=Quaternion.identity; }
    }
}
