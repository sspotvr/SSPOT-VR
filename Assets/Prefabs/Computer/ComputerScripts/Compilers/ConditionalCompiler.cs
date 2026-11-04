using System.Collections.Generic;

namespace SSpot.Ambient.ComputerCode
{
    /// <summary>
    /// Body compiler for level 4: sequence + Repetir + Se/Senão. An If is emitted as a single If cube followed
    /// by its "then" and "else" bodies; which branch runs is resolved by CubeRunner when it's reached, since
    /// it depends on the robot's position at that moment.
    /// </summary>
    internal class ConditionalCompiler : LoopCompiler
    {
        public ConditionalCompiler(CubeCompiler settings) : base(settings) { }

        protected override CompilationResult CompileStatement(IReadOnlyList<CodingCell> cells, int i, out int next)
        {
            var cell = cells[i];

            // A loop on the same cell as an If wraps it, so the loop comes first (see CompileLoopHeader).
            if (cell.HasLoop)
                return base.CompileStatement(cells, i, out next);

            if (cell.HasCondition)
                return CompileCondition(cells, i, out next);

            if (cell.HasSenao)
            {
                // A Senão is always consumed by the If right before it (AttachingCube only lets one be placed
                // right after an If's then-body, and locks that If's Range). Reaching one here means that
                // invariant broke - report it instead of misreporting it as an empty slot.
                next = i + 1;
                return Error(Settings.OrphanSenaoError, i);
            }

            return base.CompileStatement(cells, i, out next);
        }

        /// <summary>
        /// The loop's own cell can hold an If instead of a movement cube ("repetir: se obstáculo... senão...").
        /// </summary>
        protected override CompilationResult CompileLoopHeader(IReadOnlyList<CodingCell> cells, int i, out int next) =>
            cells[i].HasCondition
                ? CompileCondition(cells, i, out next)
                : base.CompileLoopHeader(cells, i, out next);

        /// <summary>
        /// Compiles the If at cells[i] and, if the cell right after its then-body is a Senão, the else-body too.
        /// </summary>
        private CompilationResult CompileCondition(IReadOnlyList<CodingCell> cells, int i, out int next)
        {
            var condition = cells[i].ConditionController;
            next = i + 1 + condition.Range;

            var thenBody = CompileRange(cells, i + 1, next);
            if (thenBody.IsError)
                return thenBody;

            var ifCube = new Cube(Cube.CubeType.If) { ThenLength = thenBody.Result.Count };
            var result = new List<Cube> { ifCube };
            result.AddRange(thenBody.Result);

            if (next < cells.Count && cells[next].HasSenao)
            {
                var senao = cells[next].ElseController;
                var elseBody = CompileRange(cells, next + 1, next + 1 + senao.Range);
                if (elseBody.IsError)
                    return elseBody;

                ifCube.ElseLength = elseBody.Result.Count;
                result.AddRange(elseBody.Result);
                next += 1 + senao.Range;
            }

            return new CompilationResult(result);
        }
    }
}
