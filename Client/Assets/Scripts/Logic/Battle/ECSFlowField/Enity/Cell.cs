using Unity.Entities;
using Unity.Mathematics;

namespace Battle 
{ 
    public struct Cell : IComponentData
    {
        public float3 Position;// TODO�Ƿ����ȥ��
        public int2 GridIndex;
        public byte Cost;
        public ushort BestCost;

        // TODO �Ƿ�ֻ��Ҫ�����������
        public GridDirection bestDirection;
    }
}