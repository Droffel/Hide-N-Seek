using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCombatController : NetworkBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackAngle = 30f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Aim Settings")]
    [SerializeField] private Transform aimTransform;

    private IPlayerInputSource inputSource;
    private PlayerHealth playerHealth;
    private float nextAttackTime;
    private SeekerPreparationController preparationController;

    private void Awake()
    {
        preparationController = GetComponent<SeekerPreparationController>();

        inputSource = GetComponent<IPlayerInputSource>();
        playerHealth = GetComponent<PlayerHealth>();

    }

    private  void Update()
    {
        if(!IsOwner)
        {
            return;
        }

        if(inputSource == null || playerHealth == null)
        {
            return;
        }

        if(!playerHealth.IsAlive)
        {
            return;
        }

        if(inputSource.AttackPressed)
        {
            RequestAttackRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestAttackRpc(RpcParams rpcParams = default)
    {
        if(!IsServer)
        {
            return;
        }

        PlayerRole attackerRole = GetComponent<PlayerRole>();

        if (attackerRole == null || !attackerRole.IsSeeker)
        {
            Debug.Log("Attack rejected: only seekers can attack.");
            return;
        }
        if (preparationController != null && preparationController.IsPreparing)
        {
            return;
        }

        if(rpcParams.Receive.SenderClientId != OwnerClientId)
        {
            return;
        }
        if(playerHealth == null || !playerHealth.IsAlive)
        {
            return;
        }

        if(Time.time < nextAttackTime)
        {
            return;
        }
        nextAttackTime  = Time.time + attackCooldown;

        PlayerHealth target = FindTarget();

        if(target == null)
        {
            Debug.Log("Attack missed. ");
            return;
        }

        if(target.TryKill())
        {
            Debug.Log(
                "Player " + OwnerClientId + 
                " eliminated player " + target.OwnerClientId
            );
        }
    }
    
private PlayerHealth FindTarget()
{
    PlayerHealth closestTarget = null;
    float closestDistance = attackRange;

    Vector3 attackDirection = aimTransform != null ? aimTransform.forward.normalized : transform.forward.normalized;
  

    PlayerHealth[] targets =
        FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);

    Debug.Log(
        $"[Combat] Attacker {OwnerClientId}: " +
        $"Found {targets.Length} PlayerHealth objects."
    );

    foreach (PlayerHealth target in targets)
    {
        if (target == null || !target.IsSpawned)
        {
            continue;
        }

        Debug.Log(
            $"[Combat] Checking target. " +
            $"Spawned: {target.IsSpawned}, " +
            $"Alive: {target.IsAlive}, " +
            $"Position: {target.transform.position}"
        );

        if (!target.IsSpawned)
        {
            Debug.Log("[Combat] Target rejected: not network-spawned.");
            continue;
        }

        if (target == playerHealth)
        {
            Debug.Log("[Combat] Target rejected: this is the attacker.");
            continue;
        }

        if (!target.IsAlive)
        {
            Debug.Log("[Combat] Target rejected: already eliminated.");
            continue;
        }

        Vector3 targetPosition = target.transform.position + Vector3.up * 0.2f;

        Vector3 direction =
            targetPosition - aimTransform.position;


        float distance = direction.magnitude;

        Debug.Log($"[Combat] Target distance: {distance:F2}");

        if (distance > attackRange || distance <= 0.001f)
        {
            Debug.Log("[Combat] Target rejected: outside attack range.");
            continue;
        }

        direction.Normalize();

        float angle = Vector3.Angle(attackDirection, direction.normalized);

        Debug.Log($"[Combat] Target angle: {angle:F2}");

        if (angle > attackAngle * 0.5f)
        {
            Debug.Log("[Combat] Target rejected: outside attack angle.");
            continue;
        }
        if(distance < closestDistance)
        {
            closestDistance = distance;
            closestTarget = target;
        }
    }

    

    if (closestTarget == null)
    {
        Debug.Log("[Combat] No valid target found.");
    }
    else
    {
        Debug.Log(
            $"[Combat] Target found: Client {closestTarget.OwnerClientId}"
        );
    }

    return closestTarget;
}

}