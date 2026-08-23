using SSpot.Ambient.ComputerCode;
using UnityEngine;

public class AddRemoveConditionRange : MonoBehaviour
{
    public bool isAdding;
    public bool isElseRange;
    public ConditionController conditionController;

    /// <summary>
    /// When player clicks on this object, it adds or removes one cell from the "then" (or "senão", if
    /// isElseRange) range of ConditionController.
    /// </summary>
    public void OnPointerClick()
    {
        if (isElseRange)
        {
            if (isAdding) conditionController.IncreaseElseRange();
            else conditionController.DecreaseElseRange();
        }
        else
        {
            if (isAdding) conditionController.IncreaseRange();
            else conditionController.DecreaseRange();
        }
    }
}
