using System.Collections.Generic;
using System.Linq;
using SSpot.Ambient.ComputerCode;
using SSpot.Level;
using SSPot.Level;
using UnityEngine;

namespace SSpot.Evaluators
{
    /// <summary>
    /// Checks the robot's final grid position (and, optionally, facing) once it finishes running, instead of
    /// comparing the program's structure against one canonical solution. Used for levels like the Fourth Level
    /// where an If/Else nested inside a Loop can be solved by more than one valid program - ExactSequenceEvaluator
    /// can't express that (it only structurally compares one level of loop nesting), but "did the robot actually
    /// get there" works regardless of which valid program the player wrote.
    /// </summary>
    public class RobotPositionEvaluator : CodeEvaluator
    {
        [SerializeField] private Vector2Int targetCell;

        [Tooltip("If true, the robot must also be facing this direction at the target cell.")]
        [SerializeField] private bool checkFacing;
        [SerializeField] private Vector2Int targetFacing = Vector2Int.up;

        [Tooltip("If true, the compiled program must contain at least one If cube - stops the player from just " +
                 "hand-writing the specific turns needed instead of using the Se/Senão block.")]
        [SerializeField] private bool requireIfCube = true;

        [SerializeField, Multiline] private string missingIfErrorMessage = "Deu ERRO!\nUse o bloco \"Se obstáculo\" para desviar!";
        [SerializeField, Multiline] private string wrongPositionErrorMessage = "Deu ERRO!\nO robô não chegou ao lugar certo!";

        public override void EvaluatePreCompilation(IReadOnlyList<CodingCell> cells) { }

        public override void EvaluatePostCompilation(IReadOnlyList<Cube> cubes)
        {
            if (requireIfCube && cubes.All(c => c.type != Cube.CubeType.If))
            {
                ReportResult(LevelResult.Error(missingIfErrorMessage));
                return;
            }

            LevelManager.Instance.OnFinishRunning.AddListener(CheckRobotPosition);
        }

        private void CheckRobotPosition()
        {
            LevelManager.Instance.OnFinishRunning.RemoveListener(CheckRobotPosition);

            var mover = LevelManager.Instance.Robot.Mover;
            bool success = mover.GridPosition == targetCell && (!checkFacing || mover.Facing == targetFacing);

            ReportResult(success ? LevelResult.Success() : LevelResult.Error(wrongPositionErrorMessage));
        }
    }
}
