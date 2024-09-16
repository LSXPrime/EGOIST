using EGOIST.Domain.Abstracts;

namespace EGOIST.Domain.Entities;

public class TextModelParameters : EntityBase
{
    private bool _contextLengthAuto;
    private long _contextLength = 4096;
    private bool _cpuThreadsAuto;
    private int _cpuThreads = 8;
    private bool _gpuSharedLayersAuto;
    private int _gpuSharedLayers;
    private bool _memoryLock;
    private bool _flashAttention = true;

    public bool ContextLengthAuto { get => _contextLengthAuto; set => Notify(ref _contextLengthAuto, value); }
    public long ContextLength { get => _contextLength; set => Notify(ref _contextLength, value); }
    public bool CpuThreadsAuto { get => _cpuThreadsAuto; set => Notify(ref _cpuThreadsAuto, value); }
    public int CpuThreads { get => _cpuThreads; set => Notify(ref _cpuThreads, value); }
    public bool GpuSharedLayersAuto { get => _gpuSharedLayersAuto; set => Notify(ref _gpuSharedLayersAuto, value); }
    public int GpuSharedLayers { get => _gpuSharedLayers; set => Notify(ref _gpuSharedLayers, value); }
    public bool MemoryLock { get => _memoryLock; set => Notify(ref _memoryLock, value); }
    public bool FlashAttention { get => _flashAttention; set => Notify(ref _flashAttention, value); }
}