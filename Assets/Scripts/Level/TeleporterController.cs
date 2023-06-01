using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TeleporterController : MonoBehaviour
{
    [SerializeField]
    private GameObject otherTeleporter;
    private PlayerController playerController;
    private SoundManager soundManager;
    [SerializeField]
    private AudioClip teleportSound;
    private LevelController levelController;

    private void Awake()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();

        gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Player";
    }

    private void Start()
    {
        gameObject.transform.parent.tag = "ActiveTeleporter";
    }

    private void Update()
    {
        if (gameObject.transform.parent.tag == "InactiveTeleporter")
        {
            gameObject.transform.parent.tag = "ActiveTeleporter";
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {   
        if (collision.tag == "Player" && gameObject.transform.parent.tag == "ActiveTeleporter" && !levelController.IsTeleportsDisabled)
        {
            StartCoroutine("Teleport");
            soundManager.PlaySoundWithoutBlocking(teleportSound);
        }

    }
    IEnumerator Teleport()
    {
        playerController.enabled = false;
        yield return new WaitForSeconds(0.1f);
        playerController.transform.position = new Vector3(otherTeleporter.transform.position.x, otherTeleporter.transform.position.y, 0f);
        yield return new WaitForSeconds(0.1f);
        playerController.enabled = true;

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player" && gameObject.transform.parent.tag == "ActiveTeleporter")
        {
            gameObject.transform.parent.tag = "InactiveTeleporter";
        }
    }
}
