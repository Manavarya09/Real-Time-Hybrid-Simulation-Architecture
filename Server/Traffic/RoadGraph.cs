using System.Text.Json.Serialization;

namespace NeuroCity.Server.Traffic;

public class RoadGraph
{
    [JsonPropertyName("nodes")]
    public List<RoadNode> Nodes { get; set; } = new();

    private readonly Dictionary<string, RoadNode> _nodeMap = new();

    public void AddNode(RoadNode node)
    {
        if (!_nodeMap.ContainsKey(node.Id))
        {
            _nodeMap[node.Id] = node;
            Nodes.Add(node);
        }
    }

    public RoadNode? GetNode(string id)
    {
        return _nodeMap.TryGetValue(id, out var node) ? node : null;
    }

    public RoadNode? GetNodeAt(float x, float z, float tolerance = 1f)
    {
        foreach (var node in Nodes)
        {
            if (MathF.Abs(node.X - x) < tolerance && MathF.Abs(node.Z - z) < tolerance)
            {
                return node;
            }
        }
        return null;
    }

    public void GenerateGridRoadNetwork(float gridSize, int gridWidth, int gridHeight, float spacing)
    {
        var offsetX = -(gridWidth * spacing) / 2f;
        var offsetZ = -(gridHeight * spacing) / 2f;
        var nodeGrid = new RoadNode[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                var node = new RoadNode(offsetX + x * spacing, offsetZ + z * spacing);
                nodeGrid[x, z] = node;
                AddNode(node);
            }
        }

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                var node = nodeGrid[x, z];

                if (x > 0)
                    node.AddNeighbor(nodeGrid[x - 1, z]);
                if (x < gridWidth - 1)
                    node.AddNeighbor(nodeGrid[x + 1, z]);
                if (z > 0)
                    node.AddNeighbor(nodeGrid[x, z - 1]);
                if (z < gridHeight - 1)
                    node.AddNeighbor(nodeGrid[x, z + 1]);
            }
        }

        foreach (var node in Nodes)
        {
            node.NeighborIds = node.Neighbors.Select(n => n.Id).ToList();
        }

        Console.WriteLine($"[RoadGraph] Generated {Nodes.Count} road nodes with grid network");
    }

    public List<RoadNode> GetRandomNodes(int count)
    {
        var random = new Random();
        var shuffled = Nodes.OrderBy(_ => random.Next()).Take(count).ToList();
        return shuffled;
    }

    public RoadNode? GetRandomNode()
    {
        if (Nodes.Count == 0) return null;
        var random = new Random();
        return Nodes[random.Next(Nodes.Count)];
    }
}
