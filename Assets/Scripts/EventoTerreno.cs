using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EventoTerreno : MonoBehaviour
{
    public ControlJugador jugador;
    public Tile tierra;
    bool ini = false;
    List<int> chkns = new List<int>();

    int i = 0;
    double T1 = 0, T2 = 0;


    public void Inicializar()
    {


        ini = true;

        //InvokeRepeating("ActualizarTerreno", 1, 1);
        //InvokeRepeating("SpawnMonstruos", 1, 1);
    }

    private void Awake()
    {
        ini = false;
    }

    private void Update()
    {
        if(ini)
        {
            T2 = Time.timeAsDouble;



            if (T2 - T1 >= 1)
            {

                if (i == 0)
                {
                    foreach (var n in jugador.Chunks)
                    {
                        chkns.Add(n);
                    }
                }

                if (jugador.Chunks.Contains(chkns[i]))
                {

                    AnalizarTilemap(ControlJugador.chunksCargados[chkns[i]], chkns[i]);

                }
                else
                {
                    i = 0;
                    T1 = Time.timeAsDouble;
                    chkns.Clear();
                }


                i++;

                if (i >= chkns.Count)
                {
                    i = 0;
                    T1 = Time.timeAsDouble;
                    chkns.Clear();
                }
            }
        }
    }

    //void captarJugador()
    //{
    //    GameObject[] jugadores = GameObject.FindGameObjectsWithTag("Player");
    //    foreach (GameObject n in jugadores)
    //    {
    //        if (n.GetComponent<ControlJugador>().local)
    //        {
    //            jugador = n.GetComponent<ControlJugador>();
    //            break;
    //        }
    //    }
    //}

    void ActualizarTerreno()
    {
        //if (jugador == null)
        //{
        //    GameObject[] jugadores = GameObject.FindGameObjectsWithTag("Player");
        //    foreach (GameObject n in jugadores)
        //    {
        //        if (n.GetComponent<ControlJugador>().local)
        //        {
        //            jugador = n.GetComponent<ControlJugador>();
        //            break;
        //        }
        //    }
        //}
        if (jugador != null)
        {
            foreach (var n in jugador.Chunks)
            {
                AnalizarTilemap(ControlJugador.chunksCargados[n], n );
                i++;
                break;
            }

        }

    }

    void AnalizarTilemap(Tilemap mapa, int chunk)
    {
        //if (mapa.ContainsTile(jugador.texturas.tiles["tierra"]))
        //{
        for (int i = mapa.cellBounds.x; i < mapa.cellBounds.xMax; i++)
            {

                int superficial = 0;
                for(int j = mapa.cellBounds.yMax; j > mapa.cellBounds.y; j--)
                {
                    if(mapa.HasTile(new Vector3Int(i,j,0)))
                    {
                        superficial = j;
                        break;
                    }
                    
                }

                if(mapa.GetTile(new Vector3Int(i, superficial, 0)) == tierra)
                {
                    if(Random.Range(0,10) == 1)
                    {
                        jugador.CMDponerBloque(new Vector3Int(i, superficial, 0), ControlJugador.DiccionarioID[jugador.texturas.tiles["hierbatierra"]], chunk);

                        int random = Random.Range(0, 50);
                        if ( random > 30)
                            jugador.CMDponerBloque(new Vector3Int(i, superficial + 1, 0), ControlJugador.DiccionarioID[jugador.texturas.tiles["cesped"]], chunk);
                        else if(random >= 1 && random <= 2)
                        {
                            jugador.CMDponerBloque(new Vector3Int(i, superficial + 1, 0), ControlJugador.DiccionarioID[jugador.texturas.tiles["seta"]], chunk);
                        }
                        else if(random == 5)
                        {
                            jugador.CMDponerBloque(new Vector3Int(i, superficial + 1, 0), ControlJugador.DiccionarioID[jugador.texturas.tiles["hongo"]], chunk);
                        }

                        //Debug.Log("Han germinado hierbecillas");
                    }

                }

            GameObject[] jugadores = GameObject.FindGameObjectsWithTag("Player");
            bool distancia = true;


            foreach (var j in jugadores)
            {

                Vector3Int posJ = mapa.WorldToCell(j.transform.position);
                //Debug.Log(posJ.x + "  "+ i);
                if (posJ.x + 40 > i  && posJ.x - 40 < i)
                {
                    distancia = false;
                    break;
                }
            }

            if (ControlJugador.dificultad > 1 && distancia && zombieScript.nZombies < zombieScript.MaxZombies && Random.Range(1, 500) == 1)
            {
                jugador.spawnZombie(new Vector2(i, superficial + 10));
            }

        }
        //}

    }

    void CambiarTile(Vector3Int vector,Tile tile,int chunk)
    {
        jugador.CMDponerBloque(vector,ControlJugador.DiccionarioID[tile],chunk);
    }

    void SpawnMonstruos()
    {
        //if (zombieScript.nZombies < zombieScript.MaxZombies)
        //{
        //    jugador.spawnZombie(new Vector2(0, 10));

        //    zombieScript.nZombies++;
        //}


    }

}
