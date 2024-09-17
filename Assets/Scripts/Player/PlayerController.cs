using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


namespace Eden.Player{

    public class PlayerController : NetworkBehaviour
    {
        public Sprite remarcador;

        //NetworkManager 
        public TerraNetworkManager network;
        public NetworkIdentity identidad;
        public Tilemap tilePrincipal;
        public Tilemap tileFondo;
        public Tilemap tileAuxiliar;
        public AudioSource fuenteSonido;
        public AudioClip sonidoDolor;
        public AudioClip sonidoGolpe;
        public string nombre;
        public Transform Sol;

        public static int dificultad = 0;

        public GameMode gmLocal = GameMode.survival;
        public bool muertoSync = false;

        public Text textoVentanaEmergente;
        public GameObject ventanaEmergente;

        public int chunkactual = 0;

        public Vector3 puntospawn = new Vector3(0, 0, 0);

        #region Gameobjects Partes del cuerpo y Rigidbody2D
        //Partes del cuerpo
        public Camera camaraJugador;
        public GameObject jugador;
        public GameObject cuerpo;
        public GameObject cabeza;
        public GameObject tronco;
        public GameObject pierna;
        public GameObject brazo;
        public GameObject mano;

        public Collider2D areaAtaque;

        private Color colorOrig;

        public Sprite cabezaMujer, troncoMujer, piernaMujer, brazoMujer;

        public Collider2D colisionPie;

        public Canvas InfoUser;
        public Text textInfoUser;

        public bool local = false;

        //RigidBody
        public Rigidbody2D rigid;

        #endregion

        #region Variables del Sistema de Inventario
        public Tile bloqueEnMano;
        public Item ItemEnMano;

        private Vector3Int ultPosconstruir = new Vector3Int(0,-500,-50);
        private Vector3Int vectorNulo = new Vector3Int(0, -500, -50);

        private int seleccion = 0;
        public Inventory inventario;

        private double t1 = 0, t2 = 0;
        private double tiempoPulsacion = 0.0;

        #endregion

        #region variables y UI de vitalidad
        private int maxHealth = 100;
        private int health = 100;

        private int damage = 1;
        private bool isDead = false;
        private float fallSpeed;
        private float fallSpeedMax = -44;
        private float mortalFallSpeed = -70;

        #endregion

        #region  variables del mapa
     //   private string nmundo;
     //   int limiteConstruccionMapaY = 100;
     //   const int limiteChunk = 20;
      //  const int umbralBorrarChunk = 12;
     //   const int umbralCargarChunk = 10;
        #endregion

        #region variables de estado

        public Sex sex;

        public bool esHost;

        private float movimientoHorizontal;
        public float speed = 10;
        public float jumpSpeed = 17;

        //private bool procesado = false;
        //Variables de estado
        public bool jumping = false; // booleano de saltar, true: puede saltar ; false: no puede saltar
        private bool flipX = true; // true: personaje mirando hacia la izquierda, false: personaje mirando hacia la derecha
        private bool canAttack = false;
        private bool canDefend = false;
        private bool canBuild = true;
        private bool canConsume = false;
        private bool ConstruccionNecesitaAdyacente = true;
        private bool canDestroy = true;
        private bool canPlay = false;
        private bool havePermissions = true;

        private bool tunnelate = false;

        private Vector3Int worldPosition = new Vector3Int(0, 0, 0);

        private GameMode gameMode = GameMode.survival;
        public bool pvp = false;

        private DeathCause deathCause = DeathCause.unknown;
        
        #endregion

        #region variables de sincronizacion
        private bool sehaSincronizado = false;

        public static Dictionary<Tile, int> DiccionarioID = new Dictionary<Tile, int>();
        public static Dictionary<int, Tile> DiccionarioTile = new Dictionary<int, Tile>();

        [SyncVar]
        private int sem;

        [SyncVar]
        private int TamDic = -546;


        [SyncVar(hook = nameof(getSincronizado))]
        bool sincronizado = true;
        #endregion

        #region Diccionarios de sincronizacion
        public static Dictionary<int, Tilemap> chunksCargados = new Dictionary<int, Tilemap>();
        public static Dictionary<Vector3Int, int> dicCarga = new Dictionary<Vector3Int, int>();
        public List<int> Chunks = new List<int>();
        #endregion

        // MÉTODOS 

        #region funciones Networking  
        public void OnEnable()
        {
        }

        public void DebugVE()
        {
            Debug.Log("PlayerController: se va a abrir la ventana emergente");
        }

        public void Desconectar()
        {
            if(!Application.isEditor)
            {
                Cursor.SetCursor(cursores.cursorDefecto, Vector2.zero, CursorMode.ForceSoftware);
            }


            GameObject[] grids = GameObject.FindGameObjectsWithTag("grid");

            for (int i = 0; i < grids.Length; i++)
            {
                Destroy(grids[i].gameObject);
            }

            if (isClientOnly)
            {
                network.StopClient();
            }
            else
            {
                foreach(var j in network.jugadores)
                {
                    if(j.Value != nombre)
                    {
                        chat.EscribirMsg("/kick " + j.Value + " Se ha perdido la conexión con el servidor");
                    }

                }

                network.StopHost();

            }
        }

        [Command]
        void CmdPermisoDeChat()
        {
            chat.netIdentity.AssignClientAuthority(connectionToClient);
        }

        void InicializarDiccionariosDeTiles()
        {

            Tile nulo = ScriptableObject.CreateInstance<Tile>();
            DiccionarioID[nulo] = 0;
            DiccionarioTile[0] = nulo;

            int i = 1;
            foreach (var n in texturas.tiles)
            {
                DiccionarioID[n.Value] = i;
                DiccionarioTile[i] = n.Value;
                i++;
            }

        }

        public override void OnStartServer()
        {
            if (isLocalPlayer && hasAuthority)
            {

                sem = network.semilla;
                GameObject grid = Instantiate(network.spawnPrefabs[0]);
                grid.gameObject.name = "grid";
                tilePrincipal = GameObject.Find("TilePrincipal").GetComponent<Tilemap>();
                tileFondo = GameObject.Find("TileFondo").GetComponent<Tilemap>();
                tileAuxiliar = GameObject.Find("TileAuxiliar").GetComponent<Tilemap>();
                texturas = GameObject.Find("CTexturas").GetComponent<CargadorTexturas>();
                texturas.Inicializar();

                InicializarDiccionariosDeTiles();
                InicializarInventario();


                //Se pide al servidor los datos necesarios para sincronizarse
                CMDsincronizacion(false);
                sehaSincronizado = true;
                //Debug.Log("CLIENT: Recibiendo datos de sincronizacion");
                SincronizarNuevoJugador();
                cargaMapa(0);

                grid = Instantiate(network.spawnPrefabs[0]);
                grid.gameObject.name = "grid";
                cargaMapa(-1);

                grid = Instantiate(network.spawnPrefabs[0]);
                grid.gameObject.name = "grid";
                cargaMapa(1);

                grid = Instantiate(network.spawnPrefabs[0]);
                grid.gameObject.name = "grid";
                cargaMapa(-2);

                grid = Instantiate(network.spawnPrefabs[0]);
                grid.gameObject.name = "grid";
                cargaMapa(2);

                grid = Instantiate(network.spawnPrefabs[0]);
                grid.gameObject.name = "grid";
                cargaMapa(-3);

                grid = Instantiate(network.spawnPrefabs[0]);
                grid.gameObject.name = "grid";
                cargaMapa(3);

                eventos = GameObject.Find("CTexturas").GetComponent<EventoTerreno>();
                eventos.jugador = this;
                eventos.tierra = texturas.tiles["tierra"];
                eventos.Inicializar();

            }

        }

        public override void OnStartClient()
        {
            if (!isLocalPlayer)
            {
                camaraJugador.enabled = false;
                gameObject.GetComponent<AudioListener>().enabled = false;
                rigid.simulated = false;
                rigid.isKinematic = true;
                textInfoUser.text = nombre.ToLower();

                if (!isClientOnly)
                {
                    //Debug.Log("HOST: El diccionario es LDic: " + network.espaciosReservados() + "   SDic: " + diccionarioCarga.Count);
                }
            }
            if (isLocalPlayer && hasAuthority && isClientOnly)
            {
                local = true;
                rigid.simulated = false;
                sexo = PlayerPrefs.GetInt("sexo");
                texturas = GameObject.Find("CTexturas").GetComponent<CargadorTexturas>();
                texturas.Inicializar();
                InicializarDiccionariosDeTiles();
                InicializarInventario();
                eventos = GameObject.Find("CTexturas").GetComponent<EventoTerreno>();
                eventos.jugador = this;
                eventos.tierra = texturas.tiles["tierra"];
                eventos.Inicializar();
                nombre = PlayerPrefs.GetString("nombre");
                esHost = false;
                addJugador(nombre, sexo);
                CMDPedirMensajesConTodosLosNombres(); // función de concentración de datos en el nodo usuario que está recien conectado
                CMDpedirNombreServidor(); // se pide el nombre del mundo



                chat.CMDregistro(nombre);
                textInfoUser.text = nombre.ToLower();
                CMDsincronizacion(false);
                sehaSincronizado = true;
                //Debug.Log("CLIENT: Recibiendo datos de sincronizacion");
                SincronizarNuevoJugador();

            }
            else if (isLocalPlayer && hasAuthority)
            {
                local = true;
                sexo = PlayerPrefs.GetInt("sexo");
                nombre = PlayerPrefs.GetString("nombre");
                nmundo = network.nombreMundo;
                esHost = true;
                addJugador(nombre, sexo);
                chat.Registro(nombre);
                textInfoUser.text = nombre.ToLower();
                setHost(nombre.ToLower());
                guardarPartida("Mundo guardado automáticamente");
            }

        }

