using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace LanguageServer
{
    public class LanguageServer : INotifyPropertyChanged
    {
        private int maxProblems = -1;
        private readonly JsonRpc rpc;
        private readonly LanguageServerTarget target;
        private readonly ManualResetEvent disconnectEvent = new ManualResetEvent(false);
        private Dictionary<string, DiagnosticSeverity> diagnostics;
        private TextDocumentItem textDocument = null;

        private int counter = 100;

        public LanguageServer(Stream sender, Stream reader, Dictionary<string, DiagnosticSeverity> initialDiagnostics = null)
        {
            this.target = new LanguageServerTarget(this);
            this.rpc = JsonRpc.Attach(sender, reader, this.target);
            this.rpc.Disconnected += OnRpcDisconnected;
            this.diagnostics = initialDiagnostics;

            this.target.Initialized += OnInitialized;
        }

        public string CustomText
        {
            get;
            set;
        }

        public string CurrentSettings
        {
            get; private set;
        }

        public event EventHandler Disconnected;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnInitialized(object sender, EventArgs e)
        {
            var timer = new Timer(LogMessage, null, 0, 5 * 1000);
        }

        public void OnTextDocumentOpened(DidOpenTextDocumentParams messageParams)
        {
            this.textDocument = messageParams.TextDocument;
            
            SendDiagnostics();
        }

        public void SetDiagnostics(Dictionary<string, DiagnosticSeverity> diagnostics)
        {
            this.diagnostics = diagnostics;
        }

        public void SendDiagnostics()
        {
            if (this.textDocument == null || this.diagnostics == null)
            {
                return;
            }

            string[] lines = this.textDocument.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            List<Diagnostic> diagnostics = new List<Diagnostic>();
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                int j = 0;
                while (j < line.Length)
                {
                    Diagnostic diagnostic = null;
                    foreach (var tag in this.diagnostics)
                    {
                        diagnostic = GetDiagnostic(line, i, ref j, tag.Key, tag.Value);

                        if (diagnostic != null)
                        {
                            break;
                        }
                    }

                    if (diagnostic == null)
                    {
                        ++j;
                    }
                    else
                    {
                        diagnostics.Add(diagnostic);
                    }
                }
            }
            
            PublishDiagnosticParams parameter = new PublishDiagnosticParams();
            parameter.Uri = textDocument.Uri;
            parameter.Diagnostics = diagnostics.ToArray();

            if (this.maxProblems > -1)
            {
                parameter.Diagnostics = parameter.Diagnostics.Take(this.maxProblems).ToArray();
            }

            this.rpc.NotifyWithParameterObjectAsync(Methods.TextDocumentPublishDiagnostics, parameter);
        }

        public void SendDiagnostics(string uri, string text)
        {
            if (this.diagnostics == null)
            {
                return;
            }

            string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            List<Diagnostic> diagnostics = new List<Diagnostic>();
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                int j = 0;
                while (j < line.Length)
                {
                    Diagnostic diagnostic = null;
                    foreach (var tag in this.diagnostics)
                    {
                        diagnostic = GetDiagnostic(line, i, ref j, tag.Key, tag.Value);

                        if (diagnostic != null)
                        {
                            break;
                        }
                    }

                    if (diagnostic == null)
                    {
                        ++j;
                    }
                    else
                    {
                        diagnostics.Add(diagnostic);
                    }
                }
            }

            PublishDiagnosticParams parameter = new PublishDiagnosticParams();
            parameter.Uri = uri;
            parameter.Diagnostics = diagnostics.ToArray();

            if (this.maxProblems > -1)
            {
                parameter.Diagnostics = parameter.Diagnostics.Take(this.maxProblems).ToArray();
            }

            this.rpc.NotifyWithParameterObjectAsync(Methods.TextDocumentPublishDiagnostics, parameter);
        }

        public void LogMessage(object arg)
        {
            this.LogMessage(arg, MessageType.Info);
        }

        public void LogMessage(object arg, MessageType messageType)
        {
            this.LogMessage(arg, "testing " + counter++, messageType);
        }

        public void LogMessage(object arg, string message, MessageType messageType)
        {
            LogMessageParams parameter = new LogMessageParams
            {
                Message = message,
                MessageType = messageType
            };
            this.rpc.NotifyWithParameterObjectAsync(Methods.WindowLogMessage, parameter);
        }

        public void ShowMessage(string message, MessageType messageType)
        {
            ShowMessageParams parameter = new ShowMessageParams
            {
                Message = message,
                MessageType = messageType
            };
            this.rpc.NotifyWithParameterObjectAsync(Methods.WindowShowMessage, parameter);
        }

        public async Task<MessageActionItem> ShowMessageRequestAsync(string message, MessageType messageType, string[] actionItems)
        {
            ShowMessageRequestParams parameter = new ShowMessageRequestParams
            {
                Message = message,
                MessageType = messageType,
                Actions = actionItems.Select(a => new MessageActionItem { Title = a }).ToArray()
            };

            var response = await this.rpc.InvokeWithParameterObjectAsync<JToken>(Methods.WindowShowMessageRequest, parameter);
            return response.ToObject<MessageActionItem>();
        }

        public void SendSettings(DidChangeConfigurationParams parameter)
        {
            this.CurrentSettings = parameter.Settings.ToString();
            this.NotifyPropertyChanged(nameof(CurrentSettings));

            JToken parsedSettings = JToken.Parse(this.CurrentSettings);
            int newMaxProblems = parsedSettings.Children().First().Values<int>("maxNumberOfProblems").First();
            if (this.maxProblems != newMaxProblems)
            {
                this.maxProblems = newMaxProblems;
                this.SendDiagnostics();
            }
        }

        public void WaitForExit()
        {
            this.disconnectEvent.WaitOne();
        }

        public void Exit()
        {
            this.disconnectEvent.Set();

            Disconnected?.Invoke(this, new EventArgs());
        }

        private Diagnostic GetDiagnostic(string line, int lineOffset, ref int characterOffset, string wordToMatch, DiagnosticSeverity severity)
        {
            if ((characterOffset + wordToMatch.Length) <= line.Length)
            {
                var subString = line.Substring(characterOffset, wordToMatch.Length);
                if (subString.Equals(wordToMatch, StringComparison.OrdinalIgnoreCase))
                {
                    var diagnostic = new Diagnostic();
                    diagnostic.Message = "This is an " + Enum.GetName(typeof(DiagnosticSeverity), severity);
                    diagnostic.Severity = severity;
                    diagnostic.Range = new Range();
                    diagnostic.Range.Start = new Position(lineOffset, characterOffset);
                    diagnostic.Range.End = new Position(lineOffset, characterOffset + wordToMatch.Length);
                    diagnostic.Code = "Test" + Enum.GetName(typeof(DiagnosticSeverity), severity);
                    characterOffset = characterOffset + wordToMatch.Length;

                    return diagnostic;
                }
            }

            return null;
        }

        private void OnRpcDisconnected(object sender, JsonRpcDisconnectedEventArgs e)
        {
            Exit();
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
