using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace InfimaGames.LowPolyShooterPack.Interface
{
    public class TextHealthCurrent : MonoBehaviour
    {
        #region FIELDS SERIALIZED
            
        [Header("Colors")]
        
        [Tooltip("Determines if the color of the text should changes as health decrease.")]
        [SerializeField]
        private bool updateColor = true;
        
        [Tooltip("Determines how fast the color changes as health decrease.")]
        [SerializeField]
        private float emptySpeed = 1.5f;
        
        [Tooltip("Color used on this text when the player character is dead.")]
        [SerializeField]
        private Color emptyColor = Color.red;

        protected TextMeshProUGUI textMesh;


        protected IGameModeService gameModeService;
        protected CharacterBehaviour playerCharacter;
        
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            //Get Game Mode Service. Very useful to get Game Mode references.
            gameModeService = ServiceLocator.Current.Get<IGameModeService>();
            
            //Get Player Character.
            playerCharacter = gameModeService.GetPlayerCharacter();

            //Get Text Mesh.
            textMesh = GetComponent<TextMeshProUGUI>();
        }

        // Update is called once per frame
        void Update()
        {
            //Current health
            float current = playerCharacter.getCurrentHealth();
            //Total health
            float total = 100;
            
            //Update Text.
            //textMesh.text = current.ToString(CultureInfo.InvariantCulture);
            textMesh.text = current.ToString();

            //Determine if we should update the text's color.
            if (updateColor)
            {
                //Calculate Color Alpha. Helpful to make the text color change based on count.
                float colorAlpha = (current / total) * emptySpeed;
                //Lerp Color. This makes sure that the text color changes based on count.
                textMesh.color = Color.Lerp(emptyColor, Color.white, colorAlpha);   
            }
            
        }
    }
}