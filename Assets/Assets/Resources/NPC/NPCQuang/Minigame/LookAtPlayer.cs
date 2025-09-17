using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = Camera.main.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(player.transform);
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }
}
