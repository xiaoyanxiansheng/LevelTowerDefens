using Unity.Mathematics;

namespace Battle
{
    public struct GridDirection
    {
        public static readonly int2 None = new int2(0, 0);
        public static readonly int2 North = new int2(0, 1);
        public static readonly int2 South = new int2(0, -1);
        public static readonly int2 East = new int2(1, 0);
        public static readonly int2 West = new int2(-1, 0);
        public static readonly int2 NorthEast = new int2(1, 1);
        public static readonly int2 NorthWest = new int2(-1, 1);
        public static readonly int2 SouthEast = new int2(1, -1);
        public static readonly int2 SouthWest = new int2(-1, -1);

        public static readonly int2[] CardinalDirections = { North, East, South, West };
        public static readonly int2[] AllDirections = { None, North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest };
    }
}