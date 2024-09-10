using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;

namespace TMG.FlowFieldECS
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(SceneSystemGroup))]
    public partial struct InitializationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ECSBootstrap>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
             state.Enabled = false;

            ECSBootstrap eCSBootstrap = SystemAPI.GetSingleton<ECSBootstrap>();

            int2 gridSize = eCSBootstrap.GridSize;

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            for (int y = 0; y < gridSize.y; y++)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    Entity cellEntity = ecb.CreateEntity();
                    ecb.AddComponent<Cell>(cellEntity);
                    ecb.AddComponent<Position>(cellEntity, new Position() {
                        GridIndex = new int2(x, y)
                    });
                }
            }

            // 采用一次性创建，不要每个循环创建
            ecb.Playback(state.EntityManager);
        }
    }
}
