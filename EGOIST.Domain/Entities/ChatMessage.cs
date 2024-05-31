using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Domain.Entities;

public class ChatMessage : MessageBase<string>, IMessage;