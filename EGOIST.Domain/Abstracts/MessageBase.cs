using EGOIST.Domain.Interfaces;

namespace EGOIST.Domain.Abstracts;

public abstract class MessageBase<TSender> : EntityBase, IMessage<TSender>
{
    private TSender? _sender;
    private string _message;

    public TSender? Sender
    {
        get => _sender;
        set => Notify(ref _sender, value);
    }
        
    public string Message
    {
        get => _message;
        set => Notify(ref _message, value);
    }
}