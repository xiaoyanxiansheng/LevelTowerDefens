using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TMG.FlowFieldECS
{
    [BurstCompile]
    [UpdateAfter(typeof(InitializationSystem))]
    public partial struct UpdateSystem : ISystem
    {
        private EntityQuery m_GridQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ECSBootstrap>();

            // 定义查询以获取所有具有 Cell 组件的实体
            m_GridQuery = SystemAPI.QueryBuilder().WithAll<Cell, Position>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<ECSBootstrap>();

            // 获取网格数据
            var gridSize = config.GridSize;
            var destinationIndex = SystemAPI.GetSingleton<Destination>().DestinationIndex;
            // 使用临时数组来存储网格单元
            var cellArray = m_GridQuery.ToComponentDataArray<Cell>(Allocator.Temp);
            var positionArray = m_GridQuery.ToComponentDataArray<Position>(Allocator.Temp);
            var cellArrayEntity = m_GridQuery.ToEntityArray(Allocator.Temp); // 获取实体数组

            // 初始化
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    int index = y * gridSize.x + x;
                    var cell = cellArray[index];
                    cell.BestCost = ushort.MaxValue;
                    cell.BestDirection = CellDirection.None;
                    cell.Cost = 1;
                    foreach (var (localtoworld ,occlusion) in SystemAPI.Query<LocalToWorld, Occlusion>())
                    {
                        Matrix4x4 matrix = localtoworld.Value;
                        Matrix4x4 inverseMatrix = matrix.inverse; // 计算逆矩阵

                        float3 cellCenter = new float3((x + 0.5f) * config.CellRadius * 2, 0, (y + 0.5f) * config.CellRadius * 2);

                        // 将格子中心点转换到遮挡体的局部空间
                        float3 localCenter = inverseMatrix.MultiplyPoint3x4(cellCenter);
                        // 假设遮挡体的本地坐标范围是 [-0.5, 0.5]，检查格子中心点是否在这个范围内
                        if (math.abs(localCenter.x) <= 0.5f && math.abs(localCenter.z) <= 0.5f)
                        {
                            cell.Cost = byte.MaxValue;
                            break;
                        }
                    }
                    cellArray[index] = cell;
                }
            }

            // 创建队列用于 BFS 搜索
            NativeQueue<int2> cellsToCheck = new NativeQueue<int2>(Allocator.Temp);
            // 初始化：将目标单元格的成本设置为 0，并将其放入队列中
            int targetIndex = destinationIndex.y * gridSize.x + destinationIndex.x;
            var targetCell = cellArray[targetIndex];
            targetCell.BestCost = 0;
            cellArray[targetIndex] = targetCell; // 更新数组中的目标单元
            state.EntityManager.SetComponentData(cellArrayEntity[targetIndex], targetCell); // 更新 ECS 中的 Cell 组件
            cellsToCheck.Enqueue(destinationIndex);
            // BFS 搜索，处理每个单元
            while (cellsToCheck.Count > 0)
            {
                int2 currentIndex = cellsToCheck.Dequeue();
                int currentFlatIndex = currentIndex.y * gridSize.x + currentIndex.x;
                var currentCell = cellArray[currentFlatIndex];

                foreach (var direction in CellDirection.CardinalDirections)
                {
                    int2 neighborIndex = currentIndex + direction;
                    if (neighborIndex.x >= 0 && neighborIndex.x < gridSize.x &&
                        neighborIndex.y >= 0 && neighborIndex.y < gridSize.y)
                    {
                        int neighborFlatIndex = neighborIndex.y * gridSize.x + neighborIndex.x;
                        var neighborCell = cellArray[neighborFlatIndex];

                        if (neighborCell.Cost == byte.MaxValue) continue; // 跳过不可行走单元

                        int totalCost = neighborCell.Cost + currentCell.BestCost;
                        if (totalCost < neighborCell.BestCost)
                        {
                            neighborCell.BestCost = (ushort)totalCost;
                            neighborCell.BestDirection = direction;
                            cellArray[neighborFlatIndex] = neighborCell; // 更新数组中的邻居单元
                            state.EntityManager.SetComponentData(cellArrayEntity[neighborFlatIndex], neighborCell); // 更新 ECS 中的 Cell 组件
                            cellsToCheck.Enqueue(neighborIndex); // 将邻居单元加入队列
                        }
                    }
                }

                // 更新当前单元
                cellArray[currentFlatIndex] = currentCell;
                state.EntityManager.SetComponentData(cellArrayEntity[currentFlatIndex], currentCell); // 更新 ECS 中的 Cell 组件
            }

            // 释放临时分配
            cellsToCheck.Dispose();
            
            for (int i = 0; i < cellArray.Length; i++)
            {
                var cell = cellArray[i];
                var position = positionArray[i];
                var entity = cellArrayEntity[i];
                var BestCost = cell.BestCost;
                foreach (var direction in CellDirection.CardinalAndIntercardinalDirections)
                {
                    int2 neighborIndex = position.GridIndex + direction;
                    if (neighborIndex.x >= 0 && neighborIndex.x < gridSize.x &&
                        neighborIndex.y >= 0 && neighborIndex.y < gridSize.y)
                    {
                        int neighborFlatIndex = neighborIndex.y * gridSize.x + neighborIndex.x;
                        var neighbor = cellArray[neighborFlatIndex];
                        if(neighbor.BestCost < BestCost)
                        {
                            BestCost = neighbor.BestCost;
                            cell.BestDirection = direction;
                        }
                    } 
                }
                state.EntityManager.SetComponentData(entity, cell);
            }

            cellArray.Dispose();
            cellArrayEntity.Dispose();
        }
    }
}
