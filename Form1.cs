using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeShare.Controls;

namespace CodeShare;

public partial class Form1 : Form
{
    private const string ServerUrl = "wss://server-7xh6.onrender.com";

    private ClientWebSocket? socket;
    private CancellationTokenSource? cancellationTokenSource;

    // Header
    private StatusPill statusPill = null!;

    // Room bar
    private Label roomLabel = null!;
    private Label usersLabel = null!;

    // Inputs
    private ModernTextBox usernameBox = null!;
    private ModernTextBox roomBox = null!;
    private ModernTextBox messageBox = null!;

    // Messages
    private ChatListBox chatList = null!;

    // Buttons
    private ModernButton createButton = null!;
    private ModernButton joinButton = null!;
    private ModernButton sendButton = null!;
    private ModernButton copyButton = null!;
    private ModernButton pasteButton = null!;
    private ModernButton historyButton = null!;
    private ModernButton leaveButton = null!;

    private readonly List<ChatMessage> history = new();

    // Stores only the text of the latest message
    private string lastMessageText = "";

    public Form1()
    {
        InitializeComponent();

        BuildInterface();

        Shown += async (_, _) => await ConnectToServer();
        FormClosing += (_, _) => Disconnect();
    }

    private void BuildInterface()
    {
        Controls.Clear();

        const int margin = 28;
        const int gap = 14;

        Text = "CodeShare";
        Width = 980;
        Height = 900;
        MinimumSize = new Size(860, 760);
        StartPosition = FormStartPosition.CenterScreen;

        BackColor = AppTheme.WindowBackground;
        ForeColor = AppTheme.TextPrimary;
        Font = AppTheme.FontBody;
        DoubleBuffered = true;

        int cardWidth = Math.Max(320, ClientSize.Width - (margin * 2));

        // ---------------- HEADER ----------------
        var logoMark = new LogoMark
        {
            Location = new Point(margin, 20),
        };

        var title = new Label
        {
            Text = "CodeShare",
            Font = AppTheme.FontDisplay,
            ForeColor = AppTheme.TextPrimary,
            BackColor = Color.Transparent,
            Location = new Point(margin + 52, 14),
            AutoSize = true,
        };

        var subtitle = new Label
        {
            Text = "Share text and code, instantly.",
            Font = AppTheme.FontSubtitle,
            ForeColor = AppTheme.TextSecondary,
            BackColor = Color.Transparent,
            Location = new Point(margin + 54, 44),
            AutoSize = true,
        };

        statusPill = new StatusPill
        {
            Top = 24,
        };

        var divider = new Panel
        {
            Location = new Point(margin, 80),
            Size = new Size(cardWidth, 1),
            BackColor = AppTheme.Border,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        };

        // ---------------- CONNECT CARD ----------------
        var connectCard = new RoundedPanel
        {
            Location = new Point(margin, 95),
            Size = new Size(cardWidth, 140),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        };

        var nameLabel = MakeSectionLabel("YOUR NAME", 20, 12);

        usernameBox = new ModernTextBox
        {
            Location = new Point(20, 32),
            Size = new Size(230, 42),
            MaxLength = 32,
            PlaceholderText = "Enter your name",
        };

        var roomCodeLabel = MakeSectionLabel("ROOM CODE", 270, 12);

        roomBox = new ModernTextBox
        {
            Location = new Point(270, 32),
            Size = new Size(100, 42),
            MaxLength = 4,
            PlaceholderText = "0000",
            TextAlignment = HorizontalAlignment.Center,
        };
        roomBox.Inner.Font = AppTheme.FontRoomCode;

        createButton = MakeButton("Create Room", ButtonKind.Primary, 20, 90, 200, 38);
        joinButton = MakeButton("Join Room", ButtonKind.Secondary, 230, 90, 160, 38);

        connectCard.Controls.Add(nameLabel);
        connectCard.Controls.Add(usernameBox);
        connectCard.Controls.Add(roomCodeLabel);
        connectCard.Controls.Add(roomBox);
        connectCard.Controls.Add(createButton);
        connectCard.Controls.Add(joinButton);

        // ---------------- ROOM STATUS BAR ----------------
        var roomBar = new RoundedPanel
        {
            Location = new Point(margin, connectCard.Bottom + gap),
            Size = new Size(cardWidth, 60),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            CornerRadius = 14,
        };

        roomLabel = new Label
        {
            Text = "Room: —",
            Font = AppTheme.FontRoomLabel,
            ForeColor = AppTheme.TextPrimary,
            BackColor = Color.Transparent,
            Location = new Point(20, 10),
            AutoSize = true,
        };

        usersLabel = new Label
        {
            Text = "0 / 10 users",
            Font = AppTheme.FontBody,
            ForeColor = AppTheme.TextSecondary,
            BackColor = Color.Transparent,
            Location = new Point(20, 33),
            AutoSize = true,
        };

        leaveButton = MakeButton("Leave Room", ButtonKind.Danger, roomBar.Width - 114, 12, 94, 36);
        leaveButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        leaveButton.Enabled = false;

        roomBar.Controls.Add(roomLabel);
        roomBar.Controls.Add(usersLabel);
        roomBar.Controls.Add(leaveButton);

        // ---------------- COMPOSE CARD ----------------
        var composeCard = new RoundedPanel
        {
            Location = new Point(margin, roomBar.Bottom + gap),
            Size = new Size(cardWidth, 190),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        };

        var composeLabel = MakeSectionLabel("COMPOSE", 20, 12);

        messageBox = new ModernTextBox
        {
            Location = new Point(20, 32),
            Size = new Size(composeCard.Width - 40, 92),
            Multiline = true,
            AcceptsTab = true,
            PlaceholderText = "Write your code or message here...",
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        };
        messageBox.Inner.Font = AppTheme.FontMono;

        pasteButton = MakeButton("Paste", ButtonKind.Ghost, 20, 136, 90, 34);
        copyButton = MakeButton("Copy", ButtonKind.Ghost, 120, 136, 90, 34);
        historyButton = MakeButton("History", ButtonKind.Ghost, 220, 136, 100, 34);

        sendButton = MakeButton("Send", ButtonKind.Primary, composeCard.Width - 130, 136, 110, 34);
        sendButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        sendButton.Enabled = false;

        composeCard.Controls.Add(composeLabel);
        composeCard.Controls.Add(messageBox);
        composeCard.Controls.Add(pasteButton);
        composeCard.Controls.Add(copyButton);
        composeCard.Controls.Add(historyButton);
        composeCard.Controls.Add(sendButton);

        // ---------------- MESSAGES CARD ----------------
        var messagesCard = new RoundedPanel
        {
            Location = new Point(margin, composeCard.Bottom + gap),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        };
        messagesCard.Size = new Size(cardWidth, Math.Max(140, ClientSize.Height - messagesCard.Top - margin));

        var messagesLabel = MakeSectionLabel("MESSAGES", 20, 12);

        chatList = new ChatListBox
        {
            Location = new Point(14, 40),
            Size = new Size(messagesCard.Width - 28, Math.Max(80, messagesCard.Height - 56)),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        };

        messagesCard.Controls.Add(messagesLabel);
        messagesCard.Controls.Add(chatList);

        // ---------------- EVENTS ----------------
        createButton.Click += async (_, _) => await CreateRoom();
        joinButton.Click += async (_, _) => await JoinRoom();
        sendButton.Click += async (_, _) => await SendMessage();

        pasteButton.Click += (_, _) => PasteText();
        copyButton.Click += (_, _) => CopyReceived();
        historyButton.Click += (_, _) => ShowHistory();

        leaveButton.Click += async (_, _) => await LeaveRoom();

        Resize += (_, _) => RepositionStatusPill();

        // ---------------- ASSEMBLE ----------------
        Controls.Add(logoMark);
        Controls.Add(title);
        Controls.Add(subtitle);
        Controls.Add(statusPill);
        Controls.Add(divider);
        Controls.Add(connectCard);
        Controls.Add(roomBar);
        Controls.Add(composeCard);
        Controls.Add(messagesCard);

        SetStatus("Connecting...", AppTheme.Warning);
    }

