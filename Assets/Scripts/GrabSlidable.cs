using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabSlidable : MonoBehaviour
{
    private GrabSlider grabber;
    private Collider grabberCollider;
    private Transform startPosition;

    private float value = 1f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    /// <summary>
    /// Notifies the object that it has been grabbed.
    /// </summary>
    virtual public void SlideBegin(GrabSlider hand, Collider grabPoint)
    {
        Debug.Log("Start scale: " + value);
        grabber = hand;
        grabberCollider = grabPoint;
    }

    /// <summary>
    /// Notifies the object that it has been released.
    /// </summary>
    virtual public void SlideEnd()
    {
        value = transform.localScale.x;
        Debug.Log("End scale: " + value);
        grabber = null;
        grabberCollider = null;
    }

}
