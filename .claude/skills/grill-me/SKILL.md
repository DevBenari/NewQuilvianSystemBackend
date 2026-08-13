---
name: grill-me
description: Wawancarai pemilik kebutuhan secara kritis sebelum desain atau implementasi modul Quilvian. Gunakan ketika user ingin memulai modul/fitur baru, menutup scope dan aturan bisnis, mendefinisikan status serta exception, menentukan kewenangan frontend, atau menyelesaikan pertanyaan setelah capability audit. Jangan gunakan untuk audit source code mendalam atau langsung menulis kode.
---

# Grill Me

Bangun decision log yang dapat diuji. Pisahkan keputusan manusia dari fakta source code dan
jangan mengubah aplikasi.

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

1. Temukan Git root `QuilvianBackend` dan `QuilvianFrontEnd`; jangan menganggap current
   directory selalu salah satunya.
2. Normalisasi nama modul menjadi kebab-case.
3. Gunakan canonical output:
   `QuilvianBackend/docs/module-blueprints/<module-name>/00-interview-decisions.md`.
4. Baca `blueprint-manifest.md` jika tersedia. Jangan lanjut memakai revision yang
   `superseded`.
5. Untuk format, baca [interview template](references/interview-template.md).

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

Default interaksi adalah satu pertanyaan kritis per giliran. Untuk setiap pertanyaan,
selalu berikan 2--3 pilihan yang saling eksklusif bila pilihan tersebut dapat ditentukan
dengan aman, tandai tepat satu sebagai **(Direkomendasikan)**, dan jelaskan singkat dasar
evidence atau trade-off rekomendasinya. Selalu sediakan `Other — tuliskan pilihan atau
constraint lain`; jika UI interaktif sudah menambahkan `Other` otomatis, jangan menduplikasi
opsi tersebut. Jangan menyajikan rekomendasi sebagai keputusan atau approval; owner berwenang
tetap harus memilih dan menyetujuinya. Jika pilihan aman belum dapat dirumuskan, jadikan
rekomendasi sebagai langkah fail-closed untuk meminta keputusan/evidence owner, tetap dengan
opsi `Other`.

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
- langkah berikutnya: `/trace-existing-capabilities` untuk scope pass atau
  `/design-business-module` setelah closure dan approval cukup.

Jangan membuat PRD, arsitektur final, roadmap, migration, endpoint, atau UI pada skill ini.
