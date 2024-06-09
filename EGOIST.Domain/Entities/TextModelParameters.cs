using EGOIST.Domain.Abstracts;

namespace EGOIST.Domain.Entities;

public class TextModelParameters : EntityBase
{
    private bool _contextLengthAuto;
    private int _contextLength;
    private bool _cpuThreadsAuto;
    private int _cpuThreads;
    private bool _gpuSharedLayersAuto;
    private int _gpuSharedLayers;
    private bool _memoryLock;

    public bool ContextLengthAuto { get => _contextLengthAuto; set => Notify(ref _contextLengthAuto, value); }
    public int ContextLength { get => _contextLength; set => Notify(ref _contextLength, value); }
    public bool CpuThreadsAuto { get => _cpuThreadsAuto; set => Notify(ref _cpuThreadsAuto, value); }
    public int CpuThreads { get => _cpuThreads; set => Notify(ref _cpuThreads, value); }
    public bool GpuSharedLayersAuto { get => _gpuSharedLayersAuto; set => Notify(ref _gpuSharedLayersAuto, value); }
    public int GpuSharedLayers { get => _gpuSharedLayers; set => Notify(ref _gpuSharedLayers, value); }
    public bool MemoryLock { get => _memoryLock; set => Notify(ref _memoryLock, value); }
}