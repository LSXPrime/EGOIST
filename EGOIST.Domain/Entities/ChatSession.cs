using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Domain.Entities;

public class ChatSession() : SessionBase<ChatMessage>($"Chat {DateTime.Now}")
{
    [NonSerialized]
    public IInference? Executor;
}