using PimDeWitte.UnityMainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class ChatManager : MonoBehaviour
{
    public InputField chatInput;

    public GameObject leftBubblePrefab;
    public GameObject rightBubblePrefab;
    public ScrollRect scrollRect;
    private RectTransform content;
    //ͳһ�涨���ڷָ��û�������Ϣ�ķָ���
    public string separator = "@#:#@";
    [SerializeField]
    private float maxTextWidth;//�ı����ݵ������

    public static ChatManager instance;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        content = transform.Find("ViewPort/Content").GetComponent<RectTransform>();
    }

    public void AddMessage(string message, string username, bool isMyMessage)
    {
        // ʵ��������
        GameObject message_bubble = Instantiate(isMyMessage ? rightBubblePrefab : leftBubblePrefab, content);

        //�޸�username
        message_bubble.GetComponentInChildren<UnityEngine.UI.Text>().text = username;

        //ͨ��Transform.Find()�𼶲���ChatText���޸�ui
        Transform bubbleTransform = message_bubble.transform.Find("Bubble");       
        Transform chatTextTransform = bubbleTransform.Find("ChatText");

        UnityEngine.UI.Text chatTextComponent = chatTextTransform.GetComponent<UnityEngine.UI.Text>();
        chatTextComponent.text = message;

        if (chatTextComponent.preferredWidth > maxTextWidth)
        {
            chatTextComponent.GetComponent<LayoutElement>().preferredWidth = maxTextWidth;
        }

        // ǿ�Ƹ��²���
        Canvas.ForceUpdateCanvases();       //�ؼ�����
        scrollRect.verticalNormalizedPosition = 0f;  //�ؼ�����
        Canvas.ForceUpdateCanvases();   //�ؼ�����

    }

    //�ɷ��Ͱ�ť���������뺯�������ڰ������������ַ��͸�������������ʾ���������
    public void Input_Message()
    {
        string username = Login.Instance.InputName.text;
        if (chatInput.text != "")
        {
            string addText = chatInput.text;
            //�ڹ�������ʾ�Ҳ�������Ϣ
            AddMessage(addText, username, true);
            //��������������Ϣ
            NetworkClient.instance.BeginSendMessagesToServer(username + separator + addText);
            chatInput.text = "";
            chatInput.ActivateInputField();
            Canvas.ForceUpdateCanvases();       //�ؼ�����
            scrollRect.verticalNormalizedPosition = 0f;  //�ؼ�����
            Canvas.ForceUpdateCanvases();   //�ؼ�����
        }
    }

    //�����Է���������Ϣ��ӡ��������
    public void System_Input_Message(string s)
    {
        string addText = s;
        //��ΪҪ����ui������ǿ�������߳�����в���
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            //�ڹ�������ʾ���������Ϣ
            ParseMessage(s, out string username, out string _text);
            AddMessage(_text, username, false);
            Canvas.ForceUpdateCanvases();       //�ؼ�����
            scrollRect.verticalNormalizedPosition = 0f;  //�ؼ�����
            Canvas.ForceUpdateCanvases();   //�ؼ�����
            UnityEngine.Debug.Log("����Ϣ��ӡ�����칫���ɹ�");
        });

    }

    /// <summary>
    /// �������Է���������Ϣ�ַ���
    /// </summary>
    /// <param name="input">�����ַ���</param>
    /// <param name="username">����û���</param>
    /// <param name="addText">�����Ϣ����</param>
    /// <returns>�����Ƿ�ɹ�</returns>
    public static bool ParseMessage(string input, out string username, out string addText)
    {
        username = "";
        addText = "";

        if (string.IsNullOrEmpty(input))
        {
            Debug.LogError("�����ַ���Ϊ�գ�");
            return false;
        }

        // ʹ��Split�����ָ��ַ���
        string[] splitResult = input.Split(new[] { "@#:#@" }, StringSplitOptions.None);

        // ��֤�ָ���
        if (splitResult.Length != 2)
        {
            Debug.LogError($"��Ϣ��ʽ����Ԥ�ڷָ�2���֣�ʵ�ʵõ� {splitResult.Length} ����");
            return false;
        }

        username = splitResult[0];
        addText = splitResult[1];
        return true;
    }
}
