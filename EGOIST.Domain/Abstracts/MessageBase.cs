using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Domain.Abstracts;

public abstract class MessageBase<TSender> : EntityBase, IMessage<TSender>
{
    private TSender? _sender;
    private DateTime _timestamp;
    private string _message = string.Empty;
    private Citation[] _citations = [];

    public TSender? Sender
    {
        get => _sender;
        set => Notify(ref _sender, value);
    }

    public DateTime Timestamp
    {
        get => _timestamp;
        set => Notify(ref _timestamp, value);
    }

    public string Message
    {
        get => _message;
        set => Notify(ref _message, value);
    }

    public Citation[] Citations
    {
        get => _citations;
        set => Notify(ref _citations, value);
    }
    
    public override string ToString() => $"{Sender?.ToString()} - {Timestamp:dd/MM/yyyy HH:mm:ss}: {Message}";
}