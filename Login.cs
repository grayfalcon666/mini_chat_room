using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    public InputField InputName;
    public InputField InputServeAddress;
    public static Login Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void OnConnectButtonClicked()
    {
        SceneManager.LoadScene("ChatInterface");
    }

}
