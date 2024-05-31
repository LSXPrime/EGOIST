using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EGOIST.Data;

public partial class RoleplayCharacter : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;
    [ObservableProperty]
    private string _summary = string.Empty;
    [ObservableProperty]
    private string _scenario = string.Empty;
    [ObservableProperty]
    private string _description = string.Empty;
    [ObservableProperty]
    private string _firstMessage = string.Empty;
    [ObservableProperty]
    private string _creator = string.Empty;
    [ObservableProperty]
    private string _version = "1.0";
    [ObservableProperty]
    private string _notes = string.Empty;
    [ObservableProperty]
    private string _avatar = string.Empty;
    [JsonIgnore]
    public string AvatarPath => $"{AppConfig.Instance.CharactersPath}\\{Name}\\{Avatar}";
    private ObservableCollection<string> _tags = new();
    public ObservableCollection<string> Tags
    {
        get { return _tags; }
        set
        {
            if (_tags != value)
            {
                _tags = value;
                OnPropertyChanged(nameof(Tags));
            }
        }
    }
    [ObservableProperty]
    private RPCharacterInteractionFrequancy _interactionFrequancy = RPCharacterInteractionFrequancy.Normal;
    [ObservableProperty]
    private ObservableCollection<ChatMessage> _exampleDialogue = new();

    [RelayCommand]
    private void AddTag() => Tags.Add("RandomTag");
    [RelayCommand]
    private void RemoveTag(object tag) => Tags.Remove((string)tag);
    [RelayCommand]
    private void AddDialogueLine() => ExampleDialogue.Add(new() { Sender = "Player" });
    [RelayCommand]
    private void RemoveDialogueLine(object message) => ExampleDialogue.Remove((ChatMessage)message);
    [RelayCommand]
    private void SetInteraction(string level) => InteractionFrequancy = (RPCharacterInteractionFrequancy)Enum.Parse(typeof(RPCharacterInteractionFrequancy), level);
    [RelayCommand]
    private void SetAvatar()
    {
        var openFileDialog = new System.Windows.Forms.OpenFileDialog { Filter = "Images|*.jpg;*.jpeg;*.png;*.tiff;*.webp" };
        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            Avatar = openFileDialog.FileName;
    }

    public static RoleplayCharacter FromJson(string json) => JsonConvert.DeserializeObject<RoleplayCharacter>(json);
    public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);
}

public enum RPCharacterInteractionFrequancy : short
{
    Shy = 0,
    Normal = 1,
    Chatty = 2
}

public class RoleplayCharacterTavenAIConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(RoleplayCharacter);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);

        var character = new RoleplayCharacter
        {
            Name = (string)jsonObject["name"],
            Summary = (string)jsonObject["personality"],
            Scenario = (string)jsonObject["scenario"],
            Description = (string)jsonObject["description"],
            FirstMessage = (string)jsonObject["first_mes"],
            Creator = (string)jsonObject["user_name_view"],
            Version = (string)jsonObject["spec_version"],
            Tags = new ObservableCollection<string>(((JArray)jsonObject["categories"]).Select(x => x.ToString()))
        };

        var messages = ((string)jsonObject["mes_example"]).Split("\r\n");
        foreach (var message in messages)
        {
            if (message.Contains("<START>") || message.Contains("<END>"))
                continue;

            Console.WriteLine(message);

            if (message.StartsWith("{{user}}:"))
                character.ExampleDialogue.Add(new ChatMessage { Sender = "User", Message = message[9..] });
            else if (message.StartsWith("{{char}}:"))
                character.ExampleDialogue.Add(new ChatMessage { Sender = "Assistant", Message = message[9..] });
        }

        return character;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

/*
[ObservableObject]
public static partial class RoleplayCharacterExtensions
{

    [RelayCommand]
    private static void AddTag(this RoleplayCharacter character) => character.Tags.Add("RandomTag");
    [RelayCommand]
    private static void RemoveTag(this RoleplayCharacter character, object tag) => character.Tags.Remove((string)tag);
    [RelayCommand]
    private static void AddDialogueLine(this RoleplayCharacter character) => character.ExampleDialogue.Add(new() { Sender = "Player" });
    [RelayCommand]
    private static void RemoveDialogueLine(this RoleplayCharacter character, object message) => character.ExampleDialogue.Remove((ChatMessage)message);
    [RelayCommand]
    private static void SetInteraction(this RoleplayCharacter character, string level) => character.InteractionFrequancy = (RPCharacterInteractionFrequancy)Enum.Parse(typeof(RPCharacterInteractionFrequancy), level);
    [RelayCommand]
    private static void SetAvatar(this RoleplayCharacter character)
    {
        var openFileDialog = new System.Windows.Forms.OpenFileDialog { Filter = "Images|*.jpg;*.jpeg;*.png;*.tiff;*.webp" };
        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            character.Avatar = openFileDialog.FileName;
    }
}
*/