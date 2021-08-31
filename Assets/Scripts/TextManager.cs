using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextManager : SingletonMonoBehaviour<TextManager>
{

    public GameObject CanvasPrefab;
    private GameObject Canvas;

    // Start is called before the first frame update
    void Start()
    {
        Canvas = Instantiate(CanvasPrefab);
        Canvas.name = Canvas.name.Substring(0, Canvas.name.Length - 7); // (clone)‚Ì•”•ª‚ğíœ
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetPhaseText(string phase)
    {
        Canvas.GetComponentInChildren<Text>().text = phase;
    }
}
