using System;
using NaughtyAttributes;
using Photon.Pun;
using SSPot.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace SSpot.Ambient.ComputerCode
{
    /// <summary>
    /// Drives the "Se obstáculo" (If) block. Mirrors <see cref="LoopController"/>: Range covers the "then"
    /// body starting at (and including) this cell, and can optionally grow a "senão" (else) body of ElseRange
    /// cells immediately after it. Unlike a loop, whether the "then" or "else" branch runs is resolved at
    /// runtime by CubeRunner, not baked in by CubeCompiler.
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
                if (value < MinRange)
                {
                    range = MinRange;
                    gameObject.SetActive(false);
                    return;
                }

                range = Mathf.Clamp(value, MinRange, cachedMaxThenRange);
                if (increaseAmountButton) increaseAmountButton.SetActive(range < cachedMaxThenRange);
                if (rangeText) rangeText.text = range.ToString();
                UpdatePanelScale();

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
                if (addElseButton) addElseButton.SetActive(!hasElse);
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
                    return;
                }

                elseRange = Mathf.Clamp(value, MinRange, cachedMaxElseRange);
                if (increaseElseAmountButton) increaseElseAmountButton.SetActive(elseRange < cachedMaxElseRange);
                if (elseRangeText) elseRangeText.text = elseRange.ToString();
                UpdateElsePanelScale();
            }
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

        private void UpdatePanelScale()
        {
            if (!plane) return;

            Vector3 position = plane.transform.localPosition;
            position.y = -(Range - 1) * (.5f + planeGrowthOffset * .5f);
            plane.transform.localPosition = position;

            Vector3 scale = plane.transform.localScale;
            scale.z = Range * planeSize + (Range - 1) * planeSize * planeGrowthOffset;
            plane.transform.localScale = scale;
        }

        private void UpdateElsePanelScale()
        {
            if (!elsePlane) return;

            if (!hasElse || elseRange <= 0) return;

            Vector3 position = elsePlane.transform.localPosition;
            position.y = -Range * (.5f + planeGrowthOffset * .5f) - (ElseRange - 1) * (.5f + planeGrowthOffset * .5f);
            elsePlane.transform.localPosition = position;

            Vector3 scale = elsePlane.transform.localScale;
            scale.z = ElseRange * planeSize + (ElseRange - 1) * planeSize * planeGrowthOffset;
            elsePlane.transform.localScale = scale;
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

            cachedMaxThenRange = Mathf.Min(nextBlockIndex - index, Settings.maxThenRange);
            Range = range; // setter also calls RecomputeElseBounds()
        }

        private void RecomputeElseBounds()
        {
            if (!ParentCell) return;

            int index = ParentCell.Index;
            int panelCount = ParentCell.Computer.Cells.Count;

            int elseStart = index + Range;
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
            HasElse = false;
            Range = MinRange;
        }
    }
}
