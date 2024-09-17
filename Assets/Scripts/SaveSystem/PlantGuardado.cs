using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class V3
{
    public float v1, v2, v3;

    public V3(float x, float y , float z)
    {
        v1 = x;
        v2 = y;
        v3 = z;
    }
}

[System.Serializable]
public class PlantGuardado
{
    public List<int> cordX;
    public List<int> cordY;
    public List<int> objeto;

    public List<string> nombreJugador;
    public List<V3> posJ;
    public List<string> ModoJuego;



    public int semilla;

    public PlantGuardado()
    {
        cordX = new List<int>();
        cordY = new List<int>();
        objeto = new List<int>();
        nombreJugador = new List<string>();
        posJ = new List<V3>();
        ModoJuego = new List<string>();
        semilla = 0;
    }
}
