using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlBotonesMultijugador : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void funcionHost()
    {
        SceneManager.LoadScene("HostMenu");
        
    }

    public void funcionUnirse()
    {
        SceneManager.LoadScene("ClientMenu");
    }

    public void funcionAtras()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }
}
