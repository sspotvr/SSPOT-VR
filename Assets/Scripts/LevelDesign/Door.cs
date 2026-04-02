using System;
using UnityEngine;
using System.Collections;

namespace SSPot
{
    public class Door : MonoBehaviour
    {
        private bool isClosed = true;
        private bool canOperate = true;
        
        private Transform doorRight;
        private Vector3 doorRightClosed = new (-3f, 0, 1f);
        private Vector3 doorRightOpened = new (-12f, 0, 2f);
        
        private Transform doorLeft;
        private Vector3 doorLeftClosed = new (6f, 0, 0f);
        private Vector3 doorLeftOpened = new (12f, 0, 0);
        private void Start()
        {
            doorRight = transform.GetChild(0);
            doorLeft = transform.GetChild(1);
        }

        private void Update()
        {
            //if (Input.GetKeyDown("space"))
            //{
            //    Operate();
            //}
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
                    doorRight.localPosition -= new Vector3 (6 * Time.deltaTime, 0, 0); // go left
                    doorLeft.localPosition += new Vector3 (6 * Time.deltaTime, 0, 0); // go right
                    yield return null;
                }

                doorRight.localPosition = doorRightOpened;
                doorLeft.localPosition = doorLeftOpened;
            }
            else
            {
                for (float time = 1f; time > 0f; time -= Time.deltaTime)
                {
                    doorRight.localPosition += new Vector3 (6 * Time.deltaTime, 0, 0); // go right
                    doorLeft.localPosition -= new Vector3 (6 * Time.deltaTime, 0, 0); // go left
                    yield return null;
                }
                
                doorRight.localPosition = doorRightClosed;
                doorLeft.localPosition = doorLeftClosed;
            }
            
            isClosed = !isClosed;
            canOperate = true;
        }
    }
}
