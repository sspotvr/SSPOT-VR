using Photon.Pun;
using SSpot.Level;
using UnityEngine;

namespace SSpot.Ambient.ComputerCode
{
    public class CodingCell : MonoBehaviourPun
    {
        public CubeClass CurrentCube => AttachingCube.CurrentCube;
        
        public bool HasLoop => LoopController != null && LoopController.gameObject.activeSelf;

        public bool HasCondition => ConditionController != null && ConditionController.gameObject.activeSelf;

        [field: SerializeField]
        public AttachingCube AttachingCube { get; private set; }

        [field: SerializeField]
        public LoopController LoopController { get; private set; }

        [field: SerializeField]
        public ConditionController ConditionController { get; private set; }

        [field: SerializeField]
        public MeshRenderer Renderer { get; private set; }
        
        public CubeComputer Computer { get; private set; }
        public int Index { get; private set; }
        
        public void Init(int index, CubeComputer computer)
        {
            Index = index;
            Computer = computer;
            
            AttachingCube.ParentCell = this;
            if (LoopController)
            {
                LoopController.ParentCell = this;
                SetLoopRpc(false);
            }

            if (ConditionController)
            {
                ConditionController.ParentCell = this;
                SetConditionRpc(false);
            }
        }

        public void Clear()
        {
            if (CurrentCube != null)
                AttachingCube.ClearCell();

            if (LoopController)
                LoopController.ResetLoopData();

            if (ConditionController)
                ConditionController.ResetConditionData();
        }

        public void SetLoop(bool loopActive)
        {
            if (LoopController == null)
            {
                Debug.LogError($"Null LoopController but called {nameof(SetLoop)}(true)", gameObject);
                return;
            }

            photonView.RPC(nameof(SetLoopRpc), RpcTarget.AllBuffered, loopActive);
        }

        [PunRPC]
        private void SetLoopRpc(bool loopActive)
        {
            LoopController.gameObject.SetActive(loopActive);
        }

        public void SetCondition(bool conditionActive)
        {
            if (ConditionController == null)
            {
                Debug.LogError($"Null ConditionController but called {nameof(SetCondition)}(true)", gameObject);
                return;
            }

            photonView.RPC(nameof(SetConditionRpc), RpcTarget.AllBuffered, conditionActive);
        }

        [PunRPC]
        private void SetConditionRpc(bool conditionActive)
        {
            ConditionController.gameObject.SetActive(conditionActive);
        }
    }
}