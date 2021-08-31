using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public static void Line([CallerLineNumber] int lineNumber = 0)
    {
        Debug.Log($"Line = {lineNumber}");
    }
}
