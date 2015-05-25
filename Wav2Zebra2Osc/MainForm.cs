﻿using System;
using System.Windows.Forms;
using System.IO;

using CommonUtils;
using CommonUtils.Audio;

namespace Wav2Zebra2Osc
{
	/// <summary>
	/// Main Form
	/// </summary>
	public partial class MainForm : Form
	{
		private float[][] harmonicsData;
		private float[][] soundData;
		private float[][] dftData;
		private float[] emptyData;
		private float[] sineData;
		private string rawExportName;
		
		public Wav2Zebra2Osc.WaveDisplayUserControl[] waveDisplays;
		
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			// Initialize Bass
			BassProxy instance = BassProxy.Instance;

			// Ensure the paint methods are called if resized
			ResizeRedraw = true;
			
			// Initialize the wave display cells
			waveDisplays = new Wav2Zebra2Osc.WaveDisplayUserControl[16];
			int counter = 0;
			for( int row = 0; row != 4; ++row ) {
				for( int col = 0; col != 4; ++col ) {
					this.waveDisplays[counter] = new Wav2Zebra2Osc.WaveDisplayUserControl(this);
					this.tableLayoutPanel.Controls.Add(waveDisplays[counter], col, row);
					this.waveDisplays[counter].ResumeLayout(false);
					this.waveDisplays[counter].PerformLayout();
					counter++;
				}
			}
			this.waveDisplays[0].Selected = true;

			// Initalize the data arrays
			this.harmonicsData = RectangularArrays.ReturnRectangularFloatArray(16, 128);
			this.soundData = RectangularArrays.ReturnRectangularFloatArray(16, 128);
			this.dftData = RectangularArrays.ReturnRectangularFloatArray(16, 128);
			this.emptyData = new float[128];
			this.sineData = new float[128];

			for (int j = 0; j < 128; j++)
			{
				this.emptyData[j] = 0.0F;
				this.sineData[j] = (float) Math.Sin(j * Conversions.TWO_PI / 128.0F);
			}
			
			// clear data
			for (int i = 0; i < 16; i++)
			{
				Array.Copy(this.harmonicsData[i], 0, this.emptyData, 0, 128);
				Array.Copy(this.soundData[i], 0, this.emptyData, 0, 128);
				Array.Copy(this.dftData[i], 0, this.emptyData, 0, 128);
			}
			
			Array.Copy(this.dftData[0], 0, this.sineData, 0, 128);
			Array.Copy(this.dftData[15], 0, this.sineData, 0, 128);
			this.harmonicsData[0][0] = 1.0F;
			this.harmonicsData[15][0] = 1.0F;

			this.waveDisplays[0].Loaded = true;
			this.waveDisplays[15].Loaded = true;
			this.waveDisplays[0].DftData = this.dftData[0];
			this.waveDisplays[15].DftData = this.dftData[15];
			this.waveDisplays[0].Refresh();
			this.waveDisplays[15].Refresh();
			
			String fileName = "Wav2Zebra2.ini";
			string pathName = "";
			if (File.Exists(fileName)) {
				try {
					StreamReader reader = File.OpenText(fileName);
					pathName = reader.ReadLine();
					reader.Close();
				} catch (Exception e) {
					System.Diagnostics.Debug.WriteLine(e);
				}
			}
			this.OutputText = "Export path is: " + pathName;
		}

		public virtual bool DoShowRAWWaves
		{
			get
			{
				return this.showRAWRadioButton.Checked;
			}
		}

		public virtual bool DoShowDFTWaves
		{
			get
			{
				return this.showDFTRadioButton.Checked;
			}
		}

		public virtual bool DoExportDFTWaves
		{
			get
			{
				return this.exportDFTWavesCheckBox.Checked;
			}
		}

		public virtual bool DoExportRAWWaves
		{
			get
			{
				return this.exportRAWWavesCheckBox.Checked;
			}
		}

		public virtual string OutputText
		{
			set
			{
				this.outputField.Text = value;
			}
			get
			{
				return this.outputField.Text;
			}
		}

		public virtual int SelectedWaveDisplay
		{
			get
			{
				int selected = -1;
				for (int i = 0; i < 16; i++)
				{
					if (this.waveDisplays[i].Selected)
					{
						selected = i;
					}
				}
				return selected;
			}
		}
		
