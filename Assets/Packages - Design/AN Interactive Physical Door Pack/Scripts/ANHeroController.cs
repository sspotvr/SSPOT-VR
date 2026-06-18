using UnityEngine;

public class ANHeroController : MonoBehaviour
{
    [Tooltip("Character settings (rigid body)")]
    public float moveSpeed = 30f, jumpForce = 200f, sensitivity = 70f;
    private bool jumpFlag = true; // to jump from surface only

    private CharacterController character;
    private Rigidbody rb;
    private Vector3 moveVector;

    private Transform cam;
    private float yRotation;

    protected void Start()
    {
        character = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.GetComponent<Transform>();

        Cursor.lockState = CursorLockMode.Locked; // freeze cursor on screen center
        Cursor.visible = false; // invisible cursor
    }

    protected virtual void Update()
    {
        // camera rotation
        float xMouse = Input.GetAxis("Mouse X") * Time.deltaTime * sensitivity;
        float yMouse = Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity;
        transform.Rotate(Vector3.up * xMouse);
        yRotation -= yMouse;
        yRotation = Mathf.Clamp(yRotation, -85f, 60f);
        cam.localRotation = Quaternion.Euler(yRotation, 0, 0);

        if (Input.GetButtonDown("Jump") && jumpFlag) rb.AddForce(transform.up * jumpForce);
    }

    void FixedUpdate()
    {
        // body moving
        moveVector = transform.forward * (moveSpeed * Input.GetAxis("Vertical")) +
            transform.right * (moveSpeed * Input.GetAxis("Horizontal")) +
            transform.up * rb.linearVelocity.y;
        rb.linearVelocity = moveVector;
    }
    
    private void OnTriggerStay(Collider other)
    {
        jumpFlag = true; // hero can jump
    }

    private void OnTriggerExit(Collider other)
    {
        jumpFlag = false;
    }

}