    private static Label MakeSectionLabel(string text, int x, int y)
    {
        return new Label
        {
            Text = text,
            Font = AppTheme.FontSectionLabel,
            ForeColor = AppTheme.TextMuted,
            BackColor = Color.Transparent,
            Location = new Point(x, y),
            AutoSize = true,
        };
    }

    private static ModernButton MakeButton(string text, ButtonKind kind, int x, int y, int width, int height)
    {
        return new ModernButton
        {
            Text = text,
            Kind = kind,
            Location = new Point(x, y),
            Size = new Size(width, height),
        };
    }

    private void RepositionStatusPill()
    {
        if (statusPill == null)
            return;

        statusPill.Left = Math.Max(28, ClientSize.Width - statusPill.Width - 28);
    }

    private async Task ConnectToServer()
    {
        try
        {
            socket = new ClientWebSocket();
            cancellationTokenSource = new CancellationTokenSource();

            await socket.ConnectAsync(
                new Uri(ServerUrl),
                cancellationTokenSource.Token
            );

            SetStatus("Connected", AppTheme.Success);

            _ = ReceiveMessages();
        }
        catch (Exception ex)
        {
            SetStatus("Connection failed", AppTheme.Danger);

            MessageBox.Show(
                $"Could not connect to CodeShare server.\n\n{ex.Message}",
                "CodeShare",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private async Task ReceiveMessages()
    {
        if (socket == null)
            return;

        byte[] buffer = new byte[1024 * 1024];

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                using var memory = new System.IO.MemoryStream();

                WebSocketReceiveResult result;

                do
                {
                    result = await socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        cancellationTokenSource!.Token
                    );

                    if (result.MessageType == WebSocketMessageType.Close)
                        return;

                    memory.Write(buffer, 0, result.Count);

                } while (!result.EndOfMessage);

                string json = Encoding.UTF8.GetString(memory.ToArray());

                HandleServerMessage(json);
            }
        }
        catch
        {
            if (!IsDisposed)
                SetStatus("Disconnected", AppTheme.Danger);
        }
    }

