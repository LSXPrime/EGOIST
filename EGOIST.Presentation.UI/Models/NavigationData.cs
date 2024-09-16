using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EGOIST.Presentation.UI.Services;
using EGOIST.Presentation.UI.ViewModels.Pages;
using EGOIST.Presentation.UI.ViewModels.Pages.Image;
using EGOIST.Presentation.UI.ViewModels.Pages.Management;
using EGOIST.Presentation.UI.ViewModels.Pages.Management.Characters;
using EGOIST.Presentation.UI.ViewModels.Pages.Text;
using FluentIcons.Common;

namespace EGOIST.Presentation.UI.Models;

public static class NavigationData
{
    public static NavigationItemGroup[] Items { get; } =
    [
        new NavigationItemGroup(
            null,
            [
                new NavigationItem(typeof(HomePageViewModel), NavigationItemType.Main, "Home", Symbol.Home.ToString())
            ]),
        new NavigationItemGroup(
            new NavigationItem(typeof(ImagePageViewModel), NavigationItemType.Main, "Image", Symbol.Image.ToString()),
            [
                new NavigationItem(typeof(ImaginePageViewModel), NavigationItemType.Sub, "Imagine", Symbol.ImageAltText.ToString(), "Bring your imagination to life by typing in descriptive text; the AI will then generate a corresponding, unique image based on your prompt."),
                new NavigationItem(typeof(TransformPageViewModel), NavigationItemType.Sub, "Transform", Symbol.ImageSparkle.ToString(), "Transform existing images with the power of words! Provide an initial image and a text prompt, and watch as the AI seamlessly blends your vision with the original, generating a novel, remixed image."),
                new NavigationItem(typeof(UpscalePageViewModel), NavigationItemType.Sub, "Upscale", Symbol.ExpandUpRight.ToString(), "Envision your favorite images in breathtaking detail, with any desired scale factor. Upscale your images without losing any of the original charm, ensuring a crisp and vivid outcome.")
            ]),
        new NavigationItemGroup(
            new NavigationItem(typeof(TextPageViewModel), NavigationItemType.Main, "Text", Symbol.Textbox.ToString()),
            [
                new NavigationItem(typeof(ChatPageViewModel), NavigationItemType.Sub, "Chat", Symbol.Chat.ToString(), "Engage in captivating conversations with our AI. Explore ideas, get instant feedback, or simply enjoy a friendly chat."),
                new NavigationItem(typeof(CompletionPageViewModel), NavigationItemType.Sub, "Completion", Symbol.Pen.ToString(), "Watch your ideas bloom. Provide a starting point and let our AI weave a tapestry of words, completing your thoughts with astonishing fluency."),
                new NavigationItem(typeof(MemoryPageViewModel), NavigationItemType.Sub, "Memory", Symbol.Record.ToString(), "Integrate your expertise. Upload your documents and leverage the power of your personal data to get contextually relevant AI assistance like never before."),
                new NavigationItem(typeof(RoleplayPageViewModel), NavigationItemType.Sub, "Roleplay", Symbol.Person.ToString(), "The ultimate fan experience. Bring your cherished waifu to life through personalized conversations and tailor-made storylines, forging a connection deeper than you ever imagined.")
            ]),
        new NavigationItemGroup(
            null, 
            [
                new NavigationItem(typeof(CharactersPageViewModel), NavigationItemType.Main, "Characters", Symbol.Person.ToString())
            ]),
        new NavigationItemGroup(
            null,
            [
                new NavigationItem(typeof(SettingsPageViewModel), NavigationItemType.Main, "Settings", Symbol.Settings.ToString())
            ])
    ];
    
    public static readonly Dictionary<Type, (NavigationItemType NavType, Action<NavigationItemType, Dictionary<string, object>?> Action)> Actions = new()
    {
        { typeof(HomePageViewModel), (NavigationItemType.Main, (navType, parameters) => _ = NavigationService.NavigateTo<HomePageViewModel>(parameters: parameters, type: navType)) },
        { typeof(ImagePageViewModel), (NavigationItemType.Main, (navType, parameters) => _ = NavigationService.NavigateTo<ImagePageViewModel>(parameters: parameters, type: navType)) },
        { typeof(ImaginePageViewModel), (NavigationItemType.Sub, (navType, parameters) => _ = NavigationService.NavigateTo<ImaginePageViewModel>(parameters: parameters, type: navType)) },
        { typeof(TransformPageViewModel), (NavigationItemType.Sub, (navType, parameters) => _ = NavigationService.NavigateTo<TransformPageViewModel>(parameters: parameters, type: navType)) },
        { typeof(UpscalePageViewModel), (NavigationItemType.Sub, (navType, parameters) => _ = NavigationService.NavigateTo<UpscalePageViewModel>(parameters: parameters, type: navType)) },
        { typeof(TextPageViewModel), (NavigationItemType.Main, (navType, parameters) => _ = NavigationService.NavigateTo<TextPageViewModel>(parameters: parameters, type: navType)) },
        { typeof(ChatPageViewModel), (NavigationItemType.Sub, (navType, parameters) => _ = NavigationService.NavigateTo<ChatPageViewModel>(parameters: parameters, type: navType)) },
        { typeof(CompletionPageViewModel), (NavigationItemType.Sub, (navType, parameters) => _ = NavigationService.NavigateTo<CompletionPageViewModel>(parameters: parameters, type: navType)) },
        { typeof(MemoryPageViewModel), (NavigationItemType.Sub, (navType, parameters) => _ = NavigationService.NavigateTo<MemoryPageViewModel>(parameters: parameters, type: navType)) },
        { typeof(RoleplayPageViewModel), (NavigationItemType.Sub, (navType, parameters) => _ = NavigationService.NavigateTo<RoleplayPageViewModel>(parameters: parameters, type: navType)) },
        { typeof(CharactersPageViewModel), (NavigationItemType.Main, (navType, parameters) => _ = NavigationService.NavigateTo<CharactersPageViewModel>(parameters: parameters, type: navType)) },
        { typeof(PreviewPageViewModel), (NavigationItemType.Sub, (navType, parameters) => _ = NavigationService.NavigateTo<PreviewPageViewModel>(parameters: parameters, type: navType)) },
        { typeof(SettingsPageViewModel), (NavigationItemType.Main, (navType, parameters) => _ = NavigationService.NavigateTo<SettingsPageViewModel>(parameters: parameters, type: navType)) }
    };
    
    public static void NavigateTo(Type? navType)
    {
        if (navType == null || !Actions.TryGetValue(navType, out var navData)) 
            return;
        
        var (navItemType, action) = navData; 

        var targetItem = Items
            .SelectMany(group => group.Children)
            .FirstOrDefault(item => item.ViewModel == navType);

        if (targetItem?.NavType == NavigationItemType.Sub)
        {
            var parentItem = Items
                .FirstOrDefault(group => group.Children.Contains(targetItem))?
                .Parent;

            if (parentItem != null && Actions.TryGetValue(parentItem.ViewModel, out var parentNavData))
            {
                parentNavData.Action.Invoke(parentNavData.NavType, null);
            }
        }

        action.Invoke(navItemType, null);
    }
}