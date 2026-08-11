---
description: Kerjakan kebutuhan master data di backend sesuai kontrak baku, lalu serahkan perintah git ke user
argument-hint: <ID tugas | entitas + kebutuhan>
---

# Set Master Data Backend

Tugas yang diminta: **$ARGUMENTS**

Kalau bagian di atas menyebut ID tugas (`T1`, `GAP-3`), **baca register-nya dulu** di
`docs/hamzah/task/`. Di situ sudah ada Definition of Done, pola acuan, dan daftar objek
terdampak — jangan menyusun ulang dari nol.

## Yang harus dilakukan

Baca **`.claude/skills/master-data-set/SKILL.md`** dan ikuti seluruh langkahnya. File itu
sumber kebenaran prosedurnya — jangan menyusun ulang alur sendiri.

Aturan yang mengikat:

| Dokumen | Isi |
|---|---|
| `.claude/rules/master-data-contract.md` | Kontrak yang harus dipenuhi — **baca sebelum menulis kode** |
| `.claude/rules/git-read-only.md` | Batas pekerjaan + checklist verifikasi |

## Empat batas yang paling sering dilanggar

1. **Ikuti pola yang sudah ada di repo.** Setiap kebutuhan di kontrak sudah punya
   implementasi acuan. Menulis versi baru membuat satu API punya dua semantik untuk hal
   yang sama.

2. **Seluruh penambahan bersifat aditif.** `GET` tanpa parameter harus menghasilkan output
   yang **sama persis** seperti sebelumnya. Ini butir yang paling sering terlewat.

3. **Build pakai `--no-incremental`.** Build polos melewati kompilasi kalau tidak ada file
   yang berubah, lalu melaporkan `0 Warning` yang menyesatkan. Baseline repo ini saat
   rebuild penuh: **125 warning bawaan, 0 error**.

4. **Jangan menjalankan `git add`, `commit`, atau `push`.** Berhenti setelah file tertulis
   dan build lolos, lalu serahkan: daftar file, hasil verifikasi, dan blok perintah git
   dengan pesan commit yang **sudah jadi** — user tinggal menyalin.

## Yang wajib ikut ditulis

- Halaman laporan di `docs/hamzah/report/<topik>.md` — **sebelum** diserahkan
- Register di `docs/hamzah/task/<topik>.md` diperbarui kalau tugas ini berasal dari sana,
  dan disebutkan di serah terima agar ikut di-stage pada commit yang sama

Kalau di tengah pekerjaan ternyata butuh **migration** atau menyentuh `appsettings*` —
berhenti dan tanya user dulu. Keduanya berdampak di luar repo.
