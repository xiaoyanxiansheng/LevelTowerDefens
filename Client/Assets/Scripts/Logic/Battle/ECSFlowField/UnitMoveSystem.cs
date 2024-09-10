using Battlt;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace TMG.FlowFieldECS
{
    [BurstCompile]
    [UpdateAfter(typeof(UintSpawnSystem))]
    public partial struct UnitMoveSystem : ISystem
    {
        private EntityQuery m_GridQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Uint>();
            m_GridQuery = SystemAPI.QueryBuilder().WithAll<Cell, Position>().Build();
        }

        // ≤…”√ WIth(moveUint)
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<ECSBootstrap>();
            float diameter = config.CellRadius * 2;
            int2 gridSize = config.GridSize;
            float deltaTime = SystemAPI.Time.DeltaTime;
            var cellArray = m_GridQuery.ToComponentDataArray<Cell>(Allocator.Temp);
            var entityArray = m_GridQuery.ToEntityArray(Allocator.Temp);
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (localTransform, moveUint ,entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveUint>>().WithEntityAccess())
            {
                var newLocalTransform = localTransform.ValueRW;
                int x = (int)(newLocalTransform.Position.x / diameter);
                int y = (int)(newLocalTransform.Position.z / diameter);
                int index = y * gridSize.x + x;
                var cell = cellArray[index];

                float3 moveDirection = new float3(cell.BestDirection.Vector.x, 0, cell.BestDirection.Vector.y);
                var position = newLocalTransform.Position + moveDirection * 1f * deltaTime;
                
                newLocalTransform.Position = position;
                ecb.SetComponent(entity, newLocalTransform);
            }
            ecb.Playback(state.EntityManager);
        }
    }
}
