using System.Collections.Generic;
using SSpot.Ambient.ComputerCode;
using SSPot.Level;
using UnityEngine;

namespace SSpot.Evaluators
{
	public class TutorialSequenceEvaluator : CodeEvaluator
	{
		[SerializeField, Multiline]
		private string errorMessage = "Você esqueceu de iniciar ou finalizar o código!";

		public override void EvaluatePreCompilation(IReadOnlyList<CodingCell> cells)
		{
			Debug.Log("Entrou na pre-comilação");
			// Verifica se tem pelo menos os dois cubos (Início e Fim)
			if (cells.Count < 2)
			{
				ReportResult(LevelResult.Error(errorMessage));
				return;
			}

			bool hasBegin = cells[0].CurrentCube != null && cells[0].CurrentCube.type == Cube.CubeType.Begin;

			var lastCell = cells[cells.Count - 1];
			bool hasEnd = lastCell.CurrentCube != null && lastCell.CurrentCube.type == Cube.CubeType.End;

			if (hasBegin && hasEnd)
			{
				ReportResult(LevelResult.Success());
				Debug.Log("Código iniciado e finalizado corretamente!");
			}
			else
			{
				ReportResult(LevelResult.Error(errorMessage));
				Debug.Log("Código deve começar com 'Início' e terminar com 'Fim'!");
			}
		}

		public override void EvaluatePostCompilation(IReadOnlyList<Cube> cubes)
		{
			// Não precisamos avaliar o código compilado para este caso
		}
	}
}