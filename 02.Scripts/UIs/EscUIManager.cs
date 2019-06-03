using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
namespace hcp
{
    public class EscUIManager : MonoBehaviour
    {
        private void Awake()
        {
            int sortOrder = 0;
            Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();
            for (int i=0;i< canvases.Length;i++)
            {
                if (canvases[i].isActiveAndEnabled && sortOrder < canvases[i].sortingOrder)
                    sortOrder = canvases[i].sortingOrder;
            }
            GetComponent<Canvas>().sortingOrder = sortOrder + 1;
        }
        public void OnClick_BackToGameBtn()
        {
            if (InGameUIManager.Instance)
            {
                if (!InGameUIManager.Instance.IsSetCursorLock())
                    InGameUIManager.Instance.SetCursorLock(true);
                Destroy(this.gameObject);
            }
            else
                OnClick_LeftGameBtn();
        }
        public void OnClick_LeftGameBtn()
        {
            StartCoroutine(leaveRoomWaitFrame());
        }
        IEnumerator leaveRoomWaitFrame()
        {
            InGameUIManager.Instance.SetCursorLock(false);
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LeaveLobby();
            PhotonNetwork.Disconnect();
                while (PhotonNetwork.IsConnected)
                yield return null;

            Destroy(NetworkManager.instance.gameObject);
            UnityEngine.SceneManagement.SceneManager.LoadScene("WaitingScene");
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnClick_BackToGameBtn();
            }
        }
    }
}