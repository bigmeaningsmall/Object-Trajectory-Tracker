using System;
using MotionEnums;
using UnityEngine;

public class MotionEventManager : MonoBehaviour
{
    public delegate void MotionEventHandler(MotionTag motionTag, MotionDataFormat motionData);
    public static  MotionEventHandler OnMotionEvent;
    
}