    private void HandleServerMessage(string json)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);

            string type = document.RootElement
                .GetProperty("type")
                .GetString() ?? "";

            switch (type)
            {
                case "connected":
                    break;

                case "room_created":
                case "joined":
                    string room = document.RootElement
                        .GetProperty("room")
                        .GetString() ?? "";

                    RunOnUi(() =>
                    {
                        roomLabel.Text = $"Room: {room}";
                        sendButton.Enabled = true;
                        leaveButton.Enabled = true;

                        history.Clear();
                        chatList.ClearMessages();
                        lastMessageText = "";

                        SetStatus("In room", AppTheme.Success);
                    });

                    break;

                case "peer_joined":
                    string joinedUsername = document.RootElement
                        .GetProperty("username")
                        .GetString() ?? "Anonymous";

                    RunOnUi(() =>
                    {
                        SetStatus($"{joinedUsername} joined", AppTheme.Success);
                    });

                    break;

                case "peer_left":
                    string leftUsername = document.RootElement
                        .GetProperty("username")
                        .GetString() ?? "Anonymous";

                    RunOnUi(() =>
                    {
                        SetStatus($"{leftUsername} left", AppTheme.Warning);
                    });

                    break;

                case "users":
                    int userCount = document.RootElement
                        .GetProperty("users")
                        .GetArrayLength();

                    RunOnUi(() =>
                    {
                        usersLabel.Text = $"{userCount} / 10 users";
                    });

                    break;

                case "history":
                    JsonElement messages = document.RootElement
                        .GetProperty("messages");

                    RunOnUi(() =>
                    {
                        history.Clear();
                        chatList.ClearMessages();
                        lastMessageText = "";

                        foreach (JsonElement message in messages.EnumerateArray())
                        {
                            AddMessageToScreen(message);
                        }
                    });

                    break;

                case "message":
                    RunOnUi(() =>
                    {
                        AddMessageToScreen(document.RootElement);
                        SetStatus("New message", AppTheme.Success);
                    });

                    break;

                case "left":
                    RunOnUi(() =>
                    {
                        roomLabel.Text = "Room: —";
                        usersLabel.Text = "0 / 10 users";
                        sendButton.Enabled = false;
                        leaveButton.Enabled = false;
                        chatList.ClearMessages();
                        history.Clear();
                        lastMessageText = "";

                        SetStatus("Connected", AppTheme.Success);
                    });

                    break;

                case "error":
                    string error = document.RootElement
                        .GetProperty("message")
                        .GetString() ?? "Unknown error.";

                    RunOnUi(() =>
                    {
                        MessageBox.Show(
                            error,
                            "CodeShare",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    });

                    break;
            }
        }
        catch
        {
            // Ignore malformed server messages.
        }
    }

    private void AddMessageToScreen(JsonElement message)
    {
        string username = message.TryGetProperty("username", out JsonElement user)
            ? user.GetString() ?? "Anonymous"
            : "Anonymous";

        string text = message.TryGetProperty("text", out JsonElement content)
            ? content.GetString() ?? ""
            : "";

        // Save ONLY the latest message for the COPY button
        lastMessageText = text;

        string time = message.TryGetProperty("time", out JsonElement timestamp)
            ? timestamp.GetString() ?? ""
            : "";

        string formattedTime = time;

        if (DateTime.TryParse(time, out DateTime parsed))
            formattedTime = parsed.ToLocalTime().ToString("HH:mm:ss");

        var chatMessage = new ChatMessage(username, formattedTime, text);

        history.Add(chatMessage);
        chatList.AddMessage(chatMessage);
    }

    private async Task SendJson(object data)
    {
        if (socket == null || socket.State != WebSocketState.Open)
            return;

        string json = JsonSerializer.Serialize(data);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        await socket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            cancellationTokenSource!.Token
        );
    }

    private async Task CreateRoom()
    {
        string username = usernameBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            MessageBox.Show(
                "Enter your name first.",
                "CodeShare"
            );

            usernameBox.Focus();
            return;
        }

        await SendJson(new
        {
            type = "create",
            username = username
        });
    }

    private async Task JoinRoom()
    {
        string username = usernameBox.Text.Trim();
        string room = roomBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            MessageBox.Show(
                "Enter your name first.",
                "CodeShare"
            );

            usernameBox.Focus();
            return;
        }

        if (room.Length != 4 || !int.TryParse(room, out _))
        {
            MessageBox.Show(
                "Enter a valid 4-digit room code.",
                "CodeShare"
            );

            roomBox.Focus();
            return;
        }

        await SendJson(new
        {
            type = "join",
            room = room,
            username = username
        });
    }

    private async Task SendMessage()
    {
        string text = messageBox.Text;

        if (string.IsNullOrWhiteSpace(text))
            return;

        await SendJson(new
        {
            type = "message",
            text = text
        });

        messageBox.Clear();
        SetStatus("Sent", AppTheme.Success);
    }

    private void PasteText()
    {
        if (Clipboard.ContainsText())
            messageBox.Text = Clipboard.GetText();
    }

    private void CopyReceived()
    {
        // COPY ONLY THE LATEST MESSAGE
        if (!string.IsNullOrEmpty(lastMessageText))
        {
            Clipboard.SetText(lastMessageText);
            SetStatus("Last message copied", AppTheme.Success);
        }
    }

    private void ShowHistory()
    {
        using Form historyForm = new Form
        {
            Text = "CodeShare — History",
            Width = 760,
            Height = 620,
            MinimumSize = new Size(480, 360),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = AppTheme.WindowBackground,
            ForeColor = AppTheme.TextPrimary,
            Font = AppTheme.FontBody,
        };

        var headerLabel = new Label
        {
            Text = "Message History",
            Font = AppTheme.FontRoomLabel,
            ForeColor = AppTheme.TextPrimary,
            BackColor = Color.Transparent,
            Location = new Point(24, 20),
            AutoSize = true,
        };

        var countLabel = new Label
        {
            Text = history.Count == 1 ? "1 message" : $"{history.Count} messages",
            Font = AppTheme.FontSmall,
            ForeColor = AppTheme.TextMuted,
            BackColor = Color.Transparent,
            Location = new Point(24, 46),
            AutoSize = true,
        };

        var historyList = new ChatListBox
        {
            Location = new Point(20, 76),
            Size = new Size(historyForm.ClientSize.Width - 40, historyForm.ClientSize.Height - 96),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        };

        historyList.LoadMessages(history);

        historyForm.Controls.Add(historyList);
        historyForm.Controls.Add(headerLabel);
        historyForm.Controls.Add(countLabel);

        historyForm.ShowDialog(this);
    }

    private async Task LeaveRoom()
    {
        await SendJson(new
        {
            type = "leave"
        });
    }

    private void SetStatus(string text, Color color)
    {
        if (IsDisposed)
            return;

        statusPill.SetStatus(text, color);
        RepositionStatusPill();
    }

    private void RunOnUi(Action action)
    {
        if (IsDisposed)
            return;

        if (InvokeRequired)
            BeginInvoke(action);
        else
            action();
    }

    private void Disconnect()
    {
        try
        {
            cancellationTokenSource?.Cancel();
            socket?.Dispose();
        }
        catch
        {
        }
    }
}
