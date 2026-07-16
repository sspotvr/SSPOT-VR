using System.Collections;
using UnityEngine;

namespace SSPot
{
    public class MarkPod : MonoBehaviour
    {
        private bool isClosed = true;
        private bool canOperate = true;

        [SerializeField] private Transform rotatingDoor;
        [SerializeField] private Transform mark;
        
        // private void Start()
        // {
        //     rotatingDoor = transform.GetChild(0);
        // }

        private void Update()
        {
            if (Input.GetKeyDown("space")) Operate();
            if (Input.GetKeyDown("s")) StartCoroutine(MarkSpin());
        }

        public void Operate()
        {
            StartCoroutine(RotateDoor());
        }
        
        public void Operate(float time)
        {
            StartCoroutine(Delay(time));
        }
        private IEnumerator Delay(float time)
        {
            yield return new WaitForSeconds(time);
            StartCoroutine(RotateDoor());
        }
        
        private IEnumerator RotateDoor()
        {
            if (!canOperate) yield break;
            
            canOperate = false;
            
            if (isClosed)
            {
                for (float time = 1f; time > 0f; time -= Time.deltaTime)
                {
                    rotatingDoor.localEulerAngles -= new Vector3(0, 180f * Time.deltaTime, 0);
                    yield return null;
                }
                rotatingDoor.localEulerAngles = new Vector3(0, 0, 0);
            }
            else
            {
                for (float time = 1f; time > 0f; time -= Time.deltaTime)
                {
                    rotatingDoor.localEulerAngles += new Vector3(0, 180f * Time.deltaTime, 0);
                    yield return null;
                }
                rotatingDoor.localEulerAngles = new Vector3(0, 180f, 0);
            }
            
            isClosed = !isClosed;
            canOperate = true;
        }

        private IEnumerator MarkSpin()
        {
            for (float time = 1f; time > 0f; time -= Time.deltaTime)
            {
                mark.localEulerAngles += new Vector3(0, 360f * Time.deltaTime, 0);
                yield return null;
            }
            mark.localEulerAngles = new Vector3(0, 180f, 0);
        }
    }
}
