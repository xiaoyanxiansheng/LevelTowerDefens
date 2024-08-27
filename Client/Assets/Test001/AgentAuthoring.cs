using Unity.Entities;
using UnityEngine;

public class AgentAuthoring : MonoBehaviour
{
    public GameObject Prefab;

    // In baking, this Baker will run once for every SpawnerAuthoring instance in a subscene.
    // (Note that nesting an authoring component's Baker class inside the authoring MonoBehaviour class
    // is simply an optional matter of style.)
    class Baker : Baker<AgentAuthoring>
    {
        public override void Bake(AgentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new MonsterAgent
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct MonsterAgent : IComponentData
{
    public Entity Prefab;
}
