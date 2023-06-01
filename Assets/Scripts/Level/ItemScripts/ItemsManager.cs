using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemsManager : MonoBehaviour
{
    private CoordinateSystem coordinateSystem;
    private PlayerController player;
    private List<Vector3> itemSpawnLocations;
    private List<Vector3> spawnedItemsLocations;

    [SerializeField] private Sprite gunSprite;
    [SerializeField] private Sprite bulletSprite;

    [SerializeField] private Sprite laserGunSprite;
    [SerializeField] private Sprite laserBeamSprite;

    [SerializeField] private Sprite axeSprite;


    [SerializeField] private Sprite bombSprite;
    [SerializeField] private Sprite usedBombSprite;

    [SerializeField] private Sprite bladeSprite;
    [SerializeField] private Sprite usedBladeSprite;

    [SerializeField] private Sprite bananaSkinSprite;
    [SerializeField] private Sprite usedBananaSkinSprite;

    [SerializeField] private Sprite spellSprite;
    [SerializeField] private Sprite usedSpellSprite;

    [SerializeField] private AudioClip bombExplosionSound;
    [SerializeField] private AudioClip itemDropSound;
    [SerializeField] private AudioClip spellDropSound;
    private SoundManager soundManager;
    private Dictionary<String, AudioClip> itemDropSoundsDict;

    private Dictionary<String, Sprite>[] itemsSpriteDict;
    private Dictionary<String, int> spawnedItemsCount;

    private Transform monsterFactories;
    private Transform monsters;
    private LevelController levelController;

    private void Awake()
    {
        itemsSpriteDict = new[]
                    {
                        new Dictionary<string, Sprite>(), // normal/dropped items sprite
                        new Dictionary<string, Sprite>()  // used items sprite
                    };

        itemsSpriteDict[0].Add("Gun", gunSprite);
        itemsSpriteDict[0].Add("Bullet", bulletSprite);
        itemsSpriteDict[0].Add("LaserGun", laserGunSprite);
        itemsSpriteDict[0].Add("LaserBeam", laserBeamSprite);
        itemsSpriteDict[0].Add("Axe", axeSprite);
        itemsSpriteDict[0].Add("Bomb", bombSprite);
        itemsSpriteDict[1].Add("Bomb", usedBombSprite);
        itemsSpriteDict[0].Add("Blade", bladeSprite);
        itemsSpriteDict[1].Add("Blade", usedBladeSprite);
        itemsSpriteDict[0].Add("BananaSkin", bananaSkinSprite);
        itemsSpriteDict[1].Add("BananaSkin", usedBananaSkinSprite);
        itemsSpriteDict[0].Add("Spell", spellSprite);
        itemsSpriteDict[1].Add("Spell", usedSpellSprite);

        itemDropSoundsDict = new Dictionary<string, AudioClip>();

        itemDropSoundsDict.Add("Spell", spellDropSound);

        coordinateSystem = GameObject.Find("CoordinateSystem").GetComponent<CoordinateSystem>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        monsterFactories = GameObject.Find("MonsterFactories").transform;
        monsters = GameObject.Find("Monsters").transform;
        spawnedItemsLocations = new List<Vector3>();
        spawnedItemsCount = new Dictionary<String, int>();
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        this.enabled = false;
    }

    private void Start()
    {
        HashSet<Vector3> obstacles = coordinateSystem.WallsLocations.Union(coordinateSystem.FactoryOccupiedLocations).ToHashSet<Vector3>();
        itemSpawnLocations = coordinateSystem.GetObstacleFreeLocations(obstacles, GameObject.Find("Walls").GetComponent<Tilemap>().cellBounds).ToList<Vector3>();
    }

    private void Update()
    {
        if (levelController.IsGameActive)
        {
            int itemsRequired = monsterFactories.childCount - player.GetInventoryItemCount("Bomb") - GetSpawnedItemsCount("Bomb");

            if (itemsRequired > 0)
            {
                Debug.Log("Bomb Required : " + itemsRequired);
                SpawnItem("Bomb", itemsRequired);
            }

            String deathItem;
            foreach (Transform monsterType in monsters)
            {
                if (monsterType.childCount > 0)
                {
                    deathItem = (monsterType.GetChild(0).GetComponent(monsterType.name.Replace("Monsters", "") + "Controller") as MonsterController).DeathItem;

                    if (deathItem == "Bullet") deathItem = "Gun";
                    else if (deathItem == "LaserBeam") deathItem = "LaserGun";
                    itemsRequired = monsterType.childCount - player.GetInventoryItemCount(deathItem) - GetSpawnedItemsCount(deathItem);

                    if (itemsRequired > 0)
                    {
                        Debug.Log("DeathItem : " + deathItem);
                        Debug.Log("Required : " + itemsRequired);
                        SpawnItem(deathItem, itemsRequired);
                    }
                }
            }

            if (coordinateSystem.FactoryDestroyed)
            {
                HashSet<Vector3> obstacles = coordinateSystem.WallsLocations.Union(coordinateSystem.FactoryOccupiedLocations).ToHashSet<Vector3>();
                itemSpawnLocations = coordinateSystem.GetObstacleFreeLocations(obstacles, GameObject.Find("Walls").GetComponent<Tilemap>().cellBounds).ToList<Vector3>();
            }
        }
    }

    private int GetSpawnedItemsCount(String itemName)
    {
        if (spawnedItemsCount.ContainsKey(itemName)) return spawnedItemsCount[itemName];
        else return 0;
    }

    public void SpawnItem(String itemName, int quantity)
    {
        for (int i = 0; i < quantity; i++) SpawnItem(itemName);
    }

    public void SpawnItem(String itemName)
    {
        Vector3 spawnLocation;
        do
        {
            spawnLocation = itemSpawnLocations[UnityEngine.Random.Range(0, itemSpawnLocations.Count)];
        } while (spawnedItemsLocations.Contains(spawnLocation));

        SpawnItem(itemName, spawnLocation);
    }

    public GameObject SpawnItem(String itemName, Vector3 spawnLocation, bool is_used = false)
    {
        bool play_sound = false;

        if (itemName.Contains("Dropped") || itemName.Contains("Used"))
        {
            play_sound = true;
        }

        if (itemName == "Gun" || itemName == "LaserGun")
        {
            itemName = itemName + "_3";
        }

        GameObject itemObject = new GameObject(itemName);

        if (itemName.Contains("Gun"))
        {
            play_sound = false;
            itemName = itemName.Split('_')[0];
        }

        itemName = itemName.Replace("Dropped", "").Replace("Used", "");
        itemObject.tag = itemName + "Item";

        if (spawnedItemsCount.ContainsKey(itemName)) spawnedItemsCount[itemName]++;
        else spawnedItemsCount.Add(itemName, 1);
        spawnedItemsLocations.Add(spawnLocation);

        spawnLocation.x = Mathf.Floor(spawnLocation.x) + 0.5f;
        spawnLocation.y = Mathf.Floor(spawnLocation.y) + 0.5f;

        itemObject.transform.position = spawnLocation;
        itemObject.transform.parent = GameObject.Find("Items").transform;
        itemObject.transform.localScale = new Vector3(0.5f, 0.5f, 1);

        itemObject.AddComponent<SpriteRenderer>();
        itemObject.GetComponent<SpriteRenderer>().sprite = itemsSpriteDict[(is_used ? 1 : 0)][itemName];
        itemObject.GetComponent<SpriteRenderer>().sortingLayerName = "Player";

        itemObject.AddComponent<CircleCollider2D>();
        itemObject.GetComponent<CircleCollider2D>().isTrigger = true;

        if (itemName == "Bomb" && is_used)
        {
            BombController bombController = itemObject.AddComponent<BombController>();
            bombController.ExplosionSound = bombExplosionSound;
        }

        if (play_sound)
        {
            if (itemDropSoundsDict.ContainsKey(itemName) && is_used)
            {
                soundManager.PlaySoundWithoutBlocking(itemDropSoundsDict[itemName], 0.2f);
            }
            else soundManager.PlaySoundWithoutBlocking(itemDropSound, 0.2f);
        }

        return itemObject;
    }

    public void RemoveItem(String itemName, Vector3 itemLocation)
    {
        spawnedItemsLocations.Remove(itemLocation);

        itemName = itemName.Replace("Used", "").Replace("Dropped", "");
        if (itemName.Contains("Gun"))
        {
            itemName = itemName.Split('_')[0];
        }

        spawnedItemsCount[itemName]--;
    }

    public int GetspawnedItemsCount(string itemName)
    {
        return spawnedItemsCount[itemName];
    }

    public Sprite GetItemSprite(String itemName)
    {
        return itemsSpriteDict[0][itemName];
    }
}
