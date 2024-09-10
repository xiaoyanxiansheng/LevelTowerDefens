using Unity.Entities;
using Unity.Mathematics;

namespace Battle 
{ 
    public struct Cell : IComponentData
    {
        public float3 Position;// TODO是否可以去除
        public int2 GridIndex;
        public byte Cost;
        public ushort BestCost;

        // TODO 是否只需要方向参数即可
        public GridDirection bestDirection;
    }
}