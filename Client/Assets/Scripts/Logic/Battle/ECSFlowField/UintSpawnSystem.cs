using TMG.FlowFieldECS;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Battlt
{
    [BurstCompile]
    [UpdateAfter(typeof(UpdateSystem))]
    public partial struct UintSpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Uint>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;

            var prefab = SystemAPI.GetSingleton<Uint>().Prefab;

            for (int i = 0; i < 10; i++)  // 假设我们要实例化10个单位
            {
                Entity instance = state.EntityManager.Instantiate(prefab);
                // state.EntityManager.SetComponentData(instance, new LocalTransform { Position = float3.zero });
                state.EntityManager.AddComponent<MoveUint>(instance);
            }
        }
    }
}
