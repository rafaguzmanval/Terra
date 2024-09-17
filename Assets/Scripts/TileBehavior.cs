using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileBehavior : MonoBehaviour
{
    ControlJugador jugador;
    Vector3Int posicion;
    int chunk;
    Tilemap mapa;


    public TileBehavior(ControlJugador j, Vector3Int pos,int chunk,Tilemap map)
    {
        jugador = j;
        posicion = pos;
        this.chunk = chunk;
        mapa = map;

    }


    void Start()
    {
        InvokeRepeating("AnalizarTerreno",1,1);
    }

    void AnalizarTerreno()
    {
        if(mapa.HasTile(new Vector3Int(posicion.x,posicion.y + 1, posicion.z)))
        {
            jugador.CMDponerBloque(posicion,0,chunk);
        }
    }
}
