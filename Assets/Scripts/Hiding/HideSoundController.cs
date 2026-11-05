using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class HideSoundController : NetworkBehaviour
{
[Header("Sound Settings")]
[SerializeField] private AudioClip hidingSound;
[SerializeField] private float minimumInterval = 10f;
[SerializeField] private float maximumInterval = 25f;

[Header("3D Sound Settings")]
[SerializeField] private float minimumDistance = 2f;
[SerializeField] private float maximumDistance = 50f;

private AudioSource audioSource;
private PlayerHideController hideController;
private Coroutine soundRoutine;

public override void OnNetworkSpawn()
{
    audioSource = GetComponent<AudioSource>();
    hideController = GetComponent<PlayerHideController>();

    audioSource.playOnAwake = false;
    audioSource.spatialBlend = 1f;
    audioSource.rolloffMode = AudioRolloffMode.Linear;
    audioSource.minDistance = minimumDistance;
    audioSource.maxDistance = maximumDistance;

    if (IsServer)
        soundRoutine = StartCoroutine(MonitorHiding());
}

private IEnumerator MonitorHiding()
{
    while (true)
    {
        if (hideController.IsHiding)
        {
            float waitTime = Random.Range(minimumInterval, maximumInterval);
            yield return new WaitForSeconds(waitTime);

            if (hideController.IsHiding)
                PlaySoundClientRpc();
        }
        else
        {
            yield return null;
        }
    }
}

[ClientRpc]
private void PlaySoundClientRpc()
{
    if (hidingSound == null)
        return;

    audioSource.PlayOneShot(hidingSound);
}

public override void OnNetworkDespawn()
{
    if (soundRoutine != null)
    {
        StopCoroutine(soundRoutine);
        soundRoutine = null;
    }
}

}