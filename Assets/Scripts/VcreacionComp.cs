using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Discovery;

public class VcreacionComp : NetworkBehaviour
{
    public Canvas lienzo;
    public Button botonQuitar;
    public Button botonValidar;
    public Text TextoNombre;
    public InputField inputNombre;
    public Text TextoSemilla;
    public InputField inputSemilla;
    public Button botonRecarga;
    public HostControl control;

    public void DestruirVentana()
    {
        Destroy(lienzo.gameObject);
    }

    public void generarSemillaAleatoria()
    {
        inputSemilla.text = Random.Range(1000000, 9999999).ToString();
    }

    public void aniadirHost()
    {
        botonValidar.onClick.AddListener(control.funcionHost);
    }



}
