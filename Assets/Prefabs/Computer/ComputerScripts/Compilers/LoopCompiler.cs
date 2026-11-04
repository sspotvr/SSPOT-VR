using System.Collections.Generic;

namespace SSpot.Ambient.ComputerCode
{
    /// <summary>
    /// Body compiler for levels 1 to 3 and the labyrinth: sequence + Repetir. A loop is unrolled at compile
    /// time - its body (the loop's own cell plus the next Range - 1 cells) is compiled once and repeated
    /// Iterations times.
    /// </summary>
    internal class LoopCompiler : SequenceCompiler
    {
        public LoopCompiler(CubeCompiler settings) : base(settings) { }

        protected override CompilationResult CompileStatement(IReadOnlyList<CodingCell> cells, int i, out int next)
        {
            if (!cells[i].HasLoop)
                return base.CompileStatement(cells, i, out next);

            var loop = cells[i].LoopController;
            next = i + loop.Range;

            var body = CompileLoopBody(cells, i, next);
            if (body.IsError)
                return body;

            var result = new List<Cube>();
            for (int r = 0; r < loop.Iterations; r++)
                result.AddRange(body.Result);

            return new CompilationResult(result);
        }

        /// <summary>
        /// Compiles a loop body [start, end), whose first cell is the loop's own cell.
        /// </summary>
        private CompilationResult CompileLoopBody(IReadOnlyList<CodingCell> cells, int start, int end)
        {
            var header = CompileLoopHeader(cells, start, out int headerEnd);
            if (header.IsError)
                return header;

            var rest = CompileRange(cells, headerEnd, end);
            if (rest.IsError)
                return rest;

            var result = new List<Cube>(header.Result);
            result.AddRange(rest.Result);
            return new CompilationResult(result);
        }

        /// <summary>
        /// Compiles what sits on the loop's own cell - here always a movement cube (the loop can't nest
        /// itself, so it isn't compiled as a statement again).
        /// </summary>
        protected virtual CompilationResult CompileLoopHeader(IReadOnlyList<CodingCell> cells, int i, out int next)
        {
            next = i + 1;
            return CompileCube(cells[i], i);
        }
    }
}
