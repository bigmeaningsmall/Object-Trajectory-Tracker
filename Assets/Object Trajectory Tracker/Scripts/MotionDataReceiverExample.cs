using UnityEngine;
using MotionEnums;

public class MotionDataReceiverExample : MonoBehaviour
{
    public bool active = false;
    public MotionTag motionTag;
    
    public MotionDataFormat motionData;

    private void OnEnable()
    {
        MotionEventManager.OnMotionEvent += HandleMotionEvent;
    }

    private void OnDisable()
    {
        MotionEventManager.OnMotionEvent -= HandleMotionEvent;
    }

    private void HandleMotionEvent(MotionTag motionTag, MotionDataFormat motionData){
        
        if (active && this.motionTag == motionTag){
            Debug.Log(motionTag);
            this.motionData = motionData;
            // Process the received motion data
            // Debug.Log($"Received motion data for {motionTag}: Position - {motionData.position}, Rotation - {motionData.rotation}");
        }
    }

}
