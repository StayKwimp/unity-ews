using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchRoom : MonoBehaviour
{
    public string roomName;

    public void SwitchToScene() {
        SceneManager.LoadScene(roomName);
    }
}
