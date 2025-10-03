---
layout: post
title:  "Hackropole.fr | re | babyfuscation"
date:   2025-09-12 09:23:50 +0000
categories: Reverse
---

### I. Introductions
---

Hackropole.fr

* chall: [rev/babyfuscation](https://hackropole.fr/en/challenges/reverse/fcsc2025-reverse-babyfuscation/)

### II. Writeups
---

#### 1. first look
* chall cho chúng ta một file elf:
```
$ file babyfuscation
babyfuscation: ELF 64-bit LSB pie executable, x86-64, version 1 (SYSV), dynamically linked, interpreter /lib64/ld-linux-x86-64.so.2, BuildID[sha1]=80e2baa56f7f889a55c075839bfbb875d883476b, for GNU/Linux 3.2.0, not stripped
```
* chạy thử babyfuscation
```
$ ./babyfuscation 
Enter the flag: 
fuck
Wrong flag. Try again!
$ ./babyfuscation 
Enter the flag: 
bruh
Wrong flag. Try again!
```
<p>Có thể thấy thì babyfuscation là một flag checker :)</p>

#### 2. Re
* load elf lên IDA
```c
int __fastcall main(int argc, const char **argv, const char **envp)
{
  int i; // [rsp+4h] [rbp-Ch]
  int j; // [rsp+8h] [rbp-8h]
  int k; // [rsp+Ch] [rbp-4h]

  for ( i = 0; i <= 79; ++i )
  {
    envp = (const char **)VYeXkgjLLMrczyw7i7dJPkyAbxqgCahe;
    VYeXkgjLLMrczyw7i7dJPkyAbxqgCahe[i] ^= 0x42u;
  }
  for ( j = 0; j <= 79; ++j )
  {
    envp = (const char **)a93rEUcvwf4Ec9KHKqzFx7wL;
    a93rEUcvwf4Ec9KHKqzFx7wL[j] ^= 0x13u;
  }
  for ( k = 0; k <= 79; ++k )
  {
    envp = (const char **)ouPrjEhgqPVNXCqchuzw7WTWLHnkbwqj;
    ouPrjEhgqPVNXCqchuzw7WTWLHnkbwqj[k] ^= 0x37u;
  }
  VsvYbpipYYgRoCeFtoxhtAmdFuNu3WvV(argc, argv, envp);
  wKtyPoT4WdyrkVzhvYUfvqo3M9iPVMd3();
  return VakkEeHbtHMpNqXPMkadR4v7K();
}
```

* đập vào mắt người xem đầu tiên là 3 vòng for và `envp = (const char **)...;`. Theo mình thì các dòng `envp = (const char **)...` không ảnh hưởng lắm đến chương trình nên có thể bỏ qua. Lý do tại sao thì tí nữa chúng ta sẽ thấy.