		// http://stackoverflow.com/questions/7995470/how-can-i-resample-wav-file
		public static float[] ReSampleToArbitrary(float[] input, int size) {
			
			//int fromSampleRate = 44100;
			//int toSampleRate = 692;
			int quality = 10;
			
			int srcLength = input.Length;
			//int destLength = (int) ((double) input.Length * (double) toSampleRate / (double) fromSampleRate);
			int destLength = size;
			double dx = (double) srcLength / (double) destLength;

			//var samples = new List<float>();
			var samples = new float[destLength];
			
			// fmax : Nyqist half of destination sampleRate
			// fmax / fsr = 0.5;
			const double fmaxDivSR = 0.5;
			const double r_g = 2 * fmaxDivSR;

			// Quality is half the window width
			int wndWidth2 = quality;
			int wndWidth = quality * 2;

			double x = 0;
			int i, j = 0;
			double r_y;
			int tau;
			double r_w;
			double r_a;
			double r_snc;
			
			for (i=0; i < destLength; ++i)
			{
				r_y = 0.0;
				for (tau=-wndWidth2; tau < wndWidth2; ++tau)
				{
					// input sample index
					j = (int)(x+tau);

					// Hann Window. Scale and calculate sinc
					r_w = 0.5 - 0.5 * Math.Cos(2 * Math.PI * (0.5 + (j - x) / wndWidth));
					r_a = 2 * Math.PI * (j-x) * fmaxDivSR;
					r_snc = 1.0;
					if (r_a != 0)
						r_snc = Math.Sin(r_a)/r_a;

					if ((j >= 0) && (j < srcLength))
					{
						r_y += r_g * r_w * r_snc * input[j];
					}
				}
				samples[i] = (float) r_y;
				x += dx;
			}
			
			return samples;
		}
		
		public virtual void SetImportSound(FileInfo file, int selected)
		{
			float[] tempAudioBuffer = BassProxy.ReadMonoFromFile(file.FullName);
			float[] tempAudioBuffer2 = MathUtils.ReSampleToArbitrary(tempAudioBuffer, 128);
			//float[] tempAudioBuffer2 = ReSampleToArbitrary(tempAudioBuffer, 128);
			
			#if DEBUG
			Export.ExportCSV(file.Name + ".csv", tempAudioBuffer2);
			#endif
			
			Array.Copy(tempAudioBuffer2, 0, this.soundData[selected], 0, 128);
			
			// TODO: use lomont instead
			this.harmonicsData[selected] = Conversions.DFT(this.soundData[selected]);
			//this.harmonicsData[selected] = Fourier.DFTTransformer.AbsFFT(this.soundData[selected]);
			this.dftData[selected] = Conversions.iDFT(this.harmonicsData[selected]);
			//this.dftData[selected] = Fourier.DFTTransformer.IFFT(this.harmonicsData[selected], false, false);
			
			this.waveDisplays[selected].HarmonicsData = this.harmonicsData[selected];
			this.waveDisplays[selected].WaveData = this.soundData[selected];
			this.waveDisplays[selected].DftData = this.dftData[selected];
			this.waveDisplays[selected].Refresh();
		}
		
		public virtual void ExportToZebra2(bool dft, bool raw)
		{
			String fileName = "Wav2Zebra2.ini";
			while (!File.Exists(fileName)) {
				this.OutputText = "Export path is not set";
				SetExportPath();
				this.OutputText = "";
			}
			
			string pathName = "";
			try {
				StreamReader reader = File.OpenText(fileName);
				pathName = reader.ReadLine();
				reader.Close();
			} catch (IOException e) {
				System.Diagnostics.Debug.WriteLine(e);
			}
			
			if (!HasExportFileNames)
			{
				this.OutputText = "There's nothing to export.";
			} else {
				string exportName = "";
				if (dft) {
					exportName = pathName + Path.DirectorySeparatorChar + this.exportFileName.Text;
					exportName = FixExportNames(exportName, true);
					exportName = RenameIfFileExists(exportName);
					
					try
					{
						StreamWriter writer = File.CreateText(exportName);
						string temp1 = "";
						temp1 = temp1 + "#defaults=no \n";
						temp1 = temp1 + "#cm=OSC \n";
						temp1 = temp1 + "Wave=2 \n";
						temp1 = temp1 + "<? \n\n";
						temp1 = temp1 + "float Wave[ 128 ]; \n";
						
						for (int j = 0; j < 16; j++)
						{
							for (int i = 0; i < 128; i++)
							{
								string sampleValue = "" + this.dftData[j][i];
								if ((this.dftData[j][i] < 0.001F) && (this.dftData[j][i] > -0.001F))
								{
									sampleValue = "0.0";
								}
								temp1 = temp1 + "Wave[" + i + "] = " + sampleValue + "; \n";
							}
							temp1 = temp1 + "Selected.WaveTable.set( " + (j + 1) + " , Wave ); \n\n";
						}
						
						temp1 = temp1 + "?> \n";
						writer.Write(temp1);
						writer.Close();
					} catch (IOException e) {
						System.Diagnostics.Debug.WriteLine(e);
					}
				}
				
				if (raw)
				{
					exportName = pathName + Path.DirectorySeparatorChar + this.exportFileName.Text;
					exportName = FixExportNames(exportName, false);
					exportName = RenameIfFileExists(exportName);
					
					try
					{
						StreamWriter writer = File.CreateText(exportName);
						string temp1 = "";
						temp1 = temp1 + "#defaults=no \n";
						temp1 = temp1 + "#cm=OSC \n";
						temp1 = temp1 + "Wave=2 \n";
						temp1 = temp1 + "<? \n\n";
						temp1 = temp1 + "float Wave[ 128 ]; \n";
						
						for (int j = 0; j < 16; j++)
						{
							if (this.waveDisplays[j].FileName != "")
							{
								for (int i = 0; i < 128; i++)
								{
									string sampleValue = "" + this.soundData[j][i];
									if ((this.soundData[j][i] < 0.001F) && (this.soundData[j][i] > -0.001F))
									{
										sampleValue = "0.0";
									}
									temp1 = temp1 + "Wave[" + i + "] = " + sampleValue + "; \n";
								}
								temp1 = temp1 + "Selected.WaveTable.set( " + (j + 1) + " , Wave ); \n\n";
							}
						}
						
						temp1 = temp1 + "?> \n";
						writer.Write(temp1);
						writer.Close();
					} catch (IOException e) {
						System.Diagnostics.Debug.WriteLine(e);
					}
				}
				this.outputField.Text = "File exported as: " + exportName;
			}
		}
		
