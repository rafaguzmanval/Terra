using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ChatBehaviour : NetworkBehaviour
{

    [SerializeField]
    private TerraNetworkManager network;
    public InputField input;
    public Text textoChat;

    double limite = 1.0;
    int mensajesPorMinuto = 0;
    int limiteMensajesPorMinuto = 10;
    double reloj1 = 0, reloj2 = 0;

    double T1 = -70, T2 = 0;

    int strike = 0;

    //string nombrecomando, string descripcion
    private Dictionary<string, string> ComandosConsola = new Dictionary<string, string>();

    public ControlJugador jugador;
    public static int permisos;
    public static string nombreHost;
    private bool yaActivo = false;

    public bool chatActivo = false;

    void Start()
    {
        network = GameObject.Find("Network").GetComponent<TerraNetworkManager>();
        input = GameObject.Find("InputField").GetComponent<InputField>();
        textoChat = GameObject.Find("TextChat").GetComponent<Text>();

        InicializarComandos();
    }

    public override void OnStartClient()
    {

        if (isClientOnly && !yaActivo && hasAuthority)
        {
            network = GameObject.Find("Network").GetComponent<TerraNetworkManager>();
            permisos = 0;
            CMDprocesoEntrada(PlayerPrefs.GetString("nombre"));
        }
        else if(!isClientOnly)
        {
            network = GameObject.Find("Network").GetComponent<TerraNetworkManager>();
            permisos = 1;
        }

        yaActivo = true;
    }

    [Command]
    private void CMDprocesoEntrada(string nombre)
    {
        if (network.permisos.ContainsKey(nombre))
        {
            RpcdevolucionPermisos(network.permisos[nombre],network.nombreHost.Value);
        }
        else
        {
            RpcdevolucionPermisos(0,network.nombreHost.Value);
        }
    }

    [TargetRpc]
    private void RpcdevolucionPermisos(int p,string nHost)
    {
        permisos = p;
        nombreHost = nHost;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!chatActivo)
            {
                input.interactable = true;
                input.ActivateInputField();
                chatActivo = true;
            }
            else
            {
                reloj2 = Time.fixedTimeAsDouble;
                if(reloj2 > reloj1 + 59)
                {
                    reloj1 = Time.fixedTimeAsDouble;
                    mensajesPorMinuto = 0;
                    strike = 0;
                }

                T2 = Time.fixedTimeAsDouble;
                if (!string.IsNullOrEmpty(input.text) && T2 - T1 >= limite && mensajesPorMinuto < limiteMensajesPorMinuto)
                {
                    EscribirMsgClient(input.text);
                    mensajesPorMinuto++;

                    T1 = Time.fixedTimeAsDouble;
                }
                else if(!string.IsNullOrEmpty(input.text))
                {
                    strike++;
                    if(strike >= 3)
                    {
                        jugador.ban("Has sido vetado automáticamente después de varias llamadas de atención de no saturar el chat");
                    }
                    EscribirMensajeLocal("<color=red>PROHIBIDO HACER SPAM</color> Solo puedes escribir cada 3 segundos  y no mas de 10 mensajes por minuto :" + strike + "º aviso");

                }
                input.DeactivateInputField();
                input.interactable = false;
                chatActivo = false;
            }

        }

    }

    [Client]
    private void EscribirMsgClient(string msg)
    {
        if (!string.IsNullOrWhiteSpace(msg) && hasAuthority)
        {
            EscribirMsg(msg);
            input.text = "";
        }
    }

    [Client]
    public void EscribirMsg(string msg)
    {
        if (msg[0] == '/')
        {
            EscribirMensajeLocal("<color=white>" + msg + "</color>");
            string comando = msg.Substring(1);
            string[] subs = comando.Split(' ', '\t');

            if (ComandosConsola.ContainsKey(subs[0]) && hasAuthority)
            {
                ejecutar(subs,permisos);
            }
            else
            {
                EscribirMensajeLocal("<color=red>Comando no reconocido, escriba \"help\" para consultar la lista de comandos</color>");
            }

        }
        else
        {
            CMDSincMsg(msg);
        }

    }

    [Command]
    void ejecutar(string[] comando, int p)
    {
        switch (comando[0])
        {

            case "help":
                CMDHelp();
                break;
            case "kick":
                CMDkick(comando, p);
                break;
            case "ban":
                CMDban(comando, p);
                break;
            case "unban":
                CMDunban(comando,p);
                break;
            case "s":
                CMDs(comando, p);
                break;
            case "p":
                CMDp(comando, p);
                break;
            case "q":
                CMDq(comando, p);
                break;
            case "clear":
                LimpiarChat();
                break;
            case "kill":
                CMDkill(comando, p);
                break;
            case "roll":
                CMDroll(comando);
                break;
            case "tp":
                CMDtp(comando, p);
                break;
            case "gm":
                CMDgm(comando,p);
                break;
            case "pvp":
                CMDpvp(comando, p);
                break;
            case "pve":
                CMDpve(comando, p);
                break;
            case "d":
                CMDcambiarDificultad(comando, p);
                break;

        }

    }

    #region comandosconsola
    void InicializarComandos()
    {
        ComandosConsola.Add("help", ": muestra la lista de todos los comandos");
        ComandosConsola.Add("kick", " #jugador #motivo: expulsa del servidor a jugador, solo tiene permisos de administrador y no se puede expulsar al host");
        ComandosConsola.Add("ban", " #jugador #motivo : veta a un jugador del servidor permanentemente, pero es revocable, solo tiene permisos de administrador y no se puede banear al host");
        ComandosConsola.Add("unban", " #jugador : permite la entrada al servidor de un jugador vetado previamente, solo tiene permisos de administrador");
        ComandosConsola.Add("s", " #mensaje : envia un mensaje como servidor, solo tiene permisos de administrador");
        ComandosConsola.Add("p", " #jugador : da permisos de administrador a #jugador");
        ComandosConsola.Add("q", " #jugador : quita permisos de administrador a #jugador. No se pueden quitar permisos al host del mundo");
        ComandosConsola.Add("clear", " : limpia la caja de chat ");
        ComandosConsola.Add("kill", " #jugador  : mata al jugador");
        ComandosConsola.Add("roll", " #numero_de dados #tamaño_de_los_dados.  Limite de dados: 10 , Limite de tamaño : 1000");
        ComandosConsola.Add("tp", " #jugador :  te teletransporta al jugador. Es necesario tener permisos de administrador");
        ComandosConsola.Add("gm"," (s)urvival/(c)reative : Hace que el jugador este en modo creativo o supervivencia");
        ComandosConsola.Add("pvp", " : Te activa el pvp");
        ComandosConsola.Add("pve", " : Te desactiva el pvp");
        ComandosConsola.Add("d", " #numerodeDificultad : Solo puede ejecutarla el host, cambia el nivel de dificultad , 0:pacifico, 1:fácil , 2:normal");
    }

    [Server]
    private void CMDHelp()
    {
        foreach (KeyValuePair<string, string> cmd in ComandosConsola)
        {
            RPCmensajeLocal("<color=black>" +cmd.Key + cmd.Value + "</color>\n");
        }

    }

    [TargetRpc]
    private void LimpiarChat()
    {
        textoChat.text = "";
    }

    #region comandos de sanción
    [Server]
    private void CMDkick(string[] comando, int p)
    {
        if (comando.Length > 1 && p == 1)
        {
            if (network.jugadores.ContainsValue(comando[1].ToLower()))
            {
                Debug.Log("tiene mas de un argumento");
                if (comando[1].ToLower() != network.nombreHost.Value)
                {
                    KeyValuePair<NetworkConnection, string> target;
                    foreach (KeyValuePair<NetworkConnection, string> n in network.jugadores)
                    {
                        if (n.Value == comando[1].ToLower())
                        {
                            target = n;
                            break;
                        }
                    }

                    string msg = "";
                    for (int i = 2; i < comando.Length; i++)
                    {
                        msg += comando[i] + " ";
                    }

                    RPCkick(target.Key, msg);
                    ServerMsg(network.nombreHost.Key, "Se ha expulsado a " + comando[1].ToLower() + "\nMotivo: " + msg);
                }
                else
                {
                    RPCmensajeLocal("<color=red>No puedes expulsar al Host</color>");
                }
            }
            else
            {
                Debug.Log("no existe jugador");
                RPCmensajeLocal("<color=red>No existe el jugador " + comando[1].ToLower() + "</color>");
            }

        }
        else if (p == 0)
        {
            Debug.Log("error de permisos");
            RPCmensajeLocal("<color=red>No tienes permiso para ejecutar este comando</color>");
        }
        else
        {
            Debug.Log("Error de sintaxis");
            RPCmensajeLocal("<color=red>Error de sintaxis : ./kick #jugador #motivo(opcional)</color>");
        }
    }
    [TargetRpc]
    private void RPCkick(NetworkConnection target,string msg)
    {
        jugador = gameObject.GetComponent<ControlJugador>();
        Debug.Log("se pide al ControlJugador la aparicion de la ventana emergente");
        jugador.VentanaEmergenteExpulsion(msg);
    }

    [Server]
    private void CMDban(string[] comando, int p)
    {
        if (comando.Length > 1 && p == 1)
        {
            if (network.jugadores.ContainsValue(comando[1].ToLower()))
            {
                Debug.Log("tiene mas de un argumento");
                if (comando[1].ToLower() != network.nombreHost.Value)
                {
                    KeyValuePair<NetworkConnection, string> target;
                    foreach (KeyValuePair<NetworkConnection, string> n in network.jugadores)
                    {
                        if (n.Value == comando[1].ToLower())
                        {
                            target = n;
                            break;
                        }
                    }

                    string msg = "";
                    for (int i = 2; i < comando.Length; i++)
                    {
                        msg += comando[i] + " ";
                    }

                    network.listanegra.Add(target.Key.address, msg);
                    network.simpln.Add(comando[1].ToLower(), target.Key.address);
                    RPCBan(target.Key, msg);
                    ServerMsg(network.nombreHost.Key, "Se ha vetado a " + comando[1].ToLower() + "\nMotivo: " + msg);
                }
                else
                {
                    RPCmensajeLocal("<color=red>No puedes vetar al Host</color>");
                }
            }
            else
            {
                Debug.Log("no existe jugador");
                RPCmensajeLocal("<color=red>No existe el jugador " + comando[1].ToLower() + "</color>");
            }

            }
        else if (p == 0)
        {
            Debug.Log("error de permisos");
            RPCmensajeLocal("<color=red>No tienes permiso para ejecutar este comando</color>");
        }
        else
        {
            Debug.Log("Error de sintaxis");
            RPCmensajeLocal("<color=red>Error de sintaxis : ./ban #jugador #motivo(opcional)</color>");
        }
    }

    [TargetRpc]
    private void RPCBan(NetworkConnection target, string msg)
    {
        jugador = gameObject.GetComponent<ControlJugador>();
        Debug.Log("se pide al ControlJugador la aparicion de la ventana emergente");
        jugador.VentanaEmergenteBaneo(msg);
    }

    [Server]
    private void CMDunban(string[] comando, int p)
    {

        if (comando.Length > 1 && p == 1)
        {
            if (network.simpln.ContainsKey(comando[1].ToLower()))
            {
                network.listanegra.Remove(network.simpln[comando[1]]);
                network.simpln.Remove(comando[1]);
                ServerMsg(network.nombreHost.Key, "Se ha perdonado del veto a " + comando[1].ToLower());

            }
            else
            {
                Debug.Log("no existe jugador");
                RPCmensajeLocal("<color=red>El jugador " + comando[1].ToLower() + " no se encuentra en la lista negra</color>");
            }

        }
        else if (p == 0)
        {
            Debug.Log("error de permisos");
            RPCmensajeLocal("<color=red>No tienes permiso para ejecutar este comando</color>");
        }
        else
        {
            Debug.Log("Error de sintaxis");
            RPCmensajeLocal("<color=red>Error de sintaxis : ./unban #jugador #motivo(opcional)</color>");
        }
    }

    [Server]
    private void CMDkill(string[] comando, int p)
    {
        if (comando.Length > 1 && network.jugadores.ContainsValue(comando[1].ToLower()) && p == 1)
        {
            KeyValuePair<NetworkConnection, string> target;
            foreach (KeyValuePair<NetworkConnection, string> n in network.jugadores)
            {
                if (n.Value == comando[1].ToLower())
                {
                    target = n;
                    break;
                }
            }
            ServerM(network.jugadores[connectionToClient] + " ha ordenado matar a " + comando[1].ToString());
            RPCkill(target.Key);
        }
        else if (p == 0)
        {
            RPCmensajeLocal("<color=red>No tienes permiso para ejecutar este comando</color>");
        }
        else if (!network.jugadores.ContainsValue(comando[1].ToLower()))
        {
            RPCmensajeLocal("<color=red>No existe el jugador " + comando[1].ToLower() + "</color>");
        }
        else
        {
            RPCmensajeLocal("<color=red>Error de sintaxis : ./kill #jugador </color>");
        }
    }

    [TargetRpc]
    private void RPCkill(NetworkConnection target)
    {
        jugador = GameObject.Find("P" + PlayerPrefs.GetString("nombre").ToLower()).GetComponent<ControlJugador>();
        Debug.Log("el servidor me ha matado");
        jugador.setVida(-1);
    }

    #endregion


    [Server]
    private void CMDs(string[] comando, int p)
    {
        if (comando.Length > 1 && p == 1)
        {
            string msg = "";
            for (int i = 1; i < comando.Length; i++)
            {
                msg += comando[i] + " ";
            }
            ServerM(msg);
        }
        else if (p == 0)
        {
            RPCmensajeLocal("<color=red>No tienes permiso para ejecutar este comando</color>");
        }
        else
        {
            RPCmensajeLocal("<color=red>Error de sintaxis : ./s #mensaje </color>");
        }
    }

    #region comandos de permiso
    [Server]
    private void CMDp(string[] comando, int p)
    {
        if (comando.Length > 1 && network.jugadores.ContainsValue(comando[1].ToLower()) && p == 1)
        {
            CMDcambiopermisos(1, comando[1].ToLower());
        }
        else if (p == 0)
        {
            RPCmensajeLocal("<color=red>No tienes permiso para ejecutar este comando</color>");
        }
        else if (!network.jugadores.ContainsValue(comando[1].ToLower()))
        {
            RPCmensajeLocal("<color=red>No existe el jugador " + comando[1].ToLower() + "</color>");
        }
        else
        {
            RPCmensajeLocal("<color=red>Error de sintaxis : ./p #jugador </color>");
        }
    }

    [Server]
    private void CMDq(string[] comando, int p)
    {
        if (comando.Length > 1 && network.jugadores.ContainsValue(comando[1].ToLower()) && p == 1)
        {
            if (comando[1].ToLower() != network.nombreHost.Value)
            {
                CMDcambiopermisos(0, comando[1].ToLower());
            }
            else
            {
                RPCmensajeLocal("<color=red>No puedes quitar permisos al Host</color>");
            }

        }
        else if (p == 0)
        {
            RPCmensajeLocal("<color=red>No tienes permiso para ejecutar este comando</color>");
        }
        else if (!network.jugadores.ContainsValue(comando[1].ToLower()))
        {
            RPCmensajeLocal("<color=red>No existe el jugador " + comando[1].ToLower() + "</color>");
        }
        else
        {
            RPCmensajeLocal("<color=red>Error de sintaxis : ./p #jugador </color>");
        }

    }

    [Server]
    private void CMDcambiopermisos(int permisos, string nombre)
    {
        KeyValuePair<NetworkConnection, string> target;
        foreach (KeyValuePair<NetworkConnection, string> n in network.jugadores)
        { 
            if(n.Value == nombre)
            {
                target = n;
                break;
            }
        }
        network.permisos[nombre] = permisos;
        RCPtcambiopermisos(target.Key,permisos);

        if (permisos == 1)
            RPCmensajeLocal("<color=yellow>Has cambiado los permisos de " + nombre + " a administrador</color>");
        else
        {
            RPCmensajeLocal("<color=yellow>Has cambiado los permisos de " + nombre + " a usuario</color>");
        }

    }

    [TargetRpc]
    private void RCPtcambiopermisos(NetworkConnection target, int p)
    {
        permisos = p;
        if(p == 1)
            EscribirMensajeLocal("<color=yellow>Ahora tienes permisos de administrador</color>");
        else
        {
            EscribirMensajeLocal("<color=yellow>Ahora tienes permisos de usuario</color>");
        }

    }

    private void cambioDePermisos(int p)
    {
        permisos = 1;
        Debug.Log(p + "  " + permisos);
    }

    #endregion

    [Server]
    private void CMDroll(string[] comando)
    {
        if(comando.Length == 3)
        {
            int ndados = 0;
            int tam = 0;
            int maxDados = 10;
            int maxTam = 1000;
            if(int.TryParse(comando[1],out ndados) && int.TryParse(comando[2], out tam))
            {
                if(ndados > 0 && tam > 0)
                {
                    if(ndados <= maxDados && tam <= maxTam)
                    {
                        string sumaT = "";
                        int suma = 0;
                        int dado;
                        for (int i = 0; i < ndados; i++)
                        {
                            dado = Random.Range(1, tam + 1);
                            suma += dado;

                            if (i == ndados - 1)
                            {
                                sumaT += dado;
                            }
                            else
                                sumaT += dado + " + ";
                        }

                        ServerM("<color=purple>" + network.jugadores[connectionToClient] + " tira " + ndados + "d" + tam + ": " + sumaT + " = <b>[" + suma + "]</b> </color>");

                    }
                    else
                    {
                        RPCmensajeLocal("<color=red>Error: <color=yellow>#numero_de_dados</color> no puede superar "+ maxDados +" y <color=yellow>#tamaño_de_los_dados</color> no  puede superar " + maxTam + "</color>");
                    }

                }
                else
                {
                    RPCmensajeLocal("<color=red>Error: <color=yellow>#numero_de_dados</color> y <color=yellow>#tamaño_de_los_dados</color>  deben de ser números enteros positivos</color>");
                }
            }
            else
            {
                RPCmensajeLocal("<color=red>Error: <color=yellow>#numero_de_dados</color> y <color=yellow>#tamaño_de_los_dados</color>  deben de ser números enteros positivos</color>");
            }
        }
        else
        {
            RPCmensajeLocal("<color=red>Error de sintaxis : ./roll #numero_de_dados #tamaño_de_los_dados </color>");
        }
    }

    #region comandos de tp
    [Server]
    private void CMDtp(string[] comando, int p)
    {
        if (comando.Length > 1 && p == 1)
        {
            if(network.jugadores.ContainsValue(comando[1].ToLower()))
            {
                Vector3 posicion = GameObject.Find("P" + comando[1].ToLower()).transform.position;
                RPCtp(posicion);
            }
            else
            {
                RPCmensajeLocal("<color=red>No existe el jugador " + comando[1].ToLower() + "</color>");
            }
        }
        else if (p == 0)
        {
            RPCmensajeLocal("<color=red>No tienes permiso para ejecutar este comando</color>");
        }
        else
        {
            RPCmensajeLocal("<color=red>Error de sintaxis : ./tp #jugador </color>");
        }
    }

    [TargetRpc]
    private void RPCtp(Vector3 nposicion)
    {
        jugador = gameObject.GetComponent<ControlJugador>();
        jugador.teletransporte(nposicion);
    }


    #endregion

    [Server]
    private void CMDgm(string[] comando, int p)
    {
        if (comando.Length > 1 && p == 1)
        {
            switch(comando[1])
            {
                case "c":
                    RPCgm("creative");
                    RPCmensajeLocal("<color=yellow>Ahora estás en el modo creativo</color>");
                    break;
                case "creative":
                    RPCgm("creative");
                    RPCmensajeLocal("<color=yellow>Ahora estás en el modo creativo</color>");
                    break;
                case "s":
                    RPCgm("survival");
                    RPCmensajeLocal("<color=yellow>Ahora estás en el modo supervivencia</color>");
                    break;
                case "survival":
                    RPCgm("survival");
                    RPCmensajeLocal("<color=yellow>Ahora estás en el modo supervivencia</color>");
                    break;
                default:
                    RPCmensajeLocal("<color=red>No existe el modo "+comando[1]+"</color>");
                    break;
            }
        }
        else if (p == 0)
        {
            RPCmensajeLocal("<color=red>No tienes permiso para ejecutar este comando</color>");
        }
        else
        {
            RPCmensajeLocal("<color=red>Error de sintaxis : ./gm #modo </color>");
        }
    }

    [TargetRpc]
    private void RPCgm(string modo)
    {
        jugador = gameObject.GetComponent<ControlJugador>();
        jugador.SetGamemode(modo);
    }


    [Server]
    private void CMDpvp(string[] comando, int p)
    {
        RPCpvp();
        RPCpvpSync();
    }

    [TargetRpc]
    private void RPCpvp()
    {
        ControlJugador.pvp = true;
        EscribirMensajeLocal("<color=yellow>Ahora tienes <b>PVP</b></color>");
    }

    [ClientRpc]
    private void RPCpvpSync()
    {
        jugador.pvpSync = true;
        jugador.CambiarTextoInfo("<color=red>" + jugador.nombre + "</color>");
   }


    [Server]
    private void CMDpve(string[] comando, int p)
    {
        RPCpve();
        RPCpveSync();
    }

    [TargetRpc]
    private void RPCpve()
    {
        ControlJugador.pvp = false;

        EscribirMensajeLocal("<color=yellow>Ahora tienes <b>PVE</b></color>");
    }

    [ClientRpc]
    private void RPCpveSync()
    {
        jugador.pvpSync = false;
        jugador.CambiarTextoInfo("<color=black>" + jugador.nombre + "</color>");

    }




    [Server]
    private void CMDcambiarDificultad(string[] comando, int p)
    {
        if (comando[1].ToLower() != network.nombreHost.Value)
        {
            int resultado;
            if(int.TryParse(comando[1], out resultado))
            {
                if(resultado == 0 || resultado == 1 || resultado == 2)
                {
                    RPCcambiarDificultad(resultado);
                    string msg = "pacífico";
                    if (resultado == 1)
                    {
                        msg = "fácil";
                    }
                    if(resultado == 2)
                    {
                        msg = "normal";
                    }
                    ServerM("Se ha cambiado la dificultad a " + msg);
                }
                else
                {
                    RPCmensajeLocal("<color=red>Error: la dificultad debe de ser '0' (pacífico) ó '1' (fácil) ó '2' (normal)</color>");
                }

            }
            else
            {
                RPCmensajeLocal("<color=red>Error: dato numérico de la dificultad no ha podido leerse satisfactoriamente</color>");
            }
        }
        else
        {
            RPCmensajeLocal("<color=red>No eres el anfitrión</color>");
        }
    }

    [ClientRpc]
    private void RPCcambiarDificultad(int d)
    {
        ControlJugador.dificultad = d;
    }
   
    #endregion


    #region Operacionesbasicas
    [Client]
    public void Registro(string nombre)
    {
        if (hasAuthority)
            CMDregistro(nombre);
    }

    [Client]
    public void Desc(string nombre)
    {
        if (hasAuthority)
            CMDdesconexion(nombre);
    }

    [Command]
    public void CMDregistro(string nombre)
    {
        Debug.Log("CHAT: Intento de registro");

        string msg = "<color=lime>" + nombre + " se ha conectado</color>";
        RPCSincMsg(msg);

    }

    [Command]
    public void CMDdesconexion(string nombre)
    {
        Debug.Log("CHAT: Intento de Desconectarse");

        RPCSincMsg("<color=red>" + nombre + " se ha desconectado</color>");

    }

    [TargetRpc]
    public void ServerMsg(NetworkConnection target,string msg)
    {
        if(hasAuthority)
        CMDServerMsg(msg);
    }

    [Command]
    private void CMDServerMsg(string msg)
    {
        ServerM(msg);
    }

    [Server]
    private void ServerM(string msg)
    {
        RPCSincMsg("<color=yellow><b>SERVER</b>: " + msg + "</color>");
    }

    [Command]
    private void CMDSincMsg(string msg)
    {
        string j = network.jugadores[connectionToClient];
        if(string.IsNullOrWhiteSpace(j))
        {
            j = connectionToClient.address;
        }
        RPCSincMsg("<color=blue><b>" + j + "</b>: " + msg + "</color>");
    }

    [Command]
    public void CMDmsgPersonalizado(string msg)
    { 
        RPCSincMsg(msg);
    }


    [ClientRpc]
    private void RPCSincMsg(string msg)
    {
        textoChat = GameObject.Find("TextChat").GetComponent<Text>();
        textoChat.text += msg + "\n";
    
    }

    [TargetRpc]
    public void RPCmensajeLocal(string msg)
    {
        textoChat.text += msg + "\n";
    }

    public void EscribirMensajeLocal(string msg)
    {
        textoChat = GameObject.Find("TextChat").GetComponent<Text>();
        textoChat.text += msg + "\n";
    }


    #endregion


}
