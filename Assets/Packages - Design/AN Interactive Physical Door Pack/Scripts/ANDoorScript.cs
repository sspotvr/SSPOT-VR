using UnityEngine;

public class ANDoorScript : MonoBehaviour
{
    [Tooltip("If it is false door can't be used")]
    public bool locked;
    [Tooltip("It is true for remote control only")]
    public bool remote;
    [Space]
    [Tooltip("Door can be opened")]
    public bool canOpen = true;
    [Tooltip("Door can be closed")]
    public bool canClose = true;
    [Space]
    [Tooltip("Door locked by red key (use key script to declare any object as key)")]
    public bool redLocked;
    public bool blueLocked;
    [Tooltip("It is used for key script working")]
    private ANHeroInteractive heroInteractive;
    [Space]
    public bool isOpened;
    [Range(0f, 4f)]
    [Tooltip("Speed for door opening, degrees per sec")]
    public float openSpeed = 3f;

    // NearView()
    private float distance;
    float angleView;
    private Vector3 direction;

    // Hinge
    [HideInInspector]
    public Rigidbody rbDoor;
    private HingeJoint hinge;
    private JointLimits hingeLim;
    private float currentLim;

    private void Start()
    {
        rbDoor = GetComponent<Rigidbody>();
        hinge = GetComponent<HingeJoint>();
        heroInteractive = FindAnyObjectByType<ANHeroInteractive>();
    }

    private void Update()
    {
        if ( !remote && Input.GetKeyDown(KeyCode.E) && NearView() )
            Action();
    }

    public void Action() // void to open/close door
    {
        if (!locked)
        {
            // key lock checking
            if (heroInteractive && redLocked && heroInteractive.redKey)
            {
                redLocked = false;
                heroInteractive.redKey = false;
            }
            else if (heroInteractive && blueLocked && heroInteractive.blueKey)
            {
                blueLocked = false;
                heroInteractive.blueKey = false;
            }
            
            // opening/closing
            if (isOpened && canClose && !redLocked && !blueLocked)
            {
                isOpened = false;
            }
            else if (!isOpened && canOpen && !redLocked && !blueLocked)
            {
                isOpened = true;
                rbDoor.AddRelativeTorque(new Vector3(0, 0, 20f)); 
            }
        
        }
    }

    private bool NearView() // it is true if you near interactive object
    {
        distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        direction = transform.position - Camera.main.transform.position;
        angleView = Vector3.Angle(Camera.main.transform.forward, direction);
        return distance < 3f; // angleView < 35f && 
    }

    private void FixedUpdate() // door is physical object
    {
        if (isOpened)
        {
            currentLim = 85f;
        }
        else
        {
            // currentLim = hinge.angle; // door will be closed from current opened angle
            if (currentLim > 1f)
                currentLim -= .5f * openSpeed;
        }

        // using values to door object
        hingeLim.max = currentLim;
        hingeLim.min = -currentLim;
        hinge.limits = hingeLim;
    }
}
