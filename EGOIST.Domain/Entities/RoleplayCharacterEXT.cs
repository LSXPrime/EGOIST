using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Domain.Entities;

public class RoleplayCharacterEXT : EntityBase
{
    private RoleplayCharacter? _character;

    public RoleplayCharacter? Character { get => _character; set => Notify(ref _character, value); }
    [NonSerialized]
    public IInference? Executor;
}
