
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TMG.FlowFieldECS
{
    public class DestinationAuthoring : MonoBehaviour
    {
        class Baker : Baker<DestinationAuthoring>
        {
            public override void Bake(DestinationAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Destination{});
            }
        }
    }

    public struct Destination : IComponentData
    {
        public int2 DestinationIndex;
    }

    // 实时更新目标位置的系统
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct UpdateDestinationSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (destination, transform) in SystemAPI.Query<RefRW<Destination>, RefRO<LocalTransform>>())
            {
                var position = transform.ValueRO.Position;
                destination.ValueRW.DestinationIndex = new int2((int)position.x, (int)position.z);
            }
        }
    }
}
