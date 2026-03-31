using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

namespace SSPot
{
    public class PlayerSensor : MonoBehaviour
    {
        [Flags]
        private enum TriggerCall
        {
            OnEnter = 1 << 0,
            OnStay = 1 << 1,
            OnExit = 1 << 2
        }

        private enum Retrigger
        {
            OnlyOnce,
            EveryNewTrigger,
            Repeatedly
        }
        
        [Header("Player interaction trigger")] [EnumFlags]
        [SerializeField] private TriggerCall triggerCall;
        [ShowIf("ShowTriggerTimer")] [SerializeField]   
        private float triggerTimer;
        
        [Header("Trigger options")]
        [InfoBox("OnlyOnce - deletes the trigger after the initial call\nEveryNewTrigger - triggers every time after the player leaves and re-enters the area\nRepeatedly - re-triggers after X seconds")]
        [SerializeField] private Retrigger retrigger;
        [ShowIf("retrigger", Retrigger.Repeatedly)] [SerializeField]
        private float retriggerTimer;
        
        [Header("Activation options")]
        [SerializeField] private bool controlDoor;
        [ShowIf("controlDoor")] [SerializeField]
        private GameObject[] doors;

        [SerializeField] private bool playAudioClip;
        [ShowIf("playAudioClip")] [SerializeField]
        private AudioObject[] clips;
        
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            print("player trigger");

            if ((triggerCall & TriggerCall.OnEnter) != 0) Activate();
            if ((triggerCall & TriggerCall.OnStay) != 0) StartCoroutine(Timer(triggerTimer));
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            if ((triggerCall & TriggerCall.OnStay) != 0) StopAllCoroutines();
            if ((triggerCall & TriggerCall.OnExit) != 0) Activate();
            if (retrigger == Retrigger.OnlyOnce) Destroy(gameObject);
        }

        private IEnumerator Timer(float time)
        {
            yield return new WaitForSeconds(time);
            Activate();
        }

        private void Activate()
        {
            print("activated");
            if (controlDoor) foreach (GameObject go in doors) go.GetComponent<Door>().Operate();
            if (playAudioClip) Voice.instance.Speak(clips);

            if (retrigger == Retrigger.Repeatedly) StartCoroutine(Timer(retriggerTimer));
        }
        
        private bool ShowTriggerTimer()
        {
            return (triggerCall & TriggerCall.OnStay) != 0;
        }
        
    }
}
