﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel; // for BindingList

using Jacobi.Vst.Core.Host;
using Jacobi.Vst.Interop.Host;

using DiffPlex;
using DiffPlex.Model;

using System.Linq;

using CommonUtils;
using CommonUtils.Diff;

namespace SynthAnalysisStudio
{
	public enum DiffType : int
	{
		Binary,
		Text
	}

	/// <summary>
	/// The HostCommandStub class represents the part of the host that a plugin can call.
	/// </summary>
	class HostCommandStub : IVstHostCommandStub
	{
		// should we store the chunk files on every paramater change when investigating the preset format?
		private bool debugChunkFiles = true;

		// these three variables are used to track changes to a preset file content, in order to reverse engineer the binary content
		private byte[] previousChunkData;
		private bool investigatePluginPresetFileFormat = false; // determine if we are tracking the preset file chunk at all
		private BindingList<InvestigatedPluginPresetFileFormat> investigatedPluginPresetFileFormatList = new BindingList<InvestigatedPluginPresetFileFormat>();
		
		private bool trackPluginPresetFileFormat = false; // track specific number of bytes from a specific position
		private int trackPluginPresetFilePosition = -1;
		private int trackPluginPresetFileNumberOfBytes = 0;
		private byte[] trackPluginPresetFileBytes;
		
		private DiffType investigatePluginPresetFileFormatDiffType = DiffType.Binary;

		public DiffType InvestigatePluginPresetFileFormatDiffType {
			set {
				this.investigatePluginPresetFileFormatDiffType = value;
			}
			get {
				return this.investigatePluginPresetFileFormatDiffType;
			}
		}
		
		public bool DoTrackPluginPresetFileFormat {
			set {
				this.trackPluginPresetFileFormat = value;
			}
			get {
				return this.trackPluginPresetFileFormat;
			}
		}

		public int TrackPluginPresetFilePosition {
			set {
				this.trackPluginPresetFilePosition = value;
			}
			get {
				return this.trackPluginPresetFilePosition;
			}
		}

		public int TrackPluginPresetFileNumberOfBytes {
			set {
				this.trackPluginPresetFileNumberOfBytes = value;
			}
			get {
				return this.trackPluginPresetFileNumberOfBytes;
			}
		}

		public byte[] TrackPluginPresetFileBytes {
			set {
				this.trackPluginPresetFileBytes = value;
			}
			get {
				return this.trackPluginPresetFileBytes;
			}
		}
		
		public bool DoInvestigatePluginPresetFileFormat {
			set {
				this.investigatePluginPresetFileFormat = value;
			}
			get {
				return this.investigatePluginPresetFileFormat;
			}
		}

		public BindingList<InvestigatedPluginPresetFileFormat> InvestigatedPluginPresetFileFormatList {
			set {
				this.investigatedPluginPresetFileFormatList = value;
			}
			get {
				return this.investigatedPluginPresetFileFormatList;
			}
		}
		
		Jacobi.Vst.Core.VstTimeInfo vstTimeInfo = new Jacobi.Vst.Core.VstTimeInfo();
		
		/// <summary>
		/// Raised when one of the methods is called.
		/// </summary>
		public event EventHandler<PluginCalledEventArgs> PluginCalled;

		private void RaisePluginCalled(string message)
		{
			EventHandler<PluginCalledEventArgs> handler = PluginCalled;

			if(handler != null)
			{
				handler(this, new PluginCalledEventArgs(message));
			}
		}

		#region IVstHostCommandsStub Members

		/// <inheritdoc />
		public IVstPluginContext PluginContext { get; set; }
		
		#endregion

		#region IVstHostCommands20 Members

		/// <inheritdoc />
		public bool BeginEdit(int index)
		{
			RaisePluginCalled("BeginEdit(" + index + ")");
			return false;
		}

		/// <inheritdoc />
		public Jacobi.Vst.Core.VstCanDoResult CanDo(string cando)
		{
			RaisePluginCalled("CanDo(" + cando + ")");
			return Jacobi.Vst.Core.VstCanDoResult.Unknown;
		}

		/// <inheritdoc />
		public bool CloseFileSelector(Jacobi.Vst.Core.VstFileSelect fileSelect)
		{
			RaisePluginCalled("CloseFileSelector(" + fileSelect.Command + ")");
			return false;
		}

		/// <inheritdoc />
		public bool EndEdit(int index)
		{
			RaisePluginCalled("EndEdit(" + index + ")");
			return false;
		}

		/// <inheritdoc />
		public Jacobi.Vst.Core.VstAutomationStates GetAutomationState()
		{
			RaisePluginCalled("GetAutomationState()");
			return Jacobi.Vst.Core.VstAutomationStates.Off;
		}

