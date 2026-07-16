using UnityEngine;

public class ANPlugScript : MonoBehaviour
{
    [Tooltip("Feature for one using only")]
    public bool oneTime;
    [Tooltip("Plug follow this local EmptyObject")]
    public Transform heroHandsPosition;
    [Tooltip("SocketObject with collider(sphere, box etc.) (is trigger = true)")]
    public Collider socket; // need Trigger
    public ANDoorScript doorObject;

    // NearView()
    private float distance;
    private float angleView;
    private Vector3 direction;

    private bool follow, isConnected, followFlag, youCan = true;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Update()
    {
        if (youCan) Interaction();

        // frozen if it is connected to PowerOut
        if (isConnected)
        {
            gameObject.transform.position = socket.transform.position;
            gameObject.transform.rotation = socket.transform.rotation;
            doorObject.isOpened = true;
        }
        else
        {
            doorObject.isOpened = false;
        }
    }

    protected virtual void Interaction()
    {
        if (NearView() && Input.GetKeyDown(KeyCode.E) && !follow)
        {
            isConnected = false; // unfrozen
            follow = true;
            followFlag = false;
        }

        if (follow)
        {
            rb.linearDamping = 10f;
            rb.angularDamping = 10f;
            if (followFlag)
            {
                distance = Vector3.Distance(transform.position, Camera.main.transform.position);
                if (distance > 3f || Input.GetKeyDown(KeyCode.E))
                {
                    follow = false;
                }
            }

            followFlag = true;
            rb.AddExplosionForce(-1000f, heroHandsPosition.position, 10f);
            // second variant of following
            //gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, objectLerp.position, 1f);
        }
        else
        {
            rb.linearDamping = 0f;
            rb.angularDamping = .5f;
        }
    }

    bool NearView() // it is true if you near interactive object
    {
        distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        direction = transform.position - Camera.main.transform.position;
        angleView = Vector3.Angle(Camera.main.transform.forward, direction);
        return distance < 3f && angleView <35f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == socket)
        {
            isConnected = true;
            follow = false;
            doorObject.rbDoor.AddRelativeTorque(new Vector3(0, 0, 20f));
        }
        if (oneTime) youCan = false;
    }
}
