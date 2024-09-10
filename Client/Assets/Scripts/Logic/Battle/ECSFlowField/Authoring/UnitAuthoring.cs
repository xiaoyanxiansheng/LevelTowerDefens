using JetBrains.Annotations;
using Unity.Entities;
using UnityEngine;

namespace TMG.FlowFieldECS
{
    public class UintAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public class Baker : Baker<UintAuthoring>
        {
            public override void Bake(UintAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,new Uint 
                {
                    Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic)
                });

            }
        }
    }

    public struct Uint : IComponentData
    {
        public Entity Prefab;
    }

    public struct MoveUint : IComponentData { }
}