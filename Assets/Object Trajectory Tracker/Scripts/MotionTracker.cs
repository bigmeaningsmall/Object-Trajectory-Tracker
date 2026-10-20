using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Labs.SuperScience;
using MotionEnums;

/// <summary>
/// Created by n.mcshane 2022 - niallsirocco@gmail.com
/// This plugin can be attached to any object to record motion trajectory data in realtime
/// </summary>

public class MotionTracker : MonoBehaviour
{
    private PhysicsTracker physicsTracker;
    private DataWriter dataWriter;

    public bool recordEnabled; //set in inspector 
    [Space(10)] public bool incrementFileNumber = true;
    
    [Space(10)]
    public Transform motionObject;

    [Space(10)]
    public MotionTag motionTag;
    public string targetTag = "Tracking Target Here";
    public string fileName = "Name Here";
    public int fileNumber = 1;
    public string dataID = "ID Here";

    // public string id;

    private bool recordData = false;

    [Header("PHYSICS MOTION TRACKING")]
    public MotionDataFormat motionData;
    
    private Vector3 position;
    private Vector3 rotation;
    private float elapsedTime;
    private string timeStamp;

    public Vector3 velocity;
    
    public Vector3 previousPos;
    private float p_speed;
    private Vector3 p_velocity;
    private float p_velocityMagnitude;
    private Vector3 p_acceleration;

    //public float accelerationStrength;

    private float p_accelerationStrength;
    private Vector3 p_direction;

    private float p_angularSpeed;
    private Vector3 p_angularVelocity;
    private Vector3 p_angularAcceleration;
    private float p_angularAccelerationStrength;
    private Vector3 p_angularAxis;
    
    [Space(8)]
    [Range(0.01f, 5f)] 
    public float motionThreshold = 0.08f; //the minimum movement threshold to flag the object in motion
    public bool inMotion;

    #region Event Subscriptions

    private void OnEnable(){
        MotionTrackerControl.OnRecordAction += MotionTrackerControlOnRecordAction;
    }
    private void OnDisable(){
        MotionTrackerControl.OnRecordAction -= MotionTrackerControlOnRecordAction;
    }
    private void MotionTrackerControlOnRecordAction(string recordAction){
        if (recordAction == "Start")
        {
            RecordStart( fileNumber.ToString());
            
            if (incrementFileNumber)
            {
                fileNumber++;
            }
        }

        if (recordAction == "Stop")
        {
            RecordStop(fileNumber.ToString());
            
        }
    }

    #endregion

    #region Initialise

    void Awake(){
        if (motionObject == null){
            motionObject = this.transform;
        }
        dataWriter = new DataWriter();
    }

    void Start(){
        physicsTracker = gameObject.GetComponent<PhysicsData>().m_MotionData;
    }

    private string GenerateDataID(string id){
        string prefix = motionTag == MotionTag.Null ? targetTag : motionTag.ToString();

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            parts.Add(prefix);
        }
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            parts.Add(fileName); 
        }

        if (!string.IsNullOrWhiteSpace(id))
        {
            parts.Add(id);
        }
            

        return string.Join("_", parts);
    }

    #endregion

    #region Control Functions

    public void RecordStart(string id){
        if (!recordEnabled) return;

        dataWriter = new DataWriter();
        dataID = GenerateDataID(id);
        elapsedTime = 0;
        recordData = true;
        Debug.Log("---- Start Trajectory Motion Capture : " + dataID);
    }

    public void RecordStop(string id){
        if (!recordEnabled || !recordData) return;

        recordData = false;
        dataWriter.WriteData(dataID);
        Debug.Log("---- Stop Trajectory Motion Capture : " + dataID);
    }

    // Call this to start (true) or stop (false) recording
    private void SetMotionTrackingRecord(bool start, string id){
        if (start) RecordStart(id);
        else       RecordStop(id);
    }

    #endregion

    #region Update Loops

    private void Update(){
        //POSITION / ROTATION
        GetPositionRotation();

        //PHYSICS MOTION
        GetPhysicsMotion();

        //velocity requires the previous frame position from LateUpdate
        velocity = (motionObject.position - previousPos) / Time.deltaTime;

        //format the data for writing
        FormatMotionData();

        inMotion = CheckMotionThreshold();
    }

    void LateUpdate(){
        previousPos = motionObject.position;

        if (MotionEventManager.OnMotionEvent !=null){
            MotionEventManager.OnMotionEvent(motionTag, motionData);
        }
        
    }

    // Fixed update time step should be set in unity preferences
    //note : steam VR will override the timestep
    void FixedUpdate(){
        if (recordData && recordEnabled){
            elapsedTime += Time.deltaTime;
            TimeSpan t = TimeSpan.FromSeconds(elapsedTime);

            timeStamp = string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D3}",
                t.Hours,
                t.Minutes,
                t.Seconds,
                t.Milliseconds);

            dataWriter.WriteTrajectoryData(timeStamp, elapsedTime.ToString("f2"), motionTag.ToString(), targetTag,
                inMotion.ToString(), motionThreshold, position, rotation,
                p_speed, p_velocity, velocity, p_velocityMagnitude, p_acceleration, p_accelerationStrength, p_direction,
                p_angularSpeed, p_angularVelocity, p_angularAcceleration, p_angularAccelerationStrength, p_angularAxis);
        }
    }

    #endregion

    #region Tracking Functions

    private bool CheckMotionThreshold(){
        bool motion;
        if (motionData.speed > motionThreshold){
            motion = true;
        }
        else{
            motion = false;
        }

        return motion;
    }
    public void GetPositionRotation(){
        position = motionObject.transform.position;
        rotation = motionObject.transform.eulerAngles;
        Vector3 rot = motionObject.transform.eulerAngles;;
        EulerLimitExtension eulerLimitExtension = new EulerLimitExtension();
        rotation = eulerLimitExtension.NormaliseAngle(rot);
    }

    public void GetPhysicsMotion(){
        p_speed = physicsTracker.Speed;
        p_velocity = physicsTracker.Velocity;
        p_velocityMagnitude = p_velocity.magnitude;
        p_acceleration = physicsTracker.Acceleration;
        p_accelerationStrength = physicsTracker.AccelerationStrength;
        p_direction = physicsTracker.Direction;

        p_angularSpeed = physicsTracker.AngularSpeed;
        p_angularVelocity = physicsTracker.AngularVelocity;
        p_angularAcceleration = physicsTracker.AngularAcceleration;
        p_angularAccelerationStrength = physicsTracker.AngularAccelerationStrength;
        p_angularAxis = physicsTracker.AngularAxis;
    }
    private void FormatMotionData()
    {
        motionData.tag = ((Component)this).tag;
        motionData.position = position;
        motionData.rotation = rotation;

        motionData.speed = p_speed;
        motionData.velocity = p_velocity;
        motionData.velocityMagnitude = p_velocityMagnitude;
        motionData.acceleration = p_acceleration;
        motionData.accelerationStrength = p_accelerationStrength;
        motionData.direction = p_direction;

        motionData.angularSpeed = p_angularSpeed;
        motionData.angularVelocity = p_angularVelocity;
        motionData.angularAcceleration = p_angularAcceleration;
        motionData.angularAccelerationStrength = p_angularAccelerationStrength;
        motionData.angularAxis = p_angularAxis;
    }

    #endregion


    
 
}
