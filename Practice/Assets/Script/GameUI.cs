﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public Image fadePlane;
    public GameObject gameOverUI;
    public GameObject inGameUI;

    public RectTransform newWaveBanner;
    public Text newWaveTitle;
    public Text newWaveEnemyCount;
    public Text scoreUI;
    public Text gameOverScoreUI;

    public Text currentEnemyLeftUI;
    public RectTransform healthBar;

    Spawner spawner;
    Player player;

    void Awake() {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

    void Start() {
        player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;
    }

    void Update() {
        scoreUI.text = ScoreKeeper.score.ToString("D6");
        currentEnemyLeftUI.text = $"{spawner.enemiesRemainingAlive} Enemies Left";
        float healthPercent = 0;
        if (player != null) {
            healthPercent = player.health / player.startingHealth;
        }
        healthBar.localScale = new Vector3(healthPercent, 1, 1);
    }


    void OnNewWave(int waveNumber) {
        string[] numbers = {"One", "Two", "Three", "Four", "Five"};
        newWaveTitle.text = $"- Wave {numbers[waveNumber - 1]} -";
        if (waveNumber == spawner.waves.Length) {
            newWaveEnemyCount.text = "Enemies: Infinite";
            currentEnemyLeftUI.gameObject.SetActive(false);
        } 
        else newWaveEnemyCount.text = $"Enemies: {spawner.waves[waveNumber - 1].enemyCount}";
        
        StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");
    } 

    IEnumerator AnimateNewWaveBanner() {
        float delayTime = 1.5f;
        float speed = 3f;
        float animatePercent = 0;
        int dir = 1;

        float endDelayTime = Time.time + delayTime + 1 / speed;
        while (animatePercent >= 0) {
            animatePercent += Time.deltaTime * speed * dir;

            if (animatePercent >= 1) {
                animatePercent = 1;
                if (Time.time > endDelayTime) {
                    dir = -1;
                }
            }

            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(220, 540, animatePercent);
            yield return null;
        }
    }

    void OnGameOver()
    {
        StartCoroutine(Fade(Color.clear, new Color(0, 0, 0, .9f), 1));
        gameOverScoreUI.text = scoreUI.text;
        inGameUI.SetActive(false);
        Cursor.visible = true;
        gameOverUI.SetActive(true);
    }

    IEnumerator Fade(Color from, Color to, float time) {
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1) {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    //UI input
    public void StartNewGame() {
        SceneManager.LoadScene("Game");
    }

    public void ReturnToMainMenu() {
        SceneManager.LoadScene("GameMenu");
    }
}
