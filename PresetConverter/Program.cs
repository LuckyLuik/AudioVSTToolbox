﻿using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using CommonUtils;

namespace PresetConverter
{
	class Program
	{
		// define the log file
		static FileInfo outputStatusLog = new FileInfo("preset_converter_log.txt");
		
		const string _version = "1.3.1";
		
		public static void Main(string[] args)
		{
			//Zebra2Preset.GenerateClassFields(@"C:\Users\perivar.nerseth\Documents\My Projects\AudioVSTToolbox\PresetConverter\_SharedResources\Zebra2-Default Sylenth1 Template.h2p", @"C:\Users\perivar.nerseth\Documents\My Projects\AudioVSTToolbox\PresetConverter\_SharedResources\properties.cs");
			//Zebra2Preset.GenerateWriteMethod(@"C:\Users\perivar.nerseth\Documents\My Projects\AudioVSTToolbox\PresetConverter\_SharedResources\Zebra2-Default Sylenth1 Template.h2p", @"C:\Users\perivar.nerseth\Documents\My Projects\AudioVSTToolbox\PresetConverter\_SharedResources\getpreset.cs");
			
			bool processDirectory = false;
			bool doProcessInitPresets = false;
			bool doOutputSylenthPresetTextDump = false;
			bool doDebug = false;
			int skipAfterCounter = -1;
			
			// Command line parsing
			string presetInputFileOrDirectory = "";
			string presetOutputFileDirectoryPath = "";
			
			var CommandLine = new Arguments(args);
			if(CommandLine["in"] != null) {
				presetInputFileOrDirectory = CommandLine["in"];
			}
			if(CommandLine["out"] != null) {
				presetOutputFileDirectoryPath = CommandLine["out"];
			}
			if(CommandLine["init"] != null) {
				doProcessInitPresets = true;
			}
			if(CommandLine["text"] != null) {
				doOutputSylenthPresetTextDump = true;
			}
			if(CommandLine["skipafter"] != null) {
				int skipafterResult = -1;
				if (int.TryParse(CommandLine["skipafter"], out skipafterResult)) {
					skipAfterCounter = skipafterResult;
				}
			}
			if(CommandLine["debug"] != null) {
				doDebug = true;
			}
			if (presetInputFileOrDirectory == "" || presetOutputFileDirectoryPath == "") {
				PrintUsage();
				return;
			}
			
			// Build preset file paths
			string sylenthPresetDirString = "";
			string sylenthPreset = "";
			
			// check if input is file or directory
			if (IOUtils.IsDirectory(presetInputFileOrDirectory)) {
				sylenthPresetDirString = presetInputFileOrDirectory;
				processDirectory = true;
			} else {
				sylenthPreset = presetInputFileOrDirectory;
				processDirectory = false;
			}
			
			// define default sylenth template for Zebra2
			const string zebra2_Sylenth1_PresetTemplate = @"Zebra2-Default Sylenth1 Template.h2p";
			
			IEnumerable<FileInfo> presetFiles;
			if (processDirectory) {
				// process directory
				var sylenthPresetDir = new DirectoryInfo(sylenthPresetDirString);
				presetFiles = sylenthPresetDir.GetFilesByExtensions(".fxb", ".fxp");
				
				Console.WriteLine("Processing {0} files in directory: '{1}' ...", presetFiles.Count(), sylenthPresetDir.Name);
				IOUtils.LogMessageToFile(outputStatusLog, String.Format("Processing {0} files in directory: '{1}' ...", presetFiles.Count(), sylenthPresetDir.Name));
			} else {
				// process single preset
				var sylenthPresetFile = new FileInfo(sylenthPreset);
				presetFiles = new FileInfo[] { sylenthPresetFile };

				Console.WriteLine("Processing preset file '{0}' ...", sylenthPresetFile.Name);
				IOUtils.LogMessageToFile(outputStatusLog, String.Format("Processing preset file '{0}' ...", sylenthPresetFile.Name));
			}
			
			foreach (FileInfo presetFile in presetFiles) {
				// read preset file
				var sylenth1 = new Sylenth1Preset();
				if (doDebug) Logger.logLevel = Logger.LogLevel.Debug;
				
				if (sylenth1.Read(presetFile.FullName)) {
					
					// Output a dump of the Sylenth1 Preset File
					if (doOutputSylenthPresetTextDump) {
						string outSylenthPresetTextDumpPath = Path.GetFileNameWithoutExtension(presetFile.Name) + "_Text.txt";
						TextWriter tw = new StreamWriter(outSylenthPresetTextDumpPath);
						tw.WriteLine(sylenth1);
						tw.Close();
					}

					// define output dir
					string outputDir = "";
					if (presetFile.Name.EndsWith(".fxb")) {
						outputDir = Path.Combine(presetOutputFileDirectoryPath, Path.GetFileNameWithoutExtension(presetFile.Name));
						if (!Directory.Exists(outputDir)) {
							Directory.CreateDirectory(outputDir);
						}
					} else {
						outputDir = presetOutputFileDirectoryPath;
					}
					
					// and convert to zebra 2
					var zebraAdapter = new Sylenth1ToZebra2PresetAdapter(sylenth1);
					List<Zebra2Preset> zebra2ConvertedList = zebraAdapter.ToZebra2Preset(zebra2_Sylenth1_PresetTemplate, doProcessInitPresets, skipAfterCounter);
					int count = 1;
					foreach (Zebra2Preset zebra2Converted in zebra2ConvertedList) {
						string presetName = StringUtils.MakeValidFileName(zebra2Converted.PresetName);
						string zebraGeneratedPreset = Path.Combine(outputDir, String.Format("{0:000}_{1}.h2p", zebra2Converted.BankIndex, presetName));
						zebra2Converted.Write(zebraGeneratedPreset);
						count++;
					}
				}
			}
		}
		
