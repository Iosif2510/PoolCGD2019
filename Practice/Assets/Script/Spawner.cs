using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool devMode;

    public Wave[] waves;
    public Enemy enemy;
    public EnemyHealthBar enemyHealthBar;
    LivingEntity playerEntity;
    Transform playerT;

    public enum ColorState { Red, Blue, Green, Magenta, Cyan, Yellow, Null };
    public ParticleSystem[] deathParticles;
    public Dictionary<ColorState, ParticleSystem> deathParticlesDict = new Dictionary<ColorState, ParticleSystem>();

    Wave currentWave;
    int currentWaveNumber;

    int enemiesRemainingToSpawn;
    public int enemiesRemainingAlive { get; private set; }
    float nextSpawnTime;

    MapGenerator map;

    float timeBtwCampingChecks = 3;
    float campThresholdDistance = 1.5f; 
    float nextCampCheckTime;
    Vector3 campPositionOld;
    bool isCamping;

    bool isDisabled;

    public event System.Action<int> OnNewWave;

    void Start() {
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;

        nextCampCheckTime = timeBtwCampingChecks + Time.time;
        campPositionOld = playerT.position;
        playerEntity.OnDeath += OnPlayerDeath;

        foreach (ParticleSystem deathParticle in deathParticles) {
            ColorState particleColorState;
            switch (deathParticle.name) {
                case "Red Death Effect":
                    particleColorState = ColorState.Red;
                    break;
                case "Green Death Effect":
                    particleColorState = ColorState.Green;
                    break;
                case "Blue Death Effect":
                    particleColorState = ColorState.Blue;
                    break;
                case "Magenta Death Effect":
                    particleColorState = ColorState.Magenta;
                    break;
                case "Cyan Death Effect":
                    particleColorState = ColorState.Cyan;
                    break;
                case "Yellow Death Effect":
                    particleColorState = ColorState.Yellow;
                    break;
                default:
                    particleColorState = ColorState.Null;
                    break;
            }
            deathParticlesDict.Add(particleColorState, deathParticle);
        }

        map = FindObjectOfType<MapGenerator>();
        NextWave();
    }

    void Update() {
        if (!isDisabled) {
            //Camping Check
            if (Time.time > nextCampCheckTime) {
                nextCampCheckTime = Time.time + timeBtwCampingChecks;

                isCamping = Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance;
                campPositionOld = playerT.position;
            }

            //Spawn Enemy
            if ((currentWave.infinite || enemiesRemainingToSpawn > 0) && Time.time > nextSpawnTime) {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

                StartCoroutine("SpawnEnemy");
            }
        }

        if (devMode) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                StopCoroutine("SpawnEnemy");
                foreach (Enemy enemy in FindObjectsOfType<Enemy>()) {
                    Destroy(enemy.gameObject);
                }
                foreach (EnemyHealthBar enemyHB in FindObjectsOfType<EnemyHealthBar>()) {
                    Destroy(enemyHB.gameObject);
                }
                NextWave();
            }
        }

    }

    IEnumerator SpawnEnemy() {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform spawnTile = map.GetRandomOpenTile();
        if (isCamping) spawnTile = map.GetTileFromPosition(playerT.position);

        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initialColor = Color.white;

        int chooseEnemy = Random.Range(0, currentWave.colorsToSpawn.Length);
        ColorState enemyColorState = currentWave.colorsToSpawn[chooseEnemy];
        Color enemyColor = ReturnColor(enemyColorState); //적의 색깔과 스폰 장소 색깔을 결정
        float spawnTimer = 0;

        while(spawnTimer < spawnDelay) {

            tileMat.color = Color.Lerp(initialColor, enemyColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;
        if (deathParticlesDict.ContainsKey(enemyColorState)) {
            spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, enemyColor, deathParticlesDict[enemyColorState]);
        }

        EnemyHealthBar spawnedEnemyHealthBar = Instantiate(enemyHealthBar, spawnedEnemy.transform.position + EnemyHealthBar.initialPosition, Quaternion.identity) as EnemyHealthBar;
        spawnedEnemyHealthBar.SetEnemy(spawnedEnemy);

        if (tileMat.color != initialColor) tileMat.color = initialColor;
    }

    Color ReturnColor(ColorState colorState) {
        switch(colorState) {
            case ColorState.Red:
                return new Color(1, 0, 0, 1);
            case ColorState.Blue:
                return new Color(0, 0, 1, 1);
            case ColorState.Green:
                return new Color(0, 1, 0, 1);
            case ColorState.Magenta:
                return new Color(1, 0, 1, 1);
            case ColorState.Yellow:
                return new Color(1, 1, 0, 1);
            case ColorState.Cyan:
                return new Color(0, 1, 1, 1);
            default:
                return new Color(0, 0, 0, 0);
        }
    }

    void OnPlayerDeath() {
        isDisabled = true;
    }

    void OnEnemyDeath() {
        enemiesRemainingAlive--;
        
        if (enemiesRemainingAlive == 0) NextWave();
    }

    void ResetPlayerPosition() {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
    }

    void NextWave() {
        currentWaveNumber++;

        if (currentWaveNumber > 1) {
            AudioManager.instance.PlaySound2D("Level Complete");
        }

        if (currentWaveNumber - 1 < waves.Length) {
            print("Wave: " + currentWaveNumber);
            currentWave = waves[currentWaveNumber - 1];

            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;

            if (OnNewWave != null) {
                OnNewWave(currentWaveNumber);
            }
            ResetPlayerPosition();
        }

    }

    [System.Serializable]
    public class Wave {
        public bool infinite;
        public int enemyCount;
        public float timeBetweenSpawns;

        public float moveSpeed;
        public int hitsToKillPlayer;
        public int gunDropChance;
        public float enemyHealth;
        public ColorState[] colorsToSpawn;
   }
}
