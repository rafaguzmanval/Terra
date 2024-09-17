using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Inventario : MonoBehaviour
{
    public List<item> objetos;

    public item itemVacio;

    private Dictionary<int, int> indireccion = new Dictionary<int, int>();
    private Dictionary<int, item> inverso = new Dictionary<int, item>();

    [SerializeField]
    private List<Image> HuecoAureola;

    [SerializeField]
    private List<Text> HuecoTexto;

    [SerializeField]
    private List<SpriteRenderer> HuecoSprite;

    private int seleccion;

    private int pila = 0;

    private void Start()
    {
        for(int i = 0; i < objetos.Capacity; i++)
        {
            CambioSprite(i, objetos[i].ImagenInventario);
            CCantidad(i, 0);
        }
    }

    public void addItem(item item,int cantidad)
    {
        bool espacio = false;


        for(int i = 0; i < objetos.Capacity; i++)
        {
            if(objetos[i] == itemVacio)
            {
                pila = i;
                espacio = true;
                break;
            }
        }

        if(espacio)
        {
            objetos[pila] = item;
            indireccion[item.id] = pila;
            inverso[pila] = item;
            CCantidad(pila, cantidad);
            CambioSprite(pila, item.ImagenInventario);

        }

    }

    public void quitarItem(int hueco)
    {
        if(inverso.ContainsKey(hueco))
        {
            item i = inverso[hueco];
            indireccion.Remove(i.id);
        }

        objetos[hueco] = itemVacio;
        HuecoTexto[hueco].text = "";
        HuecoSprite[hueco].sprite = null;

        inverso.Remove(hueco);
    }

    public  void CambioSeleccionUI(int nuevaSeleccion)
    {
        HuecoAureola[seleccion].color = Color.black;

        HuecoAureola[nuevaSeleccion].color = Color.white ;

        seleccion = nuevaSeleccion;
    }

    private void CCantidad(int hueco, int cantidad)
    {
        if(cantidad > 0)
        {
            HuecoTexto[hueco].text = cantidad.ToString();
        }
        else
        {
            quitarItem(hueco);
        }

    }

    public void CambioCantidad(int id, int cantidad)
    {
        CCantidad(indireccion[id],cantidad);
    }

    public void incrementarCantidad(int id, int cantidad, Tile tile)
    {
        if(indireccion.ContainsKey(id))
        CCantidad(indireccion[id], int.Parse(HuecoTexto[indireccion[id]].text.ToString()) + cantidad);
        else
        {
            if(tile.name == "hongo" || tile.name == "seta" || tile.name == "cesped")
            {
                item nuevo = ScriptableObject.CreateInstance<item>();
                nuevo.setConsumible(tile.name, id, tile.sprite);

                if(tile.name == "seta")
                {
                    nuevo.vida = 10;
                }
                else if(tile.name == "hongo")
                {
                    nuevo.vida = -20;
                }
                
                addItem(nuevo,1);
            }
            else
            {
                addItem(new item(tile.name, id, tile.sprite, tile), 1);
            }
        }
    }

    public void decrementarCantidad(int id, int cantidad)
    {
        CCantidad(indireccion[id], int.Parse(HuecoTexto[indireccion[id]].text.ToString()) - cantidad);
    }

    public void CambioSprite(int hueco, Sprite nuevoSprite)
    {
        HuecoSprite[hueco].sprite = nuevoSprite;
        HuecoSprite[hueco].drawMode = SpriteDrawMode.Sliced;
        HuecoSprite[hueco].size = new Vector2(30,30);
    }







}