        private void InicializarInventario()
        {
            inventario.addItem(new item("tierra", DiccionarioID[texturas.tiles["tierra"]], texturas.tiles["tierra"].sprite, texturas.tiles["tierra"]), 100);
            inventario.addItem(new item("piedra", DiccionarioID[texturas.tiles["piedra"]], texturas.tiles["piedra"].sprite, texturas.tiles["piedra"]), 100);
            inventario.addItem(new item("ladrillo", DiccionarioID[texturas.tiles["ladrillo"]], texturas.tiles["ladrillo"].sprite, texturas.tiles["ladrillo"]), 100);
            inventario.addItem(new item("ladrillopiedra", DiccionarioID[texturas.tiles["ladrillopiedra"]], texturas.tiles["ladrillopiedra"].sprite, texturas.tiles["ladrillopiedra"]), 100);
        }

        private void analizarPuntoSpawnInicial()
        {
            for (int i = 0; i < 10; i++)
            {
                if (tilePrincipal.GetTile(new Vector3Int(0, i, 0)) == null)
                {
                    puntospawn = new Vector3(0, i + 3, 0);
                    break;
                }
            }
        }

        [Command]
        private void CMDpedirNombreServidor()
        {
            RPCdevolvernombreServidor(network.nombreMundo);
        }

        [TargetRpc]
        private void RPCdevolvernombreServidor(string nombre)
        {
            nmundo = nombre;
        }

        [Command]
        private void setHost(string nombre)
        {
            network.nombreHost = new KeyValuePair<NetworkConnection, string>(connectionToClient, nombre);
        }

        [Command]
        //Procedimiento para añadir al jugador a la lista de jugadores del servidor y hacerlo reconocible para el resto de usuarios
        private void addJugador(string nombre, int sexo)
        {
            //Siempre se añadirán jugadores cuyo nombre sea distinto al resto de jugadores que existen en la sesión, 
            // esto se hace para evitar ambigüedades al ejecutar comandos y realizar ciertas rutinas del servidor
            if (!network.jugadores.ContainsValue(nombre.ToLower()))
            {
                if (network.listanegra.ContainsKey(connectionToClient.address))
                {
                    //impide la entrada a jugadores baneados
                    Debug.Log(connectionToClient.address + " ha intentado entrar");
                    ban(network.listanegra[connectionToClient.address]);
                }
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    //Se registra al jugador en la lista con un nombre en minúscula
                    network.jugadores[connectionToClient] = nombre.ToLower();

                    //Procedimiento para conocer si el usuario con el nombre previo estaba en una posición
                    Vector3 pos = network.CaracteristicasJugador.ContainsKey(nombre.ToLower()) ? network.CaracteristicasJugador[nombre.ToLower()].posRespawn : new Vector3(0,0,0);
                    devolverPuntoSpawn(pos);// esta función teletransporta al jugador recien conectado al punto del mapa de donde se desconectó

                    string gm = network.CaracteristicasJugador.ContainsKey(nombre.ToLower()) ? network.CaracteristicasJugador[nombre.ToLower()].modoJuego : "survival";

                    //
                    network.CaracteristicasJugador[nombre.ToLower()] = new PlayerProperties { sex = sexo, name = nombre.ToLower(), posRespawn = pos, mode = gm };
                    IdentificarJugador(nombre.ToLower(), sexo, gm, dificultad);
                }

                Debug.Log(nombre + " añadido " + connectionToClient.ToString());

            }
            else
            {
                kick("Ya hay un usuario conectado llamado " + nombre.ToLower());
                chat.ServerMsg(network.nombreHost.Key, "Se ha expulsado a un jugador entrante que tenía el mismo nombre que un jugador ya conectado");
                //Se expulsa al jugador cuyo nombre está siendo usado.
            }

        }

        [TargetRpc]
        public void devolverPuntoSpawn(Vector3 pS)
        {
            Debug.Log("devuelto punto de spawn en " + pS.x + " " + pS.y);
            puntospawn.x = pS.x; puntospawn.y = pS.y;

            if (pS.x == 0 && pS.y == 0 && !isClientOnly)
            {
                analizarPuntoSpawnInicial();
            }

            if (!isClientOnly)
            {
                teletransporte(puntospawn);
            }
        }

        [ClientRpc]
        public void IdentificarJugador(string n, int sex, string gm, int dificultad)
        {
            nombre = n;
            sexo = sex;
            gameObject.name = "P" + n;
            textInfoUser.text = n;
            SetGamemode(gm);
            dificultad = dificultad;

            if (sexo == 0) // es mujer
            {
                //Debug.Log("cambio de sexo para " + nombre);
                cabeza.GetComponent<SpriteRenderer>().sprite = cabezaMujer;
                tronco.GetComponent<SpriteRenderer>().sprite = troncoMujer;
                pierna.GetComponent<SpriteRenderer>().sprite = piernaMujer;
                brazo.GetComponent<SpriteRenderer>().sprite = brazoMujer;
                sonidoDolor = sonidoDolorMujer;
            }

        }

        [Command]
        public void CMDPedirMensajesConTodosLosNombres()
        {
            foreach (KeyValuePair<NetworkConnection, string> n in network.jugadores)
            {
                bool eH = n.Value == network.nombreHost.Value ? true : false;

                IdentificarJugadoresParaClienteNuevo(n.Key.identity.netId, n.Value, network.CaracteristicasJugador[n.Value].sexo, eH);
            }
        }

