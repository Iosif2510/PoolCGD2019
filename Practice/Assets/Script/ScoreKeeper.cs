using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    private static ScoreKeeper instance;
    public static ScoreKeeper Instance {
        get { return instance; }
    }

    public static int score { get; private set; }
    public int enemyKillScore;
    public int itemSecureScore;
    public static int highscore { get; private set; }

    void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        }
        else {
            instance = this;
        }

        highscore = PlayerPrefs.GetInt("highscore", 0);
    }

    void Start() {
        Enemy.OnDeathStatic += OnEnemyKilled;
        Item.ItemSecure += OnItemSecure;
        score = 0;
        if (FindObjectOfType<Player>() != null) FindObjectOfType<Player>().OnDeath += OnPlayerDeath;
    }

    void OnEnemyKilled() {
        score += enemyKillScore;
    }

    void OnItemSecure() {
        score += itemSecureScore;
    }

    void OnPlayerDeath() {
        Enemy.OnDeathStatic -= OnEnemyKilled;
        Item.ItemSecure -= OnItemSecure;
        if (score > highscore) {
            highscore = score;
            PlayerPrefs.SetInt("highscore", score);
        }
    }
}
