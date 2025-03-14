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
    //统一规定用于分割用户名和消息的分隔符
    public string separator = "@#:#@";
    [SerializeField]
    private float maxTextWidth;//文本内容的最大宽度

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
        // 实例化气泡
        GameObject message_bubble = Instantiate(isMyMessage ? rightBubblePrefab : leftBubblePrefab, content);

        //修改username
        message_bubble.GetComponentInChildren<UnityEngine.UI.Text>().text = username;

        //通过Transform.Find()逐级查找ChatText并修改ui
        Transform bubbleTransform = message_bubble.transform.Find("Bubble");       
        Transform chatTextTransform = bubbleTransform.Find("ChatText");

        UnityEngine.UI.Text chatTextComponent = chatTextTransform.GetComponent<UnityEngine.UI.Text>();
        chatTextComponent.text = message;

        if (chatTextComponent.preferredWidth > maxTextWidth)
        {
            chatTextComponent.GetComponent<LayoutElement>().preferredWidth = maxTextWidth;
        }

        // 强制更新布局
        Canvas.ForceUpdateCanvases();       //关键代码
        scrollRect.verticalNormalizedPosition = 0f;  //关键代码
        Canvas.ForceUpdateCanvases();   //关键代码

    }

    //由发送按钮触发的输入函数，用于把输入栏的文字发送给服务器，并显示在聊天框里
    public void Input_Message()
    {
        string username = Login.Instance.InputName.text;
        if (chatInput.text != "")
        {
            string addText = chatInput.text;
            //在公屏上显示右侧气泡消息
            AddMessage(addText, username, true);
            //给服务器发送消息
            NetworkClient.instance.BeginSendMessagesToServer(username + separator + addText);
            chatInput.text = "";
            chatInput.ActivateInputField();
            Canvas.ForceUpdateCanvases();       //关键代码
            scrollRect.verticalNormalizedPosition = 0f;  //关键代码
            Canvas.ForceUpdateCanvases();   //关键代码
        }
    }

    //把来自服务器的消息打印到公屏上
    public void System_Input_Message(string s)
    {
        string addText = s;
        //因为要处理ui，所以强制在主线程里进行操作
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            //在公屏上显示左侧气泡消息
            ParseMessage(s, out string username, out string _text);
            AddMessage(_text, username, false);
            Canvas.ForceUpdateCanvases();       //关键代码
            scrollRect.verticalNormalizedPosition = 0f;  //关键代码
            Canvas.ForceUpdateCanvases();   //关键代码
            UnityEngine.Debug.Log("把消息打印到聊天公屏成功");
        });

    }

    /// <summary>
    /// 解析来自服务器的消息字符串
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <param name="username">输出用户名</param>
    /// <param name="addText">输出消息内容</param>
    /// <returns>解析是否成功</returns>
    public static bool ParseMessage(string input, out string username, out string addText)
    {
        username = "";
        addText = "";

        if (string.IsNullOrEmpty(input))
        {
            Debug.LogError("输入字符串为空！");
            return false;
        }

        // 使用Split方法分割字符串
        string[] splitResult = input.Split(new[] { "@#:#@" }, StringSplitOptions.None);

        // 验证分割结果
        if (splitResult.Length != 2)
        {
            Debug.LogError($"消息格式错误，预期分割2部分，实际得到 {splitResult.Length} 部分");
            return false;
        }

        username = splitResult[0];
        addText = splitResult[1];
        return true;
    }
}
