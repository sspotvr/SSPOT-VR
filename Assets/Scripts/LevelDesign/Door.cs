using System;
using UnityEngine;
using System.Collections;

namespace SSPot
{
    public class Door : MonoBehaviour
    {
        private bool isClosed = true;
        private bool canOperate = true;
        
        private Transform doorLeft;
        private Vector3 doorLeftClosed = new (0, 0, 1f);
        private Vector3 doorLeftOpened = new (-3f, 0, 1f);
        
        private Transform doorRight;
        private Vector3 doorRightClosed = new (6f, 0, 0);
        private Vector3 doorRightOpened = new (9f, 0, 0);
        private void Start()
        {
            doorRight = transform.GetChild(0);
            doorLeft = transform.GetChild(1);
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
            if (!canOperate) yield break;
            
            canOperate = false;
            
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
            canOperate = true;
        }
    }
}
