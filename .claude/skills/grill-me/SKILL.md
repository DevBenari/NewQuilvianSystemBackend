---
name: grill-me
description: Wawancarai pemilik kebutuhan secara kritis sebelum desain atau implementasi modul Quilvian. Gunakan ketika user ingin memulai modul/fitur baru, menutup scope dan aturan bisnis, mendefinisikan status serta exception, menentukan kewenangan frontend, atau menyelesaikan pertanyaan setelah capability audit. Jangan gunakan untuk audit source code mendalam atau langsung menulis kode.
---

# Grill Me

Bangun decision log yang dapat diuji. Pisahkan keputusan manusia dari fakta source code dan
jangan mengubah aplikasi.

## Effort dan model minimum

| Field | Nilai |
| --- | --- |
| Minimum effort | `medium` |
| Model Claude minimum | Claude Sonnet 5 |
| Model Claude disarankan | Claude Opus 5 |
| Model GPT setara | GPT-5, reasoning `medium` |
| Alasan | Wawancara memerlukan penalaran atas invariant dan konsekuensi keputusan, bukan sekadar menyusun daftar pertanyaan |

Jika sesi berjalan di bawah batas ini, beritahu pengguna sebelum mulai dan minta konfirmasi.
Model kecil cenderung menerima jawaban kabur sebagai keputusan final.

## Aturan output dokumentasi

Setiap dokumen yang ditulis skill ini wajib mengikuti
[aturan output dokumentasi](../../rules/rule-output/aturan-output-dokumentasi.md): Bahasa Indonesia,
bahasa yang mudah dipahami orang umum, penjelasan detail beserta contoh, bisnis proses yang
jelas, dan endpoint bergaya Swagger. Baca aturan tersebut sebelum menulis decision log.

## Tentukan mode

Pilih salah satu mode dari keadaan blueprint:

- **Scope pass**: belum ada `00-interview-decisions.md`; gali tujuan, batas, aktor, dan
  risiko yang diperlukan sebelum audit existing.
- **Closure pass**: capability map sudah ada; fokus pada conflict, unknown, dan keputusan
  yang memblokir desain.
- **Amendment pass**: blueprint sudah disetujui; buat revisi keputusan, jangan menimpa
  histori approval.

Jika mode tidak jelas, simpulkan dari artefak yang tersedia dan jelaskan asumsi singkat.

## Temukan workspace dan blueprint

1. Temukan Git root `NewQuilvianSystemBackend` dan `QuilvianSystemFrontendDev`; jangan menganggap current
   directory selalu salah satunya.
2. Normalisasi nama modul menjadi kebab-case.
3. Gunakan canonical output:
   `NewQuilvianSystemBackend/docs/module-blueprints/<module-name>/00-interview-decisions.md`.
4. Baca `blueprint-manifest.md` jika tersedia. Jangan lanjut memakai revision yang
   `superseded`.
5. Untuk format, baca [interview template](references/interview-template.md).

## Kunci batas scope sebelum bertanya

Wawancara wajib tetap berada di dalam modul yang diberikan pengguna. Sebelum pertanyaan
pertama:

1. Tuliskan nama modul dan satu kalimat batas scope-nya.
2. Tuliskan daftar **Di dalam scope** dan **Di luar scope** berdasarkan permintaan pengguna,
   bukan berdasarkan kelengkapan sistem menurut agent.
3. Konfirmasikan kedua daftar itu ke pengguna, lalu simpan di decision log.

Selama wawancara berjalan:

- Setiap pertanyaan harus dapat dijelaskan keterkaitannya dengan modul tersebut. Jika tidak
  bisa, jangan ditanyakan.
- Modul tetangga hanya boleh disinggung sebatas titik sentuhnya: data yang dipakai bersama,
  kontrak antar modul, atau urutan proses. Jangan menggali aturan internal modul tetangga.
- Jika muncul kebutuhan penting yang berada di luar scope, jangan mengejarnya dalam sesi ini.
  Catat pada bagian `Di luar scope — untuk modul lain` beserta alasan singkat, lalu lanjutkan
  ke pertanyaan yang masih di dalam scope.
- Jika pengguna sendiri yang memperluas scope, minta konfirmasi eksplisit bahwa batas scope
  diperbarui, lalu perbarui kedua daftar tadi sebelum melanjutkan.

Contoh: untuk modul `pengkajian-rawat-inap`, pertanyaan tentang isi formulir pengkajian dan
siapa yang boleh mengoreksinya berada **di dalam** scope. Pertanyaan tentang aturan tarif
kamar atau alur klaim asuransi berada **di luar** scope, walaupun sama-sama menyangkut pasien
rawat inap. Cukup tanyakan titik sentuhnya, misalnya "apakah pengkajian ini harus terkunci ke
admission yang sudah ada?".

## Jalankan wawancara

1. Catat hal yang sudah diberikan sebagai **Fact**, **Decision**, **Assumption**,
   **Conflict**, atau **Open Question**.
2. Tanyakan hanya pertanyaan yang mengubah scope, ownership, invariant, legal state,
   permission, privacy, integration, failure behavior, atau acceptance criteria.
3. Jangan menanyakan detail yang dapat ditemukan aman dari source code; teruskan ke
   `/trace-existing-capabilities`.
