//using Unity.Burst;
//using Unity.Entities;
//using Unity.Mathematics;
//using UnityEngine;

//namespace TMG.FlowFieldECS
//{
//    [BurstCompile]
//    [UpdateAfter(typeof(GridInitializationSystem))]
//    public partial struct GridInteractionSystem : ISystem
//    {
//        private Camera mainCamera;
//        private EndSimulationEntityCommandBufferSystem commandBufferSystem;

//        [BurstCompile]
//        public void OnCreate(ref SystemState state)
//        {
//            mainCamera = Camera.main;
//            commandBufferSystem = state.World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
//        }

//        [BurstCompile]
//        public void OnUpdate(ref SystemState state)
//        {
//            if (Input.GetMouseButtonDown(0))
//            {
//                HandleLeftClick(ref state);
//            }
//            else if (Input.GetMouseButtonDown(1))
//            {
//                HandleRightClick(ref state);
//            }
//        }

//        private void HandleLeftClick(ref SystemState state)
//        {
//            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
//            if (Physics.Raycast(ray, out RaycastHit hit))
//            {
//                float3 hitPosition = hit.point;
//                Entity clickedEntity = GetEntityAtPosition(ref state, hitPosition);

//                if (clickedEntity != Entity.Null)
//                {
//                    var ecb = commandBufferSystem.CreateCommandBuffer();
//                    ecb.AddComponent(clickedEntity, new DestinationComponent
//                    {
//                        DestinationIndex = GetGridIndexFromPosition(hitPosition)
//                    });
//                }
//            }
//        }

//        private void HandleRightClick(ref SystemState state)
//        {
//            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
//            if (Physics.Raycast(ray, out RaycastHit hit))
//            {
//                float3 hitPosition = hit.point;
//                Entity clickedEntity = GetEntityAtPosition(ref state, hitPosition);

//                if (clickedEntity != Entity.Null)
//                {
//                    var ecb = commandBufferSystem.CreateCommandBuffer();
//                    ecb.SetComponent(clickedEntity, new Cell
//                    {
//                        GridIndex = GetGridIndexFromPosition(hitPosition),
//                        Cost = byte.MaxValue
//                    });
//                }
//            }
//        }

//        private Entity GetEntityAtPosition(ref SystemState state, float3 position)
//        {
//            Entity closestEntity = Entity.Null;
//            float closestDistance = float.MaxValue;

//            foreach (var (positionComponent, entity) in SystemAPI.Query<RefRO<PositionComponent>>().WithEntityAccess())
//            {
//                float distance = math.distance(positionComponent.ValueRO.Position, position);
//                if (distance < closestDistance)
//                {
//                    closestDistance = distance;
//                    closestEntity = entity;
//                }
//            }

//            return closestEntity;
//        }

//        private int2 GetGridIndexFromPosition(float3 position)
//        {
//            return new int2((int)math.floor(position.x), (int)math.floor(position.z));
//        }
//    }
//}
