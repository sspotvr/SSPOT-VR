using System;
using System.Collections.Generic;
using SSPot.Utilities;
using UnityEngine;

namespace SSpot.Ambient.ComputerCode
{
    /// <summary>
    /// Entry point of the cube compiler, configured per scene in the LevelManager. Validates the frame shared
    /// by every algorithm (Início ... Fim, filled slots) and delegates the body to the compiler of the scene's
    /// <see cref="CompilerType"/>, each one only knowing the blocks its levels actually use.
    /// </summary>
    [Serializable]
    public class CubeCompiler
    {
        public enum CompilerType
        {
            /// <summary>Tutorial (área D): plain sequence of movement cubes.</summary>
            Tutorial,
            /// <summary>Levels 1 to 3: sequence + Repetir.</summary>
            Levels,
            /// <summary>Labyrinth: sequence + Repetir; unused slots after "Fim" are allowed.</summary>
            Labyrinth,
            /// <summary>Level 4: sequence + Repetir + Se/Senão.</summary>
            Conditional
        }

        [SerializeField] private CompilerType type = CompilerType.Levels;

        [Header("Errors")]
        [SerializeField] private string emptyError = "Deu ERRO!\nPreencha todos os espaços com cubos";
        [SerializeField] private string beginError = "Deu ERRO!\nVerifique se o algoritmo comeca com \"Início\"";
        [SerializeField] private string endError = "Deu ERRO!\nVerifique se o algoritmo termina com \"Fim\"";
        [SerializeField] private string allSlotsError = "Deu ERRO!\nVocê deve preencher todas as placas de programação";
        [SerializeField] private string noHolesError = "Deu ERRO!\nSeu algoritmo não pode ter placas vazias";
        [SerializeField] private string beginEndInMiddleError = "Deu ERRO!\nInício e Fim devem ser usados no lugar certo";
        [SerializeField] private string orphanSenaoError = "Deu ERRO!\nO bloco \"Senão\" precisa vir logo depois de um \"Se\"";

        internal string NoHolesError => noHolesError;
        internal string BeginEndInMiddleError => beginEndInMiddleError;
        internal string OrphanSenaoError => orphanSenaoError;

        private bool MustUseAllSlots => type != CompilerType.Labyrinth;

        private static CompilationResult Error(string error) => new(error, -1);

        /// <summary>
        /// Compiles a list of coding cells into a sequence of cubes, checking for syntax and logical errors.
        /// </summary>
        /// <returns>A CompilationResult containing the compiled list of cubes or an error if compilation fails.</returns>
        public CompilationResult Compile(IReadOnlyList<CodingCell> cells)
        {
            int lastIndex = cells.FindLastIndex(cell => cell.CurrentCube != null);
            if (lastIndex == -1)
                return Error(emptyError);

            if (MustUseAllSlots && lastIndex < cells.Count - 1)
                return Error(allSlotsError);

            if (cells[0].CurrentCube is not {type: Cube.CubeType.Begin})
                return Error(beginError);

            if (cells[lastIndex].CurrentCube is not {type: Cube.CubeType.End})
                return Error(endError);

            var bodyResult = CreateBodyCompiler().CompileRange(cells, 1, lastIndex);
            if (bodyResult.IsError)
                return bodyResult;

            var result = new List<Cube> {new(Cube.CubeType.Begin)};
            result.AddRange(bodyResult.Result);
            result.Add(new(Cube.CubeType.End));

            return new CompilationResult(result);
        }

        private SequenceCompiler CreateBodyCompiler() => type switch
        {
            CompilerType.Tutorial => new SequenceCompiler(this),
            CompilerType.Levels or CompilerType.Labyrinth => new LoopCompiler(this),
            CompilerType.Conditional => new ConditionalCompiler(this),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
