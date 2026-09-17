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
    /// immediately after this one, and optionally growing a "senão" (else) body of ElseRange cells after
    /// that. Unlike a loop, whether the "then" or "else" branch runs is resolved at runtime by CubeRunner,
    /// not baked in by CubeCompiler. A cell can also HasLoop at the same time - the loop then wraps this
    /// If directly with no action cube of its own, and SyncLoopRange keeps the loop's Range matching this
    /// If's total span automatically.
    /// </summary>
    public class ConditionController : MonoBehaviourPun
    {
        private const int MinRange = 1;

        [Serializable]
        public class ConditionSettings
        {
            [AllowNesting, MinValue(MinRange)]
            public int maxThenRange = 3;

            [AllowNesting, MinValue(MinRange)]
            public int maxElseRange = 3;
        }

        public CodingCell ParentCell { get; set; }

        [field: BoxGroup("Current Values"), SerializeField, ReadOnly]
        private int range = MinRange, elseRange;

        [field: BoxGroup("Current Values"), SerializeField, ReadOnly]
        private bool hasElse;

        [BoxGroup("Condition Settings"), SerializeField]
        private bool overrideGlobalSettings;
        [BoxGroup("Condition Settings"), SerializeField, ShowIf(nameof(overrideGlobalSettings))]
        private ConditionSettings settings;

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

        [BoxGroup("Visuals - Senão"), SerializeField]
        private GameObject addElseButton;
        [BoxGroup("Visuals - Senão"), SerializeField]
        private GameObject elseLabel;
        [BoxGroup("Visuals - Senão"), SerializeField]
        private GameObject elsePlane;
        [BoxGroup("Visuals - Senão"), SerializeField]
        private Text elseRangeText;
        [BoxGroup("Visuals - Senão"), SerializeField]
        private GameObject increaseElseAmountButton;
        [BoxGroup("Visuals - Senão"), SerializeField]
        private GameObject decreaseElseAmountButton;

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
                if (increaseAmountButton) increaseAmountButton.SetActive(range < cachedMaxThenRange);
                if (rangeText) rangeText.text = range.ToString();
                UpdatePanelScale();
                UpdateElseRowPosition();

                // The else body starts right after the then body, so its bounds move with Range.
                // (Not RefreshLimits() — that would reassign Range and recurse back into this setter.)
                RecomputeElseBounds();
            }
        }

        [PunRPC]
        private void SetHasElseRPC(bool value) => HasElse = value;
        public bool HasElse
        {
            get => hasElse;

            private set
            {
                hasElse = value;
                // The button only adds a senão - once there is one, hide it (DecreaseElseRange below
                // MinRange is what removes it and brings the button back, mirroring how Range works).
                if (addElseButton) addElseButton.SetActive(!hasElse);
                if (elseLabel) elseLabel.SetActive(hasElse);
                if (elsePlane) elsePlane.SetActive(hasElse);
                if (increaseElseAmountButton) increaseElseAmountButton.SetActive(hasElse);
                if (decreaseElseAmountButton) decreaseElseAmountButton.SetActive(hasElse);

                ElseRange = hasElse ? Mathf.Max(elseRange, MinRange) : 0;
            }
        }

        [PunRPC]
        private void SetElseRangeRPC(int value) => ElseRange = value;
        public int ElseRange
        {
            get => elseRange;

            private set
            {
                if (!hasElse)
                {
                    elseRange = 0;
                    UpdateElsePanelScale();
                    SyncLoopRange();
                    return;
                }

                if (value < MinRange)
                {
                    // Shrinking below the minimum removes the senão entirely - same convention as
                    // Range itself - which also brings the "add senão" button back (see HasElse's setter).
                    HasElse = false;
                    return;
                }

                if (cachedMaxElseRange < MinRange)
                {
                    // Growing the "then" body left no room at all for a "senão" body. Leaving HasElse on here
                    // would clamp elseRange to 0 while the else controls stay visible but permanently unable to
                    // grow or shrink (Mathf.Clamp with min > max always returns max), so retract it entirely -
                    // the same way Range does when it drops below MinRange.
                    hasElse = false;
                    elseRange = 0;
                    if (addElseButton) addElseButton.SetActive(true);
                    if (elseLabel) elseLabel.SetActive(false);
                    if (elsePlane) elsePlane.SetActive(false);
                    if (increaseElseAmountButton) increaseElseAmountButton.SetActive(false);
                    if (decreaseElseAmountButton) decreaseElseAmountButton.SetActive(false);
                    UpdateElsePanelScale();
                    SyncLoopRange();
                    return;
                }

                elseRange = Mathf.Clamp(value, MinRange, cachedMaxElseRange);
                if (increaseElseAmountButton) increaseElseAmountButton.SetActive(elseRange < cachedMaxElseRange);
                if (elseRangeText) elseRangeText.text = elseRange.ToString();
                UpdateElsePanelScale();
                SyncLoopRange();
            }
        }

        /// <summary>
        /// If this cell also HasLoop, the loop wraps this If directly with no action cube of its own -
        /// its Range must always equal this If's total raw-cell span (header + then + else), so the
        /// player's own then/else +/- buttons are effectively the loop's size controls too. No-op when
        /// there's no co-located loop (the common case for a standalone If).
        /// </summary>
        private void SyncLoopRange()
        {
            if (ParentCell != null && ParentCell.HasLoop)
                ParentCell.LoopController.SyncRangeTo(1 + Range + ElseRange);
        }

        private int cachedMaxThenRange = int.MaxValue;
        private int cachedMaxElseRange = int.MaxValue;

        private void OnEnable() => RefreshEarlierPanels();

        private void OnDisable()
        {
            RefreshEarlierPanels();
            ResetRpc();
        }

        #region Increase/Decrease

        [Button]
        public void IncreaseRange() =>
            photonView.RPC(nameof(SetRangeRPC), RpcTarget.AllBuffered, Range + 1);

        [Button]
        public void DecreaseRange() =>
            photonView.RPC(nameof(SetRangeRPC), RpcTarget.AllBuffered, Range - 1);

        [Button]
        public void ToggleElse() =>
            photonView.RPC(nameof(SetHasElseRPC), RpcTarget.AllBuffered, !HasElse);

        [Button]
        public void IncreaseElseRange() =>
            photonView.RPC(nameof(SetElseRangeRPC), RpcTarget.AllBuffered, ElseRange + 1);

        [Button]
        public void DecreaseElseRange() =>
            photonView.RPC(nameof(SetElseRangeRPC), RpcTarget.AllBuffered, ElseRange - 1);

        #endregion

        #region Refreshing

        // A "row" here is one CodingCell's worth of world spacing. (.5 + planeGrowthOffset*.5) is only
        // HALF that row's actual spacing (confirmed empirically: a plane/label positioned at N of these
        // units lands only halfway to row N) - a single row at index R sits at local Y = -2*R*RowUnit,
        // and a span's center is the average of its first/last row: -(A+B)*RowUnit.
        private float RowUnit => .5f + planeGrowthOffset * .5f;

        private void UpdatePanelScale()
        {
            if (!plane) return;

            // The "then" body spans rows [1, Range] (row 0 is this header, which no longer needs a
            // coexisting action cube) - center = -(1 + Range) * RowUnit.
            Vector3 position = plane.transform.localPosition;
            position.y = -(1 + Range) * RowUnit;
            plane.transform.localPosition = position;

            Vector3 scale = plane.transform.localScale;
            scale.z = Range * planeSize + (Range - 1) * planeSize * planeGrowthOffset;
            plane.transform.localScale = scale;
        }

        private void UpdateElsePanelScale()
        {
            if (!elsePlane) return;

            if (!hasElse || elseRange <= 0) return;

            // The else body spans rows [Range+1, Range+ElseRange] - center = -(2*Range + ElseRange + 1) * RowUnit.
            Vector3 position = elsePlane.transform.localPosition;
            position.y = -(2 * Range + ElseRange + 1) * RowUnit;
            elsePlane.transform.localPosition = position;

            Vector3 scale = elsePlane.transform.localScale;
            scale.z = ElseRange * planeSize + (ElseRange - 1) * planeSize * planeGrowthOffset;
            elsePlane.transform.localScale = scale;
        }

        /// <summary>
        /// Moves the "Senão" label and its +/- range controls to the boundary between the then body's
        /// last row (Range) and the else body's first row (Range+1) - between slots, not centered on
        /// either one, since the label marks a transition point rather than occupying a slot itself.
        /// Visibly tracks Range instead of sitting at a fixed row that's only correct when Range == 1.
        /// The toggle button itself (addElseButton) is NOT moved - it stays fixed below the then-range
        /// UP/DOWN buttons, separate from the label, so activating/deactivating Senão never relocates it.
        /// </summary>
        private void UpdateElseRowPosition()
        {
            float y = -(2 * Range + 1) * RowUnit;
            SetLocalY(elseLabel, y);
            SetLocalY(increaseElseAmountButton, y);
            SetLocalY(decreaseElseAmountButton, y);
        }

        private static void SetLocalY(GameObject go, float y)
        {
            if (!go) return;
            Vector3 p = go.transform.localPosition;
            p.y = y;
            go.transform.localPosition = p;
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

            int nextBlockIndex = ParentCell.Computer.Cells.FindIndex(index + 1,
                cell => cell.HasLoop || cell.HasCondition);
            if (nextBlockIndex == -1) nextBlockIndex = panelCount;

            // The "then" body no longer includes this header cell itself (the If IS the header, it
            // doesn't also need a coexisting action cube), so it only has room in [index+1, nextBlockIndex).
            cachedMaxThenRange = Mathf.Min(nextBlockIndex - (index + 1), Settings.maxThenRange);
            Range = range; // setter also calls RecomputeElseBounds()
        }

        private void RecomputeElseBounds()
        {
            if (!ParentCell) return;

            int index = ParentCell.Index;
            int panelCount = ParentCell.Computer.Cells.Count;

            int elseStart = index + 1 + Range;
            int nextBlockIndexAfterThen = elseStart < panelCount
                ? ParentCell.Computer.Cells.FindIndex(elseStart, cell => cell.HasLoop || cell.HasCondition)
                : -1;
            if (nextBlockIndexAfterThen == -1) nextBlockIndexAfterThen = panelCount;

            cachedMaxElseRange = Mathf.Min(nextBlockIndexAfterThen - elseStart, Settings.maxElseRange);
            ElseRange = elseRange;
        }

        #endregion

        public void ResetConditionData() => photonView.RPC(nameof(ResetRpc), RpcTarget.AllBuffered);

        [PunRPC]
        private void ResetRpc()
        {
            if (!ParentCell) return;

            HasElse = false;
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
