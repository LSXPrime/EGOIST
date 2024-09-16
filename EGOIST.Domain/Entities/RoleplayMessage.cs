using EGOIST.Domain.Abstracts;

namespace EGOIST.Domain.Entities;

public class RoleplayMessage : MessageBase<RoleplayCharacter>
{
    public override string ToString() => $"{Sender?.Name} - {Timestamp:dd/MM/yyyy HH:mm:ss}: {Message}";
}