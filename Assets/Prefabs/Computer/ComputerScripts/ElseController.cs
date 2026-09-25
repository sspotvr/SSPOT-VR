using System;
using NaughtyAttributes;
using Photon.Pun;
using SSPot.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace SSpot.Ambient.ComputerCode
{
    /// <summary>
    /// Drives the "Senão" (Else) block. Occupies its own slot, exactly like <see cref="ConditionController"/>
    /// (If), with Range covering its own body of cells immediately after this one - but unlike an If, it
    /// can only ever be attached to the cell immediately following some If's then-body (see
    /// AttachingCube.AttachCube). OwningIf is that If; growing/shrinking this Range keeps the owning If's
    /// co-located loop (if any) in sync via OwningIf.SyncLoopRange().
    /// </summary>
    public class ElseController : MonoBehaviourPun
    {
        private const int MinRange = 1;

        [Serializable]
        public class ElseSettings
        {
            [AllowNesting, MinValue(MinRange)]
            public int maxRange = 3;
        }

        public CodingCell ParentCell { get; set; }

        /// <summary>The If this Senão belongs to. Set by AttachingCube when this block is attached.</summary>
        public ConditionController OwningIf { get; set; }

        [field: BoxGroup("Current Values"), SerializeField, ReadOnly]
        private int range = MinRange;

        [BoxGroup("Else Settings"), SerializeField]
        private bool overrideGlobalSettings;
        [BoxGroup("Else Settings"), SerializeField, ShowIf(nameof(overrideGlobalSettings))]
        private ElseSettings settings;

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

        private ElseSettings Settings => overrideGlobalSettings
            ? settings
            : ParentCell.Computer.GlobalElseSettings;

        [PunRPC]
        private void SetRangeRPC(int value) => Range = value;
        public int Range
        {
            get => range;

            private set
            {
                if (value < MinRange || cachedMaxRange < MinRange)
                {
                    range = MinRange;
                    gameObject.SetActive(false);
                    return;
                }

                range = Mathf.Clamp(value, MinRange, cachedMaxRange);
                if (increaseAmountButton) increaseAmountButton.SetActive(range < cachedMaxRange);
                if (decreaseAmountButton) decreaseAmountButton.SetActive(true);
                if (rangeText) rangeText.text = range.ToString();
                UpdatePanelScale();

                OwningIf?.SyncLoopRange();
            }
        }

        private int cachedMaxRange = int.MaxValue;

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

            cachedMaxRange = Mathf.Min(nextBlockIndex - (index + 1), Settings.maxRange);
            Range = range;
        }

        #endregion

        public void ResetConditionData() => photonView.RPC(nameof(ResetRpc), RpcTarget.AllBuffered);

        [PunRPC]
        private void ResetRpc()
        {
            if (!ParentCell) return;

            var owner = OwningIf;
            if (owner != null && owner.AttachedSenao == this)
                owner.AttachedSenao = null;
            OwningIf = null;

            // Re-runs the owning If's Range setter so its manual +/- buttons reappear (AttachedSenao is
            // now null) and its co-located loop's Range (if any) drops this Senão's span.
            owner?.RefreshLimits();

            Range = MinRange;
            gameObject.SetActive(false);
        }
    }
}