		public static void PrintUsage() {
			Console.WriteLine("Preset Converter. Version {0}.", _version);
			Console.WriteLine("Copyright (C) 2010-2015 Per Ivar Nerseth.");
			Console.WriteLine("NOTE! This version only supports Sylenth1 to Zebra2 preset conversion");
			Console.WriteLine();
			Console.WriteLine("Usage: PresetConverter.exe <Arguments>");
			Console.WriteLine();
			Console.WriteLine("Mandatory Arguments:");
			Console.WriteLine("\t-in=<path to a preset file or a directory to convert from>");
			Console.WriteLine("\t-out=<path to the directory to convert to>");
			Console.WriteLine();
			Console.WriteLine("Optional Arguments:");
			Console.WriteLine("\t-init <Do process presets with name 'init'. Default = disabled>");
			Console.WriteLine("\t-text <Dump the Sylenth1 Presets to text files. Default = disabled>");
			Console.WriteLine("\t-skipafter=number <skip presets after this item. Default = include all>");
			Console.WriteLine("\t-debug <Output debug information to the log file>");
			Console.WriteLine();
		}
		
		private static string GetDevelopProjectDir() {
			// to get the location the assembly is executing from
			//(not neccesarily where the it normally resides on disk)
			// in the case of the using shadow copies, for instance in NUnit tests,
			// this will be in a temp directory.
			string execPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

			//once you have the path you get the directory with:
			string execDirectory = System.IO.Path.GetDirectoryName(execPath);
			
			// move two directories up to get the base dir when executing from SharpDevelop
			// since it will then run from either the project-dir/bin/Debug or project-dir/bin/Release directory
			DirectoryInfo projectDirInfo = new DirectoryInfo(execDirectory).Parent.Parent;
			string projectDir = projectDirInfo.FullName;
			
			// move yeat another dir up to get start dir for all AudioVSTToolbox projects
			DirectoryInfo allProjectDirInfo = new DirectoryInfo(projectDir).Parent;
			string allProjectDir = allProjectDirInfo.FullName;

			return allProjectDir;
		}
	}
}