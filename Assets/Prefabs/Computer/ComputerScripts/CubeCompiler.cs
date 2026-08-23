using System;
using System.Collections.Generic;
using SSPot.Utilities;
using UnityEngine;

namespace SSpot.Ambient.ComputerCode
{
    [Serializable]
    public class CubeCompiler
    {
        [SerializeField] private bool mustUseAllSlots = true;
        
        [Header("Errors")]
        [SerializeField] private string emptyError = "Deu ERRO!\nPreencha todos os espaços com cubos";
        [SerializeField] private string beginError = "Deu ERRO!\nVerifique se o algoritmo comeca com \"Início\"";
        [SerializeField] private string endError = "Deu ERRO!\nVerifique se o algoritmo termina com \"Fim\"";
        [SerializeField] private string allSlotsError = "Deu ERRO!\nVocê deve preencher todas as placas de programação";
        [SerializeField] private string noHolesError = "Deu ERRO!\nSeu algoritmo não pode ter placas vazias";
        [SerializeField] private string beginEndInMiddleError = "Deu ERRO!\nInício e Fim devem ser usados no lugar certo";
        
        private static CompilationResult Error(string error, int index) => new(error, index);
        
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
            
            if (mustUseAllSlots && lastIndex < cells.Count - 1)
                return Error(allSlotsError);
            
            if (cells[0].CurrentCube is not {type: Cube.CubeType.Begin})
                return Error(beginError);

            if (cells[lastIndex].CurrentCube is not {type: Cube.CubeType.End})
                return Error(endError);

            var bodyResult = CompileRange(cells, 1, lastIndex);
            if (bodyResult.IsError)
                return bodyResult;

            var result = new List<Cube> {new(Cube.CubeType.Begin)};
            result.AddRange(bodyResult.Result);
            result.Add(new(Cube.CubeType.End));

            return new CompilationResult(result);
        }

        /// <summary>
        /// Compiles a range of coding cells into a flat list of cubes, validating placement and type usage.
        /// Recurses into loop bodies (unrolled Iterations times, resolved at compile time) and into If bodies
        /// (emitted as a single If cube followed by its "then" and "else" bodies, resolved at runtime by
        /// CubeRunner since which branch runs depends on the robot's state when it's reached).
        /// </summary>
        /// <param name="cells">The list of coding cells to compile.</param>
        /// <param name="start">The starting index of the range to compile (inclusive).</param>
        /// <param name="end">The ending index of the range to compile (exclusive).</param>
        /// <returns>A <see cref="CompilationResult"/> containing the compiled cubes or an error if compilation fails.</returns>
        private CompilationResult CompileRange(IReadOnlyList<CodingCell> cells, int start, int end)
        {
            List<Cube> result = new();

            int i = start;
            while (i < end)
            {
                var cell = cells[i];

                if (cell.HasLoop)
                {
                    var loop = cell.LoopController;
                    var body = CompileBlockBody(cells, i, i + loop.Range);
                    if (body.IsError)
                        return body;

                    for (int r = 0; r < loop.Iterations; r++)
                        result.AddRange(body.Result);

                    i += loop.Range;
                }
                else if (cell.HasCondition)
                {
                    var condition = cell.ConditionController;
                    var thenBody = CompileBlockBody(cells, i, i + condition.Range);
                    if (thenBody.IsError)
                        return thenBody;

                    var ifCube = new Cube(Cube.CubeType.If) { ThenLength = thenBody.Result.Count };
                    result.Add(ifCube);
                    result.AddRange(thenBody.Result);

                    int next = i + condition.Range;
                    if (condition.HasElse)
                    {
                        var elseBody = CompileRange(cells, next, next + condition.ElseRange);
                        if (elseBody.IsError)
                            return elseBody;

                        ifCube.ElseLength = elseBody.Result.Count;
                        result.AddRange(elseBody.Result);
                        next += condition.ElseRange;
                    }

                    i = next;
                }
                else
                {
                    var leaf = CompileCell(cell, i);
                    if (leaf.IsError)
                        return leaf;

                    result.Add(leaf.Result[0]);
                    i++;
                }
            }

            return new CompilationResult(result);
        }

        /// <summary>
        /// Compiles a block body [start, end) whose first cell is itself the block's header (HasLoop or
        /// HasCondition is true there). The header cell's own action cube is compiled as a plain leaf instead
        /// of recursing back into CompileRange for it — otherwise it would re-detect its own HasLoop/HasCondition
        /// flag and recurse forever. The remaining cells recurse normally, so a block nested further inside the
        /// body (e.g. an If nested inside this Loop) is still handled.
        /// </summary>
        private CompilationResult CompileBlockBody(IReadOnlyList<CodingCell> cells, int start, int end)
        {
            var header = CompileCell(cells[start], start);
            if (header.IsError)
                return header;

            var rest = CompileRange(cells, start + 1, end);
            if (rest.IsError)
                return rest;

            var result = new List<Cube>(header.Result);
            result.AddRange(rest.Result);
            return new CompilationResult(result);
        }

        private CompilationResult CompileCell(CodingCell cell, int index)
        {
            var baseCube = cell.CurrentCube;
            if (baseCube == null)
                return Error(noHolesError, index);

            if (baseCube.type is Cube.CubeType.Begin or Cube.CubeType.End)
                return Error(beginEndInMiddleError, index);

            return new CompilationResult(new List<Cube> {new(baseCube.type)});
        }
    }
}