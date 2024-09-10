using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TMG.FlowFieldECS
{
    public class ConfigAuthoring : MonoBehaviour
    {
        public Vector2Int gridSize = new Vector2Int(50, 50);
        public float cellRadius = 1f;
        public int2 desIndex;

        public InitializationSystem ShouldActivateSystem;

        class Baker : Baker<ConfigAuthoring>
        {
            public override void Bake(ConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ECSBootstrap()
                {
                    GridSize = new int2(authoring.gridSize.x, authoring.gridSize.y),
                    CellRadius = authoring.cellRadius,
                    DesIndex = authoring.desIndex
                });
            }
        }
    }

    public struct ECSBootstrap : IComponentData
    {
        public int2 GridSize;
        public float CellRadius;
        public int2 DesIndex;
    }
}
