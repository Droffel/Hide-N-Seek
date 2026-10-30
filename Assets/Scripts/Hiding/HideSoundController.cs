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

    private AudioSource audioSource;
    private PlayerHideController hideController;

    private Coroutine soundRoutine;

    public override void OnNetworkSpawn()
    {
        audioSource = GetComponent<AudioSource>();

        hideController = GetComponent<PlayerHideController>();

        if (hideController == null)
        {
            Debug.LogError(
                "HideSoundController: PlayerHideController not found.",
                this
            );

            return;
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        hideController.OnHidingChanged += HandleHidingChanged;

        if (IsServer && hideController.IsHiding)
        {
            StartSoundRoutine();
        }
    }

    private void HandleHidingChanged(bool isHiding)
    {
        if (!IsServer)
        {
            return;
        }

        if (isHiding)
        {
            StartSoundRoutine();
        }
        else
        {
            StopSoundRoutine();
        }
    }

    private void StartSoundRoutine()
    {
        StopSoundRoutine();

        soundRoutine = StartCoroutine(PlayRandomSounds());
    }

    private void StopSoundRoutine()
    {
        if (soundRoutine != null)
        {
            StopCoroutine(soundRoutine);
            soundRoutine = null;
        }
    }

    private IEnumerator PlayRandomSounds()
    {
        while (hideController.IsHiding)
        {
            float waitTime = Random.Range(
                minimumInterval,
                maximumInterval
            );

            yield return new WaitForSeconds(waitTime);

            if (!hideController.IsHiding)
            {
                break;
            }

            PlaySoundClientRpc();
        }
    }

    [ClientRpc]
    private void PlaySoundClientRpc()
    {
        // The hiding player should not hear their own clue.
        if (IsOwner)
        {
            return;
        }

        if (hidingSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(hidingSound);
    }

    public override void OnNetworkDespawn()
    {
        if (hideController != null)
        {
            hideController.OnHidingChanged -= HandleHidingChanged;
        }

        StopSoundRoutine();
    }
}