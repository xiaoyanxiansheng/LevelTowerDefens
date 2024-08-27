using Unity.Burst;
using Unity.Entities;
using UnityEditor.Rendering;
using UnityEngine;

public partial struct AgentSpawnSystem : ISystem
{
    private int _InstanceIndex;

    public bool _isCreated;
    public float SwapTime;
    private float SwapTimePassTime;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _InstanceIndex = 0;

        SwapTime = 0.01f;

        _isCreated = false;

         state.RequireForUpdate<MonsterAgent>();
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //if (_isCreated) return;
        //_isCreated = true;

        SwapTimePassTime += Time.deltaTime;
        if (SwapTimePassTime > SwapTime)
        {
            SwapTimePassTime = 0;
            var config = SystemAPI.GetSingleton<MonsterAgent>();
            var entity = state.EntityManager.Instantiate(config.Prefab);
            state.EntityManager.AddComponentData<AgentComponent>(entity, new AgentComponent() { Id = _InstanceIndex++ });
        }
    }
}

public struct AgentComponent : IComponentData
{
    public int Id;
}

public struct AgentSpawned : IComponentData
{

}