using EGOIST.Domain.Entities;

namespace EGOIST.Domain.Interfaces;

public interface IMessage
{
    DateTime Timestamp { get; set; }
    string Message { get; set; }
    Citation[] Citations { get; set; }
}

public interface IMessage<TSender> : IMessage
{
    TSender? Sender { get; set; }
}