using NaughtyAttributes;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace SSpot.Grids
{
    public class GridObject : MonoBehaviourPun, ILevelGridObject
    {
        [field: SerializeField] 
        public bool CanWalkThrough { get; private set; } = true;
        
        [field: SerializeField, BoxGroup("Events")]
        public UnityEvent SteppedOnEvent { get; private set; } = new();
        
        public LevelGrid Grid { get; set; }
        
        public Vector2Int GridPosition { get; set; }
        
        //TODO proper handling of Facing
        public Vector2Int Facing
        {
            get
            {
                // Rounded, not truncated: a rotation authored as exactly 180° can come back as e.g.
                // forward.z = -0.9999999, which (int) truncates to 0 - an invalid (0, 0) facing that
                // RobotGridMover then silently replaces with Vector2Int.right.
                Vector3 forward = transform.forward;
                return new(Mathf.RoundToInt(forward.x), Mathf.RoundToInt(forward.z));
            }
        }

        public virtual void OnSteppedOn()
        {
            SteppedOnEvent.Invoke();
        }
    }
}