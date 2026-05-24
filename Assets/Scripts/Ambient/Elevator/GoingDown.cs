using System;
using UnityEngine;

public class GoingDown : MonoBehaviour
{
    // Movement
    public float finalY;        // Final Y value
    public float speed = 1f;    // Speed

    public event Action OnReachedDestination;


    // Update is called once per frame
    void Update()
    {
        // If current Y value is bigger or equal the final Y value, keep going down
        if(transform.position.y >= finalY)
            transform.Translate(0f, -speed * Time.deltaTime, 0f, Space.World);
        // Else, stop movement and disable this
        else{
            print("Desci!");
            OnReachedDestination?.Invoke();
            this.enabled = false;
        }
    }
}