        [TargetRpc]
        public void IdentificarJugadoresParaClienteNuevo(uint uid, string n, int sexo, bool esHost)
        {

            GameObject[] jug = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < jug.Length; i++)
            {
                if (jug[i].GetComponent<NetworkIdentity>().netId == uid && netId != uid)
                {
                    jug[i].name = "P" + n;
                    jug[i].GetComponent<PlayerController>().nombre = n;
                    jug[i].GetComponent<PlayerController>().textInfoUser.text = n;
                    jug[i].GetComponent<PlayerController>().esHost = esHost;
                    if (sexo == 0) // es mujer
                    {
                        //Debug.Log("cambio de sexo para " + n);
                        jug[i].GetComponent<PlayerController>().cabeza.GetComponent<SpriteRenderer>().sprite = cabezaMujer;
                        jug[i].GetComponent<PlayerController>().tronco.GetComponent<SpriteRenderer>().sprite = troncoMujer;
                        jug[i].GetComponent<PlayerController>().pierna.GetComponent<SpriteRenderer>().sprite = piernaMujer;
                        jug[i].GetComponent<PlayerController>().brazo.GetComponent<SpriteRenderer>().sprite = brazoMujer;
                    }

                    break;
                }
            }
        }


        [TargetRpc]
        public void kick(string msg)
        {
            VentanaEmergenteExpulsion(msg);
        }

        [TargetRpc]
        public void ban(string msg)
        {
            VentanaEmergenteBaneo(msg);
        }

        public void VentanaEmergenteExpulsion(string msg)
        {
            tienePermisos = false;
            Instantiate(ventanaEmergente);
            textoVentanaEmergente = GameObject.Find("TextoVE").GetComponent<Text>();
            textoVentanaEmergente.text = "";
            textoVentanaEmergente.text += "<color=red>Has sido expulsado</color>\n\n";
            textoVentanaEmergente.text += "Motivo:\n";
            textoVentanaEmergente.text += msg;

            Desconectar();
        }

        public void VentanaEmergenteBaneo(string msg)
        {
            tienePermisos = false;
            Instantiate(ventanaEmergente);
            textoVentanaEmergente = GameObject.Find("TextoVE").GetComponent<Text>();
            textoVentanaEmergente.text = "";
            textoVentanaEmergente.text += "<color=red>Has sido vetado permanentemente</color>\n\n";
            textoVentanaEmergente.text += "Motivo:\n";
            textoVentanaEmergente.text += msg;

            Desconectar();
        }

        public override void OnStopClient()
        {
            if (isLocalPlayer && hasAuthority && isClientOnly)
            {
                botonReaparecer.SetActive(true);
                textoFinJuego.enabled = true;
                dicCarga.Clear();
                chunksCargados.Clear();
            }

        }

        public override void OnStopServer()
        {

            if (isLocalPlayer && hasAuthority)
            {
                botonReaparecer.SetActive(true);
                textoFinJuego.enabled = true;
                dicCarga.Clear();
                chunksCargados.Clear();
            }
            base.OnStopServer();
        }

        #endregion

        #region comandos de sincronizacion

        [Command]
        void CMDsincronizacion(bool sync)
        {
            sincronizado = sync;
        }

        [Server]
        public void getSincronizado(bool oldValue, bool newValue)
        {
            if (newValue)
                Debug.Log("SERVER: los jugadores estan sincronizados");
            else
            {
                Debug.Log("SERVER: los jugadores no estan sincronizados");
            }
        }


        [Command]
        private void CMDmandarNuevoChunk(int i)
        {
            if (!network.Chunks.Contains(i))
                network.Chunks.Add(i);

            if (!Chunks.Contains(i))
            {
                Chunks.Add(i);
            }
        }


        [Client]
        private void LoadMap(int chunk)
        {
            if (GameObject.Find("grid" + chunk) == null)
            {
                //GameObject grid = Instantiate(network.spawnPrefabs[0]);
                //algoritmo
                Chunks.Add(chunk);
                GameObject grid = GameObject.Find("grid");
                grid.gameObject.name = "grid" + chunk;
                cargarChunk(chunk);
            }
        }

        [Client]
        private void borraMapa(int chunk)
        {
            Chunks.Remove(chunk);
            chunksCargados.Remove(chunk);
            borrarChunk(chunk);
            GameObject grid = GameObject.Find("grid" + chunk);
            grid.gameObject.name = "grid";
        }

        [Client]
        private void cargarChunk(int chunkx)
        {
            //Debug.Log("Se va a cargar nuevo chunk");

            Tilemap mapa = GameObject.Find("grid" + chunkx).transform.GetChild(1).GetComponent<Tilemap>();
            Tilemap mapaFondo = GameObject.Find("grid" + chunkx).transform.GetChild(0).GetComponent<Tilemap>();
            StartCoroutine(texturas.GenerarMapaAleatorio(chunkx, mapa, mapaFondo, sem));
            

            // Se van indicando al servidor los chunks que ha ido cargando un jugador
            if (hasAuthority)
                CMDmandarNuevoChunk(chunkx);

            if (!chunksCargados.ContainsKey(chunkx))
                chunksCargados.Add(chunkx, mapa);
            //Debug.Log("chunks cargados:  " + chunksCargados.Count);

            //Cuando se carga un nuevo chunk se inspecciona en el diccionario de actualizaciones de tiles cuales celdas han sido modificadas, para que se pinten sobre el chunk cargado

            //Debug.Log("CLIENT: espera terminada");
            bool haycarga = false;
            //Debug.Log("CLIENT: comienza la carga del diccionario " + "   SDic: " + diccionarioCarga.Count);
            foreach (KeyValuePair<Vector3Int, int> bloquedeCarga in dicCarga)
            {

                if (esCordEnChunck(bloquedeCarga.Key, chunkx))
                {
                    haycarga = true;
                    //Debug.Log("CLIENT: cargando...");
                    int valor = bloquedeCarga.Value;
                    Tile asignar;
                    if (valor != 0)
                    {
                        asignar = DiccionarioTile[valor];
                    }
                    else
                    {
                        asignar = null;
                    }

                    mapa.SetTile(bloquedeCarga.Key, asignar);

                }

            }
            if (!haycarga && isClientOnly)
            {
                Debug.LogWarning("CLIENT: no se han transferido ningún dato");
            }
            else
            {
                //Debug.Log("CLIENT: se ha descargado el diccionario");
            }




        }

        [Client]
        private void borrarChunk(int chunk)
        {
            Tilemap mapa = GameObject.Find("grid" + chunk).transform.GetChild(1).GetComponent<Tilemap>();
            Tilemap mapaFondo = GameObject.Find("grid" + chunk).transform.GetChild(0).GetComponent<Tilemap>();

            mapa.ClearAllTiles();
            mapaFondo.ClearAllTiles();
        }

        //Cada vez que un cliente actualiza el mapa, le manda un comando al servidor para que actualice el diccionario de tiles
        [Server]
        private void actualizarDiccionario(Vector3Int posCelda, int valor)
        {
            //diccionarioCarga[posCelda] = valor;
            actualizarDicCarga(posCelda, valor);
            network.actualizarDiccionario(posCelda, valor);
            //Debug.Log("SERVER: Se ha registrado la modificacion de la celda " + posCelda.x + "," + posCelda.y + "   SDic: " + dicCarga.Count + "    LDic: " + network.espaciosReservados()  );
        }

        //Una vez actualizado el diccionario del servidor se manda la información a todos los clientes para que cuando carguen parte
        // del mapa (un chunk) puedan actualizarlo con todas las modificaciones que se hicieron previamente
        [ClientRpc]
        private void actualizarDicCarga(Vector3Int posCelda, int valor)
        {
            dicCarga[posCelda] = valor;
        }

        //Todos los datos que están almacenado en el diccionario local de actualizaciones que posee el host se sincronizan mandandose 
        // mediante RPC al diccionario del nuevo jugador
        [Command]
        public void SincronizarNuevoJugador()
        {
            TamDic = network.diccionario.Count;
            //Debug.Log("SERVER: ha comenzado la sincronizacion  LDic:" + network.espaciosReservados() );
            bool haySync = false;
            sem = network.semilla;
            foreach (KeyValuePair<Vector3Int, int> bloquedeLocal in network.devolverDiccionario())
            {
                haySync = true;
                //Debug.Log("SERVER: sincronizando...");
                dicCarga[bloquedeLocal.Key] = bloquedeLocal.Value;
                SincronizarDicCarga(bloquedeLocal.Key, bloquedeLocal.Value);
                //KeyValuePair<Vector3Int, int> bloqueSync = bloquedeLocal;
                //diccionarioCarga[bloqueSync.Key] = bloqueSync.Value;
            }

            if (!haySync)
                Debug.LogError("SERVER: No se han cargado los datos");
            else
            {
                //Debug.Log("SERVER: datos de sincronizacion actualizados");
            }
            sincronizado = true;

        }

        //Se mandan los datos del diccionario del servidor al diccionario local del nuevo jugador
        [TargetRpc]
        private void SincronizarDicCarga(Vector3Int key, int value)
        {
            dicCarga[key] = value;
        }


        #endregion

        #region métodos de detección de mapa
        private bool esCordEnChunck(Vector3Int coord, int chunck)
        {
            if ((chunck) * limiteChunk <= coord.x && (chunck + 1) * limiteChunk > coord.x)
            {
                return true;
            }

            return false;
        }

        public int CordDeChunk(Vector3Int coord)
        {
            if (coord.x >= 0)
            {
                return (coord.x) / limiteChunk;
            }
            else
            {
                return ((coord.x + 1) / limiteChunk) - 1;
            }

        }

        private bool esEnChunkActualADerecha()
        {
            if ((chunkactual + 1) * limiteChunk >= tilePrincipal.WorldToCell(gameObject.transform.position).x)
            {
                return true;
            }
            return false;
        }

        private bool esEnChunkActualAIzquierda()
        {
            if (chunkactual * limiteChunk <= tilePrincipal.WorldToCell(gameObject.transform.position).x)
            {
                return true;
            }
            return false;
        }

        private bool VaACambiardeChunkActualADerecha(int limite)
        {
            if ((chunkactual + 1) * limiteChunk - limite < tilePrincipal.WorldToCell(gameObject.transform.position).x)
            {
                return true;
            }

            return false;
        }

        private bool VaACambiardeChunkActualAIzquierda(int limite)
        {
            if (chunkactual * limiteChunk + limite > tilePrincipal.WorldToCell(gameObject.transform.position).x)
            {
                return true;
            }

            return false;
        }

        private void DeteccionMapa()
        {


            if (tilePrincipal != null)
            {

                Vector3Int posActual = tilePrincipal.WorldToCell(gameObject.transform.position);

                if (!esEnChunkActualAIzquierda())
                {
                    chunkactual--;
                    tilePrincipal = chunksCargados[chunkactual];
                }

                if (!esEnChunkActualADerecha())
                {
                    chunkactual++;
                    tilePrincipal = chunksCargados[chunkactual];
                }

                if (posActual.x != posicionEnMundo.x)
                {

                    if (Chunks.Contains(chunkactual + 4) && isLocalPlayer && VaACambiardeChunkActualAIzquierda(umbralBorrarChunk))
                    {
                        // Debug.Log("va a borrar el chunk " + chunkactual + 2);
                        borraMapa(chunkactual + 4);
                    }
                    if (!Chunks.Contains(chunkactual - 3) && isLocalPlayer && VaACambiardeChunkActualAIzquierda(umbralCargarChunk))
                    {
                        cargaMapa(chunkactual - 3);
                    }

                    if (Chunks.Contains(chunkactual - 4) && isLocalPlayer && VaACambiardeChunkActualADerecha(umbralBorrarChunk))
                    {
                        //Debug.Log("va a borrar el chunk " + chunkactual + 2);
                        borraMapa(chunkactual - 4);
                    }
                    if (!Chunks.Contains(chunkactual + 3) && isLocalPlayer && VaACambiardeChunkActualADerecha(umbralCargarChunk))
                    {
                        cargaMapa(chunkactual + 3);

                    }

                    posicionEnMundo = posActual;

                }
            }

        }

        #endregion

        #region Métodos MonoBehaviour

        public void awake()
        {
            if (isClient && isLocalPlayer)
            {

            }


        }


        void Start()
        {
            if (isLocalPlayer)
            {
                inicializarUI();
                if (!inventario.objetos[0].esHerramienta)
                {
                    bloqueEnMano = inventario.objetos[0].tile;
                    inventario.CambioSeleccionUI(0);
                }

            }

        }

        [Client]
        private void Update()
        {
            float fps = 1.0f / Time.unscaledDeltaTime;
            if(textoFPS == null)
            {
                textoFPS = GameObject.Find("FPScaja").GetComponent<Text>();
            }
            textoFPS.text = "FPS : " + fps.ToString();

            if(fps < 20)
            Debug.LogWarning("Bajón de FPS: " + fps );


            puedeJugar = !muerto && hasAuthority && sincronizado && tienePermisos;
            procesado = dicCarga.Count == TamDic && TamDic > -1;

            if (puedeJugar)
            {
                if (sehaSincronizado && isClientOnly && procesado)
                {

                    Debug.LogWarning("Se ha terminado la sincronización");
                    GameObject grid = Instantiate(network.spawnPrefabs[0]);
                    grid.name = "grid";
                    tilePrincipal = grid.transform.GetChild(1).GetComponent<Tilemap>();
                    tileFondo = grid.transform.GetChild(0).GetComponent<Tilemap>();
                    cargaMapa(0);

                    grid = Instantiate(network.spawnPrefabs[0]);
                    grid.name = "grid";
                    cargaMapa(1);

                    grid = Instantiate(network.spawnPrefabs[0]);
                    grid.name = "grid";
                    cargaMapa(-1);

                    grid = Instantiate(network.spawnPrefabs[0]);
                    grid.name = "grid";
                    cargaMapa(2);

                    grid = Instantiate(network.spawnPrefabs[0]);
                    grid.name = "grid";
                    cargaMapa(-2);

                    grid = Instantiate(network.spawnPrefabs[0]);
                    grid.gameObject.name = "grid";
                    cargaMapa(-3);

                    grid = Instantiate(network.spawnPrefabs[0]);
                    grid.gameObject.name = "grid";
                    cargaMapa(3);

                    if (puntospawn.x == 0)
                        analizarPuntoSpawnInicial();
                    else
                    {
                        teletransporte(puntospawn);
                    }

                    rigid.simulated = true;
                    sehaSincronizado = false;
                    PantallaCarga(false);

                }

                DeteccionMapa();
                actualizarVida();

                if(!chat.chatActivo)
                InputsGestion();

            }
            if (!sincronizado && isClientOnly)
            {
                //Debug.Log("se va a cargar el mapa");
                sehaSincronizado = true;
                PantallaCarga(true);
            }

        }

        [Client]
        private void LateUpdate()
        {

            muertePorCaerAlVacio();

            if (pvpSync)
            {
                CambiarTextoInfo("<color=red>" + nombre.ToLower() + "</color>");
            }
            else
            {
                CambiarTextoInfo("<color=black>" + nombre.ToLower() + "</color>");
            }

            if(pvp != pvpSync && isLocalPlayer)
            {
                actualizarPVP(pvp);
            }
        }

        [ClientRpc]
        private void actualizarPVP(bool pvp)
        {
            pvpSync = pvp;
        }

        

        [Client]
        void FixedUpdate()
        {
            if (puedeJugar && sincronizado && !chat.chatActivo)
            {

                InputsMovimiento();

                ApuntaBrazoACursor();
            }

        }

        #endregion

        #region Inputs

        private void InputsMovimiento()
        {
            movimientoHorizontal = Input.GetAxisRaw("Horizontal") * velocidad;

            if (rigid.velocity.x == 0 && !saltar && !colisionPie.IsTouchingLayers() && rigid.IsTouchingLayers())
            {
                movimientoHorizontal = 0;
            }

            rigid.velocity = new Vector2(movimientoHorizontal, rigid.velocity.y);


            if (!saltar)
            {
                movimientoHorizontal = movimientoHorizontal / 1.5f;
                velocidadCaida = rigid.velocity.y;
            }

            if (Input.GetKey(KeyCode.D))
            {

                if (flipX)
                {
                    GirarEnEjeX();

                    flipX = false;
                }

            }

            if (Input.GetKey(KeyCode.A))
            {

                if (!flipX)
                {
                    GirarEnEjeX();
                    flipX = true;
                }
            }

            if ((Input.GetKey(KeyCode.Space) && saltar))
            {
                rigid.velocity = new Vector2(movimientoHorizontal, velocidadSalto);
            }


            Sol.position = new Vector3(Sol.position.x, 200, Sol.position.z);
        }

        private void InputsGestion()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                tunelar = true;
            }
            else
            {
                tunelar = false;
            }



            CambiarCursor();

            MapaAuxiliar();


            if (Input.GetMouseButton(0)) // si pulsa click izquierdo
            {
                t2 = Time.realtimeSinceStartupAsDouble;
                tiempoPulsacion = t2 - t1;

                if (puedeDestruir && tiempoPulsacion >= 0.4 && mantieneClikizquierdo &&  gamemode == "survival")
                {
                    DestruirTile();
                    t1 = Time.realtimeSinceStartupAsDouble;
                }
                else if (gamemode == "creative")
                {
                    DestruirTile();
                }

                // rellenar animacion

            }
            else
            {
                tiempoPulsacion = 0;
            }

            if (Input.GetMouseButtonDown(0))
            {
                t1 = Time.realtimeSinceStartupAsDouble;
                mantieneClikizquierdo = true;


                if (puedeAtacar)
                    Atacar();


            }

            if(Input.GetMouseButtonUp(0))
            {
                mantieneClikizquierdo = false;
            }

            if (Input.GetMouseButton(1))// si pulsa click derecho
            {
                if (puedeConstruir)
                    ConstruirObjetoEnLaMano();

                if (puedeDefender)
                    Defender();

                // rellenar animacion
            }

            if(Input.GetMouseButtonDown(1))
            {

                if (puedeConsumir)
                    Consumir();
            }


            if (Input.mouseScrollDelta.y < -0.1f)
            {
                // Debug.Log("SiguienteObjetoInventario");
                SiguienteObjetoInventario();
            }

            if (Input.mouseScrollDelta.y > 0.1f)
            {
                // Debug.Log("AnteriorObjetoInventario");
                AnteriorObjetoInventario();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                nuevoRespawn();
            }

            if (Input.GetKeyDown(KeyCode.Q) && bloqueEnMano != null)
            {
                int cantidad = 1;
                if(Input.GetKey(KeyCode.LeftShift))
                {
                    cantidad = 10;
                }

                if (DiccionarioID.ContainsKey(bloqueEnMano))
                {
                    inventario.decrementarCantidad(DiccionarioID[bloqueEnMano], cantidad);
                    cargarItem(seleccion);
                }
            }

            if (Input.GetKeyDown(KeyCode.Z) && dificultad > 0)
            {
                Vector2 pos = camaraJugador.ScreenToWorldPoint(Input.mousePosition);
                CMDspawnZombie(pos);
            }

        }

        private void MapaAuxiliar()
        {
            if(Input.GetKeyDown(KeyCode.LeftControl))
            {
                CuadroAuxiliarActivo = !CuadroAuxiliarActivo;
            }

            if(CuadroAuxiliarActivo)
            {
                Vector3 PosCursor = Input.mousePosition;
                PosCursor = camaraJugador.ScreenToWorldPoint(PosCursor);
                Vector3Int posCelda = tilePrincipal.WorldToCell(PosCursor);
                int chunckRelativo = CordDeChunk(posCelda);
                Tilemap mapa = GameObject.Find("grid" + chunckRelativo).transform.GetChild(2).GetComponent<Tilemap>();

                GameObject[] mapas = GameObject.FindGameObjectsWithTag("grid");

                foreach (var m in mapas)
                {

                    m.transform.GetChild(2).GetComponent<Tilemap>().ClearAllTiles();
                }

                Tilemap mapaP = GameObject.Find("grid" + chunckRelativo).transform.GetChild(1).GetComponent<Tilemap>();
                Tilemap mapaF = GameObject.Find("grid" + chunckRelativo).transform.GetChild(1).GetComponent<Tilemap>();
                if (mapaP.GetTile(posCelda) != null && mapaF.GetTile(posCelda) != null)
                {
                    Tile remar = ScriptableObject.CreateInstance("Tile") as Tile;
                    remar.sprite = remarcador;
                    remar.colliderType = Tile.ColliderType.None;
                    remar.name = "remar";

                    mapa.SetTile(posCelda, remar);
                }


            }
            else
            {
                GameObject[] mapas = GameObject.FindGameObjectsWithTag("grid");

                foreach(var m in mapas)
                {
                    
                    m.transform.GetChild(2).GetComponent<Tilemap>().ClearAllTiles();
                }

            }

        }

        private void CambiarCursor()
        {
            if (!Application.isEditor && !muerto)
            {


                    Vector3 PosCursor = Input.mousePosition;
                    PosCursor = camaraJugador.ScreenToWorldPoint(PosCursor);
                    Vector3Int posCelda = tilePrincipal.WorldToCell(PosCursor);
                    int chunckRelativo = CordDeChunk(posCelda);
                    Tilemap mapa1 = GameObject.Find("grid" + chunckRelativo).transform.GetChild(1).GetComponent<Tilemap>();
                    Tile bloque = mapa1.GetTile<Tile>(posCelda);

                    bool exito1 = false;
                    if (!Input.GetMouseButton(1))
                    {
                        if (bloque == texturas.tiles["tocon"] || bloque == texturas.tiles["tronco"])
                        {
                            Cursor.SetCursor(cursores.cursorTalar, Vector2.zero, CursorMode.ForceSoftware);
                            exito1 = true;
                        }
                        else if (bloque != null)
                        {
                            Cursor.SetCursor(cursores.cursorMinar, Vector2.zero, CursorMode.ForceSoftware);
                            exito1 = true;
                        }

                        //if (bloque == texturas.tiles["carbon"] || bloque == texturas.tiles["hierro"] || bloque == texturas.tiles["piedra"] || bloque == texturas.tiles["tierra"] || bloque == texturas.tiles["hierbatierra"])
                        //{
                        //    Cursor.SetCursor(cursores.cursorMinar, Vector2.zero, CursorMode.ForceSoftware);
                        //    exito1 = true;
                        //}

                    }


                bool exito = false;

                if (puedeAtacar)
                    {


                    Collider2D[] collids = new Collider2D[5];

                    ContactFilter2D contact = new ContactFilter2D();
                    LayerMask laye = LayerMask.GetMask("RangoAtaque");
                    contact.SetLayerMask(laye);
                    contact.useTriggers = true;

                    areaAtaque.OverlapCollider(contact, collids);

                    if(pvp)
                    {
                        foreach (Collider2D c in collids)
                        {
                            if (c != null && c != gameObject.transform.GetChild(2).GetComponent<BoxCollider2D>())
                            {
                                if (!c.gameObject.GetComponentInParent<PlayerController>().muertoSync && c.gameObject.GetComponentInParent<PlayerController>().gmLocal == "survival" && c.gameObject.GetComponentInParent<PlayerController>().pvpSync)
                                {
                                    if (fuenteSonido.clip != sonidoGolpe || (fuenteSonido.clip == sonidoGolpe && !fuenteSonido.isPlaying) || gamemode == "creative")
                                        Cursor.SetCursor(cursores.cursorAtacar, Vector2.zero, CursorMode.ForceSoftware);
                                    else
                                        Cursor.SetCursor(cursores.cursorEspera, Vector2.zero, CursorMode.ForceSoftware);

                                    exito = true;
                                    break;

                                }

                            }
                        }

                    }


                    if (!exito)
                    {
                        LayerMask lay = LayerMask.GetMask("RangoAtaqueEnemigo");
                        contact.SetLayerMask(lay);
                        contact.useTriggers = true;
                        areaAtaque.OverlapCollider(contact, collids);

                        foreach (Collider2D c in collids)
                        {
                            if (c != null && c.gameObject.layer.ToString() == "10")
                            {
                                if (fuenteSonido.clip != sonidoGolpe || (fuenteSonido.clip == sonidoGolpe && !fuenteSonido.isPlaying) || gamemode == "creative")
                                    Cursor.SetCursor(cursores.cursorAtacar, Vector2.zero, CursorMode.ForceSoftware);
                                else
                                    Cursor.SetCursor(cursores.cursorEspera, Vector2.zero, CursorMode.ForceSoftware);

                                exito = true;
                                break;
                            }
                        }

                    }

                }

                if (!exito && !exito1)
                        Cursor.SetCursor(cursores.cursorDefecto, Vector2.zero, CursorMode.ForceSoftware);
            }

            if(muerto)
            {
                Cursor.SetCursor(cursores.cursorDefecto, Vector2.zero, CursorMode.ForceSoftware);
            }


        }

        #endregion

        #region Métodos del sistema de Vida y Reaparicion

        private void inicializarUI()
        {
            GameObject playerUI = GameObject.Find("CanvasUIplayer");
            playerUI.GetComponent<Canvas>().worldCamera = camaraJugador;
            playerUI.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;

            barraVida = GameObject.Find("Barravida");
            vidaTexto = GameObject.Find("TextoVida").GetComponent<Text>();

            textoFPS = GameObject.Find("FPScaja").GetComponent<Text>();

            textoFinJuego = GameObject.Find("TextoMuerte").GetComponent<Text>();
            botonReaparecer = GameObject.Find("BotonReaparecer");


            Button botonR = GameObject.Find("BotonReaparecer").GetComponent<Button>();
            //Debug.Log("FUNCION DEL BOTON REAPARECER ASIGNADA");
            botonR.onClick.AddListener(Reaparecer);
            botonReaparecer.SetActive(false);
            textoFinJuego.enabled = false;

            guardar = GameObject.Find("BotonGuardar").GetComponent<Button>();
            guardar.onClick.AddListener(guardarPartida);


            saltar = false;
        }

        private void UpdateHealth()
        {

            muertePorCaerAlVacio();



            if (vida > 0)
            {
                if (gamemode == "survival")
                {
                    vidaTexto.text = vida.ToString();
                    if (barraVida == null)
                    {
                        barraVida = GameObject.Find("Barravida");
                    }
                    Vector3 barra = barraVida.transform.localScale;
                    barra.x = vida * 500 / vidaMaxima;
                    barraVida.transform.localScale = barra;

                    if (vida != vidaNetwork)
                        CMDsyncVida(vida);
                }

            }
            else
            {
                //FATAL: PONERLO EN UIHANDLER DE VIDA
                if (gamemode == GameMode.survival)
                {
                    Vector3 barra = barraVida.transform.localScale;
                    barra.x = 0;
                    barraVida.transform.localScale = barra;
                    vidaTexto.text = "0";
                }

                isDead = true;
                CMDsincronizarMuerte(true);
                // Debug.Log("CLIENT: Jugador muerto");
                FinJuego();
            }


        }

        private void muertePorCaerAlVacio()
        {

            if (jugador.transform.position.y < -200)
            {
                // Debug.Log("Cliente: cayo al vacio");
                vida = 0;
                rigid.Sleep();
                causaMuerte = "vacio";
            }
        }

        public void setVida(int i)
        {

            vida = i;

        }

        [Command]
        private void CMDsyncVida(int i)
        {
            RPCsyncvida(i);
        }

        [ClientRpc]
        private void RPCsyncvida(int i)
        {
            if (vidaNetwork > i)
            {

                cabeza.GetComponent<SpriteRenderer>().color = Color.red;
                tronco.GetComponent<SpriteRenderer>().color = Color.red;
                pierna.GetComponent<SpriteRenderer>().color = Color.red;
                brazo.GetComponent<SpriteRenderer>().color = Color.red;
                Invoke("PonerColorOriginal", 0.2f);
                fuenteSonido.clip = sonidoDolor;
                fuenteSonido.Play();
            }
            else if (vidaNetwork < i)
            {
                cabeza.GetComponent<SpriteRenderer>().color = Color.green;
                tronco.GetComponent<SpriteRenderer>().color = Color.green;
                pierna.GetComponent<SpriteRenderer>().color = Color.green;
                brazo.GetComponent<SpriteRenderer>().color = Color.green;
                Invoke("PonerColorOriginal", 0.2f);
            }

            vidaNetwork = i;

        }

        private void PonerColorOriginal()
        {
            cabeza.GetComponent<SpriteRenderer>().color = Color.white;
            tronco.GetComponent<SpriteRenderer>().color = Color.white;
            pierna.GetComponent<SpriteRenderer>().color = Color.white;
            brazo.GetComponent<SpriteRenderer>().color = Color.white;
        }


        private void quitarVida(int damage, int x, int y)
        {
            if (gamemode == GameMode.survival)
            {
                setVida(health - damage);
                if (health > 100)
                {
                    health = 100;
                }
                else if (health < 0)
                {
                    setVida(0);
                }

                GameObject[] jgs = GameObject.FindGameObjectsWithTag("Player");

                if (health > 0)
                {
                    foreach (var j in jgs)
                    {
                        if (j.GetComponent<PlayerController>().local == true)
                        {
                            j.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, y));
                            j.GetComponent<Rigidbody2D>().AddForce(new Vector2(x, 0));
                        }
                    }

                }


            }
            //ANIMACION DAÑO
        }

        private void Consumir()
        {
            quitarVida(-ItemEnMano.vida, 0, 0);
            if (ItemEnMano.vida < 0)
            {
                deathCause = DeathCause.poison;
            }
            inventario.decrementarCantidad(ItemEnMano.id, 1);
            cargarItem(seleccion);
        }


        private void GameOver()
        {
            string msg = "ha muerto";

            switch (deathCause)
            {
                case DeathCause.zombie:
                    CMDspawnZombie(transform.position);
                    msg = nombre + " se convirtió en zombi";
                    break;
                case DeathCause.poison:
                    msg = nombre + " murió por yonki";
                    break;
                case DeathCause.abyss:
                    msg = nombre + " ha caido al vacío";
                    break;
                case DeathCause.player:
                    {
                        switch (Random.Range(1, 5))
                        {
                            case 1:
                                msg = "a " + nombre + " le han metido una paliza";
                                break;
                            case 2:
                                msg = nombre + " ha sido asesinado";
                                break;
                            case 3:
                                msg = nombre + " ha sido decapitado";
                                break;
                            case 4:
                                msg = "a " + nombre + " le han hecho polvo";
                                break;
                            case 5:
                                msg = "a" + nombre + " le han partio la cabeza";
                                break;
                            default:
                                msg = nombre + " ha sido asesinado";
                                break;
                        }
                    }
                    break;
                case DeathCause.fall:
                    msg = nombre + " ha caido al suelo y ha muerto";
                    break;
                default:
                    msg = nombre + " ha muerto";
                    break;
            }

            chat.CMDmsgPersonalizado("<color=purple>" + msg + "</color>");

            textoFinJuego.enabled = true;
            botonReaparecer.SetActive(true);
        }

        private void nuevoRespawn()
        {
            puntospawn = gameObject.transform.position;
            guardarCaracteristicas(nombre.ToLower(), puntospawn, gamemode);
            chat.EscribirMensajeLocal("Has cambiado tu punto de reaparición");
            Debug.Log("nuevoPuntoderespawn");
        }

        [Command]
        private void guardarCaracteristicas(string nombre, Vector3 nuevoSpawn, string ModoJuego)
        {
            network.CaracteristicasJugador[nombre] = new PlayerProperties { name = network.CaracteristicasJugador[nombre].nombre, sex = network.CaracteristicasJugador[nombre].sexo, posRespawn = nuevoSpawn, mode = ModoJuego };
        }

        [Command]
        private void guardarTodosLosPuntosDeRespawn()
        {
            RPCnuevoRespawn();
        }

        [ClientRpc]
        private void RPCnuevoRespawn()
        {
            nuevoRespawn();
        }

        [Client]
        public void Reaparecer()
        {

            rigid.velocity = new Vector2(0, 0);
            vida = vidaMaxima;

            //if (!Chunks.Contains(chunkactual))
            //{
            chunkactual = CordDeChunk(tilePrincipal.WorldToCell(puntospawn));
            //Debug.Log("teletransporte a " + chunkactual);
            int[] c = new int[Chunks.Count];
            int i = 0;
            foreach (var n in Chunks)
            {
                c[i] = n;
                i++;
            }

            for (int j = 0; j < c.Length; j++)
            {
                if (Chunks.Contains(c[j]))
                    borraMapa(c[j]);
            }

            cargaMapa(chunkactual);
            cargaMapa(chunkactual + 1);
            cargaMapa(chunkactual - 1);
            cargaMapa(chunkactual + 2);
            cargaMapa(chunkactual - 2);
            cargaMapa(chunkactual + 3);
            cargaMapa(chunkactual - 3);

            //}

            gameObject.transform.position = puntospawn;
            textoFinJuego.enabled = false;
            botonReaparecer.SetActive(false);
            muerto = false;
            CMDsincronizarMuerte(false);

            CMDreaparecerCuerpo();

        }

        [Command]
        void CMDreaparecerCuerpo()
        {
            RPCreaparecerCuerpo();
        }

        [ClientRpc]
        void RPCreaparecerCuerpo()
        {
            Debug.Log("va a reaparecer el cuerpo");
            cabeza.GetComponent<Renderer>().enabled = true;
            tronco.GetComponent<Renderer>().enabled = true;
            pierna.GetComponent<Renderer>().enabled = true;
            brazo.GetComponent<Renderer>().enabled = true;
            mano.GetComponent<Renderer>().enabled = true;


        }

        [Client]
        public void teletransporte(Vector3 nposicion)
        {
            int[] c = new int[Chunks.Count];
            int i = 0;
            int chunkNuevo = CordDeChunk(tilePrincipal.WorldToCell(nposicion));
            chunkactual = chunkNuevo;
            //if (!Chunks.Contains(chunkNuevo))
            //{
                foreach (var n in Chunks)
                {
                    c[i] = n;
                    i++;
                }

                for (int j = 0; j < c.Length; j++)
                {
                if (Chunks.Contains(c[j]))
                    borraMapa(c[j]);
                }


            cargaMapa(chunkactual);
            cargaMapa(chunkactual + 1);
            cargaMapa(chunkactual - 1);
            cargaMapa(chunkactual + 2);
            cargaMapa(chunkactual - 2);
            cargaMapa(chunkactual + 3);
            cargaMapa(chunkactual - 3);

            //}


            jugador.transform.position = nposicion;
        }


        #endregion

        #region Métodos de inventario

            private void SiguienteObjetoInventario()
            {
                seleccion = (seleccion + 1) % inventario.objetos.Count;
                cargarItem(seleccion);
            }

            private void AnteriorObjetoInventario()
            {
                seleccion = seleccion - 1;

                if(seleccion < 0)
                {
                    seleccion += inventario.objetos.Count;
                }
                cargarItem(seleccion);
                
            }

            [Command]
            private void CMDcargarItem(int i,bool esh)
            {
                RPCcargarItem(i,esh);
            }

            private void cargarItem(int select)
            {
                item nuevoitem = inventario.objetos[select];
                ItemEnMano = nuevoitem;

                bloqueEnMano = null;

                if(nuevoitem.esHerramienta)
                {
                    bloqueEnMano = null;
                }
                else
                {
                    bloqueEnMano = inventario.objetos[seleccion].tile;

                }

                puedeAtacar = nuevoitem.puedeAtacar;
                puedeConstruir = nuevoitem.puedeConstruir;
                puedeDefender = nuevoitem.puedeDefender;
                puedeDestruir = nuevoitem.puedeDestruir;
                puedeConsumir = nuevoitem.consumible;
                ConstruccionNecesitaAdyacente = nuevoitem.ConstruccionNecesitaAdyacente;

                inventario.CambioSeleccionUI(seleccion);


                CMDcargarItem(nuevoitem.id,nuevoitem.esHerramienta);

            // cambiarSpriteMano(inventario.objetos[seleccion].ImagenInventario , nuevoitem.esHerramienta);
                
            }
            
            [ClientRpc]
            private void RPCcargarItem(int i,bool esher)
            {
                cambiarSpriteMano(DiccionarioTile[i].sprite,esher);
            }

            private void cambiarSpriteMano(Sprite nuevoSprite, bool esHerramienta)
            {
                SpriteRenderer spriteObjetoMano = mano.GetComponent<SpriteRenderer>();

                spriteObjetoMano.sprite = nuevoSprite;
            }

        #endregion

        #region Métodos de colision
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isLocalPlayer)
            {


                //Debug.Log(velocidadCaida);
                if (velocidadCaida < umbralVelocidadCaidaMortal)
                {
                    causaMuerte = "caida";
                    quitarVida(vida,0,0);
                    velocidadCaida = 0;
                }
                else if (velocidadCaida < umbralVelocidadCaida)
                {
                    causaMuerte = "caida";
                    quitarVida((int)(-velocidadCaida / 1.5),0,0);
                    velocidadCaida = 0;
                }
                if (collision.transform.tag == "Suelo")
                {
                    Vector3Int posPierna = tilePrincipal.WorldToCell(pierna.transform.position);
                    Vector3Int posDebajo = posPierna;
                    posDebajo.y -= 1;

                    if (colisionPie.IsTouching(collision.collider))
                        saltar = true;
                }

            }

        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if(colisionPie.IsTouching(collision.collider))
            {
                saltar = true;         
            }
            else
            {
                saltar = false;
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.transform.tag == "Suelo")
            {
                saltar = false;
            }
        }
        #endregion
    
        #region Protocolo de cambio de Tile
        [Client]
        private void ConstruirObjetoEnLaMano()
        {

            Vector2 posCursor = Input.mousePosition;
            posCursor = camaraJugador.ScreenToWorldPoint(posCursor);

            Vector3Int posCelda = tilePrincipal.WorldToCell(posCursor);

            //asi nos aseguramos de que un cliente solo pueda mandar una petición de construir en una celda específica
            if(ultPosconstruir == vectorNulo || posCelda != ultPosconstruir)
            {
                ultPosconstruir = posCelda;
                int chunckRelativo = CordDeChunk(posCelda);
                Tilemap mapa = GameObject.Find("grid" + chunckRelativo).transform.GetChild(1).GetComponent<Tilemap>();
                Tilemap fondo = GameObject.Find("grid" + chunckRelativo).transform.GetChild(0).GetComponent<Tilemap>();
                Vector2 posJugador = jugador.transform.position;
                Vector3Int posJ = mapa.WorldToCell(posJugador);



                bool construccionLegal = true;
                int i = 0;
                if (fondo.GetTile(posCelda) == null)
                {

                    while (ConstruccionNecesitaAdyacente && i == 0)
                    {
                        construccionLegal = false;
                        Vector3Int posAdyacente = posCelda;
                        posAdyacente.x += 1;
                        int chunk = CordDeChunk(posAdyacente);
                        Tilemap map = GameObject.Find("grid" + chunk).transform.GetChild(1).GetComponent<Tilemap>();
                        if (map.GetTile(posAdyacente) != null && map.GetTile(posAdyacente).name != "cesped" && mapa.GetTile(posAdyacente) != texturas.tiles["hongo"] && mapa.GetTile(posAdyacente) != texturas.tiles["seta"])
                        {
                            construccionLegal = true;
                            break;
                        }
                        posAdyacente = posCelda;
                        posAdyacente.x -= 1;

                        chunk = CordDeChunk(posAdyacente);
                        map = GameObject.Find("grid" + chunk).transform.GetChild(1).GetComponent<Tilemap>();
                        if (map.GetTile(posAdyacente) != null && map.GetTile(posAdyacente).name != "cesped" && mapa.GetTile(posAdyacente) != texturas.tiles["hongo"] && mapa.GetTile(posAdyacente) != texturas.tiles["seta"])
                        {
                            construccionLegal = true;
                            break;
                        }

                        posAdyacente = posCelda;
                        posAdyacente.y -= 1;
                        chunk = CordDeChunk(posAdyacente);
                        map = GameObject.Find("grid" + chunk).transform.GetChild(1).GetComponent<Tilemap>();
                        if (map.GetTile(posAdyacente) != null && map.GetTile(posAdyacente).name != "cesped" && mapa.GetTile(posAdyacente) != texturas.tiles["hongo"] && mapa.GetTile(posAdyacente) != texturas.tiles["seta"])
                        {
                            construccionLegal = true;
                            break;
                        }

                        posAdyacente = posCelda;
                        posAdyacente.y += 1;
                        chunk = CordDeChunk(posAdyacente);
                        map = GameObject.Find("grid" + chunk).transform.GetChild(1).GetComponent<Tilemap>();
                        if (map.GetTile(posAdyacente) != null && map.GetTile(posAdyacente).name != "cesped" && mapa.GetTile(posAdyacente) != texturas.tiles["hongo"] && mapa.GetTile(posAdyacente) != texturas.tiles["seta"])
                        {
                            construccionLegal = true;
                        }
                        i = 1;
                    }

                }

                if (posCelda.y > limiteConstruccionMapaY || posCelda.y < -limiteConstruccionMapaY)
                {
                    construccionLegal = false;
                }

                bool rangoMinimo = posCelda.x > posJ.x || posCelda.x < posJ.x || posCelda.y > posJ.y || posCelda.y < posJ.y - 2;
                bool rangoMaximo = posCelda.x > posJ.x - 7 && posCelda.x < posJ.x + 7 && posCelda.y < posJ.y + 6 && posCelda.y > posJ.y - 8;

                if (rangoMinimo && rangoMaximo && construccionLegal)
                {

                    Vector3Int posInf = new Vector3Int(posCelda.x, posCelda.y - 1, posCelda.z);

                    if (mapa.GetTile(posCelda) == null || mapa.GetTile(posCelda).name == "cesped" || mapa.GetTile(posCelda) == texturas.tiles["hongo"] || mapa.GetTile(posCelda) == texturas.tiles["seta"])
                    {
                        Tile bloque = bloqueEnMano;

                        if (bloque != null && isLocalPlayer)
                        {
                            CMDponerBloque(posCelda, DiccionarioID[bloque], chunckRelativo);


                            if (DiccionarioID.ContainsKey(bloque) && gamemode == "survival")
                            {
                                inventario.decrementarCantidad(DiccionarioID[bloque], 1);
                                cargarItem(seleccion);
                            }
                        }
                    }
                    if (mapa.GetTile(posInf) == texturas.tiles["hierbatierra"])
                    {
                        CMDponerBloque(posInf, DiccionarioID[texturas.tiles["tierra"]], chunckRelativo);
                    }
                }
            }



        }

        [Command]
        public void CMDponerBloque(Vector3Int posCelda,int bloque,int chunck)
        {
            RPCponerBloque(posCelda,bloque,chunck);
            actualizarDiccionario(posCelda, bloque);
        }

        [ClientRpc]
        private void RPCponerBloque(Vector3Int posCelda, int bloque,int chunck)
        {
            //Debug.Log("CLIENT : Se ha puesto un tile en la celda" + posCelda.x + "," + posCelda.y + "  de tipo " + bloque);
            if(DiccionarioTile.ContainsKey(bloque) && (chunksCargados.ContainsKey(chunck)))
            {
                //Debug.Log("Cursor clica en chunck " + CordDeChunk(posCelda));
                chunksCargados[chunck].SetTile(posCelda, DiccionarioTile[bloque]);
                ultPosconstruir = vectorNulo;
            }
            else if(!(chunksCargados.ContainsKey(chunck)))
            {
                Debug.Log("Se ha construido en otro chunk que no esta cargado en el cliente");
            }
            else
            {
                string valores = "";
                foreach(var i in DiccionarioTile)
                {
                    valores += i.Key + ",";
                }
                Debug.LogError("Identificación de bloque no se ha transmitido correctamente,  enviado: " + bloque + " disponibles: " + valores);
            }

        }

        [Client]
        private void DestruirTile()
        {
            Vector3 PosCursor = Input.mousePosition;
            PosCursor = camaraJugador.ScreenToWorldPoint(PosCursor);
            Vector3Int posCelda = tilePrincipal.WorldToCell(PosCursor);
            PeticionDestruirTile(posCelda, PosCursor);

            if(posCelda == ultPosconstruir)
            {
                ultPosconstruir = vectorNulo;
            }

            if(tunelar)
            {
                Vector3Int posCelda1;
                Vector3Int posCelda2;

                if (PosCursor.y <= cabeza.transform.position.y && PosCursor.y >= pierna.transform.position.y)
                {
                    posCelda1 = new Vector3Int(posCelda.x, posCelda.y + 1, posCelda.z);
                    posCelda2 = new Vector3Int(posCelda.x, posCelda.y - 1, posCelda.z);
                }
                else
                {
                    posCelda1 = new Vector3Int(posCelda.x + 1, posCelda.y, posCelda.z);
                    posCelda2 = new Vector3Int(posCelda.x - 1, posCelda.y, posCelda.z);
                }


                int chunckRelativo1 = CordDeChunk(posCelda1);
                Tilemap mapa1 = GameObject.Find("grid" + chunckRelativo1).transform.GetChild(1).GetComponent<Tilemap>();
                PeticionDestruirTile(posCelda1, mapa1.CellToWorld(posCelda1));


                int chunckRelativo2 = CordDeChunk(posCelda2);
                Tilemap mapa2 = GameObject.Find("grid" + chunckRelativo2).transform.GetChild(1).GetComponent<Tilemap>();
                PeticionDestruirTile(posCelda2, mapa2.CellToWorld(posCelda2));
            }

            cargarItem(seleccion);
        }

        [Client]
        private void PeticionDestruirTile(Vector3Int posCelda, Vector3 PosCursor)
        {

            int chunckRelativo = CordDeChunk(posCelda);

            Tilemap mapa = GameObject.Find("grid" + chunckRelativo).transform.GetChild(1).GetComponent<Tilemap>();

            Vector2 posJugador = jugador.transform.position;
            Vector3Int posJ = mapa.WorldToCell(posJugador);


            bool rangoMaximo = posCelda.x > posJ.x - 7 && posCelda.x < posJ.x + 7 && posCelda.y < posJ.y + 6 && posCelda.y > posJ.y - 8;

            if (rangoMaximo && mapa.GetTile(posCelda) != null && isLocalPlayer && mapa.GetTile(posCelda) != texturas.tiles["hojas"])
            {
                //Debug.Log("Cursor clica en chunck " + CordDeChunk(posCelda));
                posCelda = mapa.WorldToCell(PosCursor);
                CMDquitarBloque(posCelda, chunckRelativo);

                Tile TileQuitado = mapa.GetTile<Tile>(posCelda);

                if (DiccionarioID.ContainsKey(TileQuitado) && gamemode == "survival")
                {
                    //AQUI SE DECIDEN TODAS LAS EXCEPCIONES AL RECOGER UN BLOQUE
                    if(TileQuitado == texturas.tiles["hierbatierra"])
                    {
                        inventario.incrementarCantidad(DiccionarioID[texturas.tiles["tierra"]], 1, texturas.tiles["tierra"]);
                    }
                    else if(TileQuitado == texturas.tiles["tocon"])
                    {
                        inventario.incrementarCantidad(DiccionarioID[texturas.tiles["tronco"]], 1, texturas.tiles["tronco"]);
                    }
                    else
                    {
                        inventario.incrementarCantidad(DiccionarioID[TileQuitado], 1, TileQuitado);
                    }

                }


                Vector3Int posSup = new Vector3Int(posCelda.x, posCelda.y + 1, posCelda.z);
                if (mapa.HasTile(posSup))
                {
                if( mapa.GetTile(posSup) == texturas.tiles["cesped"] || mapa.GetTile(posSup) == texturas.tiles["hongo"] || mapa.GetTile(posSup) == texturas.tiles["seta"])
                    {
                        CMDquitarBloque(posSup,chunckRelativo);
                    }

                while(mapa.GetTile(posSup) == texturas.tiles["tronco"] || mapa.GetTile(posSup) == texturas.tiles["tocon"])
                    {
                        CMDquitarBloque(posSup, chunckRelativo);
                        inventario.incrementarCantidad(DiccionarioID[texturas.tiles["tronco"]], 1, texturas.tiles["tronco"]);
                        posSup = new Vector3Int(posSup.x,posSup.y + 1,posSup.z);
                        if(mapa.GetTile(posSup) == texturas.tiles["hojas"])
                        {
                            //Debug.Log("Proceso de destruir la copa del árbol");
                            destruirCopaArbol(posSup);
                        }
                    }

                    if (mapa.GetTile(posSup) == texturas.tiles["hojas"])
                    {
                        //Debug.Log("Proceso de destruir la copa del árbol");
                        destruirCopaArbol(posSup);
                    }
                }
            }

        }

        [Client]
        public void destruirCopaArbol(Vector3Int vector)
        {

            for (int j = vector.y; j < vector.y + texturas.copaArbol1.GetLength(0); j++)
            {
                for (int k = vector.x - 2; k < vector.x + texturas.copaArbol1.GetLength(1) - 2; k++)
                {
                    Vector3Int vec = new Vector3Int(k, j, 0);
                    int c = CordDeChunk(vec);
                    CMDquitarBloque(vec,c);
                }

            }
        }

        [Command]
        private void CMDquitarBloque(Vector3Int posCelda,int chunck)
        {
            RPCquitarbloque(posCelda,chunck);
            // Debug.Log("SERVER: Actualizando diccionarios");
            actualizarDiccionario(posCelda, 0);
        }

        [ClientRpc]
        private void RPCquitarbloque(Vector3Int posCelda,int chunck)
        {
            // Debug.Log("CLIENT : Se ha quitado un tile en la celda" + posCelda.x + "," + posCelda.y + "   SDic:" + diccionarioCarga.Count);
            if(chunksCargados.ContainsKey(chunck))
            chunksCargados[chunck].SetTile(posCelda, null);
            else
            {
                Debug.Log("fallo de sincronizacion de chuncks : " + chunksCargados.Count);
            }
        }

        #endregion

        #region Sistema de combate
        private void Atacar()
        {

            if (fuenteSonido.clip != sonidoGolpe || (fuenteSonido.clip == sonidoGolpe && !fuenteSonido.isPlaying) || gamemode == "creative")
            {
                Collider2D[] collids = new Collider2D[5];

                ContactFilter2D contact = new ContactFilter2D();
                LayerMask laye = LayerMask.GetMask("RangoAtaque");
                contact.SetLayerMask(laye);
                contact.useTriggers = true;

                areaAtaque.OverlapCollider(contact, collids);

                if (pvp)
                {
                    foreach (Collider2D c in collids)
                    {
                        if (c != null && c != gameObject.transform.GetChild(2).GetComponent<BoxCollider2D>())
                        {
                            int x = flipX ? -5000 : 5000;

                            if (!c.gameObject.GetComponentInParent<PlayerController>().muertoSync && c.gameObject.GetComponentInParent<PlayerController>().gmLocal == "survival" && c.gameObject.GetComponentInParent<PlayerController>().pvpSync)
                            {
                                int d = daño + Random.Range(4, 9);
                                CMDrecibirDañoPVP(c.gameObject.GetComponentInParent<PlayerController>().nombre, d, x, 500);
                                
                                fuenteSonido.clip = sonidoGolpe;
                                if (!fuenteSonido.isPlaying)
                                    fuenteSonido.Play();
                            }

                        }
                    }

                }

                if(dificultad > 0)
                {
                    LayerMask lay = LayerMask.GetMask("RangoAtaqueEnemigo");
                    contact.SetLayerMask(lay);
                    contact.useTriggers = true;

                    areaAtaque.OverlapCollider(contact, collids);

                    foreach (Collider2D c in collids)
                    {
                        if (c != null && c.gameObject.layer.ToString() == "10")
                        {
                            int x = flipX ? -500 : 500;

                            int d = daño + Random.Range(4, 9);
                            bool gm = gmLocal == "creative" ? true : false;

                            pegarAZombie(c.gameObject.GetComponentInParent<NetworkIdentity>().netId, -d, x, 500, gm);
                            //c.gameObject.GetComponentInParent<zombieScript>().CMDmodificarVida();

                            fuenteSonido.clip = sonidoGolpe;
                            if (!fuenteSonido.isPlaying)
                                fuenteSonido.Play();

                        }
                    }

                }

            }
        }

        private void Defender()
        {

        }

        [Command]
        public void CMDrecibirDañoPVP(string nombre, int daño, int x, int y)
        {
            foreach (var j in network.jugadores)
            {
                if (j.Value == nombre.ToLower())
                {
                    RPCrecibirDañoPVP(j.Key, daño, x, y);
                    break;
                }
            }
        }


        [TargetRpc]
        private void RPCrecibirDañoPVP(NetworkConnection conn, int daño, int x, int y)
        {
            if (pvp)
            {
                causaMuerte = "jugador";
                quitarVida(daño, x, y);
            }

        }

        [TargetRpc]
        private void RPCrecibirDañoPVE(NetworkConnection conn, int daño, int x, int y)
        {
            causaMuerte = "zombie";
            quitarVida(daño, x, y);
        }

        public void PeticionRecibirDañoPVE(string nombre, int daño, int x, int y)
        {
            CMDrecibirDañoPVE(nombre, daño, x, y);
        }

        [Command]
        public void CMDrecibirDañoPVE(string nombre, int daño, int x, int y)
        {
            foreach (var j in network.jugadores)
            {
                if (j.Value == nombre.ToLower())
                {
                    RPCrecibirDañoPVE(j.Key, daño, x, y);
                    break;
                }
            }
        }



        [Command]
        private void CMDsincronizarMuerte(bool m)
        {
            RPCsincronizarMuerte(m);
        }

        [ClientRpc]
        private void RPCsincronizarMuerte(bool m)
        {
            muertoSync = m;
            cabeza.GetComponent<Renderer>().enabled = false;
            tronco.GetComponent<Renderer>().enabled = false;
            pierna.GetComponent<Renderer>().enabled = false;
            brazo.GetComponent<Renderer>().enabled = false;
            mano.GetComponent<Renderer>().enabled = false;
            textInfoUser.text = "";
        }


        public void spawnZombie(Vector2 pos)
        {

            CMDspawnZombie(pos);
        }

        [Command]
        public void CMDspawnZombie(Vector2 pos)
        {
            if (zombieScript.nZombies < zombieScript.MaxZombies)
            {
                GameObject zombie = Instantiate(network.spawnPrefabs[3]);
                zombie.transform.position = pos;
                NetworkServer.Spawn(zombie);
                zombieScript.nZombies++;
                //se incrementa la cantidad de zombies existentes y se indica a cada cliente cual es su número
                ActualizarNZombies(zombieScript.nZombies);
            }
            else
            {
                chat.EscribirMensajeLocal("<color=red>No pueden haber mas de "+ zombieScript.MaxZombies +" zombies</color>");
            }

        }

        [ClientRpc]
        public void ActualizarNZombies(int n)
        {
            zombieScript.nZombies = n;
        }

        [Command]
        private void pegarAZombie(uint id, int daño, int x , int y , bool gamemode)
        {
            GameObject[] zombies = GameObject.FindGameObjectsWithTag("Enemigo");
            GameObject target = null;

            if(zombies.Length > 0)
            {
                foreach (var zomb in zombies)
                {
                    if (zomb.gameObject.GetComponent<NetworkIdentity>().netId == id)
                    {
                        target = zomb;
                        break;
                    }
                }

            }


            if(target != null)
            target.gameObject.GetComponent<zombieScript>().CMDmodificarVida(daño,x,y,gamemode);
        }


        public void PeticionSincronizarZombi(uint id, float x, float y,float flip)
        {
            CMDsincZombie(id,x,y,flip);
        }

        [Command]
        private void CMDsincZombie(uint id, float x, float y, float flip)
        {
            GameObject[] zombies = GameObject.FindGameObjectsWithTag("Enemigo");
            GameObject target = null;

            if (zombies.Length > 0)
            {
                foreach (var zomb in zombies)
                {
                    if (zomb.gameObject.GetComponent<NetworkIdentity>().netId == id)
                    {
                        target = zomb;
                        break;
                    }
                }

            }

            if(target != null)
            {
                target.transform.position = new Vector3(x, y, 0);
                target.transform.localScale = new Vector3(flip, target.transform.localScale.y);
            }
        }

        #endregion

        #region Métodos de orientacion del jugador
        private void GirarEnEjeX()
        {
            Vector2 escala = cuerpo.transform.localScale;
            escala.x = -escala.x;
            cuerpo.transform.localScale = escala;

        }

        private void GirarNombreUI()
        {

            Vector2 escala = InfoUser.transform.localScale;
            escala.x = -escala.x;
            InfoUser.transform.localScale = escala;
        }

        void ApuntaBrazoACursor()
        {

            Vector2 PosCursor = Input.mousePosition;
            PosCursor = camaraJugador.ScreenToWorldPoint(PosCursor);
            Vector2 direccion = new Vector2(PosCursor.x - brazo.transform.position.x, PosCursor.y - brazo.transform.position.y);


            if (! Input.GetKey(KeyCode.A) && ! Input.GetKey(KeyCode.D))
            {
                if((direccion.x > 0 && flipX) || (direccion.x < 0 && !flipX))
                {
                    flipX = !flipX;
                    GirarEnEjeX();
                }

            }

            brazo.transform.up = - direccion;

        }

        #endregion

        #region Métodos de Guardado

        public void guardarPartida(string msg)
        {
            //guardarTodosLosPuntosDeRespawn();
            SaveSystem save = new SaveSystem();
            save.setDiccionario(dicCarga);
            save.setSemilla(sem);
            if(!isClientOnly)
            {
                Debug.Log("partida guardada");
                save.setJugadores(network.CaracteristicasJugador);
            }
            save.guardarMundo(nmundo);

            chat.EscribirMensajeLocal(msg);
        }
        public void guardarPartida()
        {
            //guardarTodosLosPuntosDeRespawn();
            SaveSystem save = new SaveSystem();
            save.setDiccionario(dicCarga);
            save.setSemilla(sem);
            if (!isClientOnly)
            {
                Debug.Log("partida guardada");
                save.setJugadores(network.CaracteristicasJugador);
            }
            save.guardarMundo(nmundo);

            chat.EscribirMensajeLocal("Mundo guardado correctamente");

        }
        #endregion

        #region Sincronización del modo de juego

        public void SetGamemode(string gm)
        {
            if(barraVida == null)
            {
                barraVida = GameObject.Find("Barravida");
            }
            if(vidaTexto == null)
            {
                vidaTexto = GameObject.Find("TextoVida").GetComponent<Text>();
            }

            Debug.Log("Cambio de modo a " + gm);
            gamemode = gm;
            if(hasAuthority)
            guardarCaracteristicas(nombre.ToLower(), puntospawn, gamemode);

            if(isLocalPlayer)
            {
                if (gamemode == "creative")
                {
                    vidaTexto.text = "";
                    daño = 1000000;
                }
                else if (gamemode == "survival")
                {
                    daño = 1;
                    vidaTexto.text = vida.ToString();
                }

            }

            gmLocal = gamemode;

        }

        [Command]
        private void CMDsincronizarGM(string gm)
        {
            RPCsincronizarGM(gm);
        }

        [ClientRpc]
        private void RPCsincronizarGM(string gm)
        {
            gmLocal = gm;
        }

        public void CambiarTextoInfo(string n)
        {
            textInfoUser.text = n;
        }

        #endregion



    }

}
