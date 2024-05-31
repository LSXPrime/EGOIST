namespace EGOIST.Domain.Interfaces;

public interface IMessage
{
    string Message { get; set; }
}

public interface IMessage<TSender> : IMessage
{
    TSender Sender { get; set; }
}