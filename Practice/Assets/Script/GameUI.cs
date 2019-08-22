using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public Image fadePlane;
    public GameObject gameOverUI;
    public GameObject inGameUI;
    public GameObject pauseUI;
    public GameObject tutorialUI;

    [Header("Tutorial UI")]
    public GameObject[] tutorialWaves;
    public GameObject tutorialClear;

    [Header("New Wave UI")]
    public RectTransform newWaveBanner;
    public Text newWaveTitle;
    public Text newWaveEnemyCount;
    [Header("Game Over UI")]
    public Text scoreUI;
    public Text gameOverScoreUI;
    public Text highScoreUI;

    [Header("Wave Enemy Status UI")]
    public Text currentWaveUI;

    public Text currentEnemyLeftUI;

    bool isPaused = false;
    bool pauseDisabled = false;

    Spawner spawner;
    Player player;
    PlayerController playerController;


    [Header("Heart Status UI")]
    public Transform heartsParent;
    public GameObject heartContainerPrefab;

    private GameObject[] heartContainers;
    private Image[] heartFills;

    [Header("Color Change UI")]
    public Image currentColor;
    public Image redColor;
    public Image greenColor;
    public Image blueColor;
    Color redColorFade = new Color(1, 0, 0, 0.5f);
    Color greenColorFade = new Color(0, 1, 0, 0.5f);
    Color blueColorFade = new Color(0, 0, 1, 0.5f);
    public Font defaultFont;
    public Font fontWhenSelected;

    [Header("Current Weapon UI")]
    public Transform ammoParent;
    public GameObject ammoPrefab;
    public Text ammoText;
    public GameObject[] allGunsUI;
    public Transform gunContainer;

    private int currentGunUIIndex = 0;
    private GameObject currnetGunShow;
    private Vector3 gunShowPosition = new Vector3(-38, 12, -5);
    private Quaternion gunShowRotation = Quaternion.Euler(0, 90, 0);
    private Vector3 gunShowScale = new Vector3(150, 150, 150);
    private GameObject[] ammo;

    int currentAmmo;
    int maxAmmo;
    int ammoRemainingInMag;

    GunController gunController;

    void Awake() {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
        spawner.TutorialClear += OnTutorialClear;
        player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;
        playerController = player.GetComponent<PlayerController>();
        playerController.onColorChange += UpdateColorHUD;
        gunController = player.GetComponent<GunController>();

        Gun.OnShoot += ShootAmmoShow;
        Gun.OnReload += ReloadAmmoShow;
        gunController.OnEquipGun += ResetAmmoShow;
        gunController.OnEquipGun += CurrentWeaponShow;

        InstantiateAmmoContainers();
    }

    void Start() {
        if (MapGenerator.Instance.currentMode == MapGenerator.GameMode.Tutorial)
            tutorialUI.SetActive(true);
        player.OnHealthChange += UpdateHeartsHUD;
        heartContainers = new GameObject[player.healthLimit];
        heartFills = new Image[player.healthLimit];

        InstantiateHeartContainers();
        UpdateHeartsHUD();
    }

    void Update() {
        if (scoreUI != null) scoreUI.text = $"Score: {ScoreKeeper.score.ToString("D6")}";
        currentEnemyLeftUI.text = $"Enemies Left: {spawner.enemiesRemainingAlive}";
        if (Input.GetKeyDown(KeyCode.Escape) && !pauseDisabled) Pause();
    }

    void OnTutorialClear()
    {
        pauseDisabled = true;
        Time.timeScale = 0;
        player.paused = true;
        Cursor.visible = true;
        tutorialWaves[tutorialWaves.Length - 1].SetActive(false);
        tutorialClear.SetActive(true);
    }

    void OnNewWave(int waveNumber) {
        newWaveTitle.text = $"- Wave {waveNumber} -";
        newWaveEnemyCount.text = $"Enemies: {spawner.currentWave.enemyCount}";
        currentWaveUI.text = $"Wave {waveNumber}";

        StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");

        if(MapGenerator.Instance.currentMode == MapGenerator.GameMode.Tutorial)
        {
            if (waveNumber == 1)
            {
                inGameUI.transform.Find("ColorChangeUI").gameObject.SetActive(false);
            }
            else
            {
                tutorialWaves[waveNumber - 2].SetActive(false);
                tutorialWaves[waveNumber - 1].SetActive(true);
                inGameUI.transform.Find("ColorChangeUI").gameObject.SetActive(true);
            }
        }
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
        if (MapGenerator.Instance.currentMode == MapGenerator.GameMode.Infinite) {
            StartCoroutine(Fade(Color.clear, new Color(0, 0, 0, .9f), 1));
            gameOverScoreUI.text = scoreUI.text;
            if (ScoreKeeper.score > ScoreKeeper.highscore) highScoreUI.text = "New Highscore!";
            highScoreUI.text = $"HighScore: {ScoreKeeper.highscore.ToString("D6")}";
            inGameUI.SetActive(false);
            Cursor.visible = true;
            gameOverUI.SetActive(true);
        }
        else if (MapGenerator.Instance.currentMode == MapGenerator.GameMode.Tutorial) {
            //todo
        }
    }

    public void Pause() {
        if (!isPaused) {
            inGameUI.SetActive(false);
            pauseUI.SetActive(true);
            isPaused = true;
            Time.timeScale = 0;
            Cursor.visible = true;
            player.paused = true;
        }
        else {
            Time.timeScale = 1;
            pauseUI.SetActive(false);
            inGameUI.SetActive(true);
            isPaused = false;
            Cursor.visible = false;
            player.paused = false;
        }

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

    void BeforeLoadScene()
    {
        Time.timeScale = 1;
        Gun.OnShoot -= ShootAmmoShow;
        Gun.OnReload -= ReloadAmmoShow;
    }

    //UI input
    public void StartNewGame() {
        BeforeLoadScene();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu() {
        BeforeLoadScene();
        Cursor.visible = true;
        SceneManager.LoadScene("GameMenu");
    }

    public void ToInfinite()
    {
        BeforeLoadScene();
        SceneManager.LoadScene("Infinite");
    }

    //Heart Control

    public void UpdateHeartsHUD() {
        SetHeartContainers();
        SetFilledHearts();
    }

    public void UpdateColorHUD() {
        Color playerColor = playerController.playerColor;
        currentColor.color = playerColor;
        redColor.color = playerColor.r == 1 ? Color.red : redColorFade;
        ChangeColorHUDFont(redColor);
        greenColor.color = playerColor.g == 1 ? Color.green : greenColorFade;
        ChangeColorHUDFont(greenColor);
        blueColor.color = playerColor.b == 1 ? Color.blue : blueColorFade;
        ChangeColorHUDFont(blueColor);
    }

    void ChangeColorHUDFont(Image ColorImage)
    {
        Text colorText = ColorImage.transform.GetChild(0).GetComponent<Text>();
        if (colorText == null) return;

        if(ColorImage.color.a == 1)
        {
            colorText.font = fontWhenSelected;
            colorText.fontStyle = FontStyle.Bold;
            colorText.color = Color.white;
        }
        else
        {
            colorText.font = defaultFont;
            colorText.fontStyle = FontStyle.Normal;
            colorText.color = new Color(0, 0, 0, 0.5f);
        }
            
    }

    void SetHeartContainers() {
        for (int i = 0; i < heartContainers.Length; i++)
        {
            if (i < player.maxHealth)
            {
                heartContainers[i].SetActive(true);
            }
            else
            {
                heartContainers[i].SetActive(false);
            }
        }
    }

    void SetFilledHearts() {
        for (int i = 0; i < heartFills.Length; i++)
        {
            if (i < player.health)
            {
                heartFills[i].fillAmount = 1;
            }
            else
            {
                heartFills[i].fillAmount = 0;
            }
        }
    }

    void InstantiateHeartContainers() {
        for (int i = 0; i < player.healthLimit; i++)
        {
            heartContainers[i] = Instantiate(heartContainerPrefab);
            heartContainers[i].transform.SetParent(heartsParent, false);
            heartFills[i] = heartContainers[i].transform.Find("HeartFill").GetComponent<Image>();
        }
    }

    public void CurrentWeaponShow() {
        allGunsUI[currentGunUIIndex].SetActive(false);
        currentGunUIIndex = gunController.currnetGunIndex;
        allGunsUI[currentGunUIIndex].SetActive(true);
    }

    void InstantiateAmmoContainers() {
        ammo = new GameObject[60];
        for (int i = 0; i < ammo.Length; i++) {
            ammo[i] = Instantiate(ammoPrefab);
            ammo[i].transform.SetParent(ammoParent, false);
            ammo[i].SetActive(false);
        }
    }

    public void ResetAmmoShow() {
        /*         
        print("reset ammo show called");
        print($"current gun: {gunController.equippedGun}");
        print($"ammo Length: {ammo.Length}");
        */

        currentAmmo = gunController.equippedGun.currentAmmo / gunController.equippedGun.projectileSpawn.Length;
        ammoRemainingInMag = gunController.equippedGun.projectilesRemainingInMag / gunController.equippedGun.projectileSpawn.Length;

        if (gunController.currnetGunIndex == 0) ammoText.text = $"{ammoRemainingInMag} / ∞";
        else ammoText.text = $"{ammoRemainingInMag} / {currentAmmo - ammoRemainingInMag}";
        for (int i = 0; i < ammo.Length; i++) {
            //print(gunController.equippedGun.projectilesRemainingInMag);
            if (i < ammoRemainingInMag)
            {
                //print("Ammo SetActive");
                ammo[i].SetActive(true);
            }
            else
            {
                //print("Ammo SetInactive");
                ammo[i].SetActive(false);
            }
        }
    }

    void ReloadAmmoShow() {
        ResetAmmoShow();
    }

    public void ShootAmmoShow() {
        currentAmmo = gunController.equippedGun.currentAmmo / gunController.equippedGun.projectileSpawn.Length;
        ammoRemainingInMag = gunController.equippedGun.projectilesRemainingInMag / gunController.equippedGun.projectileSpawn.Length;

        if (gunController.currnetGunIndex == 0) ammoText.text = $"{ammoRemainingInMag} / ∞";
        else ammoText.text = $"{ammoRemainingInMag} / {currentAmmo - ammoRemainingInMag}";
         ammo[ammoRemainingInMag].SetActive(false);
    }

}
