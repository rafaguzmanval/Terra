using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;


public class SaveSystem
{

    public int semilla;
    public List<int> dicX;
    public List<int> dicY;
    public List<int> dicO;

    public List<string> nombreJugador;
    public List<V3> posJ;
    public List<string> ModoJuego;

    public SaveSystem()
    {
        semilla = 0;
        dicX = new List<int>();
        dicY = new List<int>();
        dicO = new List<int>();
        nombreJugador = new List<string>();
        posJ = new List<V3>();
        ModoJuego = new List<string>();
    }

    public void setSemilla(int sem)
    {
        semilla = sem;
    }

    public void setJugadores(Dictionary<string,PlayerProperties> j)
    {
        foreach(KeyValuePair<string,PlayerProperties> n in j)
        {
            nombreJugador.Add(n.Key);
            V3 nuevo = new V3(n.Value.posRespawn.x, n.Value.posRespawn.y, n.Value.posRespawn.z);
            posJ.Add(nuevo);
            ModoJuego.Add(n.Value.mode);
        }
    }

    public void setDiccionario(Dictionary<Vector3Int,int> dic)
    {
        foreach(KeyValuePair<Vector3Int,int> n in dic)
        {
            dicX.Add(n.Key.x);
            dicY.Add(n.Key.y);
            dicO.Add(n.Value);
        }
    }

    public void guardarMundo(string nombre)
    {
        BinaryFormatter bf = new BinaryFormatter();

        if (!Directory.Exists(Application.dataPath + "/saves"))
        {
            Directory.CreateDirectory(Application.dataPath + "/saves");
        }

        if(string.IsNullOrWhiteSpace(nombre) && string.IsNullOrEmpty(nombre))
        {
            nombre = "Nuevo_Mundo";
        }

        string path = Application.dataPath + "/saves" + "/" + nombre;
        Directory.CreateDirectory(Application.dataPath + "/saves" + "/"+ nombre);
        FileStream file = File.Create(path +"/"+ nombre + ".gd");

        PlantGuardado guardado = new PlantGuardado();

        guardado.cordX = dicX;
        guardado.cordY = dicY;
        guardado.objeto = dicO;
        guardado.semilla = semilla;
        guardado.nombreJugador = nombreJugador;
        guardado.posJ = posJ;
        guardado.ModoJuego = ModoJuego;

        bf.Serialize(file,guardado);

        file.Close();
    }
    
    public void cargarMundo(string nombre)
    {
        if(File.Exists(Application.dataPath + "/saves/"+nombre+"/"+nombre+".gd"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.dataPath + "/saves/" + nombre + "/" + nombre + ".gd", FileMode.Open);
            PlantGuardado guardado = (PlantGuardado) bf.Deserialize(file);

            semilla = guardado.semilla;
            dicX = guardado.cordX;
            dicY = guardado.cordY;
            dicO = guardado.objeto;
            nombreJugador = guardado.nombreJugador;
            posJ = guardado.posJ;
            ModoJuego = guardado.ModoJuego;

            file.Close();
        }
    }


}