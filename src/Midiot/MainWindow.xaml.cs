namespace Midiot
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MidiHandler handler;

        public MainWindow()
        {
            this.InitializeComponent();

            this.StartButton.Click += this.StartButtonOnClick;
            this.StopButton.Click += this.StopButtonOnClick;
            this.LoadButton.Click += this.LoadButtonOnClick;

            this.StopButton.IsEnabled = false;
            this.StartButton.IsEnabled = false;

            var inputDevices = MidiHandler.GetInputDevices().ToList();
            var outputDevices = MidiHandler.GetOutputDevices().ToList();

            foreach (var device in inputDevices)
            {
                this.MidiInput.Items.Add(device);
            }

            foreach (var device in outputDevices)
            {
                this.MidiOutput.Items.Add(device);
            }

            this.MidiInput.SelectedItem = inputDevices.First(x => !x.Contains("microsoft", StringComparison.CurrentCultureIgnoreCase));
            this.MidiOutput.SelectedItem = outputDevices.First(x => !x.Contains("microsoft", StringComparison.CurrentCultureIgnoreCase));
        }

        private async void LoadButtonOnClick(object sender, RoutedEventArgs e)
        {
            this.handler?.Stop();
            this.handler?.Dispose();

            this.handler = new MidiHandler(this.MidiInput.SelectedIndex, this.MidiOutput.SelectedIndex)
            {
                SendInitialMessage = true
            };

            var dialogue = new OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = true
            };

            if (dialogue.ShowDialog() ?? false)
            {
                await this.handler.LoadAsync(dialogue.FileName);

                this.CommandList.Items.Clear();
                this.CurrentCommand.Text = string.Empty;

                foreach (var command in FormatCommandList(this.handler.GetCommandList()))
                {
                    this.CommandList.Items.Add(command);
                }

                this.StartButton.IsEnabled = true;
            }
        }

        private static IEnumerable<string> FormatCommandList(IEnumerable<string> commandList)
        {
            var commands = commandList.ToList();
            var widest = commands.Max(x => x.LastIndexOf("]", StringComparison.OrdinalIgnoreCase));

            foreach (var command in commands)
            {
                var commandParts = command.Split("]");
                var padding = string.Concat(Enumerable.Repeat(" ", widest - commandParts[0].Length + 1));

                yield return $"{commandParts[0].ToUpperInvariant()}]{padding}{commandParts[1]}";
            }
        }

        private void StopButtonOnClick(object sender, RoutedEventArgs e)
        {
            this.handler.Stop();

            this.LoadButton.IsEnabled = true;
            this.StartButton.IsEnabled = true;
            this.StopButton.IsEnabled = false;

            this.handler.SendInitialMessage = false;

            this.MidiInput.IsEnabled = true;
            this.MidiOutput.IsEnabled = true;
        }

        private void StartButtonOnClick(object sender, RoutedEventArgs e)
        {
            this.handler.SendInitialMessage = this.SendInitial.IsChecked ?? true;
            this.handler.Input += this.HandlerOnInput;
            this.handler.Start();

            this.StopButton.IsEnabled = true;
            this.LoadButton.IsEnabled = false;
            this.StartButton.IsEnabled = false;

            this.MidiInput.IsEnabled = false;
            this.MidiOutput.IsEnabled = false;
        }

        private void HandlerOnInput(object sender, EventArgs e)
        {
            var command = this.handler.GetCurrentCommand();
            var index = this.handler.GetCurrentIndex();

            this.Dispatcher.Invoke(() =>
            {
                this.CommandList.SelectedIndex = index;
                this.CurrentCommand.Text = FormatCommand(command);
            });
        }

        private static string FormatCommand(string command)
        {
            var commandParts = command.Split("]");

            return commandParts[0].ToUpper() + "]\t" + commandParts[1].Trim();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.handler?.Dispose();

            base.OnClosing(e);
        }
    }
}