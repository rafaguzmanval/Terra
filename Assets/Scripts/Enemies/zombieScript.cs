using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using UnityEngine.Tilemaps;

public class zombieScript : NetworkBehaviour
{
    [SerializeField]
    private int vidaNetwork = 50;
    public GameObject cabeza;
    public GameObject tronco;
    public GameObject brazo;
    public GameObject pierna;
    public Rigidbody2D rigid;
    public AudioSource fuenteSonido;
    public AudioClip[] sonidos;
    private double cooldown = 3;
    private double t2, t3 = 0;

    private double t1 = 0;
    private double t4 = 0;

    private double emitirSonido = 5;
    private GameObject jugadorLocal;
    private GameObject jugadorHost;
    private bool atacado = false;

    public static int MaxZombies = 7;
    public static int nZombies = 0;

    public static int rangoControl = 70;
    public static int rangoPersecucion = 50;

    private GameObject janterior = null;
    private GameObject jPrevio = null;

    [SerializeField]
    int chunckactual;

    bool enRango = false;

    private bool visto = true;

    private bool flipX = false; //false: mira a izquierda , true: mira a derecha

    private double tSinc1 = 0, tSinc2;

    private Vector3 meta1 = new Vector3(0,-3000,5000),meta2;

    private Vector3 vectorNulo = new Vector3(0, -3000, 5000);

    private void Start()
    {
        GameObject[] jgs = GameObject.FindGameObjectsWithTag("Player");
        foreach(var j in jgs)
        {
           if (j.GetComponent<ControlJugador>().local == true)
            {
                jugadorLocal = j;
            }

           if(j.GetComponent<ControlJugador>().esHost)
            {
                jugadorHost = j;
            }
        }
    }

    private void Update()
    {



    }

    private void FixedUpdate()
    {
        chunckactual = jugadorLocal.GetComponent<ControlJugador>().CordDeChunk(jugadorLocal.GetComponent<ControlJugador>().tilePrincipal.WorldToCell(cabeza.transform.position));

        if(ControlJugador.dificultad == 0)
        {
            DestruirZombi();
        }

        if (!ControlJugador.chunksCargados.ContainsKey(chunckactual))
        {


            GameObject JObjetivo;
            GameObject[] jugs = GameObject.FindGameObjectsWithTag("Player");
            float max = 500;
            foreach (var j in jugs)
            {

                float d = Vector2.Distance(transform.position, j.transform.position);
                if (d <= rangoControl)
                {

                    max = d;
                    break;

                }
            }

            if(max <= rangoControl)
            {
                rigid.Sleep();
            }
            else if(!isClientOnly)
            {
                //Debug.LogWarning("Se destruye zombi porque ");
                DestruirZombi();
            }


        }
        if (ControlJugador.chunksCargados.ContainsKey(chunckactual))
        {

            GameObject JObjetivo;
            float distancia = 0;
            if (janterior != null)
                distancia = Vector2.Distance(transform.position, janterior.transform.position);

            if (janterior == null || distancia >= rangoControl)
            {
                GameObject[] jugs = GameObject.FindGameObjectsWithTag("Player");
                JObjetivo = jugs[0];
                float max = Mathf.Abs(Vector2.Distance(transform.position, JObjetivo.transform.position));
                foreach (var j in jugs)
                {

                    float d = Vector2.Distance(transform.position, j.transform.position);
                    if (d <= rangoControl)
                    {

                        if (j.GetComponent<ControlJugador>().esHost)
                        {
                            JObjetivo = j;
                            break;
                        }
                        if (Mathf.Abs(d) <= max)
                        {
                            JObjetivo = j;
                            max = Mathf.Abs(d);
                        }

                    }
                }

            }
            else
            {
                float d = Vector2.Distance(transform.position, jugadorHost.transform.position);
                if (d <= rangoControl)
                {
                    JObjetivo = jugadorHost;
                }
                else
                    JObjetivo = janterior;
            }


            if (janterior != JObjetivo)
            {
                janterior = JObjetivo;
                //Debug.Log("El nuevo jugador objetivo es : " + janterior.name);
                //jugadorLocal.GetComponent<ControlJugador>().chat.EscribirMensajeLocal("El nuevo jugador objetivo es : " + janterior.name);
            }

            EmitirSonidos();
            rigid.WakeUp();


            visto = true;

            if (JObjetivo == jugadorLocal)
            {
                PerseguirJugador();
            }

        }

    }

