using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// toggle motion recording with space bar
/// </summary>
public class MotionTrackerControl : MonoBehaviour{
    
    
    public bool recordFlag = false;

    public Text statusText;

    public delegate void RecordAction(string recordAction);
    public static event RecordAction OnRecordAction;

    private void Awake(){
        statusText.text = "RECORD READY";
        statusText.color = Color.white;
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.Space)){
            if (!recordFlag){
                recordFlag = true;
                if (OnRecordAction != null){
                    OnRecordAction("Start"); //send the event to start recording
                    statusText.text = "RECORD MOTION";
                    statusText.color = Color.red;
                }
            }
            else{
                recordFlag = false;
                if (OnRecordAction != null){ 
                    OnRecordAction("Stop"); //send the event to stop recording
                    statusText.text = "RECORD READY";
                    statusText.color = Color.white;
                }
            }
        }
    }
}
