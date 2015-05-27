﻿using System;

namespace CommonUtils
{
	/// <summary>
	/// Description of Export.
	/// </summary>
	public static class Export
	{
		#region exportCSV
		public static void ExportCSV(string filenameToSave, Array data, int length=0) {
			
			if (length == 0) {
				length = data.Length;
			}
			
			var arr = new object[length][];
			
			int count = 1;
			for (int i = 0; i < length; i++)
			{
				arr[i] = new object[2] {
					count,
					data.GetValue(i)
				};
				count++;
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}
		
		public static void ExportCSV(string filenameToSave, float[][] data) {
			var arr = new object[data.Length][];

			for (int i = 0; i < data.Length; i++)
			{
				arr[i] = new object[data[i].Length];
				for (int j = 0; j < data[i].Length; j++)
				{
					arr[i][j] = data[i][j];
				}
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}

		public static void ExportCSV(string filenameToSave, double[][] data) {
			var arr = new object[data.Length][];

			for (int i = 0; i < data.Length; i++)
			{
				arr[i] = new object[data[i].Length];
				for (int j = 0; j < data[i].Length; j++)
				{
					arr[i][j] = data[i][j];
				}
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}

		public static void ExportCSV(string filenameToSave, Array column1, Array column2) {
			if (column1.Length != column2.Length) return;
			
			var arr = new object[column1.Length][];

			int count = 1;
			for (int i = 0; i < column1.Length; i++)
			{
				arr[i] = new object[3] {
					count,
					column1.GetValue(i),
					column2.GetValue(i)
				};
				count++;
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}

		public static void ExportCSV(string filenameToSave, Array column1, Array column2, Array column3) {
			if (column1.Length != column2.Length || column1.Length != column3.Length) return;
			
			var arr = new object[column1.Length][];

			int count = 1;
			for (int i = 0; i < column1.Length; i++)
			{
				arr[i] = new object[4] {
					count,
					column1.GetValue(i),
					column2.GetValue(i),
					column3.GetValue(i)
				};
				count++;
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}

		public static void ExportCSV(string filenameToSave, Array column1, Array column2, Array column3, Array column4) {
			if (column1.Length != column2.Length
			    || column1.Length != column3.Length
			    || column1.Length != column4.Length) return;
			
			var arr = new object[column1.Length][];

			int count = 1;
			for (int i = 0; i < column1.Length; i++)
			{
				arr[i] = new object[5] {
					count,
					column1.GetValue(i),
					column2.GetValue(i),
					column3.GetValue(i),
					column4.GetValue(i)
				};
				count++;
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}

		public static void ExportCSV(string filenameToSave, Array column1, Array column2, Array column3, Array column4, Array column5) {
			if (column1.Length != column2.Length
			    || column1.Length != column3.Length
			    || column1.Length != column4.Length
			    || column1.Length != column5.Length) return;
			
			var arr = new object[column1.Length][];

			int count = 1;
			for (int i = 0; i < column1.Length; i++)
			{
				arr[i] = new object[6] {
					count,
					column1.GetValue(i),
					column2.GetValue(i),
					column3.GetValue(i),
					column4.GetValue(i),
					column5.GetValue(i)
				};
				count++;
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}

		public static void ExportCSV(string filenameToSave, Array column1, Array column2, Array column3, Array column4, Array column5, Array column6) {
			if (column1.Length != column2.Length
			    || column1.Length != column3.Length
			    || column1.Length != column4.Length
			    || column1.Length != column5.Length
			    || column1.Length != column6.Length) return;
			
			var arr = new object[column1.Length][];

			int count = 1;
			for (int i = 0; i < column1.Length; i++)
			{
				arr[i] = new object[7] {
					count,
					column1.GetValue(i),
					column2.GetValue(i),
					column3.GetValue(i),
					column4.GetValue(i),
					column5.GetValue(i),
					column6.GetValue(i),
				};
				count++;
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}

		public static void ExportCSV(string filenameToSave, Array column1, Array column2, Array column3, Array column4, Array column5, Array column6, Array column7) {
			if (column1.Length != column2.Length
			    || column1.Length != column3.Length
			    || column1.Length != column4.Length
			    || column1.Length != column5.Length
			    || column1.Length != column6.Length
			    || column1.Length != column7.Length) return;
			
			var arr = new object[column1.Length][];

			int count = 1;
			for (int i = 0; i < column1.Length; i++)
			{
				arr[i] = new object[8] {
					count,
					column1.GetValue(i),
					column2.GetValue(i),
					column3.GetValue(i),
					column4.GetValue(i),
					column5.GetValue(i),
					column6.GetValue(i),
					column7.GetValue(i),
				};
				count++;
			};
			
			var csv = new CSVWriter(filenameToSave);
			csv.Write(arr);
		}
		#endregion
		
	}
}