* Vì 3 vòng for trong main này mình không có thông tin gì về mục đích của nó hết nên mình sẽ rev hàm được gọi đầu tiên trong main là `VsvYbpipYYgRoCeFtoxhtAmdFuNu3WvV(argc, argv, envp);`.
```c
__int64 VsvYbpipYYgRoCeFtoxhtAmdFuNu3WvV()
{
  __int64 result; // rax

  puts(VYeXkgjLLMrczyw7i7dJPkyAbxqgCahe);
  fgets(aixxj3qmUvFTqgqLodmuaEap, 80, _bss_start);
  result = LdUonKvqsjsJu4JdfAgtgbU9(aixxj3qmUvFTqgqLodmuaEap, &unk_2004);
  aixxj3qmUvFTqgqLodmuaEap[result] = 0;
  return result;
}
```
* Ở đây, khi chạy thử ./babyfuscation ở lần đầu thì string đầu tiên nó in ra là `Enter the flag: `, nên mình sẽ rename các biến lại cho dễ hiểu:
```c
__int64 VsvYbpipYYgRoCeFtoxhtAmdFuNu3WvV()
{
  __int64 result; // rax

  puts(enter_the_flag);
  fgets(input, 80, _bss_start);
  result = LdUonKvqsjsJu4JdfAgtgbU9(input, &unk_2004); // unk_2004 = 0x0A
  input[result] = 0;
  return result;
}
```
* Hàm này sẽ in ra dòng `Enter the flag: ` và lấy tối đa 80 kí tự từ input của user, và gọi hàm `LdUonKvqsjsJu4JdfAgtgbU9` với byte 0x0A hardcode trong file elf.
Hàm `LdUonKvqsjsJu4JdfAgtgbU9` mục đích là để tìm unk_2004(0x0A là giá trị của Non-breaking space trong ascii) có xuất hiện trong input hay không. Nếu có thì trả về vị trí của 0x0A trong input, nếu không có 0x0A thì trả về size của input. Tức là chuyển input --> null-terminated string.
```c
LdUonKvqsjsJu4JdfAgtgbU9(input, &unk_2004);
_BYTE *__fastcall LdUonKvqsjsJu4JdfAgtgbU9(_BYTE *a1, _BYTE *a2)
{
  _BYTE *j; // [rsp+10h] [rbp-10h]
  _BYTE *i; // [rsp+18h] [rbp-8h]

  for ( i = a1; *i; ++i )
  {
    for ( j = a2; *j; ++j )
    {
      if ( *i == *j )
        return (_BYTE *)(i - a1);
    }
  }
  return (_BYTE *)(i - a1);
}
```
```c
__int64 get_input_convert_to_null__terminated_string()
{
  __int64 result; // rax

  puts(enter_the_flag);
  fgets(input, 80, _bss_start);
  result = find_non_breaking_space(input, &unk_2004); // unk_2004 = 0x0A
  input[result] = 0;
  return result;
}
```
```c
int __fastcall main(int argc, const char **argv, const char **envp)
{
  int i; // [rsp+4h] [rbp-Ch]
  int j; // [rsp+8h] [rbp-8h]
  int k; // [rsp+Ch] [rbp-4h]

  for ( i = 0; i <= 79; ++i )
    enter_the_flag[i] ^= 0x42u;
  for ( j = 0; j <= 79; ++j )
    a93rEUcvwf4Ec9KHKqzFx7wL[j] ^= 0x13u;
  for ( k = 0; k <= 79; ++k )
    ouPrjEhgqPVNXCqchuzw7WTWLHnkbwqj[k] ^= 0x37u;
  get_input_convert_to_null__terminated_string();
  wKtyPoT4WdyrkVzhvYUfvqo3M9iPVMd3();
  return VakkEeHbtHMpNqXPMkadR4v7K(argc, argv);
}
```
* mình sẽ dùng debugger để tìm ra hai chuỗi `a93rEUcvwf4Ec9KHKqzFx7wL` và `ouPrjEhgqPVNXCqchuzw7WTWLHnkbwqj` vì nó cũng được xor decrypt như chuỗi enter_the_flag.

* chuỗi `a93rEUcvwf4Ec9KHKqzFx7wL`:
![](/images/hackropolefrrebabyfuscation/output_correct.webp)


* chuỗi `ouPrjEhgqPVNXCqchuzw7WTWLHnkbwqj`
![](/images/hackropolefrrebabyfuscation/output_wrong.webp)
```c
int __fastcall main(int argc, const char **argv, const char **envp)
{
  int i; // [rsp+4h] [rbp-Ch]
  int j; // [rsp+8h] [rbp-8h]
  int k; // [rsp+Ch] [rbp-4h]

  for ( i = 0; i <= 79; ++i )
    enter_the_flag[i] ^= 0x42u;
  for ( j = 0; j <= 79; ++j )
    correct[j] ^= 0x13u;
  for ( k = 0; k <= 79; ++k )
    wrong[k] ^= 0x37u;
  get_input_convert_to_null__terminated_string();
  wKtyPoT4WdyrkVzhvYUfvqo3M9iPVMd3();
  return VakkEeHbtHMpNqXPMkadR4v7K(argc, argv);
}
```
* viết blog mệt vcl

