using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScrollBGTest
{
    [System.Serializable]
    /// <summary>
    //This script is used for background scrolling demo play.
    //Used in the editor and android.
    //This is a sample script and stability and optimization are not guaranteed.
    /// </summary>

    public class ScrollBackgroundCtrl : MonoBehaviour
    {
        //Background Layers
        public Transform[] Background;

        //Scrolling Speeds
        public float[] ScrollSpeed;

        //Renderer
        public MeshRenderer[] Ren;
        public MeshRenderer SkyRen;

        //Movement speed according to keyboard input
        public float MoveValue;
        public float MoveSpeed;

        //Scroll of the sky
        float SkyMoveValue;
        public float SkyScrollSpeed;


        void Start()
        {
            //Reset Values
            MoveValue = 0;
            SkyMoveValue = 0;

            //Get MeshRenderers
            for (int i = 0; i < Background.Length; i++)
                Ren[i] = Background[i].GetComponent<MeshRenderer>();
        }


        void Update()
        {
            // --- CHANGE 1: Reverse the direction of MoveValue update ---
            // Original: MoveValue -= MoveSpeed * Time.deltaTime; (decreases MoveValue, scrolls left)
            // New: MoveValue += MoveSpeed * Time.deltaTime; (increases MoveValue, scrolls right)
            MoveValue += MoveSpeed * Time.deltaTime;

            // Input (if you uncomment these, you'll also need to reverse their signs for consistent behavior)
            // if (Input.GetKey(KeyCode.LeftArrow))
            //     MoveValue += MoveSpeed; // Changed -= to +=
            //
            // if (Input.GetKey(KeyCode.RightArrow))
            //     MoveValue -= MoveSpeed; // Changed += to -=

            // Material OffSet
            for (int i = 0; i < Background.Length; i++)
                Ren[i].material.mainTextureOffset = new Vector2(MoveValue * ScrollSpeed[i], 0);

            // --- CHANGE 2: Reverse the direction of SkyMoveValue update ---
            // Original: SkyMoveValue += (Time.unscaledDeltaTime * -SkyScrollSpeed); (decreases SkyMoveValue, scrolls left)
            // New: SkyMoveValue -= (Time.unscaledDeltaTime * -SkyScrollSpeed); (increases SkyMoveValue, scrolls right)
            // Alternatively, and perhaps more clearly: SkyMoveValue += (Time.unscaledDeltaTime * SkyScrollSpeed);
            SkyMoveValue -= (Time.unscaledDeltaTime * -SkyScrollSpeed);
        }
    }

}
