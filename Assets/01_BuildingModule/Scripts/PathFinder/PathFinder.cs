using BuildingModule;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder 
{
    private const uint MOVE_DIAGONAL_COST = 14;
    private const uint MOVE_STRAIGHT_COST = 10;

    private readonly List<PathNode> m_OpenList = new();
    private readonly List<PathNode> m_CloseList = new();

    private readonly int[,] m_NeighborIndices =
{
        { 0, +1 }, /*{ +1, +1 },*/
        { +1, 0 }, /*{ +1, -1 },*/
        { 0, -1 }, /*{ -1, -1 },*/
        { -1, 0 }, /*{ -1, +1 }*/
    };

    public List<Vector3> FindPathOrNull(Vector3 startPos, Vector3 goalPos)
    { 
        m_OpenList.Clear();
        m_CloseList.Clear();

        var startCell = BuildingSystem.Instance.ConvertWorldToCell(startPos);
        var startTile = BuildingSystem.Instance.FindTileOrNull(startPos);
        if (startTile == null)
        {
            return null;
        }
        var startNode = new PathNode(startTile.Type, startCell.x, startCell.y);

        var goalCell = BuildingSystem.Instance.ConvertWorldToCell(goalPos);
        var goalTile = BuildingSystem.Instance.FindTileOrNull(goalPos);
        if (goalTile == null)
        {
            return null;
        }
        var goalNode = new PathNode(goalTile.Type, goalCell.x, goalCell.y);

        m_OpenList.Add(startNode);
        startNode.G = 0;
        startNode.H = CalculateHeuristic(startNode, goalNode);


        while (m_OpenList.Count > 0)
        {
            var currNode = GetPathNodeWithMinF(m_OpenList);
            if (currNode.Column == goalNode.Column && currNode.Row == goalNode.Row)
            {
                return CalculatePath(currNode);
            }
            m_OpenList.Remove(currNode);
            m_CloseList.Add(currNode);

            // 8방향 탐색 하기
            for (var i = 0; i < m_NeighborIndices.GetLength(0); ++i)
            {
                var neighborCol = currNode.Column + m_NeighborIndices[i, 0];
                var neighborRow = currNode.Row + m_NeighborIndices[i, 1];
                var neighborTile = BuildingSystem.Instance.FindTileOrNull(neighborCol, neighborRow);
                if (neighborTile == null)
                {
                    continue;
                }

                if (FindPathNodeOrNull(m_CloseList, neighborCol, neighborRow) != null)
                {
                    continue;
                }

                var neighborNode = new PathNode(neighborTile.Type, neighborCol, neighborRow);
                if (neighborTile.Type == CustomTile.TileType.Block)
                {
                    m_CloseList.Contains(neighborNode);
                    continue;
                }

                var tempG = currNode.G + 1;
                if (neighborNode.G == 0 || tempG < neighborNode.G)
                {
                    neighborNode.PrevNode = currNode;
                }

                neighborNode.G = tempG;
                neighborNode.H = CalculateHeuristic(neighborNode, goalNode);
                if (FindPathNodeOrNull(m_OpenList, neighborCol, neighborRow) == null)
                {
                    m_OpenList.Add(neighborNode);
                }

            }
        }

        return null;
    }

    private List<Vector3> CalculatePath(PathNode goalNode)
    {
        var path = new List<Vector3>();

        var currNode = goalNode;
        while (currNode != null)
        {
            var pos = BuildingSystem.Instance.ConvertCellToWorld(new Vector3Int(currNode.Column, currNode.Row));
            path.Add(pos);
            currNode = currNode.PrevNode;
        }
        path.Reverse();
        return path;

    }

    private int CalculateHeuristic(PathNode lhs, PathNode rhs)
    {
        var colDiff = (int)((lhs.Row < rhs.Row) ? (rhs.Row - lhs.Row) : (lhs.Row - rhs.Row));
        var rowDiff = (int)((lhs.Column < rhs.Column) ? (rhs.Column - lhs.Column) : (lhs.Column - rhs.Column));
        var diff = (colDiff < rowDiff) ? (rowDiff - colDiff) : (colDiff - rowDiff);
        diff -= (int)lhs.TileType + (int)rhs.TileType;

        return (int)(MOVE_DIAGONAL_COST * Mathf.Min(colDiff, rowDiff) + MOVE_STRAIGHT_COST * diff);
    }

    private PathNode GetPathNodeWithMinF(IReadOnlyList<PathNode> pathNodes)
    {
        var minPathNode = pathNodes[0];
        foreach (var pathNode in pathNodes)
        {
            if (minPathNode.F > pathNode.F)
            {
                minPathNode = pathNode;
            }
        }
        return minPathNode;
    }

    private PathNode FindPathNodeOrNull(IEnumerable<PathNode> pathNodes, int column, int row)
    {
        foreach (var pathNode in pathNodes)
        {
            if (pathNode.Column == column && pathNode.Row == row)
            {
                return pathNode;
            }
        }
        return null;
    }

}