* Tiếp theo đến hàm `wKtyPoT4WdyrkVzhvYUfvqo3M9iPVMd3()`: 
```c
__int64 wKtyPoT4WdyrkVzhvYUfvqo3M9iPVMd3()
{
  __int64 result; // rax
  int i; // [rsp+8h] [rbp-8h]
  int v2; // [rsp+Ch] [rbp-4h]

  v2 = kRvUaKbhJewpX4HHFuMuPkNWc7xJ4cUV(input) + 1;
  for ( i = 0; i < v2; ++i )
    U94y77bvL3HfcnwcAc3UA9MJTvcwjP4j[i] = ((input[i] >> 5) | (8 * input[i])) ^ (3 * i + 31);
  result = v2;
  U94y77bvL3HfcnwcAc3UA9MJTvcwjP4j[v2] = 0;
  return result;
}
```
```c
__int64 __fastcall kRvUaKbhJewpX4HHFuMuPkNWc7xJ4cUV(_BYTE *a1)
{
  __int64 v3; // [rsp+10h] [rbp-8h]

  v3 = 0;
  while ( *a1 )
  {
    ++v3;
    ++a1;
  }
  return v3;
} // Chỉ là hàm sizeof() thôi
```
* ta thấy mục đích chính là nó lặp qua tất cả các char của input và sử dụng các phép bitwise shift right, or, xor để encrypt input.
* rename lại các biến trong hàm: 
```c
__int64 encrypt_input()
{
  __int64 result; // rax
  int i; // [rsp+8h] [rbp-8h]
  int v2; // [rsp+Ch] [rbp-4h]

  v2 = sizeof(input) + 1;
  for ( i = 0; i < v2; ++i )
    encrypted_input[i] = ((input[i] >> 5) | (8 * input[i])) ^ (3 * i + 31);
  result = v2;
  encrypted_input[v2] = 0;
  return result;
}
```
* Tiếp theo trong main gọi hàm compare(Mình đã rename lại): 
```c
__int64 compare()
{
  if ( (unsigned int)cmp(encrypted_input, &hardcoded_data) )
  {
    puts(wrong);
    return 0;
  }
  else
  {
    puts(correct);
    return 1;
  }
}
```

* TÓM TẮT: flag checker này rất đơn giản mà nãy giờ mình viết nhiều vaibiu, nó sẽ lấy input, encrypt input bằng:
```c
	for ( i = 0; i < v2; ++i )
    	encrypted_input[i] = ((input[i] >> 5) | (8 * input[i])) ^ (3 * i + 31);
```
sau đó so sánh với một giá trị hardcode trong file elf là `hardcoded_data`.

* CÁCH DECRYPT ĐỂ LẤY FLAG: đầu tiên, mình sẽ dump hardcoded_data vào một mảng unsigned char, sau đó lặp từng byte trong mảng hardcoded_data và bruteforce tất cả các giá trị input in được để điều kiện sau hợp lệ: 
```c
((input[i] >> 5) | (8 * input[i])) ^ (3 * i + 31) = hardcoded_data[i]
```

```c
booyah.cpp
#include <iostream>
#include <string.h>
#include <ctype.h>
using namespace std;

unsigned char find_input(unsigned char c) { //fuckass
    for (int a = 0; a < 256; a++) {
        unsigned char res = ((a >> 5) | (8 * a)) & 0xFF;
        if (res == c && isprint(a)) {
            return (unsigned char)a;       
	}
    }
    return 0;
}

unsigned char hardcoded_data[] = {
    0x2D, 0x38, 0xBF, 0x32, 0xF0, 0x05, 0xA8, 0xB5,
    0x04, 0x9B, 0x8C, 0x53, 0xCA, 0xE7, 0xF0, 0x67,
    0xF6, 0x59, 0xC4, 0xF1, 0x50, 0xE7, 0x7A, 0xA5,
    0x74, 0xAB, 0xDC, 0xD9, 0x50, 0xF7, 0x5A, 0xBD,
    0xB6, 0x2B, 0x9E, 0x31, 0x90, 0x37, 0x08, 0x1D,
    0x3E, 0xA9, 0x2C, 0x69, 0x0A, 0x67, 0x38, 0x9F,
    0x0E, 0x2B, 0x24, 0x93, 0x72, 0x1F, 0x40, 0x6D,
    0xD4, 0x7B, 0xEE, 0x51, 0x1A, 0x4F, 0xCA, 0x6D,
    0xEC, 0xF1, 0x24, 0xCB, 0x72, 0x05, 0xF1
};


int main() {
	string flag = "";
	for(int i=0; i<sizeof(hardcoded_data); i++) {
		unsigned char c = hardcoded_data[i] ^ (3 * i + 31);
		unsigned char c_dec = find_input(c);
		cout<<c_dec<<endl;
		flag += c_dec;
		if(c_dec == '}') {
			cout<<flag<<endl;
			break;
		}
	}
	return 0;
}
```
![](/images/hackropolefrrebabyfuscation/flag_term.webp)

### III. Đôi lời gửi đến người đọc 
---

* Mình khá non trẻ, đây là lần đầu tiên mình viết blog nên cách giải thích có thể rườm rà và khó hiểu. Cảm ơn tất cả người ae đã đọc hết cái post bủh này.