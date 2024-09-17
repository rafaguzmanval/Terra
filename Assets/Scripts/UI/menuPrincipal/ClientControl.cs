using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Mirror.Discovery
{

    public class ClientControl : NetworkBehaviour
    {

        public TerraNetworkManager network;
        public TerraDiscovery networkDiscovery;
        public InputField Input;
        public Button unirse;
        public Button atras;
        public Button buscar;
        public Text textoIP;

        readonly Dictionary<long, DiscoveryResponse> discoveredServers = new Dictionary<long, DiscoveryResponse>();

        string ips = "";

        GameObject[] listaBotonesServidor;


        public struct MensajeEntrada : NetworkMessage
        {
            public string nombre;
        }

        void Start()
        {
            Debug.Log("se inicia la escena");
            network = GameObject.Find("Network").GetComponent<TerraNetworkManager>();
            networkDiscovery = GameObject.Find("Network").GetComponent<TerraDiscovery>();
           
            networkDiscovery.OnDiscoveryFound.AddListener(OnDiscoveredServer);
            Input = GameObject.Find("InputField").GetComponent<InputField>();
            unirse = GameObject.Find("botonUnirse").GetComponent<Button>();
            atras = GameObject.Find("botonAtras").GetComponent<Button>();
            buscar = GameObject.Find("botonBuscar").GetComponent<Button>();

            listaBotonesServidor = GameObject.FindGameObjectsWithTag("botonServidor");

            foreach(GameObject go in listaBotonesServidor)
            {
                go.SetActive(false);
            }

            unirse.onClick.AddListener(funcionUnirse);
            atras.onClick.AddListener(funcionAtras);
            buscar.onClick.AddListener(BuscarServidores);
        }

        void Update()
        {

        }

        void BuscarServidores()
        {
            discoveredServers.Clear();
            networkDiscovery.StartDiscovery();
        }


        void funcionUnirse()
        {
            network.networkAddress = Input.text;
            network.StartClient();
        }

        void funcionBotonUnirse(string direccion)
        {
            network.networkAddress = direccion;
            network.StartClient();
        }

        public void funcionAtras()
        {
            Destroy(network.gameObject);
            SceneManager.LoadScene("MenuPrincipal");
        }

        public void OnDiscoveredServer(DiscoveryResponse info)
        {
            // Note that you can check the versioning to decide if you can connect to the server or not using this method
            discoveredServers[info.serverId] = info;
            if(!ips.Contains(info.Address.ToString()))
            {
                ips += info.Address.ToString() + "\n";

                int i = 0;
                while(i < listaBotonesServidor.Length)
                {
                    if(listaBotonesServidor[i].activeSelf == false || discoveredServers[i].Address == info.Address)
                    {
                        listaBotonesServidor[i].GetComponentInChildren<Text>().text =info.nombre + "::"+info.Address.ToString() + "        Jugadores:" + info.nplayers + "/" +info.maxPlayers;
                        listaBotonesServidor[i].GetComponent<Button>().onClick.AddListener(delegate { funcionBotonUnirse(info.Address.ToString()); });
                        listaBotonesServidor[i].SetActive(true);
                        break;
                    }

                    i++;
                }
            }
        }
    }
}
