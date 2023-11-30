using UnityEngine;

public class SoundManager : MonoBehaviour {
    public AudioClip backgroundMusic;

    public AudioClip jumpSound;

    public AudioClip enemyDamageSound;
    public AudioClip playerDamageSound;
    public AudioClip jumpingEnemyJumpSound;
    public AudioClip targetingEnemyFocusSound;

    public AudioClip enemyDeathSound;
    public AudioClip playerDeathSound;
    public AudioClip playerDeathByFallingSound;
    public AudioClip levelCompletionSound;

    private AudioSource audioSource;
    private bool isSoundEffectOn = true;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
        PlayBackgroundMusic();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.M)) {
            if (isSoundEffectOn) {
                PauseBackgroundMusic();
            } else {
                ResumeBackgroundMusic();
            }
        }
    }

    public void PlayBackgroundMusic() {
        if (backgroundMusic != null) {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void PauseBackgroundMusic() {
        isSoundEffectOn = false;
        if (audioSource.isPlaying) {
            audioSource.Pause();
        }
    }

    public void ResumeBackgroundMusic() {
        isSoundEffectOn = true;
        if (!audioSource.isPlaying) {
            audioSource.Play();
        }
    }

    public void PlayJumpSound() {
        if (isSoundEffectOn) {
            if (jumpSound != null) {
                audioSource.PlayOneShot(jumpSound);
            }
        }
    }

    public void PlayEnemyDamageSound() {
        if (isSoundEffectOn) {
            if (enemyDamageSound != null) {
                audioSource.PlayOneShot(enemyDamageSound);
            }
        }
    }

    public void PlayPlayerDamageSound() {
        if (isSoundEffectOn) {
            if (enemyDamageSound != null) {
                audioSource.PlayOneShot(playerDamageSound);
            }
        }
    }

    public void PlayJumpingEnemyJumpSound() {
        if (isSoundEffectOn) {
            if (jumpingEnemyJumpSound != null) {
                audioSource.PlayOneShot(jumpingEnemyJumpSound);
            }
        }
    }

    public void PlayTargetingEnemyFocusSound() {
        if (isSoundEffectOn) {
            if (targetingEnemyFocusSound != null) {
                audioSource.PlayOneShot(targetingEnemyFocusSound);
            }
        }
    }

    public void StopTargetingEnemyFocusSound() {
        if (isSoundEffectOn) {
            if (audioSource.isPlaying && audioSource.clip == targetingEnemyFocusSound) {
                audioSource.Stop();
            }
        }
    }

    public void PlayEnemyDeathSound() {
        if (isSoundEffectOn) {
            if (enemyDeathSound != null) {
                audioSource.PlayOneShot(enemyDeathSound);
            }
        }
    }

    public void PlayPlayerDeathSound() {
        if (isSoundEffectOn) {
            if (playerDeathSound != null) {
                audioSource.PlayOneShot(playerDeathSound);
            }
        }
    }

    public void PlayPlayerDeathByFallingSound() {
        if (isSoundEffectOn) {
            if (playerDeathByFallingSound != null) {
                audioSource.PlayOneShot(playerDeathByFallingSound);
            }
        }
    }

    public void PlayLevelCompletionSound() {
        if (isSoundEffectOn) {
            if (levelCompletionSound != null) {
                audioSource.PlayOneShot(levelCompletionSound);
            }
        }
    }
}