		public virtual void SetExportPath()
		{
			var folderDialog = new FolderBrowserDialog();
			folderDialog.Description = "Set export path";
			
			DialogResult retval = folderDialog.ShowDialog();
			if (retval == DialogResult.OK)
			{
				string pathName = folderDialog.SelectedPath;
				const String initFileName = "Wav2Zebra2.ini";
				try
				{
					StreamWriter writer = File.CreateText(initFileName);
					writer.WriteLine(pathName);
					writer.Close();
				} catch (IOException e) {
					System.Diagnostics.Debug.WriteLine(e);
				}
				
				this.OutputText = "Export path is: " + pathName;
			}
		}
		
		public virtual void LoadCell()
		{
			var fileDialog = new OpenFileDialog();
			fileDialog.Multiselect = true;
			//fileDialog.Filter = "MP3|*.mp3|WAV|*.wav|All files (*.*)|*.*";
			//fileDialog.InitialDirectory = initialDirectory;
			fileDialog.Title = "Select an audio file";

			this.OutputText = "";
			int selected = this.SelectedWaveDisplay;
			if (selected > -1)
			{
				DialogResult retval = fileDialog.ShowDialog();
				if (retval == DialogResult.OK)
				{
					string[] files = fileDialog.FileNames;
					int count = files.Length;
					for (int i = 0; (i < count) && (i + selected < 16); i++)
					{
						try
						{
							SetImportSound(new FileInfo(files[i]), selected + i);
							this.waveDisplays[(selected + i)].FileName = files[i];
						} catch (Exception e) {
							this.OutputText = "Not an audiofile";
							System.Diagnostics.Debug.WriteLine(e);
						}
					}
				}
			} else {
				this.OutputText = "No slot selected. Click on one to select slot.";
			}
			
			bool unused = HasExportFileNames;
			this.exportFileName.Text = this.rawExportName;
			this.waveDisplays[selected].Loaded = true;
			CalculateGhosts();
		}
		
		private bool HasExportFileNames
		{
			get
			{
				string beginning = "Default";
				string end = "Default";
				for (int i = 0; i < 16; i++)
				{
					string temp = this.waveDisplays[i].FileName;
					if ((beginning == "Default") && !string.IsNullOrEmpty(temp))
					{
						beginning = Path.GetFileName(this.waveDisplays[i].FileName);
					}
				}
				
				for (int i = 15; i >= 0; i--)
				{
					string temp = this.waveDisplays[i].FileName;
					if ((end == "Default") && !string.IsNullOrEmpty(temp))
					{
						end = Path.GetFileName(this.waveDisplays[i].FileName);
					}
				}
				
				beginning = RemoveFileSuffix(beginning);
				end = RemoveFileSuffix(end);
				beginning = RemoveWhiteSpace(beginning);
				end = RemoveWhiteSpace(end);
				this.rawExportName = (beginning + "To" + end);
				return (end != "Default") || (beginning != "Default");
			}
		}
		
		private string RenameIfFileExists(string checkName)
		{
			var fileInfo = new FileInfo(checkName);
			DirectoryInfo folder = fileInfo.Directory;
			string folderName = folder.FullName;
			int version = 0;
			string fileName = fileInfo.FullName;
			
			while (fileInfo.Exists)
			{
				version++;
				fileName = RemoveFileSuffix(fileName);
				string tmp = "" + version;
				while (tmp.Length < 3)
				{
					tmp = "0" + tmp;
				}
				checkName = folderName + Path.DirectorySeparatorChar + fileName + tmp + ".h2p";
				fileInfo = new FileInfo(checkName);
			}
			
			return checkName;
		}
		
