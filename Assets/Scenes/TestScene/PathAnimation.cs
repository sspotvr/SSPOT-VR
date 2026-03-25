using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SSPot
{
    public class PathAnimation : MonoBehaviour
    {
        public List<Transform> pathUnits;

        Sequence animationSequence;

        public float duration = 0.2f;
        public float offset = 0;
        public bool startMode = false;
        public bool infiniteLoop = false;

        [Button]
        public void Animate()
        {
            animationSequence = DOTween.Sequence();

            int n = 0;
            foreach (Transform t in pathUnits)
            {
                animationSequence.Insert(2*n*duration*offset,
                    t.DOMoveY(t.position.y + 1f, duration)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetEase(Ease.InOutQuad)
                    );
                n++;
            }
        }

        private void Start()
        {
            if (startMode)
            {
                Animate();
                animationSequence.SetLoops(infiniteLoop ? -1 : 0).Play();
            }
        }
    }
}
