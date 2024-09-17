using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UIGame : NetworkBehaviour
{

    public ControlJugador jugador;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Se inicializa guardador");
    }

    public void funcionGuardar()
    {
        PlayerPrefs.SetInt("haymundo", 1);
        jugador.guardarPartida();
        Debug.Log("partida guardada");
    }
}
