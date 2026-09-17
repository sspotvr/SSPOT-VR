using System;
using NaughtyAttributes;
using Photon.Pun;
using SSPot.Utilities;
using UnityEngine;
using UnityEngine.UI;

// TODO: comentar a classe e revisar algumas fun��es
namespace SSpot.Ambient.ComputerCode
{
    public class LoopController : MonoBehaviourPun
    {
        private const int MinRange = 1;
        private const int MinIterations = 2;
        
        [Serializable]
        public class LoopSettings
        {
            [AllowNesting, MinValue(MinIterations)]
            public int maxIterations = 10;
            
            [AllowNesting, MinValue(MinRange)]
            public int maxRange = 1;
        }
        
        public CodingCell ParentCell { get; set; }

        [field: BoxGroup("Current Values"), SerializeField, ReadOnly]
        private int iterations = 2, range = 1;
        
        [BoxGroup("Loop Settings"), SerializeField]
        private bool overrideGlobalSettings;
        [BoxGroup("Loop Settings"), SerializeField, ShowIf(nameof(overrideGlobalSettings))]
        private LoopSettings settings;

        [BoxGroup("Visuals"), SerializeField]
        private Text iterationsText;
        [BoxGroup("Visuals"), SerializeField]
        private Text rangeText;
        [BoxGroup("Visuals"), SerializeField]
         private GameObject plane;
        [BoxGroup("Visuals"), SerializeField]
        private float planeSize = 0.9f;
        [BoxGroup("Visuals"), SerializeField]
        private float planeGrowthOffset = 0.05f;
        [BoxGroup("Visuals"), SerializeField]
        private GameObject increaseAmountButton;
        [BoxGroup("Visuals"), SerializeField]
        private GameObject decreaseAmountButton;

        private LoopSettings Settings => overrideGlobalSettings 
            ? settings 
            : ParentCell.Computer.GlobalLoopSettings;

        [PunRPC]
        private void SetIterationsRPC(int value) => Iterations = value;
        public int Iterations
        {
            get => iterations;
            
            private set
            {
                iterations = Mathf.Clamp(value, MinIterations, Settings.maxIterations);
                iterationsText.text = iterations.ToString();
            }
        }

        [PunRPC]
        private void SetRangeRPC(int value) => Range = value;
        public int Range
        {
            get => range;

            private set
            {
                if (value < MinRange)
                {
                    range = MinRange;
                    gameObject.SetActive(false);
                    return;
                }

                range = Mathf.Clamp(value, MinRange, cachedMaxRange);

                // When this cell also has an If, the If's own Range/ElseRange controls the total span
                // (see SyncRangeTo) - the loop's own +/- controls would just fight that, so hide them.
                bool manualControl = ParentCell == null || !ParentCell.HasCondition;
                increaseAmountButton.SetActive(manualControl && range < cachedMaxRange);
                if (decreaseAmountButton) decreaseAmountButton.SetActive(manualControl);

                rangeText.text = range == MinRange ? "x" : "<";
                UpdatePanelScale();
            }
        }

        /// <summary>
        /// Forces Range to match the total span of a co-located If's then/else bodies (1 for the header
        /// itself, which the If occupies, plus its Range and, if HasElse, its ElseRange) - used instead of
        /// the player-facing IncreaseRange/DecreaseRange when this cell also HasCondition, since in that
        /// case the loop wraps the If directly and has no independent size of its own.
        /// </summary>
        public void SyncRangeTo(int total) => photonView.RPC(nameof(SetRangeRPC), RpcTarget.AllBuffered, total);
        
        private int cachedMaxRange;

        private void OnEnable() => RefreshEarlierPanels();

        private void OnDisable() 
        {
            RefreshEarlierPanels();
            ResetRpc();
        }

        #region Increase/Decrease
        
        public void IncreaseIterations() =>
            photonView.RPC(nameof(SetIterationsRPC), RpcTarget.AllBuffered, Iterations + 1);

        public void DecreaseIterations() =>
            photonView.RPC(nameof(SetIterationsRPC), RpcTarget.AllBuffered, Iterations - 1);

        [Button]
        public void IncreaseRange()
        {
            if (ParentCell != null && ParentCell.HasCondition) return; // driven by SyncRangeTo instead
            photonView.RPC(nameof(SetRangeRPC), RpcTarget.AllBuffered, Range + 1);
        }

        [Button]
        public void DecreaseRange()
        {
            if (ParentCell != null && ParentCell.HasCondition) return; // driven by SyncRangeTo instead
            photonView.RPC(nameof(SetRangeRPC), RpcTarget.AllBuffered, Range - 1);
        }
        
        #endregion

        #region Refreshing
        
        private void UpdatePanelScale()
        {
            Vector3 position = plane.transform.localPosition;
            position.y = -(Range - 1) * (.5f + planeGrowthOffset * .5f);
            plane.transform.localPosition = position;

            Vector3 scale = plane.transform.localScale;
            scale.z = Range * planeSize + (Range - 1) * planeSize * planeGrowthOffset;
            plane.transform.localScale = scale;
        }

        private void RefreshEarlierPanels()
        {
            if (!ParentCell) return;

            for (int i = 0; i <= ParentCell.Index; i++)
            {
                var cell = ParentCell.Computer.Cells[i];
                cell.LoopController.RefreshLimits();
                if (cell.ConditionController) cell.ConditionController.RefreshLimits();
            }
        }

        public void RefreshLimits()
        {
            if (!ParentCell) return;

            int index = ParentCell.Index;
            int panelCount = ParentCell.Computer.Cells.Count;
            int nextPanelIndex  = ParentCell.Computer.Cells.FindIndex(index + 1, cell => cell.HasLoop || cell.HasCondition);
            if (nextPanelIndex == -1) nextPanelIndex = panelCount;
            
            cachedMaxRange = Mathf.Min(nextPanelIndex - index, Settings.maxRange);
            Range = Range;
            Iterations = Iterations;
        }
        
        #endregion

        public void ResetLoopData() => photonView.RPC(nameof(ResetRpc), RpcTarget.AllBuffered);
        
        [PunRPC]
        private void ResetRpc()
        {
            if (!ParentCell) return;

            Iterations = MinIterations;
            Range = MinRange;
            // Range = MinRange only resets the size - a loop at its minimum range is still "attached".
            // Reset should remove it entirely, same as dragging Range below the minimum would.
            gameObject.SetActive(false);
        }
    }
}
