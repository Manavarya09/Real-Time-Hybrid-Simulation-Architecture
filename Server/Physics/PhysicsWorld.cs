using System.Numerics;

namespace NeuroCity.Server.Physics;

public struct BoundingBox
{
    public Vector3 Min { get; set; }
    public Vector3 Max { get; set; }

    public BoundingBox(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    public bool Intersects(BoundingBox other)
    {
        return Min.X <= other.Max.X && Max.X >= other.Min.X &&
               Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
               Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;
    }

    public bool Contains(Vector3 point)
    {
        return point.X >= Min.X && point.X <= Max.X &&
               point.Y >= Min.Y && point.Y <= Max.Y &&
               point.Z >= Min.Z && point.Z <= Max.Z;
    }

    public Vector3 Center => (Min + Max) / 2f;
    public Vector3 Size => Max - Min;
}

public class PhysicsBody
{
    public string Id { get; set; } = string.Empty;
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 Acceleration { get; set; }
    public float Mass { get; set; } = 1f;
    public float Radius { get; set; } = 1f;
    public BoundingBox BoundingBox { get; set; }
    public bool IsStatic { get; set; }
    public bool IsEnabled { get; set; } = true;

    public PhysicsBody()
    {
        Id = Guid.NewGuid().ToString();
    }

    public void UpdateBoundingBox(Vector3 size)
    {
        var center = Position;
        BoundingBox = new BoundingBox(
            center - size / 2f,
            center + size / 2f
        );
    }
}

public class CollisionResult
{
    public bool HasCollision { get; set; }
    public string BodyAId { get; set; } = string.Empty;
    public string BodyBId { get; set; } = string.Empty;
    public Vector3 ContactPoint { get; set; }
    public Vector3 Normal { get; set; }
    public float PenetrationDepth { get; set; }
}

public class PhysicsWorld
{
    private readonly Dictionary<string, PhysicsBody> _bodies = new();
    private readonly List<CollisionResult> _collisionResults = new();
    private readonly float _gravity = -20f;
    private float _fixedDeltaTime = 1f / 60f;

    public List<CollisionResult> CollisionResults => _collisionResults;

    public void Initialize()
    {
        Console.WriteLine("[PhysicsWorld] Initialized");
    }

    public PhysicsBody AddBody(string id, Vector3 position, Vector3 size, bool isStatic = false)
    {
        var body = new PhysicsBody
        {
            Id = id,
            Position = position,
            IsStatic = isStatic
        };
        body.UpdateBoundingBox(size);
        
        _bodies[id] = body;
        return body;
    }

    public void RemoveBody(string id)
    {
        _bodies.Remove(id);
    }

    public PhysicsBody? GetBody(string id)
    {
        return _bodies.TryGetValue(id, out var body) ? body : null;
    }

    public void Update(float deltaTime)
    {
        _collisionResults.Clear();

        foreach (var body in _bodies.Values)
        {
            if (!body.IsEnabled || body.IsStatic) continue;

            body.Acceleration = new Vector3(0, _gravity, 0);
            body.Velocity += body.Acceleration * deltaTime;
            body.Position += body.Velocity * deltaTime;

            if (body.Position.Y < 0)
            {
                body.Position.Y = 0;
                body.Velocity = Vector3.Zero;
            }

            body.UpdateBoundingBox(body.BoundingBox.Size);
        }

        DetectCollisions();
    }

    private void DetectCollisions()
    {
        var bodyList = _bodies.Values.ToList();
        
        for (int i = 0; i < bodyList.Count; i++)
        {
            for (int j = i + 1; j < bodyList.Count; j++)
            {
                var bodyA = bodyList[i];
                var bodyB = bodyList[j];

                if (!bodyA.IsEnabled || !bodyB.IsEnabled) continue;
                if (bodyA.IsStatic && bodyB.IsStatic) continue;

                if (bodyA.BoundingBox.Intersects(bodyB.BoundingBox))
                {
                    var result = new CollisionResult
                    {
                        HasCollision = true,
                        BodyAId = bodyA.Id,
                        BodyBId = bodyB.Id,
                        ContactPoint = (bodyA.BoundingBox.Center + bodyB.BoundingBox.Center) / 2f
                    };

                    var overlap = CalculateOverlap(bodyA.BoundingBox, bodyB.BoundingBox);
                    result.PenetrationDepth = overlap.Length();
                    result.Normal = Vector3.Normalize(overlap);

                    _collisionResults.Add(result);
                    ResolveCollision(bodyA, bodyB, result);
                }
            }
        }
    }

