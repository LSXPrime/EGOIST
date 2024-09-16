using System.Globalization;
using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Domain.Entities;

public class ChatSession() : SessionBase<ChatMessage>($"Chat {DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture)}")
{
    [NonSerialized]
    public IInference? Executor;
}