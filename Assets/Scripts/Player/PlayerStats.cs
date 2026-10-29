using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerStats",
    menuName = "Game/Player Stats"
)]
public class PlayerStats : ScriptableObject
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Movement")]
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float airControl = 0.5f;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float maxLookAngle = 85f;

    
    public float MaxHealth => maxHealth;

    public float MoveSpeed => moveSpeed;
    public float Acceleration => acceleration;
    public float Deceleration => deceleration;
    public float AirControl => airControl;

    public float JumpHeight => jumpHeight;
    public float Gravity => gravity;

    public float MouseSensitivity => mouseSensitivity;
    public float MaxLookAngle => maxLookAngle;
}