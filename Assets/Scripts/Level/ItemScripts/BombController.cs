using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{
    private float startTime;
    private const float explosionTime = 1.5f;
    private int counter;
    private bool exploded;
    private CircleCollider2D circleCollider;
    private List<GameObject> destroyables;
    private ItemsManager itemsManager;
    private LevelController levelController;
    private SoundManager soundManager;
    [SerializeField]
    private AudioClip explosionSound;

    private void Awake()
    {
        circleCollider = gameObject.GetComponent<CircleCollider2D>();
        destroyables = new List<GameObject>();
        itemsManager = GameObject.Find("Items").GetComponent<ItemsManager>();
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }

    private void Start()
    {
        circleCollider.enabled = false;
        startTime = Time.time;
        counter = 0;
        exploded = false;
    }

    private void Update()
    {
        if (levelController.IsGameActive)
        {
            if ((Time.time - startTime > explosionTime) && !exploded)
            {
                if (counter == 0)
                {
                    circleCollider.enabled = true;
                    circleCollider.radius *= 4f;
                    counter++;
                }
                else if (counter > 4)
                {
                    counter = 0;
                    int count = destroyables.Count;
                    soundManager.PlaySoundWithoutBlocking(explosionSound);
                    for (int i = 0; i < count; i++)
                    {
                        GameObject destroyable = destroyables[i];
                        if (destroyable.tag.Contains("Monster") || destroyable.name.Contains("Factory") || destroyable.name == "Player")
                        {
                            Destroy(destroyable);
                        }

                    }
                    exploded = true;
                    Destroy(gameObject);
                    itemsManager.RemoveItem(name, transform.position);
                }
                else counter++;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("ProjectedItem")) destroyables.Add(collision.gameObject);
    }

    public AudioClip ExplosionSound
    {
        set { explosionSound = value; }
    }
}
