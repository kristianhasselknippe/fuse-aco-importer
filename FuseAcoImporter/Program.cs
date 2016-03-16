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
		public List<RGBColor> Colors = new List<RGBColor> ();


		public Aco(byte[] bytes)
		{
			/*if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);*/
			//TODO(kristian): check that endianness is ok
			List<UInt16> words = new List<UInt16>();
			for (var i = 0; i < bytes.Length-1; i += 2) {
				var b = new []{ bytes[i+1], bytes[i] };
				words.Add (BitConverter.ToUInt16 (b, 0));
			}
			var version = words [0];
			var nColors = words [1];
			var index = 2;
			int count = 0;
			while (true) {
				var space = words [index++];
				var w = words [index++];
				var x = words [index++];
				var y = words [index++];
				var z = words [index++];

				if (version == 0){
					Colors.Add (new RGBColor ("Color" + (++count), w, x, y));
				} else if (version == 1) {
					index++;
					var nameLength = words[index++];
					var name = BitConverter.ToString(bytes, index*2, nameLength);
					index += nameLength;
					Colors.Add(new RGBColor(name,w,x,y));
				} else {
					throw new Exception("Unrecognized aco version");
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
			var fileName = "/Users/Hassel/Projects/FuseAcoImporter/FuseAcoImporter/Exploring.aco";
			var bytes = File.ReadAllBytes (fileName);
			var aco = new Aco (bytes);
			var writer = new UXWriter(aco.Colors, "ColorPalette.ux");
		}
	}
}
