using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ClientControl : NetworkBehaviour
{

    public TerraNetworkManager network;
    public InputField Input;
    public Button unirse;

    void Start()
    {
        Debug.Log("se inicia la escena");
        network = GameObject.Find("Network").GetComponent<TerraNetworkManager>();
        Input = GameObject.Find("InputField").GetComponent<InputField>();
        unirse = GameObject.Find("botonUnirse").GetComponent<Button>();

        unirse.onClick.AddListener(funcionUnirse);
    }

    void Update()
    {
        
    }

    void funcionUnirse()
    {
        network.networkAddress = Input.text;
        network.StartClient();
    }
}
