using System.Text.Json.Serialization;
using NeuroCity.Server.Entities;

namespace NeuroCity.Server.Player;

public class PlayerController
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }

    [JsonPropertyName("z")]
    public float Z { get; set; }

    [JsonPropertyName("rotation")]
    public float Rotation { get; set; }

    [JsonPropertyName("velocityX")]
    public float VelocityX { get; set; }

    [JsonPropertyName("velocityZ")]
    public float VelocityZ { get; set; }

    [JsonPropertyName("isGrounded")]
    public bool IsGrounded { get; set; } = true;

    [JsonIgnore]
    public float MoveSpeed { get; set; } = 30f;

    [JsonIgnore]
    public float SprintMultiplier { get; set; } = 1.8f;

    [JsonIgnore]
    public float JumpForce { get; set; } = 15f;

    [JsonIgnore]
    public float Gravity { get; set; } = -40f;

    [JsonIgnore]
    public float MouseSensitivity { get; set; } = 0.002f;

    [JsonIgnore]
    public float Pitch { get; set; }

    [JsonIgnore]
    public float Yaw { get; set; }

    private bool _isSprinting;
    private float _verticalVelocity;

    public PlayerController()
    {
        Id = Guid.NewGuid().ToString();
        X = 0;
        Y = 2;
        Z = 50;
    }

    public void SetInput(float moveForward, float moveRight, bool sprint, bool jump, float deltaYaw, float deltaPitch)
    {
        _isSprinting = sprint;

        Yaw += deltaYaw;
        Pitch = Math.Clamp(Pitch + deltaPitch, -MathF.PI / 2.2f, MathF.PI / 2.2f);

        var speed = _isSprinting ? MoveSpeed * SprintMultiplier : MoveSpeed;

        VelocityX = (MathF.Sin(Yaw) * moveForward + MathF.Cos(Yaw) * moveRight) * speed;
        VelocityZ = (MathF.Cos(Yaw) * moveForward - MathF.Sin(Yaw) * moveRight) * speed;

        if (jump && IsGrounded)
        {
            _verticalVelocity = JumpForce;
            IsGrounded = false;
        }
    }

    public void Update(float deltaTime, float groundLevel = 0)
    {
        _verticalVelocity += Gravity * deltaTime;

        X += VelocityX * deltaTime;
        Y += _verticalVelocity * deltaTime;
        Z += VelocityZ * deltaTime;

        if (Y <= groundLevel + 2)
        {
            Y = groundLevel + 2;
            _verticalVelocity = 0;
            IsGrounded = true;
        }

        Rotation = Yaw;
    }
}
