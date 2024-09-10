using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TMG.FlowFieldECS
{
    public struct CellData
    {
        public int2 GridIndex;
        public ushort BestCost;
        public Vector3 Direction;
        public bool IsDestination;
    }

    public struct DebugData : IComponentData
    {
        public int DestinationIndex;
        public float CellRadius;
        public NativeArray<CellData> debugCellDatas;
    }

    [BurstCompile]
    public partial struct DebugSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ECSBootstrap>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<DebugData>())
            {
                Entity singletonEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponent<DebugData>(singletonEntity);
            }

            var flowFieldDebugDataComponent = SystemAPI.GetSingleton<DebugData>();
            var flowFieldDebugDataEntity = SystemAPI.GetSingletonEntity<DebugData>();
            var eCSBootstrap = SystemAPI.GetSingleton<ECSBootstrap>();

            flowFieldDebugDataComponent.CellRadius = eCSBootstrap.CellRadius;

            NativeArray<CellData> debugCellDatas = new NativeArray<CellData>(eCSBootstrap.GridSize.x * eCSBootstrap.GridSize.y, Allocator.Domain); // TODO 这个分配有问题
            foreach (var (position, flowFieldData) in SystemAPI.Query<RefRO<Position>, RefRO<Cell>>())
            {
                // 获取格子中心点的位置
                int2 GridIndex = position.ValueRO.GridIndex;

                // 创建一个 DebugCellData 实例来存储数据
                var debugData = new CellData
                {
                    GridIndex = GridIndex,
                    BestCost = flowFieldData.ValueRO.BestCost,
                    Direction = new Vector3(flowFieldData.ValueRO.BestDirection.Vector.x, 0, flowFieldData.ValueRO.BestDirection.Vector.y),
                    IsDestination = false,
                };

                int2 int2dex = position.ValueRO.GridIndex;
                int index = eCSBootstrap.GridSize.x * int2dex.y + int2dex.x;
                debugCellDatas[index] = debugData;
            }
            flowFieldDebugDataComponent.debugCellDatas = debugCellDatas;

             state.EntityManager.SetComponentData<DebugData>(flowFieldDebugDataEntity, flowFieldDebugDataComponent);
        }
    }
}
