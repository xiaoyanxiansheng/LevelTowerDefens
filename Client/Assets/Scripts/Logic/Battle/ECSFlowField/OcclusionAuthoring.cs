
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
                // �����任����
                Matrix4x4 matrix = authoring.transform.localToWorldMatrix;

                // ������洢��Occlusion�����
                AddComponent(entity, new Occlusion());
            }
        }
    }

    public struct Occlusion : IComponentData
    {
    }
}
