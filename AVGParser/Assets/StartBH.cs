using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AVGParser.KRKR;

public class StartBH : MonoBehaviour
{
    public TextPrinter textPrinter;

    KAGParser parser = new KAGParser();


    // Start is called before the first frame update
    void Start()
    {
        parser.LoadScript("main", "*start");
        StartCoroutine(Loop());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Loop()
    {
        while (true)
        {
            var tag = parser.GetNextTag();
            if (tag == null)
                yield break;
            Debug.Log(tag.CommandName);
            switch(tag.CommandName)
            {
                case "text":
                    textPrinter.AddText(tag.CommandParams["text"]);
                    yield return new WaitForSeconds(0.1f);
                    break;
            }
        }
    }
}
