using EGOIST.Domain.Abstracts;

namespace EGOIST.Domain.Entities;

public class ChatMessage : MessageBase<string>
{
    public override string ToString() => $"{Sender} - {Timestamp:dd/MM/yyyy HH:mm:ss}: {Message}";
}