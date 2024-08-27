using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst.Intrinsics;

[BurstCompile]
public partial struct AgentMoveSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MonsterAgent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var agentQuery = SystemAPI.QueryBuilder().WithAll<MonsterAgent, LocalTransform>().Build();

        var job = new AgentMovementJob
        {
            TranslationTypeHandle = SystemAPI.GetComponentTypeHandle<LocalTransform>(),
            AgentComponentTypeHandle = SystemAPI.GetComponentTypeHandle<MonsterAgent>(true),
            DeltaTime = SystemAPI.Time.DeltaTime
        };

        state.Dependency = job.Schedule(agentQuery, state.Dependency);
    }
}

[BurstCompile]
struct AgentMovementJob : IJobChunk
{
    public ComponentTypeHandle<LocalTransform> TranslationTypeHandle;
    [ReadOnly] public ComponentTypeHandle<MonsterAgent> AgentComponentTypeHandle;
    public float DeltaTime;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        var translations = chunk.GetNativeArray<LocalTransform>(ref TranslationTypeHandle);
        var agents = chunk.GetNativeArray<MonsterAgent>(ref AgentComponentTypeHandle);

        for (int i = 0; i < chunk.Count; i++)
        {
            translations[i] = new LocalTransform
            {
                //Position = AgentPositions[unfilteredChunkIndex + i],
                //Rotation = quaternion.identity,
                //Scale = 1f
            };
        }
    }
}