		private string FixExportNames(string fullFilePath, bool dft)
		{
			string temp = fullFilePath;
			temp = RemoveFileSuffix(temp);
			if (dft)
			{
				temp = temp + "Dft";
			}
			else
			{
				temp = temp + "";
			}
			temp = CheckH2pSuffix(temp);
			return temp;
		}
		
		private string RemoveFileSuffix(string fullPath)
		{
			return Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath));
		}
		
		private string RemoveWhiteSpace(string a)
		{
			return a.Replace(" ", "");
		}
		
		private string CheckH2pSuffix(string a)
		{
			if (!a.EndsWith(".h2p", StringComparison.Ordinal)) {
				return a + ".h2p";
			} else {
				return a;
			}
		}
		
		private void CalculateGhosts()
		{
			int from = 0;
			int to = 0;
			while (to < 16)
			{
				while ((to < 16) && (this.waveDisplays[to].Loaded))
				{
					to++;
				}
				from = to - 1;
				while ((to < 16) && (!this.waveDisplays[to].Loaded))
				{
					to++;
				}
				if ((to < 16) && (from >= 0))
				{
					System.Diagnostics.Debug.WriteLineIf((from < to), String.Format("Warning: from value ({0}) is less than to value ({1})", from, to));
					ReCalculateHarmonics(from, to);
				}
			}
			
			for (int j = 0; j < 16; j++)
			{
				// TODO: change inverse transform to Lomont
				this.dftData[j] = Conversions.iDFT(this.harmonicsData[j]);
				//this.dftData[j] = Fourier.DFTTransformer.IFFT(this.harmonicsData[j], false, false);
				
				this.waveDisplays[j].DftData = this.dftData[j];
				this.waveDisplays[j].HarmonicsData = this.harmonicsData[j];
				this.waveDisplays[j].Refresh();
			}
		}
		
		public virtual void ReCalculateHarmonics(int from, int to)
		{
			float[] harmFrom = this.harmonicsData[from];
			float[] harmTo = this.harmonicsData[to];
			float[] tempHarm = new float[128];
			
			int steps = to - from - 1;
			float stepSize = 1.0F / (steps + 1.0F);
			float stepHelper = 0.0F;
			for (int i = 0; i < steps; i++)
			{
				for (int j = 0; j < 128; j++)
				{
					stepHelper = stepSize * (i + 1);
					tempHarm[j] = (harmFrom[j] * (1.0F - stepHelper) + harmTo[j] * stepHelper);
				}
				int sanityCheck = from + i + 1;
				Array.Copy(tempHarm, 0, this.harmonicsData[sanityCheck], 0, 128);
			}
		}
		
		public virtual void ClearAllCells()
		{
			for (int i = 0; i < 16; i++)
			{
				Array.Copy(this.emptyData, 0, this.harmonicsData[i], 0, 128);
				Array.Copy(this.emptyData, 0, this.soundData[i], 0, 128);
				Array.Copy(this.emptyData, 0, this.dftData[i], 0, 128);
				this.waveDisplays[0].Loaded = false;
				this.waveDisplays[i].FileName = "";
				this.waveDisplays[i].ClearHarmonics();
				this.waveDisplays[i].ClearWaveData();
				this.waveDisplays[i].ClearDftData();
				this.waveDisplays[i].Refresh();
			}
			
			Array.Copy(this.sineData, 0, this.dftData[0], 0, 128);
			Array.Copy(this.sineData, 0, this.dftData[15], 0, 128);
			this.harmonicsData[0][0] = 1.0F;
			this.harmonicsData[15][0] = 1.0F;
			this.waveDisplays[0].Loaded = true;
			this.waveDisplays[15].Loaded = true;
			this.waveDisplays[0].Refresh();
			this.waveDisplays[15].Refresh();
			
			this.exportFileName.Text = "";
		}
		
		void HelpToolStripMenuItem1Click(object sender, EventArgs e)
		{
			new Help().Show();
		}
	}
}


//----------------------------------------------------------------------------------------
//	Copyright © 2007 - 2011 Tangible Software Solutions Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class provides the logic to simulate Java rectangular arrays, which are jagged
//	arrays with inner arrays of the same length.
//----------------------------------------------------------------------------------------
internal static partial class RectangularArrays
{
	internal static float[][] ReturnRectangularFloatArray(int Size1, int Size2)
	{
		var Array = new float[Size1][];
		for (int Array1 = 0; Array1 < Size1; Array1++)
		{
			Array[Array1] = new float[Size2];
		}
		return Array;
	}
}
