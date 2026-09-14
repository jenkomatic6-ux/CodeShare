using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeShare;

public partial class Form1 : Form
{
    private const string ServerUrl = "wss://server-7xh6.onrender.com";

    private ClientWebSocket? socket;
    private CancellationTokenSource? cancellationTokenSource;

    private Label statusLabel = null!;
    private Label roomLabel = null!;
    private Label usersLabel = null!;
    private TextBox usernameBox = null!;
    private TextBox roomBox = null!;
    private TextBox messageBox = null!;
    private TextBox receivedBox = null!;

    private Button createButton = null!;
    private Button joinButton = null!;
    private Button sendButton = null!;
    private Button copyButton = null!;
    private Button pasteButton = null!;
    private Button historyButton = null!;
    private Button leaveButton = null!;

    private readonly List<string> history = new();

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

        Text = "CodeShare";
        Width = 850;
        Height = 720;
        MinimumSize = new Size(700, 600);
        StartPosition = FormStartPosition.CenterScreen;

        BackColor = Color.FromArgb(18, 18, 18);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10);

        // TITLE
        var title = new Label
        {
            Text = "CodeShare",
            Font = new Font("Segoe UI", 26, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(30, 25),
            AutoSize = true
        };

        var subtitle = new Label
        {
            Text = "Share text and code instantly",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Gray,
            Location = new Point(33, 68),
            AutoSize = true
        };

        // USERNAME
        var usernameLabel = new Label
        {
            Text = "Your name",
            ForeColor = Color.LightGray,
            Location = new Point(32, 110),
            AutoSize = true
        };

        usernameBox = new TextBox
        {
            Location = new Point(30, 135),
            Width = 210,
            Height = 32,
            MaxLength = 32,
            BackColor = Color.FromArgb(35, 35, 35),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        // ROOM
        var roomCodeLabel = new Label
        {
            Text = "Room code",
            ForeColor = Color.LightGray,
            Location = new Point(260, 110),
            AutoSize = true
        };

        roomBox = new TextBox
        {
            Location = new Point(258, 135),
            Width = 150,
            Height = 32,
            MaxLength = 4,
            BackColor = Color.FromArgb(35, 35, 35),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        createButton = MakeButton("CREATE ROOM", 425, 133, 140);
        joinButton = MakeButton("JOIN ROOM", 575, 133, 140);

        // ROOM INFO
        roomLabel = new Label
        {
            Text = "Room: -",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(32, 185),
            AutoSize = true
        };

        usersLabel = new Label
        {
            Text = "Users: 0/10",
            ForeColor = Color.Gray,
            Location = new Point(170, 187),
            AutoSize = true
        };

        leaveButton = MakeButton("LEAVE", 615, 180, 100);
        leaveButton.Enabled = false;

        // MESSAGE AREA
        messageBox = new TextBox
        {
            Location = new Point(30, 225),
            Width = 685,
            Height = 145,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            AcceptsTab = true,
            BackColor = Color.FromArgb(28, 28, 28),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        pasteButton = MakeButton("PASTE", 30, 385, 100);

        sendButton = MakeButton("SEND", 140, 385, 120);
        sendButton.Enabled = false;

        copyButton = MakeButton("COPY", 270, 385, 100);

        historyButton = MakeButton("HISTORY", 380, 385, 120);

        // RECEIVED AREA
        var receivedLabel = new Label
        {
            Text = "Messages",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(32, 440),
            AutoSize = true
        };

        receivedBox = new TextBox
        {
            Location = new Point(30, 470),
            Width = 685,
            Height = 150,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(28, 28, 28),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        statusLabel = new Label
        {
            Text = "● Connecting...",
            ForeColor = Color.Goldenrod,
            Location = new Point(30, 635),
            AutoSize = true,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };

        // EVENTS
        createButton.Click += async (_, _) => await CreateRoom();
        joinButton.Click += async (_, _) => await JoinRoom();
        sendButton.Click += async (_, _) => await SendMessage();

        pasteButton.Click += (_, _) => PasteText();
        copyButton.Click += (_, _) => CopyReceived();
        historyButton.Click += (_, _) => ShowHistory();

        leaveButton.Click += async (_, _) => await LeaveRoom();

        Controls.Add(title);
        Controls.Add(subtitle);
        Controls.Add(usernameLabel);
        Controls.Add(usernameBox);
        Controls.Add(roomCodeLabel);
        Controls.Add(roomBox);
        Controls.Add(createButton);
        Controls.Add(joinButton);
        Controls.Add(roomLabel);
        Controls.Add(usersLabel);
        Controls.Add(leaveButton);
        Controls.Add(messageBox);
        Controls.Add(pasteButton);
        Controls.Add(sendButton);
        Controls.Add(copyButton);
        Controls.Add(historyButton);
        Controls.Add(receivedLabel);
        Controls.Add(receivedBox);
        Controls.Add(statusLabel);
    }

    private Button MakeButton(string text, int x, int y, int width)
    {
        return new Button
        {
            Text = text,
            Location = new Point(x, y),
            Width = width,
            Height = 34,
            BackColor = Color.FromArgb(45, 45, 45),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
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

            SetStatus("● Connected", Color.LightGreen);

            _ = ReceiveMessages();
        }
        catch (Exception ex)
        {
            SetStatus("● Connection failed", Color.Red);

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
                SetStatus("● Disconnected", Color.Red);
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
                        receivedBox.Clear();
                        lastMessageText = "";

                        SetStatus("● In room", Color.LightGreen);
                    });

                    break;

                case "peer_joined":
                    string joinedUsername = document.RootElement
                        .GetProperty("username")
                        .GetString() ?? "Anonymous";

                    RunOnUi(() =>
                    {
                        SetStatus($"● {joinedUsername} joined", Color.LightGreen);
                    });

                    break;

                case "peer_left":
                    string leftUsername = document.RootElement
                        .GetProperty("username")
                        .GetString() ?? "Anonymous";

                    RunOnUi(() =>
                    {
                        SetStatus($"● {leftUsername} left", Color.DarkOrange);
                    });

                    break;

                case "users":
                    int userCount = document.RootElement
                        .GetProperty("users")
                        .GetArrayLength();

                    RunOnUi(() =>
                    {
                        usersLabel.Text = $"Users: {userCount}/10";
                    });

                    break;

                case "history":
                    JsonElement messages = document.RootElement
                        .GetProperty("messages");

                    RunOnUi(() =>
                    {
                        history.Clear();
                        receivedBox.Clear();
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
                        SetStatus("● New message", Color.LightGreen);
                    });

                    break;

                case "left":
                    RunOnUi(() =>
                    {
                        roomLabel.Text = "Room: -";
                        usersLabel.Text = "Users: 0/10";
                        sendButton.Enabled = false;
                        leaveButton.Enabled = false;
                        receivedBox.Clear();
                        history.Clear();
                        lastMessageText = "";

                        SetStatus("● Connected", Color.LightGreen);
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

        string formatted =
            $"[{formattedTime}] {username}:\r\n{text}\r\n\r\n";

        history.Add(formatted);

        receivedBox.AppendText(formatted);
        receivedBox.SelectionStart = receivedBox.Text.Length;
        receivedBox.ScrollToCaret();
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
        SetStatus("● Sent", Color.LightGreen);
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
            SetStatus("● Last message copied", Color.LightGreen);
        }
    }

    private void ShowHistory()
    {
        using Form historyForm = new Form
        {
            Text = "CodeShare - History",
            Width = 750,
            Height = 600,
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.FromArgb(18, 18, 18),
            ForeColor = Color.White
        };

        TextBox historyBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(28, 28, 28),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.None,
            Font = new Font("Consolas", 10),
            Text = string.Join("", history)
        };

        historyForm.Controls.Add(historyBox);

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

        statusLabel.Text = text;
        statusLabel.ForeColor = color;
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