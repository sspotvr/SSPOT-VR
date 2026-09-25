using SSpot.Ambient.ComputerCode;
using UnityEngine;

public class RemoveConditionOnClick : MonoBehaviour
{
    // Exactly one of these should be set - this script is shared by the "Se obstáculo" cube and the
    // "Senão" cube, each removing their own block (cube and UI alike) when clicked.
    public ConditionController conditionController;
    public ElseController elseController;

    public void OnPointerClick()
    {
        if (conditionController != null) conditionController.ResetConditionData();
        else if (elseController != null) elseController.ResetConditionData();
    }
}
