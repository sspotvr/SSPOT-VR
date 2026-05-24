using UnityEngine;

namespace SSPot
{
    public enum Movement
    {
        Up,
        Stopped,
        Down
    } 

    public class VerticalMovementPlayer : MonoBehaviour
    {
        // Movement
        public float finalYdown;
        public float finalYup;        // Final Y value
        public float speed = 1f;    // Speed

        public Movement movement = Movement.Stopped;


        void GoingDown()
        {
            if(transform.position.y >= finalYdown)
                transform.Translate(0f, -speed * Time.deltaTime, 0f, Space.World);
            else{
                movement = Movement.Stopped;
                transform.position = new Vector3(transform.position.x, finalYdown, transform.position.z);
            }
        }

        void GoingUp()
        {
            if(transform.position.y <= finalYup)
            transform.Translate(0f, speed * Time.deltaTime, 0f, Space.World);
            else{
                movement = Movement.Stopped;
                transform.position = new Vector3(transform.position.x, finalYup, transform.position.z);
             }
        }

        // Update is called once per frame
        void Update()
        {
            if(movement == Movement.Up) GoingUp();
            if(movement == Movement.Down) GoingDown();
        }
    }
}
