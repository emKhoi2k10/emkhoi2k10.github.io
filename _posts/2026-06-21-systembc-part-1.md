---
title: "SystemBC part 1: reversing the implant(dll, exe)"
date: 2026-06-21
categories: 
  - malware-analysis
tags:
  - eastern-europe
  - reverse-engineering
  - c2-protocol
  - SOCKS5
header:
  overlay_image: /assets/images/systembc/systembc-on-news.png
  overlay_filter: 0.7
  caption: ảnh từ <a href="https://www.operation-endgame.com/">Operation Endgame
excerpt:
---
**Disclaimer:** *Bài post này được viết dựa trên kiến thức và kinh nghiệm cá nhân nên khả năng rất cao sẽ có sai sót. Nếu thấy sai sót hoặc muốn trao đổi thêm vui lòng liên hệ qua xmpp.*

---


Hello :), sau gần 8 tháng ngủ đông vì cảm thấy quá chán với cái chall rev trong các cuộc thi ctf thì nay mình sẽ chuyển sang rev 1 con malware khá nổi tiếng đến từ khu rừng rậm Đông Âu, và đó là SystemBC :).

---


### 1. Sơ sơ về SystemBC
---
Theo như mình quan sát, SystemBC được biết đến rộng rãi là 1 proxy tool sử dụng SOCKS5, tức là nó sẽ biến mấy nạn nhận thành 1 cái proxy, sau đó thì những máy proxy này sẽ được bán cho các bên thứ ba.

Tuy nhiên, cái mà làm cho SystemBC được dùng rất nhiều và nổi bật hơn những tool proxy khác tương tự là ví chức năng Loader của nó. Tức là nó có thể load những malware khác lên máy nạn nhân. Vì vậy, nó được sử dụng rất nhiều bởi các nhóm ransomware lớn (Conti, Lockbit, The Gentlemen,...) để load các payload khác trong các giai đoạn tiếp theo của cuộc tấn công ransomware. 

Vì chức năng loader và các chức năng khác của SystemBC nên khi đọc các bài báo về nó, thì tùy vào vấn đề mà bài báo đang nói đến, SystemBC có thể được gọi là một proxy malware, loader, dropper, hoặc là một backdoor.

### 2. Recon và đi tìm sample
---
SystemBC khá cổ, được phát triển và rao bán lần đầu tiên vào năm 2018 bởi một người dùng tên psevdo trên diễn đàn nói tiếng Nga lâu đời exploit.in . 

[![Psevdo post](/assets/images/systembc/psevdo-post-exploit-in-1.jpeg)](/assets/images/systembc/psevdo-post-exploit-in-1.jpeg)
*Bài đăng rao bán SystemBC của psevdo trên diễn đàn [exploit.in](https://forum.exploit.in/).*

Khi mà mua SystemBC, thì người mua sẽ nhận được 1 bundle như sau:

[![SystemBC bundle tree](/assets/images/systembc/bundle-tree.png)](/assets/images/systembc/bundle-tree.png)
- phần client(implant): gồm 2 bản, socks.dll (~10kb) và socks.exe (~12kb)
- phần server: bản linux server.out (~10kb), bản win server.exe (~14kb) và phần panel

Thì dựa vào cái khối lượng của từng file sẽ giúp quá trình tìm sample dễ dàng và chất lượng hơn. Tránh các trường hợp rất phổ biến khi rev malware là rev phải sample đã được crypt qua các dịch vụ crypter.

Mình sẽ lên malwarebazaar để tìm sample vì nền tảng có khá nhiều sample và cho download sample.

[![Malwarebazaar search tag](/assets/images/systembc/malwarebazaar-systembc-search.png)](/assets/images/systembc/malwarebazaar-systembc-search.png)
*<center>Các sample có tag SystemBC</center>*

Lúc đầu mình dựa vào file size của phần client(implant) để tìm sample nhưng sau khi tìm một hồi thì mình tìm được nguyên 2 cái bundle luôn :)). Một cái upload năm 2023 và một cái năm 2021.

| SHA-256 | date |
|---|---|
0bb78df6c8e049c7a33d2656555e15388a59ee96bde6f221ac5494b959cd60eb | 2023 |
3cafa584500cacaedd9f29771969bab7f499b47d1912cfcc03fc58cf662ee545 | 2021 |

