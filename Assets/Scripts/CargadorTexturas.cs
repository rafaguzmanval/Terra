using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CargadorTexturas : MonoBehaviour
{
    private ControlJugador jugador;
    public Dictionary<string, Sprite> texturas = new Dictionary<string, Sprite>();
    public Dictionary<string, Tile> tiles = new Dictionary<string, Tile>();

    public Tile[,] copaArbol1;


    public void Inicializar()
    {
        cargarSpriteSheet();

        Tile hierbatierra = ScriptableObject.CreateInstance("Tile") as Tile;
        hierbatierra.sprite = texturas["hierba-tierra"];
        tiles.Add("hierbatierra",hierbatierra);

        Tile tierra = ScriptableObject.CreateInstance("Tile") as Tile;
        tierra.sprite = texturas["tierra"];
        tiles.Add("tierra", tierra);

        Tile piedra = ScriptableObject.CreateInstance("Tile") as Tile;
        piedra.sprite = texturas["piedra"];
        tiles.Add("piedra", piedra);

        Tile tocon = ScriptableObject.CreateInstance("Tile") as Tile;
        tocon.sprite = texturas["tronco-bajo"];
        tocon.colliderType = Tile.ColliderType.None;
        tiles.Add("tocon", tocon);

        Tile tronco = ScriptableObject.CreateInstance("Tile") as Tile;
        tronco.sprite = texturas["tronco-medio"];
        tronco.colliderType = Tile.ColliderType.None;
        tiles.Add("tronco", tronco);

        Tile cesped = ScriptableObject.CreateInstance("Tile") as Tile;
        cesped.sprite = texturas["césped1"];
        cesped.colliderType = Tile.ColliderType.None;
        cesped.name = "cesped";
        tiles.Add("cesped", cesped);

        Tile hongo = ScriptableObject.CreateInstance("Tile") as Tile;
        hongo.sprite = texturas["hongo"];
        hongo.colliderType = Tile.ColliderType.None;
        hongo.name = "hongo";
        tiles.Add("hongo", hongo);

        Tile seta = ScriptableObject.CreateInstance("Tile") as Tile;
        seta.sprite = texturas["seta"];
        seta.colliderType = Tile.ColliderType.None;
        seta.name = "seta";
        tiles.Add("seta", seta);

        Tile hojas = ScriptableObject.CreateInstance("Tile") as Tile;
        hojas.sprite = texturas["hojas"];
        hojas.colliderType = Tile.ColliderType.None;
        tiles.Add("hojas", hojas);



        Tile carbon = ScriptableObject.CreateInstance("Tile") as Tile;
        carbon.sprite = texturas["mineral-carbon"];
        tiles.Add("carbon", carbon);

        Tile hierro = ScriptableObject.CreateInstance("Tile") as Tile;
        hierro.sprite = texturas["mineral-hierro"];
        tiles.Add("hierro", hierro);

        Tile ladrillo = ScriptableObject.CreateInstance("Tile") as Tile;
        ladrillo.sprite = texturas["ladrillo"];
        tiles.Add("ladrillo", ladrillo);

        Tile ladrillopiedra = ScriptableObject.CreateInstance("Tile") as Tile;
        ladrillopiedra.sprite = texturas["ladrillopiedra"];
        tiles.Add("ladrillopiedra", ladrillopiedra);



        copaArbol1 = new Tile[4, 5] { { tiles["hojas"], tiles["hojas"], tiles["hojas"], tiles["hojas"], tiles["hojas"] }, { tiles["hojas"], tiles["hojas"], tiles["hojas"], tiles["hojas"], tiles["hojas"] }, { null, tiles["hojas"], tiles["hojas"], tiles["hojas"], null }, { null, null, tiles["hojas"], tiles["hojas"], null } };
    }

    private void cargarSpriteSheet()
    {

        Debug.Log("comienza la iteración");
        Sprite[] all = Resources.LoadAll<Sprite>("spritesheet_tiles");

        foreach (Sprite s in all)
        {
            texturas[s.name] = s;
            Debug.Log(s.name);
        }


    }

    public IEnumerator GenerarMapaAleatorio(int chunkx, Tilemap mapa, Tilemap mapaFondo,int sem)
    {
        //Debug.LogWarning("Semilla : " + sem);
        //Debug.LogWarning("SemillaNetwork : " + network.semilla);
        int margen = 20;
        int ancho = margen * (chunkx + 1);
        int profundo = 50;

        //int alturaMIN = -10;
        int alturaMAX = 6;
        float suavidad = 20f;
        float[] escala = { 3.6f, 5.5f, 7.3f, 2.1f };


        for (int x = ancho - margen; x < ancho; x++)
        {
            int altura = Mathf.RoundToInt(alturaMAX * Mathf.PerlinNoise((x + sem) / suavidad, 0));

            for (int y = -profundo; y <= altura; y++)
            {

                Vector3Int vector = new Vector3Int(x, y, 0);

                float perlin = 0;
                float perlinMinerales = 0;
                for (int i = 0; i < escala.Length; i++)
                {

                    float xCord = (float)(x + sem) / margen * escala[i];
                    float yCord = (float)y / margen * escala[i];

                    perlin += Mathf.PerlinNoise(xCord, yCord);

                }

                float xC = (float)(x + sem) / margen * 2f;
                float yC = (float)y / margen * 2f;
                perlinMinerales = Mathf.PerlinNoise(xC, yC);

                perlin /= escala.Length;

                Tile tileNuevo;

                if (y == altura)
                {
                    if (perlin > 0.4f && (x % 3 == 0 && x % 2 == 0) && perlin < 0.55f && mapa.GetTile(new Vector3Int(x, y - 1, 0)) == tiles["hierbatierra"] && x > ancho - margen + 2 && x < ancho - 2)
                    {
                        tileNuevo = tiles["tocon"];
                        int i;
                        int d = Mathf.FloorToInt(perlin * 10f);
                        int max = (Mathf.Abs(x) % d) + 5;
                        for (i = 1; i < max; i++)
                        {
                            mapa.SetTile(new Vector3Int(vector.x, vector.y + i, 0), tiles["tronco"]);
                        }

                        for (int j = 0; j < copaArbol1.GetLength(0); j++)
                        {
                            for (int k = 0; k < copaArbol1.GetLength(1); k++)
                            {
                                mapa.SetTile(new Vector3Int(vector.x + k - 2, vector.y + i, 0), copaArbol1[j, k]);
                            }
                            i++;

                        }

                    }
                    else
                    {
                        if (perlin > 0.4f && mapa.GetTile(new Vector3Int(x, y - 1, 0)) == tiles["hierbatierra"])
                        {
                            tileNuevo = tiles["cesped"];
                        }
                        else if(perlin > 0.3f && perlin < 0.32f && mapa.GetTile(new Vector3Int(x, y - 1, 0)) == tiles["hierbatierra"])
                        {
                            tileNuevo = tiles["hongo"];
                        }
                        else if (perlin > 0.36f && perlin < 0.38f && mapa.GetTile(new Vector3Int(x, y - 1, 0)) == tiles["hierbatierra"])
                        {
                            tileNuevo = tiles["seta"];
                        }
                        else
                            tileNuevo = null;
                    }

                }
                else if (y > -5 && y < altura) //SECCION DE TIERRA
                {
                    if (perlin < 0.53f)
                    {
                        if (y == altura - 1)
                        {
                            tileNuevo = tiles["hierbatierra"];
                        }
                        else
                        {
                            tileNuevo = tiles["tierra"];
                        }

                    }
                    else
                    {
                        tileNuevo = null;
                    }
                    if (y <= 0)
                    {
                        if (mapaFondo != null)
                        {
                            mapaFondo.SetTile(vector, tiles["tierra"]);
                        }

                    }

                }
                else//RESTO, SECCION DE PIEDRA
                {
                    if (perlin < 0.5f)
                    {
                        if (perlin > 0.2f)
                            tileNuevo = tiles["piedra"];
                        else
                        {
                            if (perlinMinerales >= 0.8f && perlinMinerales < 0.81f)
                            {
                                tileNuevo = tiles["hierro"];
                            }
                            else if (perlinMinerales > 0.4f && perlinMinerales < 0.5f)
                            {
                                tileNuevo = tiles["carbon"];
                            }
                            else
                            {
                                tileNuevo = tiles["piedra"];
                            }


                        }

                    }
                    else
                    {
                        if (perlinMinerales > 0.7f && perlinMinerales < 0.76f && perlin > 0.4f)
                        {
                            tileNuevo = tiles["hierro"];
                        }
                        else if (perlinMinerales > 0.45f && perlinMinerales < 0.5f && perlin > 0.3f)
                        {
                            tileNuevo = tiles["carbon"];
                        }
                        else
                        {
                            tileNuevo = null;
                        }

                    }

                    mapaFondo.SetTile(vector, tiles["piedra"]);

                }

                mapa.SetTile(vector, tileNuevo);
            }
        }

        yield return null;
    }





}
