using UnityEngine;
using System;
using System.Collections;
using Unity.Netcode;
using System.Reflection;

public class MatchTimer : NetworkBehaviour
{
    [SerializeField] private int matchDurationSeconds = 300;

    private readonly NetworkVariable<int> remainingSeconds =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public int RemainingSeconds => remainingSeconds.Value;

    public event Action<int> TimeChanged;
    public event Action TimeExpired;

    private Coroutine timerCoroutine;

    public override void OnNetworkSpawn()
    {
        remainingSeconds.OnValueChanged += OnTimeChanged;
    }

    public override void OnNetworkDespawn()
    {
        remainingSeconds.OnValueChanged -= OnTimeChanged;
    }

    public void StartTimer()
    {
        if (!IsServer)
        {
            return;
        }

        if(timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }

        remainingSeconds.Value = matchDurationSeconds;
        timerCoroutine = StartCoroutine(RunTimer());
    }

    public void StopTimer()
    {
        if (!IsServer)
        {
            return;
        }

        if(timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    private IEnumerator RunTimer()
    {
        while(remainingSeconds.Value > 0)
        {
            yield return new WaitForSeconds(1f);

            remainingSeconds.Value--;
        }

        timerCoroutine = null;
        TimeExpired?.Invoke();
    }

    private void OnTimeChanged(int oldTime, int newTime)
    {
        TimeChanged?.Invoke(newTime);
    }
}
