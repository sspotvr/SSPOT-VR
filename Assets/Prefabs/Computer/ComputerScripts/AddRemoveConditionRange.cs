using SSpot.Ambient.ComputerCode;
using UnityEngine;

public class AddRemoveConditionRange : MonoBehaviour
{
    public bool isAdding;

    // Exactly one of these should be set: the then-range (Se) buttons wire conditionController, the
    // else-range (Senão) buttons wire elseController - they're separate blocks now, each with its own
    // Range.
    public ConditionController conditionController;
    public ElseController elseController;

    /// <summary>
    /// When player clicks on this object, it adds or removes one cell from whichever block's range this
    /// button belongs to.
    /// </summary>
    public void OnPointerClick()
    {
        if (conditionController != null)
        {
            if (isAdding) conditionController.IncreaseRange();
            else conditionController.DecreaseRange();
        }
        else if (elseController != null)
        {
            if (isAdding) elseController.IncreaseRange();
            else elseController.DecreaseRange();
        }
    }
}
