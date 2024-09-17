using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControlBoton : MonoBehaviour
{
    public Text textoAleatorio;
    public InputField inputf;
    public Text textoError;
    public Text textoVersion;
    public Texture2D cursorDefecto;
    void Start()
    {
        Application.targetFrameRate = 30;
        textoAleatorio.text = DevolverFraseDivertida();
        textoError.enabled = false;
        textoVersion.text = Application.version.ToString();
        if(PlayerPrefs.HasKey("nombre"))
        {
            inputf.text = PlayerPrefs.GetString("nombre");

        }
        else
        {

        }

        if(!PlayerPrefs.HasKey("sexo"))
        {
            PlayerPrefs.SetInt("sexo", 1);
        }

        Cursor.SetCursor(cursorDefecto,Vector2.zero,CursorMode.ForceSoftware);
    }  


    private string DevolverFraseDivertida()
    {
        string frase = "";
        switch(Random.Range(0,5))
        {
            case 1:
                frase = "Mozgaash parece que pierde aceite";
            break;
            case 2:
                    frase = "Mozgaash es un <color=red>m</color><color=orange>a</color><color=yellow>r</color><color=green>i</color><color=blue>c</color><color=purple>ó</color><color=magenta>n</color> de los cojones";
            break;
            case 3:
                frase = "Eden : diseselo de un jueguecillo en el que puedes construir";
            break;
            case 4:
                frase = "Tenga cuidado porque hay un enemigo llamado Mozgaash que tiene el ojete muy dilatado";
            break;
            default :
                frase = "Diseselo";
            break;
        }
//            case 4:
    //           frase = "";
//            break;

        return frase;

    } 
    
    
     void Update()
    {

    }

    public void funcionBotonUnJugador()
    {
        //SceneManager.LoadScene("juegoMulti");
    }

    public void funcionBotonMultijugador()
    {
        if(!string.IsNullOrWhiteSpace(inputf.text))
        {
            SceneManager.LoadScene("MenuMultijugador");
        }
        else
        {
            textoError.text = "*Es obligatorio que introduzcas un nombre";
            textoError.enabled = true;
        }

    }

    public void funcionBotonOpciones()
    {

    }

    public void cambiarNombre()
    {
        if (!string.IsNullOrWhiteSpace(inputf.text))
        {
            if (!inputf.text.Contains(" "))
            {
                PlayerPrefs.SetString("nombre", inputf.text);
                textoError.enabled = false;
            }
            else
            {
                textoError.text = "*No están permitidos los espacios en blanco";
                textoError.enabled = true;
                inputf.text = "";
            }
        }
    }

    public void funcionBotonSalir()
    {
        Application.Quit();
    }

    public void esMujer()
    {
        PlayerPrefs.SetInt("sexo",0);
    }

    public void esHombre()
    {
        PlayerPrefs.SetInt("sexo",1);
    }


}
