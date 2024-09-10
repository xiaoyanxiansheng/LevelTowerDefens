using System.Collections.Generic;
using Unity.Mathematics;

namespace TMG.FlowFieldECS
{
    public struct CellDirection
    {
        public int2 Vector;

        private CellDirection(int x, int y)
        {
            Vector = new int2(x, y);
        }

        public static implicit operator int2(CellDirection direction)
        {
            return direction.Vector;
        }

        public static readonly CellDirection None = new CellDirection(0, 0);
        public static readonly CellDirection North = new CellDirection(0, 1);
        public static readonly CellDirection South = new CellDirection(0, -1);
        public static readonly CellDirection East = new CellDirection(1, 0);
        public static readonly CellDirection West = new CellDirection(-1, 0);
        public static readonly CellDirection NorthEast = new CellDirection(1, 1);
        public static readonly CellDirection NorthWest = new CellDirection(-1, 1);
        public static readonly CellDirection SouthEast = new CellDirection(1, -1);
        public static readonly CellDirection SouthWest = new CellDirection(-1, -1);

        public static readonly CellDirection[] CardinalAndIntercardinalDirections =
        {
            North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest
        };

        public static readonly CellDirection[] AllDirections =
        {
            North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest, None
        };

        public static readonly CellDirection[] CardinalDirections = 
        {
            North,            East,            South,            West
        };
    }
}
