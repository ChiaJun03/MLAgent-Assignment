using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextEnd : MonoBehaviour
{
    protected TextMeshProUGUI textMesh;
    private string WinMessage;

    // Start is called before the first frame update
    void Start()
    {
        //Get Text Mesh.
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.playerDead){
            WinMessage = "YOU LOSE!";
            textMesh.text = WinMessage;
        }
    }
}
