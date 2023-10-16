using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // zorg dat de mouse cursor gewoon kan bewegen in menus
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
