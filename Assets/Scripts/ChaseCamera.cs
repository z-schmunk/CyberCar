using UnityEngine;
namespace CyberCar
{
    public sealed class ChaseCamera:MonoBehaviour
    {
        public GameSession Session;
        bool initialized;
        void LateUpdate()
        {
            if(Session==null||Session.Player==null)return;
            var car=Session.Player.transform;
            Vector3 wanted=car.position+car.rotation*new Vector3(0,7.8f,-12.8f);
            if(!Session.Running&&!Session.Paused&&Session.State==GameState.Menu)wanted=car.position+new Vector3(-9,5,10);
            transform.position=initialized?Vector3.Lerp(transform.position,wanted,1-Mathf.Exp(-6*Time.unscaledDeltaTime)):wanted;initialized=true;
            transform.LookAt(car.position+car.forward*3+Vector3.up);
        }
    }
}
