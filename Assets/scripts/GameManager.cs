using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Timers;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Linq;
using SimpleJSON;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Material")]
    public static APIForm apiform;
    public Material[] SetCard;
    public GameObject[] Cards = new GameObject[5];
    public GameObject[] Cases = new GameObject[9];
    public Button startbtn;
    private float priceValue;
    private float totalValue;
    public TMP_InputField inputPriceText;
    public TMP_Text totalPriceText;
    private int loop = 0;
    public TMP_Text alertText;
    public TMP_Text errorMsg;
    public static Globalinitial _global;

    [DllImport("__Internal")]
    private static extern void GameReady(string msg);
    BetPlayer _player;
    // Start is called before the first frame update
    
    public void RequestToken(string data)
    {
        JSONNode usersInfo = JSON.Parse(data);
        _player.token = usersInfo["token"];
        _player.username = usersInfo["userName"];
        float i_balance = float.Parse(usersInfo["amount"]);
        totalValue = i_balance;
        totalPriceText.text = totalValue.ToString("F2");
    }
    void Start()
    {
        _player = new BetPlayer();
        #if UNITY_WEBGL == true && UNITY_EDITOR == false
            GameReady("Ready");
        #endif
        priceValue = 10f;
        inputPriceText.text = priceValue.ToString("F2");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void doubleIncrease()
    {
        if (totalValue == 0)
        {
            StartCoroutine(resultAlert("Insufficient balance!"));
        }
        else {
            if (priceValue > 1000000)
            {
                StartCoroutine(resultAlert("Can't bet over 1000000!"));
            }
            else
            {
                if (totalValue >= 2 * priceValue)
                {
                    priceValue = priceValue * 2;
                }
                else
                {
                    StartCoroutine(resultAlert("Insufficient balance!"));
                    priceValue = totalValue;
                }
                inputPriceText.text = priceValue.ToString("F2");
            }
        }
    }
    public void halfDecrease()
    {
        if (totalValue >= priceValue/2){
            if (priceValue / 2 >= 10){
                priceValue = priceValue / 2;
            }
            else {
                if (totalValue == 0)
                {
                    priceValue = 0;
                }
                else { 
                    priceValue = 10f;
                }
                StartCoroutine(resultAlert("Can't decrease down 10!"));
            }
        }
        else {
            priceValue = totalValue;
        }
        inputPriceText.text = priceValue.ToString("F2");
    }
    public void play()
    {
        for (int k = 0; k < 5; k++)
        {
            Cards[k].GetComponent<MeshRenderer>().material.color = Color.white;
        }
        for (int i = 0; i < 9; i++)
        {
            Cases[i].GetComponent<TMP_Text>().color = Color.white;
        }
        if (priceValue == 0)
        {
            StartCoroutine(resultAlert("Input balace."));
        }
        else
        {
            if (priceValue > 1000000)
            {
                StartCoroutine(resultAlert("Can't bet over 1000000!"));
            } else if (priceValue < 10) { 
                StartCoroutine(resultAlert("Balance can bet over 10!"));
            }
            else {
                if (totalValue >= priceValue)
                {
                    if (totalValue >= 10)
                    {
                        startbtn.interactable = false;
                        StartCoroutine(Server());
                    }
                    else
                    {
                        StartCoroutine(resultAlert("Balance need over 10!"));
                    }
                }
                else
                {
                    StartCoroutine(resultAlert("Insufficient balance!"));
                }
            }
        }
    }
    IEnumerator Server() {
        yield return new WaitForSeconds(0.5f);
        WWWForm form = new WWWForm();
        form.AddField("userName", _player.username);
        form.AddField("betAmount", priceValue.ToString());
        form.AddField("token", _player.token);
        form.AddField("amount", totalValue.ToString());
        _global = new Globalinitial();
        UnityWebRequest www = UnityWebRequest.Post(_global.BaseUrl + "api/start-VideoPoker", form);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            string strdata = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            apiform = JsonUtility.FromJson<APIForm>(strdata);
            Debug.Log(apiform.msg);
            if (apiform.serverMsg == "Success")
            {
                if (loop == 0)
                {
                    AnimatorController.isRotate = true;
                }
                else
                {
                    AnimatorController.isRotate = false;
                    yield return new WaitForSeconds(1f);
                    AnimatorController.isRotate = true;
                }
                for (int i = 0; i < 5; i++)
                {
                    Cards[i].GetComponent<MeshRenderer>().material = SetCard[apiform.cardArray[i]];
                }
                yield return new WaitForSeconds(1f);
                for (int k = 0; k < apiform.activeArray.Length; k++)
                {
                    Cards[apiform.activeArray[k]].GetComponent<MeshRenderer>().material.color = Color.yellow;
                }
                if (apiform.cases != 9)
                {
                    Cases[apiform.cases].GetComponent<TMP_Text>().color = Color.red;
                }
                totalValue = apiform.total;
                totalPriceText.text = totalValue.ToString("F2");
                loop = loop + 1;
                if (apiform.msg == "")
                {
                    StartCoroutine(resultAlert("Better luck next time!"));
                }
                else
                {
                    StartCoroutine(resultAlert(apiform.msg));
                }
            }
            else if (apiform.serverMsg == "Bet Error!")
            {
                StartCoroutine(Error(apiform.serverMsg));
            }
            else if (apiform.serverMsg == "WinLose Error!")
            {
                StartCoroutine(Error(apiform.serverMsg));
            }
            yield return new WaitForSeconds(1.5f);
            startbtn.interactable = true;
        }
        else
        {
            StartCoroutine(Error("Can't find server!"));
        }
    }
    public void inputChanged() {
        priceValue = float.Parse(string.IsNullOrEmpty(inputPriceText.text) ? "0" : inputPriceText.text);
    }
    public void close() {
        ServerErrorAlert.isServer = false;
    }
    IEnumerator Error(string msg)
    {
        AnimatorController.isRotate = false;
        ServerErrorAlert.isServer = true;
        errorMsg.text = msg;
        yield return new WaitForSeconds(0.001f);
    }
    IEnumerator resultAlert(string msg) {
        alertText.text = msg;
        if(msg == "Better luck next time!"){
            alertText.color = Color.white;
        }else{
            alertText.color = Color.yellow;
        }
        AlertController.isAlert = true;
        yield return new WaitForSeconds(3f);
        AlertController.isAlert = false;
    }
}   
public class BetPlayer
{
    public string username;
    public string token;
}
