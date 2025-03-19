using System;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
    public static GameManager gm;
    private GameObject player;
    public enum GameStates {Playing, Death, GameOver, BeatLevel};
    public GameStates gameState = GameStates.Playing;
    public int score = 0;
    public int playerHealth = 3;
    public int healthLimit = 3;
    public bool canBeatLevel = false;
    public int beatLevelScore = 0;
    public GameObject mainCanvas;
    public TMP_Text scoreText;
    public TMP_Text healthText;
    public GameObject WinText;
    public GameObject GameOverText;
    public GameObject PlayAgainButton;
    public GameObject NextLevelButton;
    public GameObject RestartGameButton;
    public AudioClip backgroundSFX;
    public AudioClip hitWall;
    public AudioClip hitEnemy;
    public AudioClip gameOverSFX;
    public AudioClip beatLevelSFX;
    public AudioClip healthSFX;
    public AudioClip speedSFX;
    public AudioClip doubleCoinSFX;
    public AudioClip shieldSFX;
    //public GameObject PauseCanvas;
    public GameObject[] spawnPoints;
    public float maxX=20, maxZ=20;
    public GameObject EnemySMPrefab;
    public GameObject EnemyCPrefab;
    public GameObject CoinPrefab;
    public GameObject SpeedPrefab;
    public GameObject HealthPrefab;
    public GameObject DoubleCoinPrefab;
    public GameObject ShieldPrefab;
    Vector3 playerSpawnLocation;
    GameObject cam;
    AudioSource audioSource;
    float originalSpeed = 8f;
    public bool pauseGame = false;
    void Awake() {
        if (gm == null) gm = this;
    }
    void Start() {
        pauseGame = false;
        cam = GameObject.FindGameObjectWithTag("MainCamera");
        audioSource = cam.GetComponent<AudioSource>();          // might not need
        playAudioRepeat(backgroundSFX);
        if (player == null) {
        player = GameObject.FindWithTag("Player");
        }
        if (player == null) Debug.LogError("Player not found in Game Manager");
        setupDefaults();
        InvokeRepeating("spawnStoneEnemy", 3f, 3f);
        // if it's level 2, new enemies will start to spawn at a slower rate
        // & new collectables
        if (SceneManager.GetActiveScene().name == "Level2") {
            InvokeRepeating("spawnCactusEnemy", 3f, 6f);
            InvokeRepeating("spawnSpeed", 10f, 10f);
            InvokeRepeating("spawnDoubleCoins", 15f, 20f);
            InvokeRepeating("spawnHealth", 20f, 20f);
            InvokeRepeating("spawnEnemyKiller", 15f, 15f);

        }
        InvokeRepeating("spanwCoins", 2f, 2f);
    }
    void setupDefaults() {
        GameOverText.SetActive(false);
        WinText.SetActive(false);
        PlayAgainButton.SetActive(false);
        NextLevelButton.SetActive(false);
        RestartGameButton.SetActive(false);
        //PauseCanvas.SetActive(false);
        playerSpawnLocation = player.transform.position;
        displayPlayerHealth();
    }    
    void displayPlayerHealth() {
        healthText.text = "Health: " + playerHealth.ToString();
    }

    public void playSound(int sound) {
        if (sound == 1) {
            playAudioOneTime(speedSFX);
        } else if (sound == 2) {
            playAudioOneTime(shieldSFX);
        } else {
            return;
        }
    }

    public void add_score(int amount) {
        score += amount;
        // if it's a double coin, play the special sound effect
        if (amount == 2) {
            playAudioOneTime(doubleCoinSFX);
        }
        if (canBeatLevel) {
            scoreText.text = "Score = " + score.ToString() + " of " + beatLevelScore.ToString();
            if (score >= beatLevelScore) {
                WinText.SetActive(true);
                PlayAgainButton.SetActive(true);
                // if it is level two, there will be button to restart the whole game
                if (SceneManager.GetActiveScene().name == "Level2") {
                    RestartGameButton.SetActive(true);
                } else {
                // if level 1, next level button will be there instead
                    NextLevelButton.SetActive(true);
                }
                audioSource.Stop();
                playAudioOneTime(beatLevelSFX);
                destroyAllEnemy();
                pauseGame = true;
                pause();
            }
        } else scoreText.text = "Score = " + score.ToString();
    }

    public void pause() {
        float enemySpeed = 0;
        GameObject[] enemy = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject g in enemy) {
            if (pauseGame) {
                originalSpeed = g.GetComponent<Chase>().speed;
                enemySpeed = 0;
            } else {
                enemySpeed = originalSpeed;
            }
            // Apply speed change
            g.GetComponent<Chase>().speed = enemySpeed;

            // Stop enemy movement physics
            Rigidbody enemyRb = g.GetComponent<Rigidbody>();
            if (enemyRb != null) {
                if (pauseGame) {
                    enemyRb.linearVelocity = Vector3.zero;
                    enemyRb.angularVelocity = Vector3.zero;
                    enemyRb.isKinematic = true; // Freeze movement
                } else {
                    enemyRb.isKinematic = false; // Unfreeze
                }
            }
        }

        // Stop player movement
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null) {
                if (pauseGame) {
                    playerRb.linearVelocity = Vector3.zero;
                    playerRb.angularVelocity = Vector3.zero;
                    playerRb.isKinematic = true;
                } else {
                    playerRb.isKinematic = false;
                }
            }
        }

        // Stop coin movement
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Point");
        foreach (GameObject coin in coins) {
            Rigidbody coinRb = coin.GetComponent<Rigidbody>();
            if (coinRb != null) {
                if (pauseGame) {
                    coinRb.linearVelocity = Vector3.zero;
                    coinRb.angularVelocity = Vector3.zero;
                    coinRb.isKinematic = true;
                } else {
                    coinRb.isKinematic = false;
                }
            }
        }
    }      
    
    public void decHealth(string collisionWith) {
        playerHealth -= 1;
        displayPlayerHealth();
        // if the health decreased bc of contact with a invisible wall, play a sound
        if (collisionWith == "Wall" & playerHealth != 0) {
            playAudioOneTime(hitWall);
        } else if (collisionWith == "Enemy" & playerHealth != 0) {
            playAudioOneTime(hitEnemy);
        }
        if (playerHealth == 0) {
            GameOverText.SetActive(true);
            PlayAgainButton.SetActive(true);
            gameState = GameStates.GameOver;
            audioSource.Stop();
            playAudioOneTime(gameOverSFX);
            pauseGame = true;
            pause();
        } else {
            destroyAllEnemy();
            player.transform.position = Vector3.zero;
        }
    }
    public void incHealth() {
        // health cannot go over the limit
        if (playerHealth < healthLimit){
            playerHealth+= 1;
            displayPlayerHealth();
            playAudioOneTime(healthSFX);
        }
    }

    void destroyAllEnemy() {
        GameObject[] enemy = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject g in enemy) {
            Destroy(g);
        }
    }
    // used when restarting the whole game or if you're in level 1 and wanna play level 1 again
    public void loadLevel1() {
        SceneManager.LoadScene("Level1");
    }
    public void loadLevel2() {
        SceneManager.LoadScene("Level2");
    }

    void spawnStoneEnemy() {
        if (pauseGame) return;
        int spawnPoint = UnityEngine.Random.Range(0, spawnPoints.Length);
        Instantiate(EnemySMPrefab, spawnPoints[spawnPoint].transform.position, Quaternion.identity);
    }
    void spawnCactusEnemy() {
        if (pauseGame) return;
        int spawnPoint = UnityEngine.Random.Range(0, spawnPoints.Length);
        GameObject enemy = Instantiate(EnemyCPrefab, spawnPoints[spawnPoint].transform.position, Quaternion.identity);
        // mark the enemy in the chase script
        Chase chaseScript = enemy.GetComponent<Chase>();
        if (chaseScript != null) {
            chaseScript.isCactusEnemy = true;
        }
    }

    void spanwCoins() {
        if (pauseGame) return;
        float randomX = UnityEngine.Random.Range(-maxX, maxX);
        float randomZ = UnityEngine.Random.Range(-maxZ, maxZ);
        Vector3 randomSpawnPos = new Vector3(randomX, 10f, randomZ);
        Instantiate(CoinPrefab, randomSpawnPos, Quaternion.identity);
    }
    void spawnDoubleCoins() {
        if (pauseGame) return;
        float randomX = UnityEngine.Random.Range(-maxX, maxX);
        float randomZ = UnityEngine.Random.Range(-maxZ, maxZ);
        Vector3 randomSpawnPos = new Vector3(randomX, 10f, randomZ);
        Instantiate(DoubleCoinPrefab, randomSpawnPos, Quaternion.identity);
    }

    // spawn collectables 
    void spawnSpeed() {
        if (pauseGame) return;
        float randomX = UnityEngine.Random.Range(-24.0f, -20.0f);
        float randomZ = UnityEngine.Random.Range(-15.5f, -11.5f);
        Vector3 randomSpawnPos = new Vector3(randomX, 8.5f, randomZ);
        Instantiate(SpeedPrefab, randomSpawnPos, Quaternion.identity);
    }

    void spawnHealth() {
        if (pauseGame) return;
        float randomX = UnityEngine.Random.Range(16.5f, 21.5f);
        float randomZ = UnityEngine.Random.Range(-10.5f, -6.5f);
        Vector3 randomSpawnPos = new Vector3(randomX, 10f, randomZ);
        Instantiate(HealthPrefab, randomSpawnPos, Quaternion.identity);
    }
    void spawnEnemyKiller() {
        if (pauseGame) return;
        float randomX = UnityEngine.Random.Range(-18.5f, -12.5f);
        float randomZ = UnityEngine.Random.Range(8.0f, 14.0f);
        Vector3 randomSpawnPos = new Vector3(randomX, 13.5f, randomZ);
        Instantiate(ShieldPrefab, randomSpawnPos, Quaternion.identity);
    }

    void playAudioOneTime(AudioClip clip) {
        AudioSource.PlayClipAtPoint(clip, cam.transform.position);
    }
    void playAudioRepeat(AudioClip clip) {
        audioSource = cam.GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();
    }
}