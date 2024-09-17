using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Nuevo item", menuName = "Item")]
public class item : ScriptableObject
{
    public string nombre;
    public int id;
    public Sprite ImagenInventario;
    public Tile tile;

    public bool esHerramienta;

    public bool puedeAtacar = false;
    public bool puedeDefender = false;
    public bool puedeConstruir = false;
    public bool ConstruccionNecesitaAdyacente = true;
    public bool puedeDestruir = true;
    public bool consumible = false;
    public int vida = 0;

    public item(string nombre,int id,Sprite ImagenInventario,Tile tile)
    {
        if(tile == null)
        {
            this.nombre = nombre;
            this.id = id;
            this.ImagenInventario = ImagenInventario;
            esHerramienta = true;
            puedeAtacar = false;
            puedeDefender = false;
            puedeConstruir = false;
            ConstruccionNecesitaAdyacente = true;
            puedeDestruir = true;
        }
        else
        {
            this.nombre = nombre;
            this.id = id;
            this.ImagenInventario = ImagenInventario;
            this.tile = tile;
            esHerramienta = false;
            puedeAtacar = false;
            puedeDefender = false;
            puedeConstruir = true;
            ConstruccionNecesitaAdyacente = true;
            puedeDestruir = true;
        }
    }

    public void setHerramienta(string nombre, int id, Sprite ImagenInventario, Tile tile)
    {
        this.nombre = nombre;
        this.id = id;
        this.ImagenInventario = ImagenInventario;
        esHerramienta = true;
        puedeAtacar = false;
        puedeDefender = false;
        puedeConstruir = false;
        ConstruccionNecesitaAdyacente = true;
        puedeDestruir = true;
    }

    public void setConsumible(string nombre, int id, Sprite ImagenInventario)
    {
        this.nombre = nombre;
        this.id = id;
        this.ImagenInventario = ImagenInventario;
        esHerramienta = false;
        puedeAtacar = false;
        puedeDefender = false;
        puedeConstruir = false;
        ConstruccionNecesitaAdyacente = false;
        puedeDestruir = false;
        consumible = true;
    }

    public void setTileConstruible(string nombre, int id, Sprite ImagenInventario, Tile tile)
    {
        this.nombre = nombre;
        this.id = id;
        this.ImagenInventario = ImagenInventario;
        this.tile = tile;
        esHerramienta = false;
        puedeAtacar = false;
        puedeDefender = false;
        puedeConstruir = true;
        ConstruccionNecesitaAdyacente = true;
        puedeDestruir = true;
    }

}
