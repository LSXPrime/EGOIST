using System.Collections.ObjectModel;
using System.Threading.Tasks;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Presentation.UI.Interfaces.Interactions;

public interface ITextViewModel
{
    string UserInput { get; set; }

    bool WebSearchEnabled { get; set; }
    int WebSearchResultsCount { get; set; }
    ObservableCollection<Citation> Citations { get; }
    
    bool MemoryEnabled { get; set; }
    int MemoryChunksCount { get; set; }
    double MemorySimilarity { get; set; }
    ObservableCollection<MemorySource> Sources { get; }

    TextGenerationParameters GenerationParameters { get; set; }
    TextPromptParameters PromptParameters { get; set; }
    TextModelParameters ModelParameters { get; set; }


    #region GenerationMethods

    Task Create();
    Task Delete();
    Task Select(ISession? session);
    Task Select(MemorySource[] session);
    Task Load(ISession? session);
    Task Unload();
    Task Generate();

    #endregion

    #region AttachmentMethods

    Task LoadAttachment();
    void RemoveAttachment(Citation citation);
    Task OpenAttachment(Citation citation);

    #endregion
}