    [Server]
    public void CMDmodificarVida(int vida,int x, int y , bool creativo)
    {

        if (vidaNetwork > 0 && !creativo)
            RPCmodificarvida(vida, x, y);


        if (vidaNetwork <= 0 || creativo)
        {
            DestruirZombi();
        }
    }

    [ClientRpc]
    private void RPCmodificarvida(int vida,int x, int y)
    {


        if (vida < 0)
        {
            cabeza.GetComponent<SpriteRenderer>().color = Color.red;
            tronco.GetComponent<SpriteRenderer>().color = Color.red;
            pierna.GetComponent<SpriteRenderer>().color = Color.red;
            brazo.GetComponent<SpriteRenderer>().color = Color.red;
            Invoke("PonerColorOriginal", 0.2f);
            rigid.AddForce(new Vector2(x,y));
            atacado = true;
        }
        else if (vida > 0)
        {
            cabeza.GetComponent<SpriteRenderer>().color = Color.green;
            tronco.GetComponent<SpriteRenderer>().color = Color.green;
            pierna.GetComponent<SpriteRenderer>().color = Color.green;
            brazo.GetComponent<SpriteRenderer>().color = Color.green;
            Invoke("PonerColorOriginal", 0.2f);
        }

        vidaNetwork = vidaNetwork + vida;

        if (vidaNetwork > 50)
        {
            vidaNetwork = 50;
        }
    }

