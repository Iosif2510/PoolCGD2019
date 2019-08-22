using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private static Spawner instance;
    public static Spawner Instance {
        get { return instance; }
    }

    public enum GameMode { Infinite, Tutorial};
    GameMode currentMode;

    public bool devMode;
    bool dummyCreating = false;
    int spawnOrder = 0;

    public Wave[] waves;
    public Enemy enemy;
    LivingEntity playerEntity;
    Transform playerT;
    GunController gunController;

    public enum ColorState { White = 0, Red, Blue, Green, Magenta, Cyan, Yellow };
    public ParticleSystem[] deathParticles;
    public Dictionary<ColorState, ParticleSystem> deathParticlesDict = new Dictionary<ColorState, ParticleSystem>();

    public GunItem gunItem;
    public HealingItem healthItem;
    public MaxHealthItem injector;

    public Elevator elevator;
    Vector3 elevatorPosition;
    Elevator spawnedElevator;

    Vector3 currentMapCenter;

    public Wave currentWave {
        get; private set;
    }
    int currentWaveNumber = 0;

    int enemiesRemainingToSpawn;
    public int enemiesRemainingAlive { get; private set; }
    float nextSpawnTime;

    MapGenerator map;

    public float timeBtwCampingChecks = 6;
    float campThresholdDistance = 1.5f; 
    float nextCampCheckTime;
    Vector3 campPositionOld;
    bool isCamping;

    bool isDisabled;

    public event System.Action<int> OnNewWave;
    public event System.Action TutorialClear;

    void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        }
        else {
            instance = this;
        }
    }

    void Start() {
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;
        gunController = playerEntity.GetComponent<GunController>();

        nextCampCheckTime = timeBtwCampingChecks + Time.time;
        campPositionOld = playerT.position;
        playerEntity.OnDeath += OnPlayerDeath;

        map = FindObjectOfType<MapGenerator>();
        if (map.currentMode == MapGenerator.GameMode.Infinite)
            currentMode = GameMode.Infinite;
        else
            currentMode = GameMode.Tutorial;

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
        IEnumerator spawnEnemy = SpawnEnemy();
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

                StartCoroutine(spawnEnemy);
            }
        }

        if (devMode) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                StopCoroutine(spawnEnemy);
                foreach (Enemy enemy in FindObjectsOfType<Enemy>()) {
                    Destroy(enemy.gameObject);
                }
                NextWave();
            }
        }

    }

    IEnumerator SpawnEnemy() {
        float spawnDelay = 1.5f;
        float tileFlashSpeed = 4;

        Transform spawnTile = map.GetRandomOpenTile();
        if (isCamping && currentWave.campingCheck) spawnTile = map.GetTileFromPosition(playerT.position);

        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initialColor = Color.white;

        ColorState enemyColorState;

        if (dummyCreating) //모든 색깔 순서대로
        {
            enemyColorState = currentWave.colorsToSpawn[spawnOrder++ % 6];
        }
        else //랜덤 색깔
        {
            int chooseEnemy = Random.Range(0, currentWave.colorsToSpawn.Length);
            enemyColorState = currentWave.colorsToSpawn[chooseEnemy];
        }
        Color enemyColor = ReturnColor(enemyColorState);
        float spawnTimer = 0;

        while(spawnTimer < spawnDelay) {

            tileMat.color = Color.Lerp(initialColor, enemyColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeathPosition += OnEnemyDeath;
        if (deathParticlesDict.ContainsKey(enemyColorState)) {
            spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, enemyColor, deathParticlesDict[enemyColorState]);
        }
        if (dummyCreating)
        {
            spawnedEnemy.SetDummy(true);
        }

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
        
        float randomNum = Random.Range(0f, 1f);
        if ((gunController != null) && (randomNum < currentWave.gunDropChance)) { //spawn gun item by chance
            GunItem spawnedGunItem = Instantiate(gunItem, _position, Quaternion.identity) as GunItem;
            //Debug.Log("Item spawned");
            int randomItemIndex = (int)Random.Range(1,spawnedGunItem.standardGunsNum);
            //Debug.Log(randomItemIndex);
            spawnedGunItem.SetGunItem(randomItemIndex);
            if (currentMode == GameMode.Tutorial) spawnedGunItem.SetDisappearance(false);
        }

        else if (randomNum < (currentWave.gunDropChance + currentWave.healthDropChance)) {
            HealingItem spawnedHealthItem = Instantiate(healthItem, _position, Quaternion.identity) as HealingItem;
            if (currentMode == GameMode.Tutorial) spawnedHealthItem.SetDisappearance(false);
        }

        else if (randomNum < (currentWave.gunDropChance + currentWave.healthDropChance + currentWave.injectorDropChance)) {
            MaxHealthItem spawnedInjector = Instantiate(injector, _position, Quaternion.identity) as MaxHealthItem;
            if (currentMode == GameMode.Tutorial) spawnedInjector.SetDisappearance(false); 
        }

        else if (gunController != null && (randomNum < (currentWave.gunDropChance + currentWave.healthDropChance + currentWave.injectorDropChance + currentWave.superGunDropChance))) {
            GunItem superGunItem = Instantiate(gunItem, _position, Quaternion.identity) as GunItem;
            //Debug.Log("Item spawned");
            int randomItemIndex = (int)Random.Range(superGunItem.standardGunsNum, gunController.allGuns.Length);
            //Debug.Log(randomItemIndex);
            superGunItem.SetGunItem(randomItemIndex);
            if (currentMode == GameMode.Tutorial) superGunItem.SetDisappearance(false);
        }




        if (enemiesRemainingAlive == 0) {
            //todo enable elevator
            //spawnedElevator.gameObject.SetActive(true);
            StartCoroutine(ActivateElevator());
        }
    }

    public void ResetPlayerPosition() {
        if ((currentMode == GameMode.Tutorial) && (currentWave == waves[0])) {
            playerT.position = map.GetTileFromPosition(new Vector3(2, 0, 3)).position + Vector3.up * 3;
        }
        else playerT.position = currentMapCenter + Vector3.up * 3;
    }

    void NextWave() {
        Wave lastWave = currentWave;
        currentWaveNumber++;

        if (currentWaveNumber > 1) {
            AudioManager.Instance.PlaySound2D("Level Complete");
        }

        if (currentWaveNumber - 1 < waves.Length) {
            //print("Prefixed Wave: " + currentWaveNumber);
            currentWave = waves[currentWaveNumber - 1];
        }
        
        else if (currentMode == GameMode.Infinite) {
            //print("Random Wave: " + currentWaveNumber);
            currentWave = new Wave();

            /*
            int minSpawn = Mathf.Min(waves[waves.Length - 1].enemyCount + (currentWaveNumber - waves.Length) * 10, 200);
            int maxSpawn = Mathf.Min(waves[waves.Length - 1].enemyCount  + (currentWaveNumber - waves.Length) * 10 + 50, 200) + 1;
            currentWave.enemyCount = psrd.Next(minSpawn, maxSpawn);
            */

            /*
            currentWave.timeBetweenSpawns = 1f;
            currentWave.moveSpeed = 3;
            currentWave.gunDropChance = 0.03f;
            */

            float currentTimeBtwSpawn = lastWave.timeBetweenSpawns;
            int currentEnemyCount = lastWave.enemyCount;
            float currentMoveSpeed = lastWave.moveSpeed;

            switch (currentWaveNumber % 3) {
                case 2:
                    currentMoveSpeed = Mathf.Min(currentMoveSpeed * 1.1f, 4f);
                    break;
                case 0:
                    currentEnemyCount = Mathf.Min((int)(currentEnemyCount * 1.1f), 150);
                    break;
                case 1:
                    currentTimeBtwSpawn = Mathf.Max(currentTimeBtwSpawn * 0.95f, 1f); 
                    break;

            }

            currentWave.SetWaveProperty(currentEnemyCount, currentTimeBtwSpawn, currentMoveSpeed, 0.1f, 0.1f, 0.03f, 0.02f);
            currentWave.SpawnAllColor();
            //print($"Gun Drop Chance: {currentWave.gunDropChance}");
        }

        if (currentMode == GameMode.Tutorial) {
            if (currentWaveNumber == 2)
                dummyCreating = true;
            else
                dummyCreating = false;

            if (currentWaveNumber > waves.Length)
            {
                isDisabled = true;
                if (TutorialClear != null)
                    TutorialClear();
                return;
            }
        }

        enemiesRemainingToSpawn = currentWave.enemyCount;
        enemiesRemainingAlive = enemiesRemainingToSpawn;

        if (OnNewWave != null) {
            OnNewWave(currentWaveNumber);
            currentMapCenter = map.GetCurrentMapCenter();
        }

        //* elevator spawn
        if (spawnedElevator != null) {
            spawnedElevator.NextWave -= NextWave;
            Destroy(spawnedElevator.gameObject);
        }
        elevatorPosition = currentMapCenter + new Vector3(0, -3, 0);
        spawnedElevator = Instantiate(elevator, elevatorPosition, Quaternion.identity);
        spawnedElevator.NextWave += NextWave;
        spawnedElevator.gameObject.SetActive(false);
        ResetPlayerPosition();

        if (enemiesRemainingAlive == 0) {
            //spawnedElevator.gameObject.SetActive(true);
            StartCoroutine(ActivateElevator());
        }

    }

    IEnumerator ActivateElevator()
    {
        float timePerColor = .5f;
        Material tileMat = map.GetTileFromPosition(currentMapCenter).GetComponent<Renderer>().material;
        Color initialColor = tileMat.color;
        Color[] effectColors = new Color[4];
        effectColors[0] = Color.red;
        effectColors[1] = Color.green;
        effectColors[2] = Color.blue;
        effectColors[3] = Color.black;

        float effectTimer = 0;
        for(int i = 0; i < 4; i++)
        {
            while(effectTimer < timePerColor)
            {
                tileMat.color = Color.Lerp(initialColor, effectColors[i], effectTimer/timePerColor);
                effectTimer += Time.deltaTime;
                yield return null;
            }
            effectTimer = 0;
            initialColor = tileMat.color;
        }

        spawnedElevator.gameObject.SetActive(true);
    }

    [System.Serializable]
    public class Wave {
        public int enemyCount;
        public float timeBetweenSpawns;
        public bool campingCheck = true;

        public float moveSpeed;

        [Range(0,1)]
        public float gunDropChance;
        [Range(0,1)]
        public float healthDropChance;
        [Range(0,1)]
        public float injectorDropChance;
        [Range(0,1)]
        public float superGunDropChance;
        public ColorState[] colorsToSpawn;

        public void SetWaveProperty(int _enemyCount, float _timeBtwSpawns, float _movSpeed, float _gun, float _health, float _injector, float _supergun) {
            enemyCount = _enemyCount;
            timeBetweenSpawns = _timeBtwSpawns;
            moveSpeed = _movSpeed;
            gunDropChance = _gun;
            healthDropChance = _health;
            injectorDropChance = _injector;
            superGunDropChance = _supergun;
        }

        public void SpawnAllColor() {
            colorsToSpawn = new ColorState[6];
            for (int i = 0; i < 6; i++) {
                colorsToSpawn[i] = (ColorState)(i+1);
            }
        }
   }
}
