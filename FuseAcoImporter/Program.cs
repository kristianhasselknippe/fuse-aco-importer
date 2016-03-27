using System;
using System.Collections.Generic;
using System.IO;

namespace FuseAcoImporter
{
	public class RGBColor
	{
		public readonly string Name;
		public readonly ushort R;
		public readonly ushort G;
		public readonly ushort B;

		public string HexString(){
			var rb = (byte)(R/256.0);
			var gb = (byte)(G/256.0);
			var bb = (byte)(B/256.0);
			string hex_ = BitConverter.ToString (
												 new byte[]{ rb, gb, bb });

			var hex = hex_.Replace("-", string.Empty);
			return "#" + hex;
		}

		public RGBColor(string name, ushort r, ushort g, ushort b)
		{
			Name = name;
			R = r;
			G = g;
			B = b;
		}
	}

	public class Aco
	{
		public List<RGBColor> Colors = new List<RGBColor>();


		public Aco(byte[] bytes)
		{
			/*if (BitConverter.IsLittleEndian)
			  Array.Reverse(bytes);*/
			//TODO(kristian): check that endianness is ok
			var words = new UInt16[bytes.Length];
            var wordCount = 0;
			for (var i = 0; i < bytes.Length-1; i += 2) {
				var b = new byte[2];
				if (BitConverter.IsLittleEndian)
				{
					b[1] = bytes[i];
					b[0] =  bytes[i+1];
				}
				else
				{
					b[0] = bytes[i];
					b[1] =  bytes[i+1];
				}
				words[wordCount++] = BitConverter.ToUInt16(b,0);
			}

			var version = words[0];
			var nColors = words[1];


			var offset = 2;

			for (var i = 0; i < nColors; i++)
			{
				var name = "color" + i;
				var colorSpace = words[offset++];
				var w = words[offset++];
				var x = words[offset++];
				var y = words[offset++];
				var z = words[offset++];
				Colors.Add(new RGBColor(name, w, x, y));
			}

			version = words[offset++];
			nColors = words[offset++];

            for (var i = 0; i < nColors; i++)
			{
				if (version == 2)
				{
					var name = "color" + i;
					var colorSpace = words[offset++];
					var w = words[offset++];
					var x = words[offset++];
					var y = words[offset++];
					var z = words[offset++];
					offset++;
					var lenPlus1 = words[offset++];
					name = System.Text.UnicodeEncoding.BigEndianUnicode.GetString(bytes, offset*2,(lenPlus1-1) * 2);
					offset += lenPlus1;

					Colors.Add(new RGBColor(name, w, x, y));

				}


            }

		}
	}

	public class UXWriter
	{
		List<string> lines = new List<string>();
		public UXWriter(List<RGBColor> colors, string outClassName)
		{
			lines.Add("<Panel ux:Class=\"" + outClassName + "\">");
			foreach (var c in colors) {
				lines.Add ("\t<float4 ux:Global=\"" + c.Name + "\" ux:Value=\"" + c.HexString() + "\" />");
			}
			lines.Add ("</Panel>");
		}
	}


	class MainClass
	{
		public static void Main (string[] args)
		{
			/*if (args.Length < 1) {
			  Console.WriteLine("You need to pass a file as an argument");
			  return;
			  }*/
			var fileName = "C:/Users/Hassel/dev/fuse-aco-importer/FuseAcoImporter/Exploring.aco";
			var bytes = File.ReadAllBytes (fileName);
			var aco = new Aco (bytes);
			var writer = new UXWriter(aco.Colors, "ColorPalette.ux");
		}
	}
}
