using UnityEngine;

public class ANDoorKey : MonoBehaviour
{
    [Tooltip("True - red key object, false - blue key")]
    public bool isRedKey = true;
    private ANHeroInteractive hero;

    // NearView()
    private float distance;
    private float angleView;
    private Vector3 direction;

    private void Start()
    {
        hero = FindAnyObjectByType<ANHeroInteractive>(); // key will get up and it will be saved in "inventory"
    }

    private void Update()
    {
        if ( NearView() && Input.GetKeyDown(KeyCode.E) )
        {
            if (isRedKey) hero.redKey = true;
            else hero.blueKey = true;
            Destroy(gameObject);
        }
    }

    private bool NearView() // it is true if you near interactive object
    {
        distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        direction = transform.position - Camera.main.transform.position;
        angleView = Vector3.Angle(Camera.main.transform.forward, direction);
        if (distance < 2f) return true; // angleView < 35f && 
        else return false;
    }
}
