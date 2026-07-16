using UnityEngine;

public class ANButton : MonoBehaviour
{
    private static readonly int LeverUp = Animator.StringToHash("LeverUp");
    private static readonly int ButtonPress = Animator.StringToHash("ButtonPress");

    [Tooltip("True for rotation like valve (used for ramp/elevator only)")]
    public bool isValve;
    [Tooltip("SelfRotation speed of valve")]
    public float valveSpeed = 10f;
    [Tooltip("If it isn't valve, it can be lever or button (animated)")]
    public bool isLever;
    [Tooltip("If it is false door can't be used")]
    public bool locked;
    [Tooltip("The door for remote control")]
    public ANDoorScript doorObject;
    [Space]
    [Tooltip("Any object for ramp/elevator behaviour")]
    public Transform rampObject;
    [Tooltip("Door can be opened")]
    public bool canOpen = true;
    [Tooltip("Door can be closed")]
    public bool canClose = true;
    [Tooltip("Current status of the door")]
    public bool isOpened;
    [Space]
    [Tooltip("True for rotation by X local rotation by valve")]
    public bool xRotation = true;
    [Tooltip("True for vertical movement by valve (if xRotation is false)")]
    public bool yPosition;
    public float max = 90f, min, speed = 5f;
    private bool valveBool = true;
    private float current, startYPosition;
    private Quaternion startQuat, rampQuat;

    private Animator anim;

    // NearView()
    private float distance;
    private float angleView;
    private Vector3 direction;

    private void Start()
    {
        anim = GetComponent<Animator>();
        startYPosition = rampObject.position.y;
        startQuat = transform.rotation;
        rampQuat = rampObject.rotation;
    }

    private void Update()
    {
        if (!locked)
        {
            if (Input.GetKeyDown(KeyCode.E) && !isValve && doorObject != null && doorObject.remote && NearView()) // 1.lever and 2.button
            {
                doorObject.Action(); // void in door script to open/close
                if (isLever) // animations
                {
                    anim.SetBool(LeverUp, doorObject.isOpened);
                }
                else anim.SetTrigger(ButtonPress);
            }
            else if (isValve && rampObject) // 3.valve
            {
                // changing value in script
                if (Input.GetKey(KeyCode.E) && NearView())
                {
                    if (valveBool)
                    {
                        if (!isOpened && canOpen && current < max) current += speed * Time.deltaTime;
                        if (isOpened && canClose && current > min) current -= speed * Time.deltaTime;

                        if (current >= max)
                        {
                            isOpened = true;
                            valveBool = false;
                        }
                        else if (current <= min)
                        {
                            isOpened = false;
                            valveBool = false;
                        }
                    }

                }
                else
                {
                    if (!isOpened && current > min) current -= speed * Time.deltaTime;
                    if (isOpened && current < max) current += speed * Time.deltaTime;
                    valveBool = true;
                }

                // using value on object
                transform.rotation = startQuat * Quaternion.Euler(0f, 0f, current * valveSpeed);
                if (xRotation) rampObject.rotation = rampQuat * Quaternion.Euler(current, 0f, 0f); // I have a doubt in working correctly
                else if (yPosition) rampObject.position = new Vector3(rampObject.position.x, startYPosition + current, rampObject.position.z);
            }
        }
    }

    bool NearView() // it is true if you near interactive object
    {
        distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        direction = transform.position - Camera.main.transform.position;
        angleView = Vector3.Angle(Camera.main.transform.forward, direction);
        if (angleView < 45f && distance < 2f) return true;
        else return false;
    }
}