		/// <inheritdoc />
		public int GetBlockSize()
		{
			RaisePluginCalled("GetBlockSize()");
			return 1024;
		}

		/// <inheritdoc />
		public string GetDirectory()
		{
			RaisePluginCalled("GetDirectory()");
			return null;
		}

		/// <inheritdoc />
		public int GetInputLatency()
		{
			RaisePluginCalled("GetInputLatency()");
			return 0;
		}

		/// <inheritdoc />
		public Jacobi.Vst.Core.VstHostLanguage GetLanguage()
		{
			RaisePluginCalled("GetLanguage()");
			return Jacobi.Vst.Core.VstHostLanguage.NotSupported;
		}

		/// <inheritdoc />
		public int GetOutputLatency()
		{
			RaisePluginCalled("GetOutputLatency()");
			return 0;
		}

		/// <inheritdoc />
		public Jacobi.Vst.Core.VstProcessLevels GetProcessLevel()
		{
			RaisePluginCalled("GetProcessLevel()");
			return Jacobi.Vst.Core.VstProcessLevels.Unknown;
		}

		/// <inheritdoc />
		public string GetProductString()
		{
			RaisePluginCalled("GetProductString()");
			return "VST.NET";
		}

		/// <inheritdoc />
		public float GetSampleRate()
		{
			RaisePluginCalled("GetSampleRate()");
			return 44.8f;
		}

		/// <inheritdoc />
		public Jacobi.Vst.Core.VstTimeInfo GetTimeInfo(Jacobi.Vst.Core.VstTimeInfoFlags filterFlags)
		{
			RaisePluginCalled("GetTimeInfo(" + filterFlags + ")");
			vstTimeInfo.SamplePosition = 0.0;
			vstTimeInfo.SampleRate = 44100;
			vstTimeInfo.NanoSeconds = 0.0;
			vstTimeInfo.PpqPosition = 0.0;
			vstTimeInfo.Tempo = 120.0;
			vstTimeInfo.BarStartPosition = 0.0;
			vstTimeInfo.CycleStartPosition = 0.0;
			vstTimeInfo.CycleEndPosition = 0.0;
			vstTimeInfo.TimeSignatureNumerator = 4;
			vstTimeInfo.TimeSignatureDenominator = 4;
			vstTimeInfo.SmpteOffset = 0;
			vstTimeInfo.SmpteFrameRate = new Jacobi.Vst.Core.VstSmpteFrameRate();
			vstTimeInfo.SamplesToNearestClock = 0;
			vstTimeInfo.Flags = 0;

			return vstTimeInfo;
		}

		/// <inheritdoc />
		public string GetVendorString()
		{
			RaisePluginCalled("GetVendorString()");
			return "Jacobi Software";
		}

		/// <inheritdoc />
		public int GetVendorVersion()
		{
			RaisePluginCalled("GetVendorVersion()");
			return 1000;
		}

		/// <inheritdoc />
		public bool IoChanged()
		{
			RaisePluginCalled("IoChanged()");
			return false;
		}

		/// <inheritdoc />
		public bool OpenFileSelector(Jacobi.Vst.Core.VstFileSelect fileSelect)
		{
			RaisePluginCalled("OpenFileSelector(" + fileSelect.Command + ")");
			return false;
		}

		/// <inheritdoc />
		public bool ProcessEvents(Jacobi.Vst.Core.VstEvent[] events)
		{
			RaisePluginCalled("ProcessEvents(" + events.Length + ")");
			return false;
		}

		/// <inheritdoc />
		public bool SizeWindow(int width, int height)
		{
			RaisePluginCalled("SizeWindow(" + width + ", " + height + ")");
			return false;
		}

		/// <inheritdoc />
		public bool UpdateDisplay()
		{
			RaisePluginCalled("UpdateDisplay()");
			return false;
		}

		#endregion

		#region IVstHostCommands10 Members

		/// <inheritdoc />
		public int GetCurrentPluginID()
		{
			RaisePluginCalled("GetCurrentPluginID()");
			if (PluginContext.PluginInfo != null) {
				return PluginContext.PluginInfo.PluginID;
			} else {
				return -1;
			}
		}

		/// <inheritdoc />
		public int GetVersion()
		{
			RaisePluginCalled("GetVersion()");
			return 1000;
		}

		/// <inheritdoc />
		public void ProcessIdle()
		{
			RaisePluginCalled("ProcessIdle()");
		}

