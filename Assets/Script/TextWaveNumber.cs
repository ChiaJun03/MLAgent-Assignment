using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextWaveNumber : MonoBehaviour
{
    protected TextMeshProUGUI textMesh;
    private int currentWave;

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
        currentWave = GameManager.waveCounter;

        //Update Text.
        //textMesh.text = current.ToString(CultureInfo.InvariantCulture);
        textMesh.text = currentWave.ToString();
    }
}
