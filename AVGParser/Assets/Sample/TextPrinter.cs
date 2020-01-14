using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TextPrinter : MonoBehaviour
{
    public Text textCtrl;

    string staticText = "";

    class TextColorPair
    {
        public Tweener tw;
        public string text;
        public Color color;
    }

    List<TextColorPair> dynamicText = new List<TextColorPair>();

    // Start is called before the first frame update
    void Start()
    {
        //AddText("test");
    }

    // Update is called once per frame
    void Update()
    {
        if (dynamicText.Count == 0 && staticText == textCtrl.text)
            return;
        string txt = staticText;
        foreach(var it in dynamicText)
        {
            txt += "<color=#" + ColorUtility.ToHtmlStringRGBA(it.color) + ">" + it.text + "</color>";
        }
        textCtrl.text = txt;
    }

    public void AddText(string txt)
    {
        Color startColor = textCtrl.color;
        startColor.a = 0;
        var dst = new TextColorPair { text = txt, color = startColor, tw = null };
        dynamicText.Add(dst);
        dst.tw = DOTween.ToAlpha(() => dst.color, (c) => dst.color = c, textCtrl.color.a, 0.5f).OnComplete(()=> 
        {
            dynamicText.Remove(dst);
            staticText += dst.text;
        });
    }

    public void Reline()
    {
        if (dynamicText.Count == 0)
        {
            staticText += '\n';
            return;
        }
        dynamicText[dynamicText.Count - 1].text += '\n';
    }

    public void ShowAll()
    {
        List<Tweener> tws = new List<Tweener>();
        foreach(var dst in dynamicText)
        {
            tws.Add(dst.tw);
        }
        foreach(Tweener tw in tws)
        {
            tw.Complete();
        }
    }
}
