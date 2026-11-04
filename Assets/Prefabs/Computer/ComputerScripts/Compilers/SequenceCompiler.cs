using System.Collections.Generic;

namespace SSpot.Ambient.ComputerCode
{
    /// <summary>
    /// Body compiler for the tutorial (área D): every cell is a single movement cube, compiled in order.
    /// Also the base the other compilers build on - they only add the blocks their levels use by
    /// overriding <see cref="CompileStatement"/>.
    /// </summary>
    internal class SequenceCompiler
    {
        protected readonly CubeCompiler Settings;

        public SequenceCompiler(CubeCompiler settings) => Settings = settings;

        protected static CompilationResult Error(string error, int index) => new(error, index);

        /// <summary>
        /// Compiles the cells in [start, end) into a flat list of cubes.
        /// </summary>
        public CompilationResult CompileRange(IReadOnlyList<CodingCell> cells, int start, int end)
        {
            var result = new List<Cube>();

            int i = start;
            while (i < end)
            {
                var statement = CompileStatement(cells, i, out int next);
                if (statement.IsError)
                    return statement;

                result.AddRange(statement.Result);
                i = next;
            }

            return new CompilationResult(result);
        }

        /// <summary>
        /// Compiles the statement starting at cells[i], reporting through <paramref name="next"/> the index
        /// right after the last cell it consumed.
        /// </summary>
        protected virtual CompilationResult CompileStatement(IReadOnlyList<CodingCell> cells, int i, out int next)
        {
            next = i + 1;
            return CompileCube(cells[i], i);
        }

        /// <summary>
        /// Compiles the single movement cube placed on a cell.
        /// </summary>
        protected CompilationResult CompileCube(CodingCell cell, int index)
        {
            var baseCube = cell.CurrentCube;
            if (baseCube == null)
                return Error(Settings.NoHolesError, index);

            if (baseCube.type is Cube.CubeType.Begin or Cube.CubeType.End)
                return Error(Settings.BeginEndInMiddleError, index);

            return new CompilationResult(new List<Cube> {new(baseCube.type)});
        }
    }
}
