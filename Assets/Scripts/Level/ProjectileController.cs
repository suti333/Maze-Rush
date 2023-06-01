using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{

    private Rigidbody2D rigidBody;
    private Vector3 projectileDirection;
    private float projectileSpeed;
    public bool Active;
    private AudioClip projectileAudio;
    private LevelController levelController;
    private SoundManager soundManager;
    private SoundManager.Bool isPlayingProjectileSound;

    private void Awake()
    {
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        isPlayingProjectileSound = new SoundManager.Bool(false);
    }

    private void FixedUpdate()
    {
        if (levelController.IsGameActive)
        {
            if (Active)
            {
                rigidBody.AddForce(projectileDirection * projectileSpeed, ForceMode2D.Impulse);

                if ((projectileAudio != null) && !isPlayingProjectileSound.Value)
                {
                    soundManager.PlaySoundWithoutBlocking(projectileAudio, isPlayingProjectileSound,0.2f);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        name = name.Replace("Dropped", "");
    }

    private void OnTriggerEnter2D (Collider2D collision)
    {
        if (!collision.CompareTag("ProjectedItem") && !collision.tag.Contains("Item"))
        {
            if (!name.Contains("Used"))
            {
                if (!collision.tag.Contains("Monster")) Destroy(gameObject);
            }
            else
            {
                if (!collision.CompareTag("Player")) Destroy(gameObject);
            }
        }
    }

    public Vector3 ProjectileDirection
    {
       set { projectileDirection = value; }
    }
    public float ProjectileSpeed
    {
        set { projectileSpeed = value; }
    }

    public AudioClip ProjectileAudio
    {
        set { projectileAudio = value; }
    }

    public SoundManager.Bool IsPlayingProjectileSound
    {
        set { isPlayingProjectileSound = value; }
    }
}
