using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Mirror;
using System;

public class ClientNetworkAuthenticator : NetworkAuthenticator
{

    [Header("Components")]
    public ClientNetworkManager manager;
    public UIHandler uiHandler;
    // login info for the local player
    // we don't just name it 'account' to avoid collisions in handshake
    [Header("Login")]
    public string loginAccount = "";
    public string loginPassword = "";

    [Header("Register")]
    public string regAccount = "";
    public string regPassword = "";
    public string regBankName = "";
    public string regBankUserName = "";
    public string regBankUserNum = "";
    public string regPhone = "1";
    public int regAvatarId = 0;

    [Header("Security")]
    public string passwordSalt = "at_least_16_byte";
    public int accountMaxLength = 16;
   // string version = "1.15";
    public bool bLogin = false;
    bool bWaitServerResponse = false;
    public InitialSceneStage sceneStage = InitialSceneStage.AskBank;
    public static ClientNetworkAuthenticator instance;
    bool bEventAdded = false;
    public static ClientNetworkAuthenticator Instance()
    {
        if (!instance)
            instance = FindObjectOfType(typeof(ClientNetworkAuthenticator)) as ClientNetworkAuthenticator;
        return instance;
    }

    public override void OnClientAuthenticate(NetworkConnection conn)
    {
        Debug.Log("OnClientAuthenticate: " + bWaitServerResponse + "|" + sceneStage);
        if (bWaitServerResponse)
            return;
        if(sceneStage == InitialSceneStage.Login)
        {
            string hash = CustomUtil.MD5Hash(loginPassword);
            int nPhoneKind = 0;
#if UNITY_ANDROID
            nPhoneKind = 1;
#elif UNITY_IOS
        nPhoneKind = 2;
#endif
            LoginMsg1 message = new LoginMsg1 { account = loginAccount, password = hash, version = CustomUtil.version, phone_kind = nPhoneKind };
            conn.Send(message);
            print("login message was sent");
        } else if (sceneStage == InitialSceneStage.Register)
        {
            string hash = CustomUtil.MD5Hash(regPassword);
            print("account_name:" + regBankUserName);

            int nPhoneKind = 0;
#if UNITY_ANDROID
            nPhoneKind = 1;
#elif UNITY_IOS
        nPhoneKind = 2;
#endif
            RegisterMsg1 message = new RegisterMsg1 { account = regAccount, password = hash, bank_name = regBankName, account_name = regBankUserName, account_number = regBankUserNum, phone_kind = nPhoneKind };
            conn.Send(message);
            print("Register message was sent");
        } else
        {
            BankInfoAskMsg message3 = new BankInfoAskMsg { };
            conn.Send(message3);
            print("Ask bank info" +  conn);
        }
        
        manager.serverConn = conn;
        manager.state = NetworkState.Handshake;
        bWaitServerResponse = true;
    }



    public void RegisterClient(NetworkConnection conn)
    {
        if (bWaitServerResponse)
            return;
        //--- 2020/12/17 ---
        sceneStage = InitialSceneStage.Register;
        manager.StartConnect2Server();
        ////--------
    }

    public void LoginClient(NetworkConnection conn)
    {
        Debug.Log("bWaitServerResponse: " + bWaitServerResponse);
        if (bWaitServerResponse)
            return;
        //--- 2020/12/17 ---

        sceneStage = InitialSceneStage.Login;
        Debug.Log("sceneStage: " + sceneStage);
        manager.StartConnect2Server();


    public override void OnServerAuthenticate(NetworkConnection conn)
    {
        //throw new System.NotImplementedException();
    }

    public void ResetLocker()
    {
        bEventAdded = false;
    }

    // client //////////////////////////////////////////////////////////////////
    public override void OnStartClient()
    {
        //if (bEventAdded) return;
        // register login success message, allowed before authenticated
        NetworkClient.RegisterHandler<LoginSuccessMsg>(OnClientLoginSuccess, false);
        NetworkClient.RegisterHandler<RegisterResultMsg>(OnClientRegisterReceived, false);
        NetworkClient.RegisterHandler<LoginFailMsg>(OnClientLoginFail, false);
        NetworkClient.RegisterHandler<BankInfoReturnMsg>(ReceiveBankInfo, false);
        bEventAdded = true;
    }

    public void OnPauseClient()
    {
        NetworkClient.UnregisterHandler<LoginSuccessMsg>();
        NetworkClient.UnregisterHandler<RegisterResultMsg>();
        NetworkClient.UnregisterHandler<LoginFailMsg>();
        NetworkClient.UnregisterHandler<BankInfoReturnMsg>();
    }

    private void OnClientRegisterReceived(NetworkConnection arg1, RegisterResultMsg arg2)
    {
        bWaitServerResponse = false;
        //throw new NotImplementedException();
        if (arg2.bSuccess)
        {
            //uiHandler.ShowLog("Register Success.");
            uiHandler.SignupSuccess();
            Debug.Log("Register success.");
        } else
        {
            uiHandler.SignupFailed(arg2.errorMessage);
            Debug.Log("Register failed.");
        }
        bWaitServerResponse = false;

        //--- 2020.12.20 ---
        manager.StopClient();
        ////------
    }

    void OnClientLoginSuccess(NetworkConnection conn, LoginSuccessMsg msg)
    {
        Debug.Log("Login success.");
        bWaitServerResponse = false;
        manager.state = NetworkState.Lobby;
        // authenticated successfully. OnClientConnected will be called.
        OnClientAuthenticated.Invoke(conn);
        //uiHandler.Go2Lobby();
        manager.serverConn = conn;
        manager.userInfo = msg.userData;
        manager.arr_bank = msg.bankData;
        manager.jackpotBonus = msg.jackpotBonus;
        manager.roomType = msg.roomType;
        manager.admin_bank_info = msg.admin_bank_info;
        ConstantDataContainer.minWithdrawAmount = (long)msg.withdrawConf.limit;
        ConstantDataContainer.withdrawUnitAmount = (long)msg.withdrawConf.unit;
        //manager.withdrawConfig = msg.withdrawConf;

        uiHandler.LoginSuccess();
        manager.GenerateHeartBeat();
        
    }

    void OnClientLoginFail(NetworkConnection conn, LoginFailMsg msg)
    {
        bWaitServerResponse = false;
        Debug.Log("Login fail.");
        uiHandler.LoginFail(msg.error);

        //--- 2020.12.23 ---
        manager.StopClient();
        ////------
    }

    public bool IsAllowedAccountName(string account)
    {
        // not too long?
        // only contains letters, number and underscore and not empty (+)?
        // (important for database safety etc.)
        return account.Length <= accountMaxLength &&
               Regex.IsMatch(account, @"^[a-zA-Z0-9_]+$");
    }

    public void ReceiveBankInfo(NetworkConnection conn, BankInfoReturnMsg msg)
    {
        Debug.Log("ReceiveBankInfo");
        uiHandler.ReceiveBankInfo(msg.bankNames);

        bWaitServerResponse = false;

        //--- 2020.12.20 ---
        manager.StopClient();
        ////------
       
    }

    void DisplayUpdateLink()
    {
        uiHandler.DisplayUpdateLink();
    }

    public void InformUIHandler(UIHandler uiHandler)
    {
        this.uiHandler = uiHandler;
    }

    public string GetVersion()
    {
        return CustomUtil.version;
    }
}

public enum InitialSceneStage
{
    AskBank,
    Login,
    Register
}
