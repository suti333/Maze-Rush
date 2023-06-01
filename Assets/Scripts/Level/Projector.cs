using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projecter
{

    private ItemsManager itemsManager;
    private GameObject projectileItem;
    private float projectileMass;
    private float projectileLinearDrag;
    private float projectileAngularDrag;
    private float projectileGravity;
    private RigidbodyInterpolation2D projectileInterpolation;
    public string ProjectileTarget;
    public float ProjectileSpeed;

    public Projecter()
    {
        itemsManager = GameObject.Find("Items").GetComponent<ItemsManager>();

        GameObject player = GameObject.Find("Player");

        projectileMass = player.GetComponent<Rigidbody2D>().mass;
        projectileLinearDrag = player.GetComponent<Rigidbody2D>().drag;
        projectileAngularDrag= player.GetComponent<Rigidbody2D>().angularDrag;
        projectileGravity = player.GetComponent<Rigidbody2D>().gravityScale;
        projectileInterpolation = player.GetComponent<Rigidbody2D>().interpolation;
    }

    public GameObject Project(String projectileName, Vector3 initialPosition, Vector3 projectileDirection, float projectileSpeed, AudioClip audioClip)
    {
        GameObject projectileItem = Project(projectileName, initialPosition, projectileDirection, projectileSpeed);
        projectileItem.transform.position = initialPosition;
        projectileItem.AddComponent<AudioSource>();
        projectileItem.GetComponent<ProjectileController>().ProjectileAudio = audioClip;
        
        return projectileItem;
    }

    public GameObject Project(String projectileName, Vector3 initialPosition, Vector3 projectileDirection, float projectileSpeed)
    {
        projectileDirection = projectileDirection.normalized;

        projectileItem = itemsManager.SpawnItem(projectileName, initialPosition + 0.6f*projectileDirection);
        projectileItem.tag = "ProjectedItem";

        float angle = Mathf.Rad2Deg*(Mathf.Atan2(projectileDirection.y, projectileDirection.x));

        projectileItem.transform.Rotate(Vector3.forward, angle);
        
        Rigidbody2D projectileItemRB2D = projectileItem.AddComponent<Rigidbody2D>();
        projectileItemRB2D.mass = projectileMass;
        projectileItemRB2D.drag = projectileLinearDrag;
        projectileItemRB2D.angularDrag = projectileAngularDrag;
        projectileItemRB2D.gravityScale = projectileGravity;
        projectileItemRB2D.interpolation = projectileInterpolation;

        ProjectileController projectileItemPC = projectileItem.AddComponent<ProjectileController>();
        projectileItemPC.ProjectileDirection = projectileDirection;
        projectileItemPC.ProjectileSpeed = projectileSpeed;
        projectileItemPC.Active = true;

        return projectileItem;
    }
}