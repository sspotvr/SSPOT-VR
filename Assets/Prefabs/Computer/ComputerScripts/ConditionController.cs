using System;
using NaughtyAttributes;
using Photon.Pun;
using SSPot.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace SSpot.Ambient.ComputerCode
{
    /// <summary>
    /// Drives the "Se obstáculo" (If) block. Occupies its own slot (like a movement/Begin/End cube -
    /// AttachingCube keeps them mutually exclusive), with Range covering the "then" body of cells
    /// immediately after this one. Whether the "then" branch runs is resolved at runtime by CubeRunner,
    /// not baked in by CubeCompiler. A cell can also HasLoop at the same time - the loop then wraps this
    /// If directly with no action cube of its own, and SyncLoopRange keeps the loop's Range matching this
    /// If's total span (and its attached Senão's, if any) automatically.
    ///
    /// "Senão" is a separate block (<see cref="ElseController"/>) that can only be placed on the cell
    /// immediately after this If's then-body - see AttachedSenao.
    /// </summary>
    public class ConditionController : MonoBehaviourPun
    {
        private const int MinRange = 1;

        [Serializable]
        public class ConditionSettings
        {
            [AllowNesting, MinValue(MinRange)]
            public int maxThenRange = 3;
        }

        public CodingCell ParentCell { get; set; }

        /// <summary>
        /// The Senão block attached immediately after this If's then-body, if any. Set by AttachingCube
        /// when a Senão cube is placed there. While set, this If's own Range is locked (see the Range
        /// setter) so the then-body can never move out from under it.
        /// </summary>
        public ElseController AttachedSenao { get; set; }

        [field: BoxGroup("Current Values"), SerializeField, ReadOnly]
        private int range = MinRange;

        [BoxGroup("Condition Settings"), SerializeField]
        private bool overrideGlobalSettings;
        [BoxGroup("Condition Settings"), SerializeField, ShowIf(nameof(overrideGlobalSettings))]
        private ConditionSettings settings;

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

        private ConditionSettings Settings => overrideGlobalSettings
            ? settings
            : ParentCell.Computer.GlobalConditionSettings;

        [PunRPC]
        private void SetRangeRPC(int value) => Range = value;
        public int Range
        {
            get => range;

            private set
            {
                // No room at all for a "then" body (e.g. this If ended up on the last cell, or right
                // before another block with nothing in between) - same as dropping Range below MinRange,
                // there's nothing valid to attach here, so retract entirely rather than storing a
                // Mathf.Clamp(value, MinRange, 0) result of 0 (min > max always returns max).
                if (value < MinRange || cachedMaxThenRange < MinRange)
                {
                    range = MinRange;
                    gameObject.SetActive(false);
                    return;
                }

                range = Mathf.Clamp(value, MinRange, cachedMaxThenRange);

                // While a Senão is attached right after this If, its position depends on Range never
                // moving - hide the manual controls instead of letting them invalidate the attachment.
                bool manualControl = AttachedSenao == null;
                if (increaseAmountButton) increaseAmountButton.SetActive(manualControl && range < cachedMaxThenRange);
                if (decreaseAmountButton) decreaseAmountButton.SetActive(manualControl);

                UpdatePanelScale();
                SyncLoopRange();
            }
        }

        /// <summary>
        /// If this cell also HasLoop, the loop wraps this If directly with no action cube of its own -
        /// its Range must always equal this If's total raw-cell span (header + then body, plus the
        /// attached Senão's own header + else body, if any), so the player's own range buttons are
        /// effectively the loop's size controls too. No-op when there's no co-located loop.
        /// </summary>
        public void SyncLoopRange()
        {
            if (ParentCell == null || !ParentCell.HasLoop) return;

            int total = 1 + Range;
            if (AttachedSenao != null) total += 1 + AttachedSenao.Range;
            ParentCell.LoopController.SyncRangeTo(total);
        }

        private int cachedMaxThenRange = int.MaxValue;

        private void OnEnable() => RefreshEarlierPanels();

        private void OnDisable()
        {
            RefreshEarlierPanels();
            ResetRpc();
        }

        #region Increase/Decrease

        [Button]
        public void IncreaseRange()
        {
            if (AttachedSenao != null) return; // locked while a Senão is attached
            photonView.RPC(nameof(SetRangeRPC), RpcTarget.AllBuffered, Range + 1);
        }

        [Button]
        public void DecreaseRange()
        {
            if (AttachedSenao != null) return; // locked while a Senão is attached
            photonView.RPC(nameof(SetRangeRPC), RpcTarget.AllBuffered, Range - 1);
        }

        #endregion

        #region Refreshing

        private void UpdatePanelScale()
        {
            if (!plane) return;


            int pos = Range + 1;
            Vector3 position = plane.transform.localPosition;
            position.y = -(pos - 1) * (.5f + planeGrowthOffset * .5f);
            plane.transform.localPosition = position;

            Vector3 scale = plane.transform.localScale;
            scale.z = pos * planeSize + (pos - 1) * planeSize * planeGrowthOffset;
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
                if (cell.ElseController) cell.ElseController.RefreshLimits();
            }
        }

        public void RefreshLimits()
        {
            if (!ParentCell) return;

            int index = ParentCell.Index;
            int panelCount = ParentCell.Computer.Cells.Count;

            int nextBlockIndex = ParentCell.Computer.Cells.FindIndex(index + 1,
                cell => cell.HasLoop || cell.HasCondition || cell.HasSenao);
            if (nextBlockIndex == -1) nextBlockIndex = panelCount;

            // The "then" body no longer includes this header cell itself (the If IS the header, it
            // doesn't also need a coexisting action cube), so it only has room in [index+1, nextBlockIndex).
            cachedMaxThenRange = Mathf.Min(nextBlockIndex - (index + 1), Settings.maxThenRange);
            Range = range;
        }

        #endregion

        public void ResetConditionData() => photonView.RPC(nameof(ResetRpc), RpcTarget.AllBuffered);

        [PunRPC]
        private void ResetRpc()
        {
            if (!ParentCell) return;

            // An orphaned Senão makes no sense - remove it along with this If.
            if (AttachedSenao != null) AttachedSenao.ResetConditionData();

            Range = MinRange;
            // If a loop was wrapping this If, its content is gone now - the loop reverts to being just
            // its own header, independently controllable again (see LoopController.Range's manualControl).
            if (ParentCell.HasLoop) ParentCell.LoopController.SyncRangeTo(1);
            // Range = MinRange only resets the size - a condition at its minimum range is still "attached".
            // Reset should remove it entirely, same as dragging Range below the minimum would.
            gameObject.SetActive(false);
        }
    }
}
