using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NAudio.Midi;

namespace MidiTest
{
	public partial class MidiInPanel : UserControl
	{
		MidiIn midiIn;
		MidiOut midiOut;
		bool monitoring;
		List<MidiEvent> events;
		int midiOutIndex;

		public MidiInPanel()
		{
			InitializeComponent();
		}

		private void MidiInForm_Load(object sender, EventArgs e)
		{
			for (int device = 0; device < MidiIn.NumberOfDevices; device++)
			{
				comboBoxMidiInDevices.Items.Add(MidiIn.DeviceInfo(device).ProductName);
			}
			if (comboBoxMidiInDevices.Items.Count > 0)
			{
				comboBoxMidiInDevices.SelectedIndex = 0;
			}
			for (int device = 0; device < MidiOut.NumberOfDevices; device++)
			{
				comboBoxMidiOutDevices.Items.Add(MidiOut.DeviceInfo(device).ProductName);
			}
			if (comboBoxMidiOutDevices.Items.Count > 0)
			{
				comboBoxMidiOutDevices.SelectedIndex = 0;
			}
			events = new List<MidiEvent>();
			for (int note = 50; note < 62; note++)
			{
				AddNoteEvent(note);
			}
		}

		private void AddNoteEvent(int noteNumber)
		{
			int channel = 2;
			var noteOnEvent = new NoteOnEvent(0, channel, noteNumber, 100, 50);
			events.Add(noteOnEvent);
			events.Add(noteOnEvent.OffEvent);
		}

		private void buttonMonitor_Click(object sender, EventArgs e)
		{
			if (!monitoring)
			{
				StartMonitoring();
			}
			else
			{
				StopMonitoring();
			}
		}

		private void StartMonitoring()
		{
			if (comboBoxMidiInDevices.Items.Count == 0)
			{
				MessageBox.Show("No MIDI input devices available");
				return;
			}
			if (midiIn == null)
			{
				midiIn = new MidiIn(comboBoxMidiInDevices.SelectedIndex);
				midiIn.MessageReceived += new EventHandler<MidiInMessageEventArgs>(midiIn_MessageReceived);
				midiIn.ErrorReceived += new EventHandler<MidiInMessageEventArgs>(midiIn_ErrorReceived);
			}
			midiIn.Start();
			monitoring = true;
			buttonMonitor.Text = "Stop";
			comboBoxMidiInDevices.Enabled = false;
			
			if (midiOut == null)
			{
				midiOut = new MidiOut(comboBoxMidiOutDevices.SelectedIndex);
			}
		}

		void midiIn_ErrorReceived(object sender, MidiInMessageEventArgs e)
		{
			progressLog1.LogMessage(Color.Red, String.Format("Time {0} Message 0x{1:X8} Event {2}",
			                                                 e.Timestamp, e.RawMessage, e.MidiEvent));
		}

		private void StopMonitoring()
		{
			if (monitoring)
			{
				midiIn.Stop();
				monitoring = false;
				buttonMonitor.Text = "Monitor";
				comboBoxMidiInDevices.Enabled = true;
			}
		}

		void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
		{
			progressLog1.LogMessage(Color.Blue, String.Format("Time {0} Message 0x{1:X8} Event {2}",
			                                                  e.Timestamp, e.RawMessage, e.MidiEvent));

			SendMidiOutMessage(e.MidiEvent);
		}

		private void MidiInPanel_Disposed(object sender, EventArgs e)
		{
			timer1.Enabled = false;
			StopMonitoring();
			if (midiIn != null)
			{
				midiIn.Dispose();
				midiIn = null;
			}
			if (midiOut != null)
			{
				midiOut.Dispose();
				midiOut = null;
			}
		}

		private void buttonClearLog_Click(object sender, EventArgs e)
		{
			progressLog1.ClearLog();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			if (checkBoxMidiOutMessages.Checked)
			{
				SendNextMidiOutMessage();
			}
		}

		private void SendMidiOutMessage(MidiEvent eventToSend)
		{
			midiOut.Send(eventToSend.GetAsShortMessage());
			progressLog1.LogMessage(Color.Green, String.Format("Sent {0}", eventToSend));
		}
		
		private void SendNextMidiOutMessage()
		{
			MidiEvent eventToSend = events[midiOutIndex++];
			SendMidiOutMessage(eventToSend);
			if (midiOutIndex >= events.Count)
			{
				midiOutIndex = 0;
			}
		}
		
		void CheckBoxMidiOutMessagesCheckedChanged(object sender, EventArgs e)
		{
			if (midiOut == null)
			{
				midiOut = new MidiOut(comboBoxMidiOutDevices.SelectedIndex);
			}
		}
	}
}