    private Vector3 CalculateOverlap(BoundingBox a, BoundingBox b)
    {
        var overlapX = Math.Min(a.Max.X - b.Min.X, b.Max.X - a.Min.X);
        var overlapY = Math.Min(a.Max.Y - b.Min.Y, b.Max.Y - a.Min.Y);
        var overlapZ = Math.Min(a.Max.Z - b.Min.Z, b.Max.Z - a.Min.Z);

        if (overlapX < overlapY && overlapX < overlapZ)
            return new Vector3(overlapX, 0, 0);
        else if (overlapY < overlapZ)
            return new Vector3(0, overlapY, 0);
        else
            return new Vector3(0, 0, overlapZ);
    }

    private void ResolveCollision(PhysicsBody a, PhysicsBody b, CollisionResult result)
    {
        if (a.IsStatic && b.IsStatic) return;

        var normal = result.Normal;
        var correction = normal * result.PenetrationDepth * 0.5f;

        if (a.IsStatic)
        {
            b.Position += correction;
        }
        else if (b.IsStatic)
        {
            a.Position -= correction;
        }
        else
        {
            a.Position -= correction;
            b.Position += correction;
        }

        var relativeVelocity = a.Velocity - b.Velocity;
        var velocityAlongNormal = Vector3.Dot(relativeVelocity, normal);

        if (velocityAlongNormal > 0) return;

        var restitution = 0.3f;
        var impulse = -(1 + restitution) * velocityAlongNormal;
        impulse /= (1 / a.Mass + 1 / b.Mass);

        var impulseVector = normal * impulse;

        if (!a.IsStatic)
            a.Velocity += impulseVector / a.Mass;
        if (!b.IsStatic)
            b.Velocity -= impulseVector / b.Mass;
    }

    public bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, out RaycastHit hit)
    {
        hit = new RaycastHit();
        
        var normalizedDir = Vector3.Normalize(direction);
        float closestDist = maxDistance;
        PhysicsBody? closestBody = null;

        foreach (var body in _bodies.Values)
        {
            if (!body.IsEnabled) continue;

            var rayHit = RayBoxIntersection(origin, normalizedDir, body.BoundingBox);
            if (rayHit.HasValue && rayHit.Value < closestDist)
            {
                closestDist = rayHit.Value;
                closestBody = body;
            }
        }

        if (closestBody != null)
        {
            hit.HasHit = true;
            hit.Distance = closestDist;
            hit.Position = origin + normalizedDir * closestDist;
            hit.BodyId = closestBody.Id;
            return true;
        }

        return false;
    }

    private float? RayBoxIntersection(Vector3 origin, Vector3 direction, BoundingBox box)
    {
        var tMin = (box.Min - origin) / direction;
        var tMax = (box.Max - origin) / direction;

        var t1 = Vector3.Min(tMin, tMax);
        var t2 = Vector3.Max(tMin, tMax);

        var tNear = MathF.Max(MathF.Max(t1.X, t1.Y), t1.Z);
        var tFar = MathF.Min(MathF.Min(t2.X, t2.Y), t2.Z);

        if (tNear > tFar || tFar < 0) return null;

        return tNear > 0 ? tNear : tFar;
    }

    public List<PhysicsBody> GetBodiesInRadius(Vector3 center, float radius)
    {
        var result = new List<PhysicsBody>();
        var radiusSquared = radius * radius;

        foreach (var body in _bodies.Values)
        {
            var distSquared = Vector3.DistanceSquared(body.Position, center);
            if (distSquared <= radiusSquared)
            {
                result.Add(body);
            }
        }

        return result;
    }

    public void Shutdown()
    {
        _bodies.Clear();
        _collisionResults.Clear();
        Console.WriteLine("[PhysicsWorld] Shutdown");
    }
}

public struct RaycastHit
{
    public bool HasHit { get; set; }
    public float Distance { get; set; }
    public Vector3 Position { get; set; }
    public string BodyId { get; set; }
}
