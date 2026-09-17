using SSpot.Ambient.ComputerCode;
using UnityEngine;

public class RemoveConditionOnClick : MonoBehaviour
{
    public ConditionController conditionController;

    /// <summary>
    /// When player clicks on this object (the "Se obstáculo" cube itself), it removes the whole If
    /// block - cube and UI alike - the same as clearing it would.
    /// </summary>
    public void OnPointerClick()
    {
        conditionController.ResetConditionData();
    }
}
