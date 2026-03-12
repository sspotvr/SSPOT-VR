using System;
using UnityEngine;
using System.Collections;

namespace SSPot
{
    public class Door : MonoBehaviour
    {
        private bool isClosed = true;
        
        private Transform doorLeft;
        private Vector3 doorLeftClosed = new (-3f, 0, 10.71f);
        private Vector3 doorLeftOpened = new (-6f, 0, 10.71f);
        
        private Transform doorRight;
        private Vector3 doorRightClosed = new (3f, 0, 9.71f);
        private Vector3 doorRightOpened = new (6f, 0, 9.71f);
        private void Start()
        {
            doorLeft = transform.GetChild(2);
            doorRight = transform.GetChild(3);
        }

        private void Update()
        {
            if (Input.GetKeyDown("space"))
            {
                Operate();
            }
        }

        public void Operate()
        {
            StartCoroutine(MoveDoors());
        }
        
        public void Operate(float time)
        {
            StartCoroutine(Delay(time));
        }
        private IEnumerator Delay(float time)
        {
            yield return new WaitForSeconds(time);
            StartCoroutine(MoveDoors());
        }
        
        private IEnumerator MoveDoors()
        {
            if (isClosed)
            {
                for (float time = 1f; time > 0f; time -= Time.deltaTime)
                {
                    doorLeft.localPosition -= new Vector3 (3 * Time.deltaTime, 0, 0); // go left
                    doorRight.localPosition += new Vector3 (3 * Time.deltaTime, 0, 0); // go right
                    yield return null;
                }

                doorLeft.localPosition = doorLeftOpened;
                doorRight.localPosition = doorRightOpened;
            }
            else
            {
                for (float time = 1f; time > 0f; time -= Time.deltaTime)
                {
                    doorLeft.localPosition += new Vector3 (3 * Time.deltaTime, 0, 0); // go right
                    doorRight.localPosition -= new Vector3 (3 * Time.deltaTime, 0, 0); // go left
                    yield return null;
                }
                
                doorLeft.localPosition = doorLeftClosed;
                doorRight.localPosition = doorRightClosed;
            }
            
            isClosed = !isClosed;
        }
    }
}
