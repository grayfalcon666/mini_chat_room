using System;
using System.Text;
using System.Linq;
using UnityEngine;


public class Message
{
    private byte[] data = new byte[1024];
    private int startIndex = 0;//���Ǵ�ȡ�˶��ٸ��ֽڵ���������������

    public static void CallNetworClientMethod(string s)
    {
        if (NetworkClient.instance != null)
        {
            ChatManager.instance.System_Input_Message(s);
        }
        else
        {
            Debug.LogError("Bʵ�������ڣ�");
        }
    }

    public void AddCount(int count)
    {
        startIndex += count;
    }
    public byte[] Data
    {
        get { return data; }
    }
    public int StartIndex
    {
        get
        {
            return startIndex;
        }
    }
    public int RemindSize
    {
        get
        {
            return data.Length - startIndex;
        }
    }
    //��������
    public void ReadMessage()
    {
        while (true)
        {
            if (startIndex <= 4)
            {
                return;
            }
            int count = BitConverter.ToInt32(data, 0);
            if ((startIndex - 4) >= count)
            {
                string s = Encoding.UTF8.GetString(data, 4, count);                

                //����NetworkClient���System_Input_Message����
                CallNetworClientMethod(s);
                UnityEngine.Debug.Log("��������һ�����ݣ�" + s);
      
                Array.Copy(data, count + 4, data, 0, startIndex - 4 - count);
                startIndex -= (count + 4);
            }
            else
            {
                break;
            }
        }

    }
    /// <summary>
    /// �õ����ݵ�Լ����ʽ
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] GetBytes(string data)
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        int dataLength = dataBytes.Length;
        byte[] lengthBytes = BitConverter.GetBytes(dataLength);
        byte[] Bytes = lengthBytes.Concat(dataBytes).ToArray();
        return Bytes;
    }
}