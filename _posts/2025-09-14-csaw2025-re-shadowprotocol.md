---
layout: post
title:  "CSAW 2025 | re | Shadow Protocol"
date:   2025-09-14 09:23:50 +0000
categories: Reverse
---

### I. Introductions
---

[CSAW 2025](https://ctf.csaw.io/)

* chall: [rev/shadow_protocol](https://ctf.csaw.io/challenges#Shadow%20Protocol-38)
* author: jackhax
* played for team V1t

### II. Writeups
---

#### 1. first look
* Đầu tiên là chall này là chall server side:
 `nc chals.ctf.csaw.io 21002`
* Và chúng ta được cung cấp elf được chạy trên `chals.ctf.csaw.io:21002` :

```
$ file shadow_protocol
shadow_protocol: ELF 64-bit LSB pie executable, x86-64, version 1 (SYSV), dynamically linked, interpreter /lib64/ld-linux-x86-64.so.2, BuildID[sha1]=dcda793c6983326619cae28e35129285cbfe0f40, for GNU/Linux 3.2.0, not stripped
```

* Kết nối thử đến `chals.ctf.csaw.io:21002` :

```
$ nc chals.ctf.csaw.io 21002
        ✦         .       *        .      ✦
   ✦        .     SHADOW PROTOCOL INITIATED     .       ✦
        *        ✦       .       ✶        .

[SPACE] A cosmic signal has been scrambled using the Shadow Protocol at time: 1915540140.
[SPACE] Encrypted message:
54E20E3D217E274445A21979303F705150CE5A22766E71484ECE1F38723D715C07FD5A15733F1E5C04E3587E7364704668FF5F7D1D39754C4EEC

[SPACE] Transmission complete.
 

Press ENTER to exit...
```
* Chạy thử trên `local`:

```
$ ./shadow_protocol 
        ✦         .       *        .      ✦
   ✦        .     SHADOW PROTOCOL INITIATED     .       ✦
        *        ✦       .       ✶        .

[SPACE] A cosmic signal has been scrambled using the Shadow Protocol at time: 1757860260.
[SPACE] Encrypted message:
53A75A37482AF59F23AB7D0C072B9EC04F802813477DAF936D

[SPACE] Transmission complete.
```
* Nhìn lần đầu mình đoán là `shadow_protocol` sẽ tạo một giá trị random dựa trên thời gian hiện tại khi chạy `shadow_protocol` và dùng giá trị random đó để encrypt cái message(khả năng cao là flag). Nên nhiệm vụ của mình sẽ là decrypt cái message đó.

#### 2. Re shadow_protocol  
* load elf lên IDA
```c
int __fastcall main(int argc, const char **argv, const char **envp)
{
  __int64 v3; // rbx
  int v5; // [rsp+4h] [rbp-ECh] BYREF
  __int64 v6; // [rsp+8h] [rbp-E8h] BYREF
  size_t i; // [rsp+10h] [rbp-E0h]
  unsigned int seed[2]; // [rsp+18h] [rbp-D8h]
  __int64 v9; // [rsp+20h] [rbp-D0h]
  __int64 v10; // [rsp+28h] [rbp-C8h]
  unsigned __int64 v11; // [rsp+30h] [rbp-C0h]
  FILE *stream; // [rsp+38h] [rbp-B8h]
  size_t v13; // [rsp+40h] [rbp-B0h]
  size_t v14; // [rsp+48h] [rbp-A8h]
  char s[136]; // [rsp+50h] [rbp-A0h] BYREF
  unsigned __int64 v16; // [rsp+D8h] [rbp-18h]

  v16 = __readfsqword(0x28u);
  *(_QWORD *)seed = 60 * (time(0) / 60);
  srand(seed[0]);
  v3 = (__int64)rand() << 32;
  v9 = v3 | rand();
  v10 = build_bittree(v9, 0, 21);
  v6 = 0;
  v5 = 0;
  shadow_tree_mix(v10, &v6, &v5);
  free_bittree(v10);
  v11 = shadow_protocol(v6);
  debug(v11);
  stream = fopen("flag.txt", "r");
  if ( stream )
  {
    if ( !fgets(s, 128, stream) )
    {
      puts("Could not read flag, contact mission control.");
      fclose(stream);
      return 1;
    }
    fclose(stream);
    v13 = strlen(s);
    if ( v13 && s[v13 - 1] == 10 )
      s[--v13] = 0;
  }
  else
  {
    strcpy(s, "CSAW{f4k3_fl4g_4_t3st1ng}");
  }
  v14 = strlen(s);
  puts(asc_2190);
  puts(asc_21C0);
  puts(asc_2200);
  printf("[SPACE] A cosmic signal has been scrambled using the Shadow Protocol at time: %lld.\n", *(_QWORD *)seed);
  puts("[SPACE] Encrypted message:");
  for ( i = 0; i < v14; ++i )
    printf("%02X", (unsigned __int8)s[i] ^ (unsigned __int8)(v11 >> (8 * ((unsigned __int8)i & 7u))));
  putchar(10);
  puts("\n[SPACE] Transmission complete.");
  getchar();
  return 0;
}
```
* Ở đây khi đọc sơ qua thì mình thấy đoạn đầu của main dùng để config một vài biến.
* Mình sẽ lượt qua hết và chỉ chú trọng tới 2 dòng dùng để encrypt message  là:
```c
for ( i = 0; i < v14; ++i )
    printf("%02X", (unsigned __int8)s[i] ^ (unsigned __int8)(v11 >> (8 * ((unsigned __int8)i & 7u))));
```
* Có thể thấy việc encrypt msg bằng các phép bitwise phụ thuộc vào một biến đó là v11. Tức là nếu biết v11 thì sẽ decrypt msg nên mình sẽ vẽ một sơ đồ như sau:
```
msg <-- v11 <-- ???
```
* Từ đó mình truy ngược lên đoạn code ở đầu main:
```c
  v16 = __readfsqword(0x28u);
  *(_QWORD *)seed = 60 * (time(0) / 60);
  srand(seed[0]);
  v3 = (__int64)rand() << 32;
  v9 = v3 | rand();
  v10 = build_bittree(v9, 0, 21);
  v6 = 0;
  v5 = 0;
  shadow_tree_mix(v10, &v6, &v5);
  free_bittree(v10);
  v11 = shadow_protocol(v6);
```
```c
_DWORD *__fastcall shadow_tree_mix(__int64 a1, unsigned __int64 *a2, _DWORD *a3)
{
  _DWORD *result; // rax

  if ( a1 )
  {
    shadow_tree_mix(*(_QWORD *)(a1 + 8), a2, a3);
    shadow_tree_mix(*(_QWORD *)(a1 + 16), a2, a3);
    result = *(_DWORD **)(a1 + 8);
    if ( !result )
    {
      result = *(_DWORD **)(a1 + 16);
      if ( !result )
      {
        *a2 = *(_BYTE *)a1 & 7 | (8 * *a2);
        result = a3;
        ++*a3;
      }
    }
  }
  return result;
}
```
* Ở đây mình sẽ xem hàm `shadow_tree_mix()` là một black box và cũng không nhất thiết phải hiểu cụ thể nó làm gì. Mình chỉ cần biết là hàm đó có thay đổi giá trị của a2(tức là v6 trong main) dựa trên a1(v10 trong main) và a3(v5 trong main):

	`*a2 = *(_BYTE *)a1 & 7 | (8 * *a2);`

* Vì v5 được init trong main bằng 0 nên mình bỏ qua và chỉ quan tâm đến v10. Vậy ta có sơ đồ như sau:
```
msg <-- v11 <-- v10 <-- ???
```
* Tiếp tục.
```c
  *(_QWORD *)seed = 60 * (time(0) / 60);
  srand(seed[0]);
  v3 = (__int64)rand() << 32;
  v9 = v3 | rand();
  v10 = build_bittree(v9, 0, 21);
```
* Ta thấy v10 phụ thuộc vào v9 mà không cần phải rev hàm `build_bittree()` làm gì. Mà v9 là phụ thuộc vào v3 và rand(). Tiếp là v3 và rand lại phụ thuộc vào `srand(seed[0])`. Mà srand lại phụ thuộc vào `seed`.
* Và đoán xem, khi chạy `shadow_protocol` thì nó đã cung cấp seed cho chúng ta:
```c
printf("[SPACE] A cosmic signal has been scrambled using the Shadow Protocol at time: %lld.\n", *(_QWORD *)seed);
```
```
[SPACE] A cosmic signal has been scrambled using the Shadow Protocol at time: 1757860260.
```
* Vậy ta có sơ đồ như sau:
```
msg <-- v11 <-- v10 <-- v9 <-- v3,rand() <-- srand() <-- seed
```
* Vậy chỉ cần có seed là ta decrypt được msg mà seed thì `shadow_protocol` đã cung cấp cho chúng ta. bắn.

> [Post-Chorus: Edward Maya]
>
> I can fix all those lies
>
> Oh, baby, babe, I run, love, I'm running to you

* Công đoạn đi viết script và decrypt
* Mình sẽ connect tới server lại để lấy seed và encrypted msg để bắt đầu decrypt.

```
$ nc chals.ctf.csaw.io 21002
        ✦         .       *        .      ✦
   ✦        .     SHADOW PROTOCOL INITIATED     .       ✦
        *        ✦       .       ✶        .

[SPACE] A cosmic signal has been scrambled using the Shadow Protocol at time: 1915543740.
[SPACE] Encrypted message:
F113AE1DAA288786E053B959BB69D093F53FFA02FD38D18AEB3FBF18F96BD19EA20CFA35F869BE9EA112F85EF832D084CD0EFF5D966FD58EEB1D

[SPACE] Transmission complete.
```
* Với seed là `1915543740` thì mình sẽ quăng lên IDA và debug để cho nó tự chạy như sơ đồ trên và mình sẽ lấy giá trị của v11 để bắt đầu decrypt.
![](/images/csaw2025reshadowprotocol/seed.webp)
![](/images/csaw2025reshadowprotocol/v11.webp)
* Tới đây thì quá dễ, trong `shadow_protocol`:
```c
for ( i = 0; i < v14; ++i )
    printf("%02X", (unsigned __int8)s[i] ^ (unsigned __int8)(v11 >> (8 * ((unsigned __int8)i & 7u))));
```
* Mình có v11 rồi, msg được xor với `(unsigned __int8)(v11 >> (8 * ((unsigned __int8)i & 7u)))` để encrypt thì mình sẽ xor lại lần nữa để decrypt thôi.

```c
#include <iostream>
#include <vector>
#include <string>
#include <cstdint>

int main() {
    std::string hex = "F113AE1DAA288786E053B959BB69D093F53FFA02FD38D18AEB3FBF18F96BD19EA20CFA35F869BE9EA112F85EF832D084CD0EFF5D966FD58EEB1D";
    uint64_t v11 = 0xFDE15CC96ACF6092ULL;
    size_t flag_len = hex.size() / 2;

    std::vector<uint8_t> bytes(flag_len);
    for (size_t i = 0; i < flag_len; ++i)
        bytes[i] = std::stoi(hex.substr(i*2, 2), nullptr, 16);

    uint8_t key[8];
    for (int i = 0; i < 8; ++i)
        key[i] = (v11 >> (8 * i)) & 0xFF;

    std::vector<uint8_t> flag(flag_len);
    for (size_t i = 0; i < flag_len; ++i)
        flag[i] = bytes[i] ^ key[i & 7];

    for (auto b : flag)
        std::cout << b;
    std::cout << std::endl;
}
```
* xem hết flag thì tự chạy nhé :).
```
$ ./solve 
csawctf{r3v3r51ng_5h4d0wy_??????????????????????????_34sy}
```

### III. Đôi lời gửi đến người đọc 
---
* Cảm ơn tất cả người ae đã đọc hết cái post bủh này. Sau chall này thì mình thấy đôi lúc khi rev thì không nhất thiết phải hiểu tất cả hàm, code làm gì mà nên tập trung vào core hơn :). Bye Bye.