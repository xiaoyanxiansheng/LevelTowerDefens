
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TMG.FlowFieldECS
{
    public class OcclusionAuthoring : MonoBehaviour
    {
        class Baker : Baker<OcclusionAuthoring>
        {
            public override void Bake(OcclusionAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                // 创建变换矩阵
                Matrix4x4 matrix = authoring.transform.localToWorldMatrix;

                // 将矩阵存储到Occlusion组件中
                AddComponent(entity, new Occlusion());
            }
        }
    }

    public struct Occlusion : IComponentData
    {
    }
}
