using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
public class KickDirectionManager : MonoBehaviour
{
    public GameObject directionIndicator;

    
    [SerializeField]
    private ARSessionOrigin arSessionOrigin;
    [SerializeField]
    private ARRaycastManager arRaycastManager;

    public TextMeshProUGUI text;
    public Vector3 aimPose;
    Vector3 defaultPosition;
    public Transform ball;
    public LineRenderer laser;






    // Start is called before the first frame update
    void Start()
    {
        defaultPosition = ball.position;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAim();
        UpdateDirection();
//        text.text = aimPose.ToString();
    }

    private void UpdateDirection()
    {
        directionIndicator.SetActive(true);
        //  directionIndicator.transform.position = aimPose.position - new Vector3(0,0,0.5f);
        //directionIndicator.transform.SetPositionAndRotation(aimPose.position, aimPose.rotation);
        //directionIndicator.transform.position = aimPose.position;
        //directionIndicator.transform.rotation = aimPose.rotation;

    }


    private void UpdateAim()
    {
        var screenCenter = arSessionOrigin.camera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(arSessionOrigin.camera.transform.position, arSessionOrigin.camera.transform.forward, out hit, Mathf.Infinity))
        {
            laser.SetPosition(0, defaultPosition);
            laser.SetPosition(1, hit.point);
            aimPose = hit.point;
        }

    }





}
