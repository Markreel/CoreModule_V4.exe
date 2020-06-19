using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Assets.Code;

public static class MessageHandler
{
    public static void SendMessage(NetworkDriver _networkDriver, MessageHeader _message, NetworkConnection _networkConnection)
    {
        var _writer = _networkDriver.BeginSend(_networkConnection);
        _message.SerializeObject(ref _writer);
        _networkDriver.EndSend(_writer);
    }

    public static MessageHeader ReadMessage<T>(DataStreamReader _reader, Queue<MessageHeader> _messageQueue) where T : MessageHeader, new()
    {
        var _msg = new T();
        _msg.DeserializeObject(ref _reader);
        _messageQueue.Enqueue(_msg);
        return _msg;
    }
}
