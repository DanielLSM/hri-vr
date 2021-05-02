using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GrabSlider : MonoBehaviour
{
    // Grip trigger thresholds for picking up objects.
    public float slideBegin = 0.55f;
    public float slideEnd = 0.35f;

    [SerializeField]
    protected OVRInput.Controller controller;

    // Child/attached transforms of the grabber.
    [SerializeField]
    protected Transform gripTransform = null;

    // Child/attached Colliders to detect candidate slidable objects.
    [SerializeField]
    protected Collider grabVolume = null;

    [SerializeField]
    protected GameObject textObject = null;
    [SerializeField]
    protected Transform centerEyeAnchor = null;

    private GrabSlidable slidingObject = null;

    private GrabSlidable slideCandidate = null; // TODO could make into a list, but for now we shouldn't have more than 2 sliders at once

    private Vector3 startPosition;
    private Vector3 startScale;

    private bool updateByRig = false;

    private int countDebug = 0;

    //Update every frame, called from rig
    void OnUpdatedAnchors()
    {

        UpdateSlider();

        float triggerPressValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller);

        CheckForSlideOrRelease(triggerPressValue);

    }

    private void UpdateSlider()
    {
        if (slidingObject == null)
        {
            return;
        }
        float distance = Vector3.Distance(gripTransform.position, startPosition);

        // Check if hand is closer to the center than the startpoint to determine if we should shrink or scale out object
        float distanceToCenterFromHand = Vector3.Distance(gripTransform.position, slidingObject.transform.position);
        float distanceToCenterFromStartPoint = Vector3.Distance(startPosition, slidingObject.transform.position);
        float sign = distanceToCenterFromHand > distanceToCenterFromStartPoint ? 1 : -1;

        Vector3 diff = new Vector3(startScale.x * distance, 0f, startScale.z * distance);
        
        slidingObject.transform.localScale = startScale + (diff * sign);

        textObject.transform.position = gripTransform.position + gripTransform.up * 0.1f;
        textObject.transform.LookAt(centerEyeAnchor.position);
        textObject.transform.Rotate(new Vector3(0f, 180f, 0f));
        textObject.GetComponentInChildren<TextMeshProUGUI>().text = $"{slidingObject.transform.localScale.x.ToString("F2")}m";
    }

    protected void CheckForSlideOrRelease(float triggerPressValue)
    {
        
        if (triggerPressValue >= slideBegin && slidingObject == null)
        {
            SlideBegin();
        }
        else if (triggerPressValue <= slideEnd && slidingObject)
        {
            SlideEnd();
        }
    }

    private void SlideBegin()
    {
        if (slideCandidate == null)
        {
            return;
        }
        startPosition = gripTransform.position;
        slidingObject = slideCandidate;

        slidingObject.SlideBegin(this, grabVolume);
        startScale = slidingObject.transform.localScale;

        textObject.SetActive(true);
    }

    private void SlideEnd()
    {
        if (slidingObject == null)
        {
            return;
        }

        //TODO add method, or call from slidable, update some RL values
        slidingObject.SlideEnd();
        slidingObject = null;
        startPosition = gripTransform.position;
        startScale = Vector3.zero;

        textObject.SetActive(false);
    }

    void OnTriggerEnter(Collider otherCollider)
    {
        // Get the grab trigger
        GrabSlidable slidable = otherCollider.GetComponent<GrabSlidable>() ?? otherCollider.GetComponentInParent<GrabSlidable>();
        if (slidable == null) return;
        slideCandidate = slidable;
    }

    void OnTriggerExit(Collider otherCollider)
    {
        GrabSlidable slidable = otherCollider.GetComponent<GrabSlidable>() ?? otherCollider.GetComponentInParent<GrabSlidable>();
        if (slidable == null) return;

        slideCandidate = null;
    }


    protected virtual void Awake()
    {
        // If we are being used with an OVRCameraRig, let it drive input updates, which may come from Update or FixedUpdate.
        OVRCameraRig rig = transform.GetComponentInParent<OVRCameraRig>();
        if (rig != null)
        {
            updateByRig = true;
            rig.UpdatedAnchors += (r) => { OnUpdatedAnchors(); };
        }

    }

    private void Update()
    {
        if (!updateByRig)
        {
            OnUpdatedAnchors();
        }
    }
}
