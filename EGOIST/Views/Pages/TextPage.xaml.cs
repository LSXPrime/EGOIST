using EGOIST.Data;
using EGOIST.ViewModels.Pages;
using Wpf.Ui.Controls;
using System.Windows.Forms;

namespace EGOIST.Views.Pages;
public partial class TextPage : INavigableView<TextViewModel>
{
    public TextViewModel ViewModel { get; }
    public SystemInfo Info { get; set; }

    public TextPage(TextViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        Info = SystemInfo.Instance;

        InitializeComponent();

    //    viewModel.ChatContainerView = ChatContainerView;
        viewModel.ChatMessagesContainer = ChatMessagesContainer;
        viewModel.MemoryContainerView = MemoryContainerView;
        viewModel.CompletionTextEditor = CompletionTextEditor;
        viewModel.CompletionSuggestionPopup = CompletionSuggestionPopup;
        viewModel.CompletionSuggestionList = CompletionSuggestionList;

        // Load the HTML content into the WebView
        string htmlContent = $@"
                <html>
                    <head>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                padding: 10px;
                            }}
                            .message-container {{
                                display: flex;
                                justify-content: space-between;
                                margin-bottom: 10px;
                            }}
                            .user-message {{
                                background-color: #cce5ff;
                                border-radius: 8px;
                                padding: 8px;
                                max-width: 70%;
                                text-align: right;
                            }}
                            .assistant-message {{
                                background-color: #e6e6e6;
                                border-radius: 8px;
                                padding: 8px;
                                max-width: 70%;
                                text-align: left;
                            }}
                            .button-container {{
                                display: flex;
                                gap: 10px;
                            }}
                        </style>
                    </head>
                    <body>
                        <div id='messagesContainer'></div>
                        <script>
                            let messages = '';
                            const messagesContainer = document.getElementById('messagesContainer');

                            function renderMessages() {{
                                messages.forEach(message => {{
                                    const messageContainer = document.createElement('div');
                                    messageContainer.classList.add('message-container');

                                    const messageElement = document.createElement('div');
                                    messageElement.textContent = message.Text;

                                    if (message.Sender === 'User') {{
                                        messageElement.classList.add('user-message');
                                    }} else if (message.Sender === 'Assistant') {{
                                        messageElement.classList.add('assistant-message');
                                    }}

                                    const buttonContainer = document.createElement('div');
                                    buttonContainer.classList.add('button-container');

                                    const copyButton = document.createElement('button');
                                    copyButton.textContent = 'Copy';
                                    copyButton.onclick = function() {{ copyMessage(message.Text); }};
                                    buttonContainer.appendChild(copyButton);

                                    const speakButton = document.createElement('button');
                                    speakButton.textContent = 'Speak';
                                    speakButton.onclick = function() {{ speakMessage(message.Text); }};
                                    buttonContainer.appendChild(speakButton);

                                    messageContainer.appendChild(messageElement);
                                    messageContainer.appendChild(buttonContainer);
                                    messagesContainer.appendChild(messageContainer);
                                }});
                            }}

                            function addMessage(newMessage) {{
                                const messageContainer = document.createElement('div');
                                messageContainer.classList.add('message-container');

                                const messageElement = document.createElement('div');
                                messageElement.textContent = newMessage.Text;

                                if (newMessage.Sender === 'User') {{
                                    messageElement.classList.add('user-message');
                                }} else if (newMessage.Sender === 'Assistant') {{
                                    messageElement.classList.add('assistant-message');
                                }}

                                const buttonContainer = document.createElement('div');
                                buttonContainer.classList.add('button-container');

                                const copyButton = document.createElement('button');
                                copyButton.textContent = 'Copy';
                                copyButton.onclick = function() {{ copyMessage(newMessage.Text); }};
                                buttonContainer.appendChild(copyButton);

                                const speakButton = document.createElement('button');
                                speakButton.textContent = 'Speak';
                                speakButton.onclick = function() {{ speakMessage(newMessage.Text); }};
                                buttonContainer.appendChild(speakButton);

                                messageContainer.appendChild(messageElement);
                                messageContainer.appendChild(buttonContainer);
                                messagesContainer.appendChild(messageContainer);
                                messages.push(newMessage);
                            }}

                            function copyMessage(text) {{
                                navigator.clipboard.writeText(text);
                            }}

                            function speakMessage(text) {{
                                window.external.MessageSpeak(text);
                            }}

                            // Call the initial rendering function
                            renderMessages();
                        </script>
                    </body>
                </html>";

        ChatMessagesContainer.NavigateToString(htmlContent);
    }

    [RelayCommand]
    private void ChangeTab(string index)
    {
        Tabs.SelectedIndex = int.Parse(index);
    }

    private void ChatMessagesContainer_Loaded(object sender, RoutedEventArgs e)
    {
        // Allow interaction between JavaScript and C#
    //    ChatMessagesContainer.InvokeScript("eval", "window.external = window");
    }
}
