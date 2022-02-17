using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerErrorAlert : MonoBehaviour
{
    public static bool isServer = false;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("isServer", isServer);
    }
}
