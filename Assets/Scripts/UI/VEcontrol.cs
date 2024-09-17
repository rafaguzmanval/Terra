using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VEcontrol : MonoBehaviour
{
    public GameObject texto;
    public GameObject panel;
    public GameObject textoBoton;
    public GameObject boton;
    public Canvas ventana;

    private void Start()
    {
        DontDestroyOnLoad(ventana);
    }

    public void destruir()
    {
        ventana.enabled = false;
        Destroy(ventana.gameObject);
    }
}
