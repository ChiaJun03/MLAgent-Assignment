using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextZombieKilled : MonoBehaviour
{
    protected TextMeshProUGUI textMesh;
    private int zombieKilled;

    // Start is called before the first frame update
    void Start()
    {
        //Get Text Mesh.
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        //Current Wave
        zombieKilled = GameManager.enemyKilled;

        //Update Text.
        //textMesh.text = current.ToString(CultureInfo.InvariantCulture);
        textMesh.text = zombieKilled.ToString();
    }
}