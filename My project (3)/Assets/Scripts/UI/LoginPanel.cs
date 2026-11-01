using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : BasePanel
{
    public TextMeshProUGUI UserName;
    public TextMeshProUGUI PassWord;
    public Button btnRegister;
    public Button btnLogin;

    public override void Init()
    {
        btnRegister.onClick.AddListener(() =>
        {
            //换到注册面板

        });

        btnLogin.onClick.AddListener(()=>
        {
            //告诉服务器 判断用户是否存在和密码是否正确
        })
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
