using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine.UI;
using System.Collections;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Threading.Tasks;

public class NetworkClient : MonoBehaviour
{
    static Message rec_Message = new Message();
    public string username = Login.Instance.InputName.text;
    private Socket clientSocket;


    public static NetworkClient instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        await StartClientAsync();
    }    
    
    //异步连接，避免阻塞主线程
    private async Task StartClientAsync()
    {
        clientSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        //开始尝试连接服务器
        try
        {
            string domain = Login.Instance.InputServeAddress.text;
            //默认域名
            if (domain == "")
            {
                domain = "example.cn";
            }
            IPAddress[] ipAddresses = Dns.GetHostAddresses(domain);
            IPAddress ipAdress = ipAddresses[0];
            

            await clientSocket.ConnectAsync(new IPEndPoint(ipAdress, 88));
            ChatManager.instance.System_Input_Message("System" + ChatManager.instance.separator + "连接服务器成功！");
            UnityEngine.Debug.Log("连接服务器成功");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            UnityEngine.Debug.Log("连接服务器失败");
            ChatManager.instance.System_Input_Message("System" + ChatManager.instance.separator + "连接服务器失败");
        }
        string addNameText_ = "\n  " + "<color=green>" + username + "</color>";
        BeginSendMessagesToServer(addNameText_+ ChatManager.instance.separator +"进入了聊天室");
        BeginReceiveMessages();
    }

    /// <summary>
    /// 开始接收来自服务端的数据
    /// </summary>
    /// <param name="toClientsocket"></param>
    private void BeginReceiveMessages()
    {
        clientSocket.BeginReceive(
            rec_Message.Data,
            rec_Message.StartIndex,
            rec_Message.RemindSize,
            SocketFlags.None,
            ReceiveCallBack,
            null
        );
    }

    /// <summary>
    /// 接收到来自服务端消息的回调函数
    /// </summary>
    /// <param name="ar"></param>
    private void ReceiveCallBack(IAsyncResult ar)
    {
        try
        {
            int count = clientSocket.EndReceive(ar);
            UnityEngine.Debug.Log("从服务端接收到数据,解析中.....");
            rec_Message.AddCount(count);
            //打印来自客户端的消息
            rec_Message.ReadMessage();
            //继续监听来自服务端的消息
            clientSocket.BeginReceive(
                rec_Message.Data, 
                rec_Message.StartIndex, 
                rec_Message.RemindSize, 
                SocketFlags.None, 
                ReceiveCallBack, 
                null
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void BeginSendMessagesToServer(string msg)
    {
        try
        {
            clientSocket.Send(Message.GetBytes(msg));
            UnityEngine.Debug.Log(msg + "发送成功!");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}