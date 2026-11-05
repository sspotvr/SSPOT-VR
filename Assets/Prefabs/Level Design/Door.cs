using UnityEngine;
using System.Collections;

namespace SSPot
{
    public class Door : MonoBehaviour
    {
        private bool isClosed = true;
        private bool canOperate = true;

        [SerializeField] private AudioClip sound;
        [SerializeField] private Transform doorRight;
        [SerializeField] private Transform doorLeft;

        [SerializeField] private float openDistance = 6f;

        private Vector3 rightClosedPosition;
        private Vector3 leftClosedPosition;


#if UNITY_EDITOR
        // Debug shortcut (editor only): operates every door in the scene at once, which would break
        // the level's sequence if it shipped in a build.
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                Operate();
        }
#endif

        public void Operate()
        {
            if (doorRight == null || doorLeft == null)
            {
                Debug.LogError("Assign Door Right and Door Left in the Inspector.", this);
                return;
            }

            StartCoroutine(MoveDoors());
        }

        public void Operate(float time)
        {
            StartCoroutine(Delay(time));
        }

        private IEnumerator Delay(float time)
        {
            yield return new WaitForSeconds(time);
            Operate();
        }

        private void Awake()
        {
            // Same leaves the door used before they became serialized fields: first two children.
            if (doorRight == null && transform.childCount > 0) doorRight = transform.GetChild(0);
            if (doorLeft == null && transform.childCount > 1) doorLeft = transform.GetChild(1);

            if ((doorRight && !doorRight.IsChildOf(transform)) || (doorLeft && !doorLeft.IsChildOf(transform)))
                Debug.LogWarning($"Door '{name}' moves leaves that belong to another object.", this);

            if (doorRight != null)
                rightClosedPosition = doorRight.localPosition;

            if (doorLeft != null)
                leftClosedPosition = doorLeft.localPosition;
        }

        private IEnumerator MoveDoors()
        {
            if (!canOperate)
                yield break;

            canOperate = false;

            Debug.Log("Door is operating. Current state: " + (isClosed ? "Closed" : "Open"));

            Vector3 rightTarget = isClosed
                ? rightClosedPosition + Vector3.left * openDistance
                : rightClosedPosition;

            Vector3 leftTarget = isClosed
                ? leftClosedPosition + Vector3.right * openDistance
                : leftClosedPosition;

            Vector3 rightStart = doorRight.localPosition;
            Vector3 leftStart = doorLeft.localPosition;

            float elapsed = 0f;
            const float duration = 1f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);

                doorRight.localPosition =
                    Vector3.Lerp(rightStart, rightTarget, progress);

                doorLeft.localPosition =
                    Vector3.Lerp(leftStart, leftTarget, progress);

                yield return null;
            }

            doorRight.localPosition = rightTarget;
            doorLeft.localPosition = leftTarget;

            Debug.Log("Door operation completed. New state: " + (isClosed ? "Open" : "Closed"));

            isClosed = !isClosed;
            canOperate = true;
        }
    }
}