using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace TMG.FlowFieldECS
{
    public struct Position : IComponentData
    {
        public int2 GridIndex;
    }

    public struct VelocityComponent : IComponentData
    {
        public float3 Velocity;
    }

    public struct Cell : IComponentData
    {
        public byte Cost;
        public ushort BestCost;
        public CellDirection BestDirection;
    }
}
