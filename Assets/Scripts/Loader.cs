using UnityEngine;

using System.Collections;

using System.Collections.Generic; //Needed for Lists

using System.Xml; //Needed for XML functionality

using System.Xml.Serialization; //Needed for XML Functionality

using System.IO;

using System.Xml.Linq; //Needed for XDocument

public class Loader : MonoBehaviour
{

    XDocument xmlDoc; //create Xdocument. Will be used later to read XML file IEnumerable<XElement> items; //Create an Ienumerable list. Will be used to store XML Items. List <XMLData> data = new List <XMLData>(); //Initialize List of XMLData objects.

    int iteration = 0, pageNum = 0;

    string charText, dialogueText;

    bool finishedLoading = false;

    void Start()

    {

        DontDestroyOnLoad(gameObject); //Allows Loader to carry over into new scene LoadXML (); //Loads XML File. Code below. StartCoroutine (“AssignData”); //Starts assigning XML data to data List. Code below

    }

    void Update()

    {

        if (finishedLoading)

        {

            Application.LoadLevel("TestScene"); //Only happens if coroutine is finished finishedLoading = false;

        }

    }

    void LoadXML()

    {

        //Assigning Xdocument xmlDoc. Loads the xml file from the file path listed. xmlDoc = XDocument.Load( “Assets/Resources/XML Files/circles_test.xml” );

        //This basically breaks down the XML Document into XML Elements. Used later. items = xmlDoc.Descendants( “page” ).Elements ();

    }

}