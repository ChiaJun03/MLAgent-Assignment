using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextZombieCount : MonoBehaviour
{
    protected TextMeshProUGUI textMesh;
    private int zombieCount;

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
        zombieCount = GameManager.currentEnemyCount;

        //Update Text.
        //textMesh.text = current.ToString(CultureInfo.InvariantCulture);
        textMesh.text = zombieCount.ToString();
    }
}