namespace Midiot
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using NAudio.Midi;

    public class MidiHandler : IDisposable
    {
        private readonly List<string> commands;
        private int current;

        private readonly MidiIn midiIn;
        private readonly MidiOut midiOut;

        private int currentChannel;

        public MidiHandler(int inIndex, int outIndex)
        {
            this.commands = new List<string>();
            this.current = 0;
            this.currentChannel = 0;

            this.midiIn = new MidiIn(inIndex);
            this.midiOut = new MidiOut(outIndex);
        }

        public bool SendInitialMessage { get; set; }

        public event EventHandler Input;

        public void Reset()
        {
            this.current = 0;
        }

        public async Task LoadAsync(string filename)
        {
            var data = await File.ReadAllLinesAsync(filename);
            this.commands.AddRange(data.Where(x => !x.StartsWith('#')));
        }

        public void Start()
        {
            this.midiIn.MessageReceived += this.MidiInOnMessageReceived;
            this.midiIn.Start();

            if (this.SendInitialMessage)
            {
                this.Next();
            }
        }

        private void MidiInOnMessageReceived(object sender, MidiInMessageEventArgs e)
        {
            if (e.MidiEvent.CommandCode == MidiCommandCode.PatchChange)
            {
                this.Next();
            }
        }

        public void Stop()
        {
            this.midiIn.Stop();
        }

        public static IEnumerable<string> GetOutputDevices()
        {
            for (var device = 0; device < MidiOut.NumberOfDevices; device++)
            {
                yield return MidiOut.DeviceInfo(device).ProductName;
            }
        }

        public static IEnumerable<string> GetInputDevices()
        {
            for (var device = 0; device < MidiIn.NumberOfDevices; device++)
            {
                yield return MidiIn.DeviceInfo(device).ProductName;
            }
        }

        public string GetCurrentCommand()
        {
            return this.commands[this.current];
        }

        public int GetCurrentIndex()
        {
            return this.current;
        }

        public IEnumerable<string> GetCommandList()
        {
            return this.commands;
        }

        private void Next()
        {
            this.OnInput();

            if (!this.commands.Any())
            {
                return;
            }

            if (this.current > this.commands.Count)
            {
                return;
            }

            // format: [Label] <commands[]>
            // commands: CH1 PC1 CC34,0
            var message = this.commands[this.current];
            var midiEvents = new List<MidiEvent>();

            var tokens = (message.StartsWith('[') ? message.Substring(message.LastIndexOf(']') + 1) : message).Split(' ');
            foreach (var token in tokens.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var midiCommand = token.Substring(0, 2);
                var value = token.Substring(2);

                switch (midiCommand)
                {
                    case "CH":
                        this.currentChannel = int.Parse(value);
                        break;

                    case "PC":
                        var programChange = new PatchChangeEvent(0, this.currentChannel, int.Parse(value));
                        midiEvents.Add(programChange);

                        break;

                    case "CC":
                        var data = value.Split(',');
                        var controlChange = new ControlChangeEvent(0, this.currentChannel, (MidiController)int.Parse(data[0]), int.Parse(data[1]));
                        midiEvents.Add(controlChange);

                        break;

                    default:
                        // not supported
                        break;
                }
            }

            foreach (var midiEvent in midiEvents)
            {
                this.midiOut.Send(midiEvent.GetAsShortMessage());
            }

            this.current = Math.Min(this.commands.Count - 1, this.current + 1);
        }

        public void Dispose()
        {
            this.midiIn?.Dispose();
            this.midiOut?.Dispose();
        }

        protected virtual void OnInput()
        {
            this.Input?.Invoke(this, EventArgs.Empty);
        }
    }
}