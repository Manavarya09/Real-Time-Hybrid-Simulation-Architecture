using System.Collections.Generic;

namespace NeuroCity.Server.Traffic;

public class PathNode
{
    public RoadNode Node { get; set; } = null!;
    public float GCost { get; set; }
    public float HCost { get; set; }
    public float FCost => GCost + HCost;
    public PathNode? Parent { get; set; }

    public PathNode(RoadNode node)
    {
        Node = node;
    }
}

public class Pathfinding
{
    private readonly RoadGraph _graph;

    public Pathfinding(RoadGraph graph)
    {
        _graph = graph;
    }

    public List<RoadNode>? FindPath(RoadNode start, RoadNode goal)
    {
        if (start == null || goal == null) return null;
        if (start.Id == goal.Id) return new List<RoadNode> { start };

        var openSet = new SortedSet<PathNode>(
            Comparer<PathNode>.Create((a, b) => 
            {
                var fCompare = a.FCost.CompareTo(b.FCost);
                if (fCompare != 0) return fCompare;
                return a.Node.Id.CompareTo(b.Node.Id);
            })
        );

        var closedSet = new HashSet<string>();
        var pathNodes = new Dictionary<string, PathNode>();

        var startPathNode = new PathNode(start) { GCost = 0, HCost = Heuristic(start, goal) };
        openSet.Add(startPathNode);
        pathNodes[start.Id] = startPathNode;

        while (openSet.Count > 0)
        {
            var current = openSet.Min!;
            openSet.Remove(current);

            if (current.Node.Id == goal.Id)
            {
                return ReconstructPath(current);
            }

            closedSet.Add(current.Node.Id);

            foreach (var neighbor in current.Node.Neighbors)
            {
                if (closedSet.Contains(neighbor.Id)) continue;

                var tentativeGCost = current.GCost + current.Node.DistanceTo(neighbor);

                if (!pathNodes.TryGetValue(neighbor.Id, out var neighborPathNode))
                {
                    neighborPathNode = new PathNode(neighbor);
                    pathNodes[neighbor.Id] = neighborPathNode;
                    openSet.Add(neighborPathNode);
                }

                if (tentativeGCost < neighborPathNode.GCost)
                {
                    neighborPathNode.Parent = current;
                    neighborPathNode.GCost = tentativeGCost;
                    neighborPathNode.HCost = Heuristic(neighbor, goal);

                    openSet.Remove(neighborPathNode);
                    openSet.Add(neighborPathNode);
                }
            }
        }

        return null;
    }

    private float Heuristic(RoadNode a, RoadNode b)
    {
        return a.DistanceTo(b);
    }

    private List<RoadNode> ReconstructPath(PathNode endNode)
    {
        var path = new List<RoadNode>();
        var current = endNode;

        while (current != null)
        {
            path.Add(current.Node);
            current = current.Parent;
        }

        path.Reverse();
        return path;
    }
}