4. Uji istilah kabur seperti aktif, selesai, batal, terintegrasi, valid, dan darurat sampai
   memiliki kondisi yang dapat diuji.
5. Cakup jalur normal, pembatalan, koreksi, reopening, duplicate request, downtime, data
   terlambat, dan partial failure bila relevan.
6. Tetapkan owner keputusan serta siapa yang boleh approve.
7. Untuk frontend, tentukan hierarchy berikut:
   security/privacy/invariant -> approved product/UI brief -> project convention ->
   developer discretion.
8. Tandai keputusan UI yang memang didelegasikan sebagai `DEV_DISCRETION`. Jangan
   menetapkan menu, route, tab/modal/drawer, warna, atau layout berdasarkan selera agent.

## Bentuk wajib setiap pertanyaan

Default interaksi adalah satu pertanyaan kritis per giliran. **Setiap** pertanyaan wajib
memuat tiga hal berikut tanpa kecuali:

1. **Pilihan** — 2 sampai 3 opsi yang saling eksklusif, ditulis dengan bahasa yang dipahami
   orang umum, bukan istilah teknis mentah.
2. **Rekomendasi** — tepat satu opsi ditandai **(Direkomendasikan)**, disertai alasan singkat
   berbasis evidence atau trade-off. Sebutkan juga konsekuensi bila opsi lain dipilih.
3. **Other** — opsi `Other — tuliskan pilihan atau batasan lain` selalu tersedia. Jika UI
   interaktif sudah menambahkannya otomatis, jangan menduplikasi.

Contoh bentuk yang benar:

```text
Ketika perawat salah mengisi pengkajian dan baru sadar keesokan harinya, apa yang boleh
dilakukan?

A. Koreksi tetap diizinkan, tetapi versi lama disimpan dan ditandai sebagai revisi
   (Direkomendasikan) — riwayat klinis tetap utuh untuk audit, dan perawat tidak perlu
   membuat pengkajian baru. Konsekuensinya perlu tampilan riwayat versi.
B. Koreksi hanya boleh oleh kepala ruangan — lebih ketat, tetapi berpotensi menunda
   perbaikan data saat kepala ruangan tidak bertugas.
C. Tidak boleh dikoreksi, harus buat pengkajian baru — paling sederhana, tetapi data lama
   yang salah tetap terbaca sebagai fakta klinis.
D. Other — tuliskan pilihan atau batasan lain.
```

Aturan tambahan:

- Jika pilihan yang aman belum dapat dirumuskan, tetap berikan pilihan. Jadikan rekomendasi
  sebagai langkah fail-closed, misalnya "tahan dulu sampai owner klinis memutuskan", dan tetap
  sediakan `Other`.
- Rekomendasi bukan keputusan dan bukan approval. Owner berwenang tetap harus memilih.
- Jangan menawarkan opsi yang melanggar keamanan, privasi, atau invariant yang sudah
  disepakati.
- Seluruh pilihan harus berada di dalam batas scope modul. Jangan memakai opsi untuk
  menyelundupkan perluasan scope.

Jika user meminta semua pertanyaan sekaligus, tulis seluruh daftar ke decision log dan
tampilkan ringkasan pendek; setiap pertanyaan tetap harus memiliki pilihan, rekomendasi, dan
`Other`.

## Kelola approval

- Gunakan `decision_id` stabil.
- Simpan status `draft`, `approved`, `rejected`, atau `superseded`.
- Simpan owner, source/evidence, allowed range, `approved_by`, dan `approved_at`.
- Jangan menganggap jawaban informal sebagai approval jika owner yang berwenang belum jelas.
- Jangan memulai desain final bila invariant klinis/bisnis kritis masih terbuka.
- Blokir hanya bagian yang bergantung pada keputusan terbuka jika bagian lain aman
  dilanjutkan.

## Selesaikan pass

Perbarui decision log setiap kali user menjawab. Akhiri dengan:

- keputusan baru/berubah;
- open question dan owner;
- blocker desain;
- acceptance criteria yang sudah dapat diuji;
- item yang sengaja ditinggalkan karena berada di luar scope;
- langkah berikutnya sesuai tabel di bawah.

Jangan membuat PRD, arsitektur final, roadmap, migration, endpoint, atau UI pada skill ini.

## Tawarkan skill berikutnya

Setelah pass ini tuntas, **selalu** tawarkan langkah berikutnya secara eksplisit kepada
pengguna, lengkap dengan alasan singkat mengapa langkah itu yang tepat:

| Kondisi setelah pass ini | Skill yang ditawarkan |
| --- | --- |
| Scope pass selesai, tujuan dan batas audit sudah jelas | `/trace-existing-capabilities` |
| Closure pass selesai, conflict dan unknown sudah tertutup, approval cukup | `/design-business-module` |
| Masih ada keputusan kritis yang terbuka | `/grill-me` lanjutan pada pass yang sama |
| Source berubah sejak audit terakhir | `/trace-existing-capabilities` mode impact scan |

Tawarkan, jangan jalankan sendiri. Tunggu persetujuan pengguna sebelum berpindah skill.
Jika masih ada blocker, sebutkan blocker itu lebih dulu dan jangan menawarkan langkah maju
seolah pekerjaan sudah tuntas.