    private void PonerColorOriginal()
    {
        cabeza.GetComponent<SpriteRenderer>().color = Color.white;
        tronco.GetComponent<SpriteRenderer>().color = Color.white;
        pierna.GetComponent<SpriteRenderer>().color = Color.white;
        brazo.GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void PerseguirJugador()
    {
        GameObject[] jugs = GameObject.FindGameObjectsWithTag("Player");
  
        if(jugs.Length > 0)
        {
            GameObject JObjetivo = jPrevio;
            t2 = Time.realtimeSinceStartupAsDouble;
            if (atacado || t2 - t4 >= 10 || JObjetivo == null)
            {
                float max = 70;
                foreach (var j in jugs)
                {
                    if(j.GetComponent<ControlJugador>().gmLocal == "survival" && !j.GetComponent<ControlJugador>().muertoSync)
                    {
                        float d = Vector2.Distance(transform.position, j.transform.position);
                        if (d <= max && d < rangoPersecucion)
                        {
                            JObjetivo = j;
                            max = Mathf.Abs(d);
                            enRango = true;
                        }
                    }


                }

                if(JObjetivo != null)
                {
                    jPrevio = JObjetivo;
                    if (Vector2.Distance(transform.position, JObjetivo.transform.position) > rangoPersecucion)
                    {
                        enRango = false;
                    }
                }
                else
                {
                    enRango = false;
                }


                if(atacado)
                {
                    atacado = false;
                }


                t4 = Time.realtimeSinceStartupAsDouble;
            }

            //jugadorLocal.GetComponent<ControlJugador>().chat.EscribirMensajeLocal("El nuevo jugador objetivo es : " + JObjetivo.name);

            GameObject m = GameObject.Find("grid" + chunckactual);
            GameObject m1 = GameObject.Find("grid" + (chunckactual + 1));
            GameObject m_1 = GameObject.Find("grid" + (chunckactual - 1));


            if (m == null || m1 == null || m_1 == null)
            {
                DestruirZombi();
            }
            else if (enRango && JObjetivo != null)
            {
                if(meta1 == vectorNulo)
                {
                    meta1 = JObjetivo.transform.position;

                }
                    Tilemap mapa = m.transform.GetChild(1).GetComponent<Tilemap>();
                    Tilemap mapa1 = m1.transform.GetChild(1).GetComponent<Tilemap>();
                    Tilemap mapa_1 = m_1.transform.GetChild(1).GetComponent<Tilemap>();


                    float cambio = flipX ? -0.5f : 0.5f;
                    Vector3 posZ = new Vector3(transform.position.x + cambio, transform.position.y, 0);

                    Vector3Int celda = mapa.WorldToCell(posZ);


                    Vector3Int celda2 = new Vector3Int(celda.x, celda.y - 1, 0);

                    Vector3Int celdaSuelo = new Vector3Int(celda.x, celda.y - 2, 0);
                    Vector3Int celdaSuelo2 = new Vector3Int(celda.x, celda.y - 3, 0);

                    Vector3Int celda3;
                    Vector3Int celda4;

                    //31
                    //42

                    if (flipX)
                    {
                        celda3 = new Vector3Int(celda.x + 1, celda.y, 0);
                        celda4 = new Vector3Int(celda2.x + 1, celda2.y, 0);
                    }
                    else
                    {
                        celda3 = new Vector3Int(celda.x - 1, celda.y, 0);
                        celda4 = new Vector3Int(celda2.x - 1, celda2.y, 0);
                    }

                    bool obice = false;

                    float impulso = 200;

                    bool deteccionCelda4 = mapa.GetTile<Tile>(celda4) != null || mapa1.GetTile<Tile>(celda4) != null || mapa_1.GetTile<Tile>(celda4) != null;

                    if (deteccionCelda4 && mapa.GetTile<Tile>(celdaSuelo) != null)
                    {
                        bool deteccionCelda4TipoDeCollider = false;
                        if (mapa.GetTile<Tile>(celda4) != null)
                        {
                            deteccionCelda4TipoDeCollider = mapa.GetTile<Tile>(celda4).colliderType != Tile.ColliderType.None;

                        }
                        else if (mapa1.GetTile<Tile>(celda4) != null && !deteccionCelda4TipoDeCollider)
                        {
                            deteccionCelda4TipoDeCollider = mapa1.GetTile<Tile>(celda4).colliderType != Tile.ColliderType.None;
                        }
                        else if (mapa_1.GetTile<Tile>(celda4) != null && !deteccionCelda4TipoDeCollider)
                        {
                            deteccionCelda4TipoDeCollider = mapa_1.GetTile<Tile>(celda4).colliderType != Tile.ColliderType.None;
                        }

                        if (deteccionCelda4TipoDeCollider)
                        {
                            bool deteccionCelda3 = mapa.GetTile<Tile>(celda3) != null || mapa1.GetTile<Tile>(celda3) != null || mapa_1.GetTile<Tile>(celda3) != null;
                            bool deteccionCelda3TipoDeCollider = false;

                            // deteccionCelda3 debe de ser falso para que la celda superior de enfrente este vacia y el zombi entonces pueda saltar
                            if (deteccionCelda3 == false)
                            {
                                rigid.AddForce(new Vector2(0, impulso));
                                obice = true;
                            }
                            else
                            {
                                if (mapa.GetTile<Tile>(celda3) != null)
                                {
                                    deteccionCelda3TipoDeCollider = mapa.GetTile<Tile>(celda3).colliderType == Tile.ColliderType.None;
                                }
                                else if (mapa1.GetTile<Tile>(celda3) != null)
                                {
                                    deteccionCelda3TipoDeCollider = mapa1.GetTile<Tile>(celda3).colliderType == Tile.ColliderType.None;
                                }
                                else if (mapa_1.GetTile<Tile>(celda3) != null)
                                {
                                    deteccionCelda3TipoDeCollider = mapa_1.GetTile<Tile>(celda3).colliderType == Tile.ColliderType.None;
                                }

                                if (deteccionCelda3TipoDeCollider)
                                {
                                    rigid.AddForce(new Vector2(0, impulso));
                                    obice = true;

                                }
                            }

                        }

                    }

                    //if (mapa.GetTile<Tile>(celdaSuelo) == null && mapa.GetTile<Tile>(celdaSuelo2) == null)
                    //{
                    //    obice = true;
                    //}


                    float fixedSpeed = 3 * Time.deltaTime;
                    if (JObjetivo.transform.position.x < transform.position.x && flipX == true)
                    {
                        voltearZombie(false);
                    }
                    else if (JObjetivo.transform.position.x > transform.position.x && flipX == false)
                    {
                        voltearZombie(true);
                    }

                    float dist = Vector2.Distance(transform.position, meta1);
                    float distanciaJugador = Vector2.Distance(transform.position, JObjetivo.transform.position);

                    if (dist > 1 && !obice)
                    {

                        if (isClientOnly)
                        {
                            Vector3 pos = Vector3.MoveTowards(transform.position, new Vector3(meta1.x, transform.position.y, 0), fixedSpeed);
                            transform.position = pos;
                            tSinc2 = Time.realtimeSinceStartupAsDouble;
                            if (tSinc1 == 0 || tSinc2 > tSinc1)
                            {
                                jugadorLocal.GetComponent<ControlJugador>().PeticionSincronizarZombi(gameObject.GetComponent<NetworkIdentity>().netId, pos.x, pos.y, gameObject.transform.localScale.x);
                                tSinc1 = Time.realtimeSinceStartupAsDouble + 0.1;
                            }

                        }
                        else
                        {
                            transform.position = Vector3.MoveTowards(transform.position, new Vector3(meta1.x, transform.position.y, 0), fixedSpeed);
                        }
                    }

                    Vector3 zombiePos = transform.position;

                    if ((meta1.x + 1 >= transform.position.x && meta1.x - 1 <= transform.position.x) || (meta1.x < zombiePos.x && zombiePos.x < JObjetivo.transform.position.x)
                        || (meta1.x > zombiePos.x && zombiePos.x > JObjetivo.transform.position.x))
                    {
                        meta1 = vectorNulo;
                    }


                    if (distanciaJugador <= 1)
                    {
                        t2 = Time.realtimeSinceStartupAsDouble;
                        if (t2 - t1 >= cooldown)
                        {
                            int x = flipX ? 5000 : -5000;
                            jugadorLocal.GetComponent<ControlJugador>().PeticionRecibirDanoPVE(JObjetivo.GetComponent<ControlJugador>().nombre, 10, x, 500);
                            t1 = Time.realtimeSinceStartupAsDouble;
                            cooldown = Random.Range(0.9f, 2f);
                        }

                    }

            }

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position,meta1);
    }
    private void EmitirSonidos()
    {
        t2 = Time.realtimeSinceStartupAsDouble;
        if (t2 - t3 > emitirSonido)
        {
            t3 = Time.realtimeSinceStartupAsDouble;
            fuenteSonido.clip = sonidos[Random.Range(0, sonidos.Length)];
            fuenteSonido.Play();
        }
    }

    private void voltearZombie(bool v)
    {
        if (v)
        {
            if(gameObject.transform.localScale.x < 0)
            {
                gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y);
            }
            else
            gameObject.transform.localScale = new Vector3(-gameObject.transform.localScale.x, gameObject.transform.localScale.y);
        }
        else
        {
            if (gameObject.transform.localScale.x < 0)
            {
                gameObject.transform.localScale = new Vector3(-gameObject.transform.localScale.x, gameObject.transform.localScale.y);
            }
            else
                gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y);
        }
        flipX = v;

    }


    void DestruirZombi()
    {
        NetworkServer.Destroy(gameObject);
        nZombies--;
    }

}
