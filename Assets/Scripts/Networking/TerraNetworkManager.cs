using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.Tilemaps;
using System.Collections.Generic;


/*
	Documentation: https://mirror-networking.com/docs/Components/NetworkManager.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

public class TerraNetworkManager : NetworkManager
{
    public GameObject grid;   
    public Dictionary<Vector3Int,int> diccionario = new Dictionary<Vector3Int, int>();

    GameObject scripter;
    public string nombreMundo;
    public int semilla = 0;
    private int nplayers = 0;


    public KeyValuePair<NetworkConnection,string> nombreHost;
    public Dictionary<NetworkConnection, string> jugadores = new Dictionary<NetworkConnection, string>();
    public Dictionary<string,PlayerProperties> CaracteristicasJugador = new Dictionary<string,PlayerProperties>();
    public Dictionary<string, int> permisos = new Dictionary<string, int>();
    public Dictionary<string, string> listanegra = new Dictionary<string, string>(); //listanegra <IP, motivo>
    public Dictionary<string, string> simpln = new Dictionary<string, string>(); // SIMPlificacionListaNegra<nombre,IP>

    public List<int> Chunks = new List<int>();
    public ChatBehaviour chat;

    public string mensajeSalida;


    #region Unity Callbacks

    #endregion

    #region Start & Stop


    #endregion

    #region Server System Callbacks

    /// <summary>
    /// Called on the server when a client disconnects.
    /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log("un jugador va a despedirse");
                if (jugadores.Count > 1 && jugadores.ContainsKey(conn)) //si el usuario que se desconecta ya estaba registrado, puede desconectarse devolviendo su nombre
                {
                    CMDdespedirse(jugadores[conn]);
                    jugadores.Remove(conn);
                }
        base.OnServerDisconnect(conn);


    }


    public void CMDdespedirse(string nombre)
    {
        Debug.Log("numero de jugadores actual: " + nplayers);

        chat = GameObject.FindGameObjectWithTag("Player").GetComponent<ChatBehaviour>();
        Debug.Log(nombre + " se ha desconectado");
        if(nplayers > 1)
        chat.CMDdesconexion(nombre);

        nplayers--;
    }

    #endregion


    /// <summary>
    /// This is invoked when a server is started - including when a host is started.
    /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.RegisterHandler<CrearPersonajeMensaje>(OnCreateCharacter);

    }


    /// <summary>
    /// This is called when a server is stopped - including when a host is stopped.
    /// </summary>
    public override void OnStopServer()
     { 
        //Debug.LogWarning("SERVER: SE HA DESTRUIDO EL GRID");
        NetworkServer.UnSpawn(grid);
        diccionario = new Dictionary<Vector3Int, int>();
        Chunks.Clear();
        jugadores = new Dictionary<NetworkConnection, string>();
        permisos = new Dictionary<string, int>();
        CaracteristicasJugador.Clear();
     }

    /// <summary>
    /// This is called when a client is stopped.
    /// </summary>
    public override void OnStopClient() { 
    }


    public void SendMessage(NetworkConnection conn, string n)
    {
        CrearPersonajeMensaje message = new CrearPersonajeMensaje
        {
            nombre = n
        };

        conn.Send(message);
        Debug.Log("se ha mandado mensaje de registro");
    }


    void OnCreateCharacter(NetworkConnectionToClient conn, CrearPersonajeMensaje message)
    {
        GameObject gameobject = Instantiate(playerPrefab);

        NetworkServer.AddPlayerForConnection(conn,gameobject);
 
    }


    public struct CrearPersonajeMensaje : NetworkMessage
    {
        public string nombre;
    }

    public void UpdateMap(Vector3Int posCelda, int valor)
    {
        diccionario[posCelda] = valor;
    }

    public int devolverValor(Vector3Int posCelda)
    {
        return diccionario[posCelda];
    }

    public int espaciosReservados()
    {
        return diccionario.Count;
    }

    public Dictionary<Vector3Int,int> devolverDiccionario()
    {
        return diccionario;
    }

    public void InstantiateHost()
    {
        scripter = Instantiate(spawnPrefabs[1]);    
    }

    public void InstantiateClient()
    {
       Instantiate(spawnPrefabs[2]);
    }


    public void SpawnNewGrid(int chunck)
    {
        grid = Instantiate(spawnPrefabs[0]);
        grid.gameObject.name = "grid" + chunck;
        NetworkServer.Spawn(grid);
    }

}
