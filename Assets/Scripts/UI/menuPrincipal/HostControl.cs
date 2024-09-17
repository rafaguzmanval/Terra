using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

namespace Mirror.Discovery
{

    public class HostControl : NetworkBehaviour
    {

        public TerraNetworkManager network;
        public TerraDiscovery networkDiscovery;

        public GameObject lienzo;

        public GameObject VentanaCrear;
        public GameObject VentanaCargar;
        public VcreacionComp interfazVC;
        public VCargarControl accesoCarga;

        public Button botonCrear;

        public Button botonCargar;

        void Start()
        {
            Debug.Log("se inicia la escena");
            network = GameObject.Find("Network").GetComponent<TerraNetworkManager>();
            botonCrear = GameObject.Find("botonCrear").GetComponent<Button>();
            botonCargar = GameObject.Find("botonCargar").GetComponent<Button>();
            networkDiscovery = GameObject.Find("Network").GetComponent<TerraDiscovery>();


            botonCrear.onClick.AddListener(funcionCrear);
            botonCargar.onClick.AddListener(fBotonCargar);
        }

        void generarSemillaAleatoria(string alea)
        {
            alea =  Random.Range(1000000, 9999999).ToString();

        }


        public void funcionCrear()
        {
            VentanaCrear = (GameObject)Resources.Load("Vcreacion");
            VentanaCrear = Instantiate(VentanaCrear);
            interfazVC= VentanaCrear.GetComponent<VcreacionComp>();
            interfazVC.control = this;
            interfazVC.aniadirHost();

            interfazVC.generarSemillaAleatoria();

        }

        public void funcionHost()
        {
            network.semilla = int.Parse(interfazVC.inputSemilla.text);
            network.nombreMundo = interfazVC.inputNombre.text;
            networkDiscovery.AdvertiseServer();
            network.StartHost();
        }

        public void fBotonCargar()
        {
            VentanaCargar = (GameObject)Resources.Load("Vcargar");
            VentanaCargar = Instantiate(VentanaCargar);
            accesoCarga = VentanaCargar.GetComponent<VCargarControl>();
            accesoCarga.control = this;
            accesoCarga.Inicializar();
        }

        public void funcionCargar(string nombre)
        {
                Debug.Log("Cargando Mundo");
                SaveSystem save = new SaveSystem();
                save.cargarMundo(nombre);
                //Dictionary<Vector3Int, int> diccionario = new Dictionary<Vector3Int, int>();
                Dictionary<int, int> dicX = new Dictionary<int, int>();
                Dictionary<int, int> dicY = new Dictionary<int, int>();
                Dictionary<int, string> j = new Dictionary<int, string>();
                Dictionary<int, string> M = new Dictionary<int, string>();

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
                i = 0;
                Debug.LogWarning(save.nombreJugador.Count);
                foreach (string n in save.nombreJugador)
                {
                    j[i] = n;
                    i++;
                }
                i = 0;
                //Debug.LogWarning(save.nombreJugador.Count);
                foreach (string n in save.ModoJuego)
                {
                    M[i] = n;
                    i++;
                }
                i = 0;
                foreach (V3 n in save.posJ)
                {
                    Vector3 nuevo = new Vector3(n.v1,n.v2,n.v3);
                    Debug.LogWarning(j[i] + " :"+nuevo);
                    network.CaracteristicasJugador[j[i]] = new PlayerProperties { name = j[i], posRespawn = nuevo, mode = M[i]};

                    i++;
                }

                network.nombreMundo = nombre;
                network.semilla = save.semilla;
                networkDiscovery.AdvertiseServer();
                network.StartHost();

        }

    }
}