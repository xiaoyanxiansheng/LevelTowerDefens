/*
    建立 ECS 和 OECA之间的数据桥梁
 */
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Unity.Transforms;
using Unity.Burst.Intrinsics;
using Unity.Assertions;
using Nebukam.ORCA;
using static UnityEngine.EventSystems.EventTrigger;

[BurstCompile]
public partial struct AgentEcsOrcaSystem : ISystem
{
    private NativeHashMap<int, float3> _agentIdToEntityPosition;

    public void OnCreate(ref SystemState state)
    {
        _agentIdToEntityPosition = new NativeHashMap<int, float3>(100, Allocator.Persistent);
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (transform, agent, entity) in 
            SystemAPI.Query<LocalTransform, AgentComponent>().WithNone<AgentSpawned>().WithEntityAccess())
        {
            OrcaManagerSetup.Instance.AddAgent(agent.Id, float3(0, 0, 0));
            ecb.AddComponent<AgentSpawned>(entity);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        OrcaManagerSetup.Instance.FetchAgentsPosition(ref _agentIdToEntityPosition);
        var entityQuery = SystemAPI.QueryBuilder().WithAll<LocalTransform, AgentComponent, AgentSpawned>().Build();
        var job = new RotationJob
        {
            AgentIdToEntityPosition = _agentIdToEntityPosition,
            TransformTypeHandle = SystemAPI.GetComponentTypeHandle<LocalTransform>(),
            AgentTypeHandle = SystemAPI.GetComponentTypeHandle<AgentComponent>(),
        };
        state.Dependency = job.Schedule(entityQuery, state.Dependency);
    }

    [BurstCompile]
    struct RotationJob : IJobChunk
    {
        [ReadOnly] public NativeHashMap<int, float3> AgentIdToEntityPosition;
        [ReadOnly] public ComponentTypeHandle<AgentComponent> AgentTypeHandle;
        public ComponentTypeHandle<LocalTransform> TransformTypeHandle;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
            in v128 chunkEnabledMask)
        {
            Assert.IsFalse(useEnabledMask);

            var transforms = chunk.GetNativeArray(ref TransformTypeHandle);
            var agents = chunk.GetNativeArray(ref AgentTypeHandle);
            for (int i = 0, chunkEntityCount = chunk.Count; i < chunkEntityCount; i++)
            {
                var transform = transforms[i];
                var agent = agents[i];
                if (AgentIdToEntityPosition.ContainsKey(agent.Id))
                {
                    transform.Position = AgentIdToEntityPosition[agent.Id];
                    transforms[i] = transform;
                }
            }
        }
    }

    //public void UpdateAgentPositions(ref SystemState state)
    //{
    //    OrcaManagerSetup.Instance.FetchAgentsPosition(ref _agentIdToEntityPosition);

    //    foreach (var (transform, agent , bspawn, entity) in
    //        SystemAPI.Query<LocalTransform, AgentComponent , AgentSpawned>().WithEntityAccess())
    //    {
    //        if (_agentIdToEntityPosition.ContainsKey(agent.Id))
    //        {
    //            var trans = state.EntityManager.GetComponentData<LocalTransform>(entity);
    //            trans.Position = _agentIdToEntityPosition[agent.Id];
    //            state.EntityManager.SetComponentData<LocalTransform>(entity, trans);
    //        }
    //    }
    //}

    public void OnDestroy(ref SystemState state)
    {
        if (_agentIdToEntityPosition.IsCreated)
        {
            _agentIdToEntityPosition.Dispose();
        }
    }

}

public struct OrcaAgentData : IBufferElementData
{
    public int AgentID;
    public float3 Position;
}

public struct OrcaAgent
{
    public Entity Entity;
    public float3 Position;

    public OrcaAgent(Entity entity, float3 position)
    {
        Entity = entity;
        Position = position;
    }
}