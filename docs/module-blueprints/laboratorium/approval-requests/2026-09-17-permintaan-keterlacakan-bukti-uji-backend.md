# Permintaan Keputusan — Keterlacakan Bukti Uji Backend

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-011` |
| `tanggal` | 2026-09-17 |
| `pengaju` | Yoga Aji Pratama — Product/Domain Owner Laboratorium (`yogaaji452@gmail.com`) |
| `rujukan` | `LAB-RDY-C04`; `06-readiness-assessment.md` revision `3` kondisi `C-04`; commit `fcabdff9` |
| `status` | `menunggu jawaban` |
| `ditujukan kepada` | Pemilik repository `NewQuilvianSystemBackend` |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |

> **Ini bukan keluhan atas pekerjaan siapa pun, dan bukan permintaan membatalkan sesuatu.**
> Commit yang menjadi pokok bahasan punya alasan yang masuk akal, dan alasannya tidak diketahui
> Laboratorium. Yang diminta adalah **satu keputusan tata kelola** yang akibatnya melintasi
> seluruh modul, bukan hanya modul ini.

---

## 1. Satu paragraf untuk yang tidak punya waktu

Seluruh berkas uji backend **dikeluarkan dari repository** pada 2026-09-11. Akibatnya, bukti uji
yang menjadi dasar puluhan baris berstatus `SELESAI` pada dokumen keterlacakan **tidak dapat
dijalankan ulang oleh siapa pun hari ini** — termasuk oleh CI, dan termasuk oleh orang yang
menulis buktinya sendiri.

**Ini bukan tuduhan bahwa ujinya tidak pernah lulus.** Ia memang lulus ketika dijalankan, dan
laporannya mencatat angkanya. Yang hilang adalah **kemampuan memeriksa ulang**.

Yang diminta: **satu keputusan** tentang bagaimana bukti uji backend tetap dapat diperiksa.

---

## 2. Yang terjadi, dibaca dari repository

Dibaca langsung pada 2026-09-17.

| Hal | Temuan |
|---|---|
| Commit | **`fcabdff9`** — *"chore: exclude test projects from repository"* |
| Tanggal | **2026-09-11** |
| Penulis | Sukma Giri Pratama |
| Isinya | **20 berkas, 9.601 baris dihapus** |
| `.gitignore` baris 378 | **`/Tests/`** |
| Direktori `Tests/` pada working tree | **Tidak ada** |
| Berkas uji terlacak git hari ini | **Nol** |

Di antara yang terhapus terdapat berkas seperti `NumberSeriesAllocatorTests.cs`,
`NumberSeriesRoleAccessContractTests.cs`, dan `ProviderRequestServiceTests.cs` — uji milik modul
lain, bukan Laboratorium. **Akibatnya karena itu melintasi seluruh repository.**

---

## 3. Kenapa ini penting bagi Laboratorium

Dokumen keterlacakan modul Laboratorium memuat **puluhan baris berstatus `SELESAI`**, dan
sebagian besarnya bersandar pada laporan task yang menuliskan hasil uji sebagai bukti — misalnya
*"20 pemeriksaan dijalankan sungguhan, seluruhnya `PASS`"*.

Selama berkas ujinya tidak ada di repository:

| Yang hilang | Akibat nyatanya |
|---|---|
| Pemeriksaan ulang oleh orang lain | Bukti hanya dapat dipercaya, tidak dapat diverifikasi |
| Perlindungan dari regresi | Perubahan yang memecahkan aturan lama **tidak akan tertangkap** |
| CI | Tidak ada yang dijalankan CI untuk backend |
| Pemeriksaan sebelum rilis | Tidak dapat dijalankan sebagai gerbang |

**Yang paling menentukan adalah yang kedua.** Modul ini sudah mengalami empat kali kelas
kesalahan yang sama — sesuatu yang berdiri tanpa pasangannya dan **tidak menimbulkan galat apa
pun** sampai seseorang membutuhkannya. Uji regresi adalah penjaga yang seharusnya menangkapnya,
dan penjaga itu sedang tidak ada di backend.

> **Perbandingan yang pantas dicatat.** Repository frontend menyimpan ujinya di dalam repository,
> dan pada 2026-09-17 ia menjalankan **1.043 uji unit** beserta **18 pemeriksaan layar** — dan
> ketika `FE-LAB-19` mengubah satu aturan, uji lama yang mengunci perilaku cacat **langsung
> gagal** dan memaksa pembahasan. Backend hari ini tidak punya mekanisme itu.

---

## 4. Yang diminta

**Satu keputusan.** Laboratorium tidak menuntut bentuk tertentu — tiga kemungkinan di bawah
ditulis hanya supaya jawabannya konkret.

| # | Kemungkinan | Konsekuensinya |
|---:|---|---|
| A | Berkas uji dikembalikan ke repository | Bukti dapat diperiksa dan CI dapat menjalankannya. Perlu memulihkan 9.601 baris dan merawatnya |
| B | Uji tinggal di repository terpisah | Keterlacakan tetap ada, tetapi laporan task perlu menyebut repository dan commitnya agar dapat ditelusuri |
| C | Backend memang tidak memakai uji otomatis | Sah sebagai keputusan, asalkan **dinyatakan**. Laporan task berhenti menuliskan hasil uji sebagai bukti, dan bentuk buktinya diganti — misalnya pemeriksaan terhadap database sungguhan, seperti yang sudah dipakai `BE-LAB-30` sampai `BE-LAB-38` |

**Pilihan C bukan pilihan buruk**, dan itu pantas ditegaskan. Seluruh task backend Laboratorium
sejak `BE-LAB-30` membuktikan hasilnya dengan **pemeriksaan sungguhan terhadap
`QuilvianNewDevYoga`** — bukan dengan uji otomatis — dan bukti itu tetap kuat karena angkanya
dibaca dari database. Yang tidak boleh terjadi hanyalah keadaan sekarang: laporan menyebut uji
sebagai bukti, sementara ujinya tidak ada di mana pun yang dapat dijangkau.

### 4.1 Satu hal yang perlu diketahui bila pilihan A diambil

Berkas yang terhapus adalah milik **beberapa modul**, bukan hanya Laboratorium. Memulihkannya
menyentuh pekerjaan tim lain, dan Laboratorium **tidak berwenang** memutuskannya sendiri — itu
sebabnya permintaan ini ditujukan kepada pemilik repository, bukan dikerjakan langsung.

---

## 5. Yang **tidak** diminta

| Hal | Keadaan |
|---|---|
| Membatalkan commit `fcabdff9` | **Tidak diminta.** Alasannya tidak diketahui Laboratorium, dan boleh jadi benar |
| Menilai pekerjaan siapa pun | Tidak. Ini perkara tata kelola, bukan perkara orang |
| Menambah kewajiban uji pada task yang sedang berjalan | Tidak. Bila pilihan C diambil, bentuk bukti yang berlaku sekarang sudah memadai |
| Perubahan pada repository frontend | Tidak. Ia menyimpan ujinya dan tidak terdampak |

---

## 6. Akibat bila belum dijawab

Ditulis apa adanya, bukan untuk mendesak.

`LAB-RDY-C04` tetap terbuka pada `06-readiness-assessment.md`, dan modul Laboratorium tetap
berstatus **`READY_WITH_CONDITIONS`** alih-alih siap penuh. Ia **tidak memblokir satu pun task**
— seluruh pekerjaan backend dan frontend modul ini sudah selesai kecuali yang menunggu pihak
lain.

Yang tertahan adalah **pernyataan siap pakai**, bukan pekerjaannya.

---

## 7. Cara menjawab

Cukup balas dokumen ini dengan menyebut A, B, atau C — atau bentuk lain yang dinilai lebih tepat.

Jawaban akan dicatat sebagai penutupan `LAB-RDY-C04` pada `blueprint-manifest.md` dan
`06-readiness-assessment.md`, dan bila jawabannya C, bentuk bukti pada template laporan task
backend akan disesuaikan.
