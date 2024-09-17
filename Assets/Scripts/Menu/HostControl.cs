using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class HostControl : NetworkBehaviour
{

    public TerraNetworkManager network;
    public InputField InputSemilla;

    public GameObject lienzo;

    public Button botonCrear;

    public Button botonCargar;

    public Button botonRecarga;
    
    
    void Start()
    {
        Debug.Log("se inicia la escena");
        network = GameObject.Find("Network").GetComponent<TerraNetworkManager>();
        InputSemilla = GameObject.Find("InputField").GetComponent<InputField>();
        botonCrear = GameObject.Find("botonCrear").GetComponent<Button>();
        botonCargar = GameObject.Find("botonCargar").GetComponent<Button>();
        botonRecarga = GameObject.Find("botonRecarga").GetComponent<Button>();

        generarSemillaAleatoria();

        botonRecarga.onClick.AddListener(generarSemillaAleatoria);
        botonCrear.onClick.AddListener(funcionHost);
        botonCargar.onClick.AddListener(funcionCargar);
    }

    void generarSemillaAleatoria()
    {
        int alea = Random.Range(1000000, 9999999);
        InputSemilla.text = alea.ToString();

    }

    void Update()
    {
        
    }

    public void funcionHost()
    {
        network.semilla = int.Parse(InputSemilla.text);
        network.StartHost();
    }

    public void funcionCargar()
    {
        if(PlayerPrefs.GetInt("haymundo") == 1)
        {
            Debug.Log("Cargando Mundo");
            SaveSystem save = new SaveSystem();
            save.cargarMundo();
            //Dictionary<Vector3Int, int> diccionario = new Dictionary<Vector3Int, int>();
            Dictionary<int, int> dicX = new Dictionary<int, int>();
            Dictionary<int, int> dicY = new Dictionary<int, int>();

            int i = 0;
            foreach (int n in save.dicX)
            {
                dicX.Add(i, n);
                i++;
            }
            i = 0;
            foreach (int n in save.dicY)
            {
                dicY.Add(i, n);
                i++;
            }
            i = 0;
            foreach (int n in save.dicO)
            {
                Vector3Int vec = new Vector3Int(dicX[i], dicY[i], 0);
                network.diccionario.Add(vec, n);
                i++;
            }

            InputSemilla.text = save.semilla.ToString();

            funcionHost();

        }
    }
    


}
