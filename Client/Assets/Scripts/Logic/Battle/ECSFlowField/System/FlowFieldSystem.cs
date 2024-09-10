using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Battle
{
    public partial struct FlowFieldSystem : ISystem
    {

        private NativeHashMap<int2, Cell> _cellMap;

        public void OnCreate(ref SystemState state)
        {
            int count = BattleConfig.CELL_WIDTH * BattleConfig.CELL_HIEGHT;
            _cellMap = new NativeHashMap<int2, Cell>(count, Allocator.Persistent);
        }

        public void OnDestroy(ref SystemState state)
        {
            if (_cellMap.IsCreated)
            {
                _cellMap.Dispose();
            }
        }

        /// <summary>
        /// TODO 是否需要修改成多线程
        /// </summary>
        /// <param name="state"></param>
        public void OnUpdate(ref SystemState state)
        {
            Cell destination = new Cell(); new int2(10, 10); // 示例目标点

            NativeQueue<Cell> cellQueue = new NativeQueue<Cell>(Allocator.Temp);
            cellQueue.Enqueue(destination);

            while (cellQueue.Count > 0)
            {
                Cell curCell = cellQueue.Dequeue();
                var neighbors = GetNeighborCells(curCell);
                for(int i = 0; i < neighbors.Length; i++)
                {
                    var neighbor = neighbors[i];
                    if (neighbor.Cost == byte.MaxValue) { continue; }
                    if (neighbor.Cost + curCell.BestCost < neighbor.BestCost)
                    {
                        neighbor.BestCost = (ushort)(neighbor.Cost + curCell.BestCost);
                        cellQueue.Enqueue(neighbor);
                    }
                }
            }

            cellQueue.Dispose();
        }

        private NativeArray<Cell> GetNeighborCells(Cell cell)
        {
            NativeList<Cell> neighbors = new NativeList<Cell>(4, Allocator.Temp);

            foreach (var direction in GridDirection.CardinalDirections)
            {
                int2 neighborPos = cell.GridIndex + direction;
                if (_cellMap.TryGetValue(neighborPos, out Cell neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors.ToArray(Allocator.Temp);
        }

    }
}