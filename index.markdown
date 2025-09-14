---
# Feel free to add content and custom Front Matter to this file.
# To modify the layout, see https://jekyllrb.com/docs/themes/#overriding-theme-defaults

layout: home
---

<p align="center">
  <img src="/images/dream-space.webp" width="150"/>
</p>

<div class="center">
  <div class="typing" id="typewriter"></div>
</div>

<style>
  .center {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 50px; /* adjust as needed */
  }

  .typing {
    font-family: monospace;
    font-size: 20px;
    white-space: nowrap;
    overflow: hidden;
    text-align: center;

    /* Keep space reserved */
    display: inline-block;
    min-width: 20ch;  /* reserve space for ~20 characters */
  }
</style>

<script>
  const texts = ["Nguyễn Đức Nguyên Khôi", "emKhoi2k10", "Safear"];
  let count = 0;
  let index = 0;
  let currentText = '';
  let speed = 120;

  function type() {
    if (count === texts.length) count = 0;
    currentText = texts[count];

    if (index < currentText.length) {
      document.getElementById("typewriter").textContent =
        currentText.slice(0, ++index);
      setTimeout(type, speed);
    } else {
      setTimeout(erase, 1000);
    }
  }

  function erase() {
    if (index > 0) {
      document.getElementById("typewriter").textContent =
        currentText.slice(0, --index);
      setTimeout(erase, speed / 2);
    } else {
      count++;
      setTimeout(type, speed);
    }
  }

  type();
</script>