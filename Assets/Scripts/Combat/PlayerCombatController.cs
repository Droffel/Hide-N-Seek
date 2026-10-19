using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Analytics;

public class PlayerCombatController : NetworkBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackAngle = 100f;
    [SerializeField] private float attackCooldown = 1f;

    private IPlayerInputSource inputSource;
    private PlayerHealth playerHealth;
    private float nextAttackTime;

    private void Awake()
    {
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
        NetworkManager networkManager = NetworkManager.Singleton;

        if(networkManager == null)
        {
            return null;
        }
        PlayerHealth closestTarget = null;
        float closestDistance = attackRange;

        Vector3 attackDirection = transform.forward;
        attackDirection.y = 0f;

        attackDirection.Normalize();

        foreach(NetworkClient client in networkManager.ConnectedClientsList)
        {
            if(client.PlayerObject == null)
            {
                continue;
            }
            if(client.ClientId == OwnerClientId)
            {
                continue;
            }

            PlayerHealth target =
                client.PlayerObject.GetComponent<PlayerHealth>();

            if(target == null || !target.IsAlive)
            {
                continue;
            }
            Vector3 direction =
                target.transform.position - transform.position;
            
            direction.y = 0f;

            float distance = direction.magnitude;

            if(distance > closestDistance || distance <= 0.001f)
            {
                continue;
            }

            direction.Normalize();

            float angle = Vector3.Angle(
                attackDirection,
                direction
            );

            if(angle>attackAngle * 0.5f)
            {
                continue;
            }

            closestDistance = distance;
            closestTarget = target;
        }

        return closestTarget;
    }
}