Thì trong bài post bài, **mình sẽ chỉ viết về phần client của bundle năm 2021**. Mình cũng đã rev bundle năm 2023 thì mình thấy có giống y hệt hoàn toàn, chỉ có chút cải tiếng là nó giao tiếp C2 bằng mạng TOR.


### 3. Rev implant (bundle năm 2021)
---


| SHA-256 | date |
|---|---|
3cafa584500cacaedd9f29771969bab7f499b47d1912cfcc03fc58cf662ee545 | 2021 |

Trong phần client, thì bản socks.dll và socks.exe hoàn toàn giống nhau, hai bản chỉ khác nhau ở chỗ lúc nó bắt đầu.

socks.dll sẽ bắt đầu ở hàm rundll

[![socks.dll export](/assets/images/systembc/socks-dll-export.png)](/assets/images/systembc/socks-dll-export.png)

[![socks.dll export](/assets/images/systembc/socks-dll-rundll.png)](/assets/images/systembc/socks-dll-rundll.png)

Hàm `rundll()` chỉ đơn giản gọi tới hàm `sub_180001667()`. Thì hàm `sub_180001667()` mới là lúc mà SystemBC bắt đầu vào việc.

[![socks.dll main](/assets/images/systembc/socks-dll-main.png)](/assets/images/systembc/socks-dll-main.png)

Còn socks.exe thì khác một chút là trước khi nó vào hàm `sub_180001667()`, socks.exe sẽ tạo một schedule task có tên là wow64. Thì task này là dùng đễ tự động chạy socks.exe khi máy tính bắt đầu.
[![wow64](/assets/images/systembc/wow64.png)](/assets/images/systembc/wow64.png)



#### 3.3 Vào việc (setup)
---

Mình đã rename các tên hàm và biến của `sub_180001667()`:

```c
void __noreturn setup()
{
    __int64 v0; // rax
    _BYTE v1[24]; // [rsp+0h] [rbp-470h] BYREF
    __int64 port; // [rsp+18h] [rbp-458h]
    _BYTE v3[8]; // [rsp+20h] [rbp-450h] BYREF
    char *host; // [rsp+28h] [rbp-448h]
    struct WSAData WSAData; // [rsp+38h] [rbp-438h] BYREF
    char v6; // [rsp+438h] [rbp-38h] BYREF

    zero(v1, &v6 - v1);
    do
    {
        Sleep(0x2710u);
        LODWORD(v0) = WSAStartup(0x202u, &WSAData);
    }
    while ( v0 );
    host = &aHost1881981478[6];
    copy_or_xor(a4174, -1, v3);
    port = string_to_int(v3);
    while ( 1 )
    {
        if ( main(host, port) )
        {
            Sleep(0x2BF20u);
        }
        else if ( host == &aHost1881981478[6] )
        {
            host = a78476446;
        }
        else
        {
            host = &aHost1881981478[6];
        }
    }
}
```

