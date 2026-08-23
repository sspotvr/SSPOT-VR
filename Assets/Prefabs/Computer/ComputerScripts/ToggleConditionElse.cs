using SSpot.Ambient.ComputerCode;
using UnityEngine;

public class ToggleConditionElse : MonoBehaviour
{
    public ConditionController conditionController;

    /// <summary>
    /// When player clicks on this object, it toggles the "Senão" branch of the ConditionController.
    /// </summary>
    public void OnPointerClick()
    {
        conditionController.ToggleElse();
    }
}