		/// <inheritdoc />
		public void SetParameterAutomated(int index, float value)
		{
			RaisePluginCalled("SetParameterAutomated(" + index + ", " + value + ")");
			
			// This chunk of code handles investigation of a preset format in order to be able to
			// reverse engineer where the different changes to the parameters are stored within the preset file.
			string name = PluginContext.PluginCommandStub.GetParameterName(index);
			string label = PluginContext.PluginCommandStub.GetParameterLabel(index);
			string display = PluginContext.PluginCommandStub.GetParameterDisplay(index);
			if ("".Equals(display)) display = "" + value;
			System.Diagnostics.Debug.WriteLine("SetParameterAutomated. Name: {0}, Label: {1}, Value: {2}", name, label, display);
			
			if (DoInvestigatePluginPresetFileFormat) {
				// read in the preset chunk and
				// do a binary comparison between what changed before and after this method was called
				byte[] chunkData = PluginContext.PluginCommandStub.GetChunk(true);
				
				// if we are tracking a specific number of bytes from the chunk, store those
				if (DoTrackPluginPresetFileFormat) {
					if (TrackPluginPresetFilePosition > -1 && TrackPluginPresetFileNumberOfBytes > 0) {
						byte[] trackedChunkData = new byte[TrackPluginPresetFileNumberOfBytes];
						Array.Copy(chunkData, TrackPluginPresetFilePosition, trackedChunkData, 0, TrackPluginPresetFileNumberOfBytes);
						TrackPluginPresetFileBytes = trackedChunkData;
					}
				}
				
				// if the chunk is not empty, try to detect what has changed
				if (chunkData != null && chunkData.Length > 0) {
					int chunkLength = chunkData.Length;
					
					// TODO: DELETE THIS
					/*
					string wavesPluginName = WavesPreset.GetPluginName(chunkData);
					if (wavesPluginName != null) {
						switch (wavesPluginName) {
							case "SSLChannel":
								WavesSSLChannel sslChannel = new WavesSSLChannel();
								sslChannel.ReadChunkData(chunkData);
								sslChannel.Write("sslchannel-output.txt");
								break;
							case "SSLComp":
								WavesSSLComp sslComp = new WavesSSLComp();
								sslComp.ReadChunkData(chunkData);
								sslComp.Write("sslcomp-output.txt");
								break;
							case "PuigChild":
								break;
						}
					}
					 */
					
					// binary comparison to find out where the chunk has changed
					if (previousChunkData != null && previousChunkData.Length > 0) {
						
						if (debugChunkFiles) {
							BinaryFile.ByteArrayToFile("Preset Chunk Data - previousChunkData.dat", previousChunkData);
							BinaryFile.ByteArrayToFile("Preset Chunk Data - chunkData.dat", chunkData);
						}
						
						if (InvestigatePluginPresetFileFormatDiffType == DiffType.Binary) {
							SimpleBinaryDiff.Diff diff = SimpleBinaryDiff.GetDiff(previousChunkData, chunkData);
							if (diff != null) {
								System.Diagnostics.Debug.WriteLine("BinDiff: {0}", diff);
								
								// store each of the chunk differences in a list
								foreach (SimpleBinaryDiff.DiffPoint point in diff.Points) {
									this.InvestigatedPluginPresetFileFormatList.Add(
										new InvestigatedPluginPresetFileFormat(point.Index, point.NewValue, name, label, display));
								}
							}
						} else if (InvestigatePluginPresetFileFormatDiffType == DiffType.Text) {
							// assume we are dealing with text and not binary data
							var d = new Differ();
							string OldText = BinaryFile.ByteArrayToString(previousChunkData);
							string NewText = BinaryFile.ByteArrayToString(chunkData);
							
							DiffResult res = d.CreateWordDiffs(OldText, NewText, true, true, new[] {' ', '\r', '\n'});
							//DiffResult res = d.CreateCharacterDiffs(OldText, NewText, true, true);

							List<UnidiffEntry> diffList = UnidiffSeqFormater.GenerateWithLineNumbers(res);
							var queryTextDiffs = from dl in diffList
								where dl.Type == UnidiffType.Insert
								select dl;
							
							foreach (var e in queryTextDiffs) {
								string text = e.Text;
								text = text.Replace("\n", "");
								if (text != "") {
									System.Diagnostics.Debug.WriteLine(String.Format("TextDiff: {0} {1}", e.Index, text));
									
									this.InvestigatedPluginPresetFileFormatList.Add(
										new InvestigatedPluginPresetFileFormat(e.Index, 0, name, label, display, text));
								}
							}
						}
					}
					previousChunkData = chunkData;
				}
			}
		}

		#endregion
	}

	/// <summary>
	/// Event arguments used when one of the mehtods is called.
	/// </summary>
	class PluginCalledEventArgs : EventArgs
	{
		/// <summary>
		/// Constructs a new instance with a <paramref name="message"/>.
		/// </summary>
		/// <param name="message"></param>
		public PluginCalledEventArgs(string message)
		{
			Message = message;
		}

		/// <summary>
		/// Gets the message.
		/// </summary>
		public string Message { get; private set; }
	}
}
