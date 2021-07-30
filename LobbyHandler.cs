using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyHandler : MonoBehaviour
{
    public UIHandler uiHandler;
    public List<LobbyRoom> listLobbyRoom = new List<LobbyRoom>();
    public GameObject[] objLobbyRoom;

    public Transform transLobbyRoomRoot;
    public GameObject objPanelWithdraw;
    public GameObject objMenuBtn;
    public GameObject lbl_jackPot;
    public GameObject txt_jackpot;
    public GameObject lbl_chip;
    public GameObject txt_chip;
    public GameObject btn_refresh_chip;
    public GameObject img_separator;

    public int nCurrentFocusRoomIndex = 0;
    public int nMaxCashRoomObjects = 5;
    public float fStartPosX = -800f;
    public TMPro.TMP_Dropdown dropRooms;
    public GameObject btn_exit;

    int nRoomStartIndex = 0;
    int nRoomEndIndex = 1;
    int nRoomAll = 4;
    public GameObject btnAddRoom;

    private void Start()
    {
#if UNITY_WEBGL
        btn_exit.SetActive(false);
#endif
        dropRooms.ClearOptions();
        List<TMP_Dropdown.OptionData> listOptionData = new List<TMP_Dropdown.OptionData>();
        RoomType[] arr_roomTypes = ClientNetworkManager.Instance().roomType;

        for (int i = 0; i <  arr_roomTypes.Length; i++)
        {
            string strRoomType = arr_roomTypes[i].name;
            listOptionData.Add(new TMP_Dropdown.OptionData (strRoomType));
        }
        listOptionData.Add(new TMP_Dropdown.OptionData("All rooms"));
        dropRooms.AddOptions(listOptionData);
        dropRooms.value = arr_roomTypes.Length;
        if (ClientNetworkManager.Instance().bChecking)
        {
            objMenuBtn.SetActive(false);
            lbl_chip.SetActive(false);
            lbl_jackPot.SetActive(false);
            txt_chip.SetActive(false);
            txt_jackpot.SetActive(false);
            btn_refresh_chip.SetActive(false);
            img_separator.SetActive(false);
        }
        else
            objMenuBtn.SetActive(true);
    }

    public void UpdateLobbyRooms(List<RoomInfo> listRoomInfos)
    {
        int nRoomCount = listRoomInfos.Count;
       
        if (nCurrentFocusRoomIndex == 0) nRoomStartIndex = 0;
        else if (nCurrentFocusRoomIndex == 1) nRoomStartIndex = 1;
        else nRoomStartIndex = nCurrentFocusRoomIndex - 2;

        for (int i = 0; i < listLobbyRoom.Count; i++)
        {
            if (!IsInRoomList(listLobbyRoom[i].nRoomId, listRoomInfos))
            {
                GameObject objRoom = listLobbyRoom[i].gameObject;
                listLobbyRoom.Remove(listLobbyRoom[i]);
                Destroy(objRoom);
            }
        }
        btnAddRoom.transform.SetParent(transLobbyRoomRoot.parent);
        

        for (int i = 0; i < listRoomInfos.Count; i++)
        {
            if (!ExistRoom(listRoomInfos[i].roomId))
            {
                GameObject objRoom = Instantiate(objLobbyRoom[listRoomInfos[i].roomkind]) as GameObject;
                objRoom.transform.SetParent(transLobbyRoomRoot);
                objRoom.transform.localScale = Vector3.one;
                LobbyRoom newRoom = objRoom.GetComponent<LobbyRoom>();
                newRoom.SetRoomTable(listRoomInfos[i], uiHandler);
                listLobbyRoom.Add(newRoom);
            } else
            {
                int nRoomNumber = GetRoomNumberFromId(listRoomInfos[i].roomId);
                if (nRoomNumber > -1)
                {
                    listLobbyRoom[nRoomNumber].SetRoomTable(listRoomInfos[i], uiHandler);
                }
            }
        }
        btnAddRoom.transform.SetParent(transLobbyRoomRoot);


    }

    int GetRoomNumberFromId(int nRoomId)
    {
        for (int i = 0; i < listLobbyRoom.Count; i++)
        {
            if (listLobbyRoom[i].nRoomId == nRoomId) return i; 
        }
        return -1;
    }

    bool ExistRoom(int roomId)
    {
        for (int i = 0; i < listLobbyRoom.Count; i++)
        {
            if (listLobbyRoom[i].nRoomId == roomId) return true;
        }
        return false;
    }

    bool IsInRoomList(int roomId, List<RoomInfo> listRoomInfos)
    {
        for (int i = 0; i < listLobbyRoom.Count; i++)
        {
            if (i >= listRoomInfos.Count) return false;
            if (listRoomInfos[i].roomId == roomId)
                return true;
        }
        return false;
    }

    public void ShowRoomWithTypes(int nKind, int nType)
    {
        nRoomAll = dropRooms.options.Count - 1;
        int nAllKind = 2;
        foreach (LobbyRoom lobbyRoom in listLobbyRoom)
        {
            if ((lobbyRoom.nRoomType == nType || nType == nRoomAll) && (lobbyRoom.roomKind == nKind || lobbyRoom.roomKind == nAllKind))
            {
                lobbyRoom.gameObject.SetActive(true);
            } 
            else
            {
                lobbyRoom.gameObject.SetActive(false);
            }
        }
    }
}

