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

    public enum ColorState { White = 0, Red, Blue, Green, Magenta, Cyan, Yellow };
    public ParticleSystem[] deathParticles;
    public Dictionary<ColorState, ParticleSystem> deathParticlesDict = new Dictionary<ColorState, ParticleSystem>();

    public GunController gunController;
    public GunItem gunItem;

    public Elevator elevator;
    Vector3 elevatorPosition;
    Elevator spawnedElevator;

    public Wave currentWave {
        get; private set;
    }
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
        gunController = playerEntity.GetComponent<GunController>();

        nextCampCheckTime = timeBtwCampingChecks + Time.time;
        campPositionOld = playerT.position;
        playerEntity.OnDeath += OnPlayerDeath;

        map = FindObjectOfType<MapGenerator>();

        /*
        spawnedElevator = Instantiate(elevator, Vector3.zero, Quaternion.identity) as Elevator;
        spawnedElevator.NextWave += NextWave;
        spawnedElevator.gameObject.SetActive(false);
        */

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
                    particleColorState = ColorState.White;
                    break;
            }
            deathParticlesDict.Add(particleColorState, deathParticle);
        }

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
            if ((enemiesRemainingToSpawn > 0) && Time.time > nextSpawnTime) {
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
        spawnedEnemy.OnDeathPosition += OnEnemyDeath;
        if (deathParticlesDict.ContainsKey(enemyColorState)) {
            spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, enemyColor, deathParticlesDict[enemyColorState]);
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

    void OnEnemyDeath(Vector3 _position) {
        enemiesRemainingAlive--;
        
        if ((gunController != null) && (Random.Range(0f, 1f) < currentWave.gunDropChance)) { //spawn gun item by chance
            GunItem spawnedGunItem = Instantiate(gunItem, _position, Quaternion.identity) as GunItem;
            //Debug.Log("Item spawned");
            int randomItemIndex = (int)Random.Range(1,gunController.allGuns.Length);
            //Debug.Log(randomItemIndex);
            spawnedGunItem.SetGunItem(randomItemIndex);
        }
        if (enemiesRemainingAlive == 0) {
            //todo enable elevator
            spawnedElevator.gameObject.SetActive(true);
        }
    }

    void ResetPlayerPosition() {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
    }

    void NextWave() {
        currentWaveNumber++;

        if (currentWaveNumber > 1) {
            AudioManager.Instance.PlaySound2D("Level Complete");
        }

        if (currentWaveNumber - 1 < waves.Length) {
            //print("Prefixed Wave: " + currentWaveNumber);
            currentWave = waves[currentWaveNumber - 1];
        }
        
        else if (MapGenerator.Instance.currentMode == MapGenerator.GameMode.Infinite) {
            //print("Random Wave: " + currentWaveNumber);
            System.Random psrd = new System.Random();
            currentWave = new Wave();
            int minSpawn = Mathf.Min(waves[waves.Length - 1].enemyCount + (currentWaveNumber - waves.Length) * 10, 200);
            int maxSpawn = Mathf.Min(waves[waves.Length - 1].enemyCount  + (currentWaveNumber - waves.Length) * 10 + 50, 200) + 1;
            currentWave.enemyCount = psrd.Next(minSpawn, maxSpawn);

            /*
            currentWave.timeBetweenSpawns = 1f;
            currentWave.moveSpeed = 3;
            currentWave.gunDropChance = 0.03f;
            currentWave.hitsToKillPlayer = 5;
            */

            currentWave.SetWaveProperty(1f, 3, 0.03f, 5);
            currentWave.SpawnAllColor();
            //print($"Gun Drop Chance: {currentWave.gunDropChance}");
        }

        else if (MapGenerator.Instance.currentMode == MapGenerator.GameMode.Tutorial) {
            //todo Tutorial Clear!
        }

        enemiesRemainingToSpawn = currentWave.enemyCount;
        enemiesRemainingAlive = enemiesRemainingToSpawn;

        if (OnNewWave != null) {
            OnNewWave(currentWaveNumber);
        }

        //* elevator spawn
        if (spawnedElevator != null) {
            spawnedElevator.NextWave -= NextWave;
            Destroy(spawnedElevator.gameObject);
        }
        elevatorPosition = map.GetTileFromPosition(Vector3.zero).transform.position + new Vector3(0, -3, 0);
        spawnedElevator = Instantiate(elevator, elevatorPosition, Quaternion.identity);
        spawnedElevator.NextWave += NextWave;
        spawnedElevator.gameObject.SetActive(false);
        ResetPlayerPosition();

    }

    [System.Serializable]
    public class Wave {
        public int enemyCount;
        public float timeBetweenSpawns;

        public float moveSpeed;
        public int hitsToKillPlayer;
        [Range(0,1)]
        public float gunDropChance;
        public ColorState[] colorsToSpawn;

        public void SetWaveProperty(float _timeBtwSpawns, float _movSpeed, float _gunDropChance, int _hitsToKillPlayer) {
            timeBetweenSpawns = _timeBtwSpawns;
            moveSpeed = _movSpeed;
            gunDropChance = _gunDropChance;
            hitsToKillPlayer = _hitsToKillPlayer;
        }

        public void SpawnAllColor() {
            colorsToSpawn = new ColorState[6];
            for (int i = 0; i < 6; i++) {
                colorsToSpawn[i] = (ColorState)(i+1);
            }
        }
   }
}
