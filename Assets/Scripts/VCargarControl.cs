using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Discovery;
using System.IO;

public class VCargarControl : NetworkBehaviour
{
    public Canvas lienzo;
    public Button botonQuitar;
    public Button[] espaciosGuardado;
    public Button[] borrar;
    public Text[] textoGuardado;
    public Dictionary<string, string> nombres = new Dictionary<string, string>();
    public Dictionary<string, int> correspondencia = new Dictionary<string, int>();
    public string[] directorios;
    public HostControl control;

    public void Destruir()
    {
        Destroy(lienzo.gameObject);
    }

    public void Inicializar()
    {
        if(!Directory.Exists(Application.dataPath + "/saves"))
        {
            Directory.CreateDirectory(Application.dataPath + "/saves");
        }
        directorios = Directory.GetDirectories(Application.dataPath + "/saves");
       // Debug.Log("<color=red>Directorios "+directorios.Length + "</color>");

        for (int j = 0; j < directorios.Length; j++)
        {

                string path = Application.dataPath + "/saves/";
                string n = directorios[j].Substring(path.Length);
            if (File.Exists(directorios[j] +"/"+ n + ".gd"))
            {
                nombres.Add(n, directorios[j]);
                correspondencia.Add(directorios[j], j);
            }

        }

        int i = 0;
        foreach(KeyValuePair<string,string> n in nombres)
        { 
            textoGuardado[i].text = "<b>" + n.Key + "</b>";
            string p = n.Value;
            borrar[i].onClick.AddListener(delegate { borra(p); });
            borrar[i].gameObject.SetActive(true);
            espaciosGuardado[i].onClick.AddListener(delegate { control.funcionCargar(n.Key); });
            i++;
        }

        for (; i < espaciosGuardado.Length; i++)
        {
           espaciosGuardado[i].onClick.AddListener(control.funcionCrear);
        }
    }


    public void borra(string path)
    {
        Debug.Log(path);
        if (Directory.Exists(path)) { Directory.Delete(path, true); }
        if (File.Exists(path + ".meta")) { File.Delete(path + ".meta"); }
        Debug.Log("se ha borrado " + path);



        //foreach (KeyValuePair<string, string> n in nombres)
        //{
        //    if(n.Value != path)
        //    {
        //        textoGuardado[i].text = "<b>" + n.Key + "</b>";
        //        string p = n.Value;
        //        borrar[i].onClick.AddListener(delegate { borra(p); });
        //        borrar[i].gameObject.SetActive(true);
        //        espaciosGuardado[i].onClick.AddListener(delegate { control.funcionCargar(n.Key); });
        //        i++;


        //    }

        //}
        int i = 0;
        for (; i < espaciosGuardado.Length; i++)
        {
            borrar[i].onClick.RemoveAllListeners();
            borrar[i].gameObject.SetActive(false);
            espaciosGuardado[i].onClick.RemoveAllListeners();
            espaciosGuardado[i].onClick.AddListener(control.funcionCrear);
            textoGuardado[i].text = "Espacio Libre";
        }

        nombres.Clear();
        correspondencia.Clear();

        Inicializar();
    }

}