- Đầu tiên thì nó sẽ khởi tạo Winsock cho đến khi nào thành công thì thôi. Với Winsocks bản 2.2 ([0x202u](https://learn.microsoft.com/en-us/windows/win32/api/winsock/nf-winsock-wsastartup)).
- Tiếp theo là lưu host và port để chuẩn bị vào hàm `main()`.
- Sau đó, nó sẽ bắt đầu 1 vòng lặp vô tận. Thì trong vòng lặp đó, nếu nó thức hiện hàm main(logic chính của SystemBC) thành công thì nó sẽ dừng lại trong vòng 3 phút (`Sleep(0x2BF20u)`). Còn  sau khi thực hiện hàm main, nếu có trục trặc với kết nối tới máy chủ C2 thì biến `host` sẽ bằng 0 và nó sẽ đổi sang host khác (trong implant nếu lỗi là đổi từ `88.198.147.80` -> `78.47.64.46`).
- Thì SystemBC sẽ có một danh sách host và port phòng trường hợp 1 trong các host bị lỗi, danh sách đó ở trong phần `.data`

[![host list](/assets/images/systembc/host-list.png)](/assets/images/systembc/host-list.png)

#### 3.4 `main()`
---

Mình sẽ viết từng giai đoạn của SystemBC theo thứ tự thực thi.

##### 3.4.1 Allocate memory
---

[![allocate mem](/assets/images/systembc/main_allocate_mem.png)](/assets/images/systembc/main_allocate_mem.png)

- Allocate memory cho các biến `recv_data` (dùng để lưu data nhận từ C2) và `username_domain` (dùng để lưu username và domain của máy tính mà implant đang chạy, tạm gọi là máy nạn nhân).


##### 3.4.2 Thu thập thông tin cơ bản và gửi tới C2
---

[![collect basic info, send to C2](/assets/images/systembc/collect_basic_info_send_c2.png)](/assets/images/systembc/collect_basic_info_send_c2.png)

SystemBC sẽ thu thập các thông tin cơ bản của máy nạn nhân như: 
- username và domain (bằng hàm `GetUserNameExA`)
- phiên bản windows (bằng hàm `RtlGetVersion`). Hàm `RtlGetVersion` được resolve bằng PEB-walk + dynamic API resolution.

[![RtlGetVersion peb walk + dynamic API resolution](/assets/images/systembc/RtlGetVersion-peb-walk-dynamic-api-reso.png)](/assets/images/systembc/RtlGetVersion-peb-walk-dynamic-api-reso.png )

- Integrity level của process mà implant đang chạy, sau đó check xem là [integrity level](https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-system_mandatory_label_ace) > 0x2000 (Medium). Tức là nó đang check xem nó có đang được chạy với quyền admin hoặc system hay không.

- Số seri của ổ đĩa mà chứa SystemBC.

Tất cả những thông tin cơ bản này được lưu vào `v5`. Sau đó 50 bytes thông tin cơ bản trong `v5` được encrypt bằng RC4 với key là `dword_180006089` (50 bytes). Theo như mình debug vài lần trên máy ảo thì 50 bytes trong `dword_180006089` đều bằng 0 nhưng mà mình không chắc là nó có bằng 0 trên các máy khác hoặc trên các bản build khác của SystemBC hay không.

Sau khi được RC4 crypt, những thông tin cơ bản này sẽ được gửi tới C2.

##### 3.4.3 Chức năng chính của SystemBC
---

Sau khi thu thập thông tin cơ bản của máy nạn nhân, implant(client) sẽ liên tục đợi lệnh từ C2 để thực hiện 1 trong 2 chức năng chính sau: Loader, SOCKS5 proxy.

Implant và C2 sẽ giao tiếp qua giao thức TCP + SOCKS5, và tất cả gói tin sẽ được encrypt bằng RC4. Thì giờ mình sẽ giải thích 1 chút về 2 giao thức này. Mặc dù cả hai đều được gọi là giao thức nhưng mà nó khác nhau ở chỗ là TCP là giao thức ở mức độ thấp hơn. Trong [OSI model](https://en.wikipedia.org/wiki/OSI_model), thì TCP thuộc Transport layer còn SOCKS thuộc Session layer. Tức là SOCKS5 sẽ tận dụng giao thức TCP.

Việc implant biết được khi nào thực hiện 1 trong 2 chức năng chính trên phụ thuộc vào 1 hoặc 2 byte data đầu tiên(sau khi được RC4 decrypt) trong gói tin mà nó nhận từ C2:
- 0xFFFF: chức năng Loader
- 0X00: chức năng proxy

[![wireshark loader command](/assets/images/systembc/wireshark-loader-command.png)](/assets/images/systembc/wireshark-loader-command.png )
*Thì để kích hoạt chức năng loader thì C2 sẽ gửi một gói tin có 2 byte data đầu sau khi được decrypt là 0XFFFF*

[![wireshark proxy command](/assets/images/systembc/wireshark-proxy-command.png)](/assets/images/systembc/wireshark-proxy-command.png )
*Tương tự để kích hoạt chức năng proxy thì C2 sẽ gửi một gói tin có byte data đầu tiên sau khi được decrypt là 0x00*


###### Proxy
---

Sau khi implant nhận lệnh từ C2 thành công, thì implant sẽ tạo một thread mới dùng để thực hiện chức năng proxy. Giả sử C2 dùng implant làm proxy để truy cập google.com thì sẽ có quy trình như sau: 

- C2 gửi gói tin 0x00 tới implant -> implant tạo một thread mới để làm proxy cho C2 -> implant relay trafic (proxy) giữa C2 và google.com -> nếu C2 ngừng dùng proxy nữa thì implant sẽ đóng thread.

[![proxy relay](/assets/images/systembc/proxy-code-relay.png)](/assets/images/systembc/proxy-code-relay.png )
*thread chạy song song với vòng lặp chính nên có thể thành công relay trafic giữa C2 và target*

###### Loader
---

Implant nhận lệnh 0xFFFF xong implant sẽ đầu tiên xác định file extension của payload là gì. SystemBC hỗ trợ các extension sau: .ps1, .cmd, .bat, .vbs, .exe

[![check file extensions](/assets/images/systembc/check-file-extensions.png)](/assets/images/systembc/check-file-extensions.png)

Nếu file extensions của payload không nằm trong 5 extensions ở trên thì implant mặc định payload là .exe

C2 sẽ không gửi trực tiếp payload cho implant mà thay vào đó chỉ gửi url chứa payload. Implant sẽ tự download payload bằng cách gửi một lệnh GET http tới url chứa payload.

Sau khi download payload, implant sẽ lưu payload vào thư mục TEMP với filename ngẫu nhiên.
```c
LODWORD(v16) = GetTempPathA(0x200u, (LPSTR)nSize);
...
create_write_File((const CHAR *)(int)nSize, (const void *)v43[0], v42[0], 2, 0);
```
[![write payload](/assets/images/systembc/write-payload.png)](/assets/images/systembc/write-payload.png)

Cơ chế loader khá đơn giản là nó sẽ tận dụng schedule task của windows để chạy payload. Loader sẽ tạo một task trong schedule task để chạy payload. Thì việc này khá thông minh là vì lỡ payload bị AV/EDR flag thì không lần ra implant được tại payload được chạy bằng schedule task :). Mình không biết là cái trick schedule task này giờ còn dùng được không nhưng mà mình nghĩ ý đồ của tác giả ban đầu là vậy.

[![schedule task code](/assets/images/systembc/schedule-task-code.png)](/assets/images/systembc/schedule-task-code.png)

Đặc biệt, nếu file là .ps1 thì task sẽ có thêm option `-WindowStyle Hidden -ep bypass -file `

Mình đã test thử cho SystemBC load thử một file exe, thì file exe này chỉ đơn giản là tạo một window với dòng chứ "hello v1t".
[![schedule task run](/assets/images/systembc/schedule-task-run.png)](/assets/images/systembc/schedule-task-run.png)

Thì khi nhìn vào thông tin của task dùng để chạy payload, có dòng là: `delete or end the task immediately / 0 seconds after it expires` . Tức là sau khi task thực hiện xong thì task sẽ được xóa khỏi schedule. Tức có nghĩa là chỉ chạy payload đúng một lần duy nhất.

### 4. Fun facts về SystemBC và psevdo
---

- Theo như nhiều nguồn tin, thì từ khi mở thread trên exploit tới giờ thì psevdo đã thu khoảng hơn 100k usd từ việc bán các bản build của SystemBC :).

- Năm 2024 thì khá xui cho SystemBC, vì infra của nó cùng các loaders khác bị Europol và các cơ quan chức năng châu Âu tịch thu trong [Operation Endgame season 1](https://www.europol.europa.eu/media-press/newsroom/news/largest-ever-operation-against-botnets-hits-dropper-malware-ecosystem). Nhưng mà khá may là psevdo không bị bắt.

Từ năm 2024 trở đi thì psevdo đã cải tiến và tiếp tục bán SystemBC bình thường.

### 5. Cảm ơn
---

Mình chân thành cảm ơn những người đã đọc hết bài post này. Vì đây là bài post đầu tiên về malware nên cách viết và giải thích có thể gây khó hiểu. Mình sẽ cố gắng viết dễ hiểu và giải thích hay hơn ở các bài post về malware sau.

Cảm ơn anh [Rawr](https://rawr.v1t.site/) vì domain v1t.site, cảm ơn anh [Nanasi](https://nanasi.v1t.site/).
