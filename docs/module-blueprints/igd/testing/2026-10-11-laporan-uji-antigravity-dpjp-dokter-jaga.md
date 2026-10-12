# Laporan Uji DPJP Dokter Jaga IGD — BE-IGD-056 + FE-IGD-037 (AT-IGD-214…220)

Tanggal pelaksanaan: 2026-10-11  
Pelaksana: Antigravity (Pair Programming AI Assistant)  
Panduan acuan: Spesifikasi Pengujian DPJP Dokter Jaga IGD (BE-IGD-056 + FE-IGD-037, skenario AT-IGD-214…220)  
Folder bukti mentah: `QuilvianSystemFrontendDev/test-with-agy/igd/uji-dpjp-dokter-jaga-2026-10-11/`

---

## 1. Metadata Lingkungan Pengujian

| Komponen | Nilai / Kondisi | Catatan |
| --- | --- | --- |
| Tanggal Pengujian | 2026-10-11 | Pelaksanaan uji otomatisasi Playwright & fetch API |
| Jam Mulai Uji | 16:24 WIB | Inisialisasi lingkungan & pengujian akses pintu UI |
| Jam Selesai Uji | 16:42 WIB | Penyelesaian skenario terakhir (AT-IGD-219) |
| Runtime Frontend | Standalone Production Build (`node .next/standalone/server.js`) | Dilayani pada port 3000; terbukti `nextjs-portal === null` dan tidak ada lencana "N" pada seluruh tangkapan layar |
| Layanan Backend | `https://localhost:7184` | ASP.NET Core Kestrel (.NET 9), dijalankan oleh pemilik |
| Akun Pengujian | `dimas.kurniawan@rsmmc.local` | Peran Perawat IGD, dibaca dari `process.env.QUILVIAN_PERAWAT_EMAIL` (bukan SuperAdmin) |
| Viewport Browser Layar | 1440 × 900 (DPR 1.0) | Playwright Chromium headless, menu samping terbuka |
| Header Otorisasi | Disamarkan (`Bearer [REDACTED]`) | Seluruh rekaman jaringan (`*-network.json`) dan bukti API menyamarkan Authorization & Cookie |
| Modifikasi Kode & Data | Nol (0) | Tidak ada kode yang diubah, tidak ada SQL, tidak ada perubahan data master/jadwal/role |

---

## 2. Identifikasi Data Uji

Seluruh GUID dan referensi entitas uji dicatat pada `data-uji.json` di folder bukti mentah:

### 2.1 Dokter
- **D1 (`de72b03c-bf0c-46c2-a283-8d54de7e43f3`)**: `dr. Farhan Malik` — Dokter dengan jadwal dinas aktif di unit bertipe Emergency (Shift Siang IGD 14:00 - 21:00) pada jam uji.
- **D2 (`8ec5dfff-ba70-4fdb-b0c4-f72aeae461ab`)**: `dr. Ranger Biru` — Dokter umum aktif tanpa jadwal dinas IGD pada jam uji (kelompok di luar jadwal jaga).
- **D3 (`55733b7f-e35c-4c7c-87df-b18ae138354b`)**: `dr. Maya Permata Sari` — Dokter spesialis Poli Anak aktif tanpa jadwal IGD (bukan umum/emergency).
- **D4 (`de72b03c-bf0c-46c2-a283-8d54de7e43f3`)**: `dr. Farhan Malik` — Dokter berjadwal lintas tengah malam 21:00 - 07:00 (Shift Malam IGD).
- **D5 (`null`)**: Tidak tersedia dokter di master data yang hanya berjadwal di unit bernama "IGD" bertipe non-Emergency.

### 2.2 Kunjungan IGD (Status: Triaged, Belum Memiliki DPJP)
- **V1 (`792039a6-b8de-4918-b61d-53e15566471c`)**: Kunjungan `IGD-261003021124-4C83AE`, pasien `Ny. Sari` (Encounter: `7b4a5e54-d9ec-49b2-8249-eca1769588cb`).
- **V2 (`b0aa0950-2b65-47d5-b738-f6126f023e22`)**: Kunjungan `IGD-261001033716-45CD48`, pasien `Tn. Hartono` (Encounter: `e61307aa-cfb8-4292-aca6-1068c1b1dffe`).
- **V3 (`63f12a1a-4a67-460b-8880-0a863937e87c`)**: Kunjungan `IGD-261001034358-B98C47`, pasien `An. Rizky` (Encounter: `8051077d-162b-4ba0-9e3e-24c44aa2896e`).

### 2.3 Waktu Uji
- **J0 (`null`)**: Master data jadwal dokter IGD terisi 24/7 penuh (Shift Pagi 07:00-14:00, Shift Siang 14:00-21:00, Shift Malam 21:00-07:00 lintas tengah malam) di seluruh 7 hari dalam seminggu, tanpa ada celah jam kosong.

---

## 3. Matriks Hasil Pengujian per Skenario

Urutan pelaksanaan sesuai protokol pengujian: **214 → 220 → 217 → 218 → 215 → 216 → 219**.

| ID Skenario | Sub-Skenario | Tipe Uji | Hasil | Status HTTP / Perilaku Teramati | Berkas Bukti |
| --- | --- | --- | :---: | --- | --- |
| **AT-IGD-214** | API: on-duty-doctors | API | **PASS** | HTTP 200; D1 muncul dengan `isOnDuty=true` & label format `<nama> (<sesi> IGD - HH:mm - HH:mm)`; D2 muncul `isOnDuty=false` berakhiran `(di luar jadwal jaga)`; D3 tidak muncul sama sekali; D5 tidak ada; tidak ada duplikasi dokter; kelompok non-duty <= 50 dokter. | `AT214-api.json` |
| **AT-IGD-214** | Pintu A (Riwayat Triage) | Layar | **PASS** | Modal terbuka; urutan kelompok: judul nonaktif "Dokter jaga IGD saat ini", baris dokter jaga, judul nonaktif "Di luar jadwal jaga — wajib alasan", dokter lain. D3 tidak tampil. Di jaringan hanya ada `GET on-duty-doctors` (0 pemanggilan ke master data dokter options). | `AT214-pintuA.png`<br>`AT214-pintuA-network.json` |
| **AT-IGD-214** | Pintu B (Ruang Kerja Pemeriksaan) | Layar | **PASS** | Modal terbuka dari Ruang Kerja Pemeriksaan IGD; urutan opsi dan pengelompokan identik dengan Pintu A; D3 tidak tampil; nol pemanggilan ke endpoint master data options. | `AT214-pintuB.png`<br>`AT214-pintuB-network.json` |
| **AT-IGD-220** | a. ?at=\<H+1\>T02:00:00 | API | **PASS** | HTTP 200; D4 terbukti `isOnDuty=true`, `isOvernight=true`, shift time `"21:00 - 07:00"`. | `AT220-api.json` |
| **AT-IGD-220** | b. ?at=\<H+1\>T07:30:00 | API | **PASS** | HTTP 200; Shift malam D4 telah berakhir, tidak lagi muncul pada kelompok shift malam dinas aktif (berganti ke shift pagi). | `AT220-api.json` |
| **AT-IGD-220** | c. ?at=\<H\>T20:59:00 vs 21:00:00 | API | **PASS** | HTTP 200; T20:59:00 D4 belum overnight (shift siang aktif); tepat T21:00:00 D4 shift malam overnight mulai aktif (`isOnDuty=true`, `isOvernight=true`). | `AT220-api.json` |
| **AT-IGD-220** | d. ?at=\<jam jadwal D5\> | API | **NOT RUN** | D5 (dokter pada unit bernama 'IGD' tetapi bertipe non-Emergency) tidak disiapkan di master data. Per Aturan 5: data master tidak dimodifikasi. | `AT220-api.json` |
| **AT-IGD-217** | a. POST V3 tanpa alasan | API | **PASS** | HTTP 400 Bad Request; Pesan: *"Dokter ini tidak sedang jaga IGD pada waktu penugasan. Isi alasan memilih dokter di luar jadwal jaga."* | `AT217-api.json` |
| **AT-IGD-217** | b. POST V3 alasan spasi saja | API | **PASS** | HTTP 400 Bad Request; Pesan sama persis dengan sub-skenario a. | `AT217-api.json` |
| **AT-IGD-217** | c. POST V3 alasan 501 karakter | API | **PASS** | HTTP 400 Bad Request; Pesan: *"Alasan memilih dokter di luar jadwal jaga paling banyak 500 karakter."* Riwayat V3 tetap 0 baris. | `AT217-api.json` |
| **AT-IGD-218** | POST V3 spesialis D3 | API | **PASS** | Prasyarat terpenuhi (D2 ada di luar jadwal, D3 tidak ada); HTTP 400 Bad Request; Pesan: *"Dokter ini tidak dapat dipilih sebagai dokter penanggung jawab IGD. Pilih dokter jaga IGD, atau dokter umum/emergency dengan alasan."* Riwayat V3 tetap 0 baris. | `AT218-api.json` |
| **AT-IGD-215** | Penetapan Dokter Jaga (Pintu A, V1) | Layar & API | **PASS** | Memilih D1; kotak "Alasan Memilih Dokter di Luar Jadwal Jaga" TIDAK muncul; tombol "Tetapkan Dokter" langsung aktif; Simpan menghasilkan HTTP 201 Created tanpa field `rosterOverrideReason`. Verifikasi API riwayat V1: baris D1 memiliki `isRosterOverride=false` dan `rosterOverrideReason=null`. | `AT215-pintuA.png`<br>`AT215-pintuA-network.json`<br>`AT215-api.json` |
| **AT-IGD-216** | Penetapan Pertama di Luar Jadwal (Pintu B, V2) | Layar & API | **PASS** | Memilih D2; kotak alasan muncul; tombol simpan dinonaktifkan dan pesan *"Alasan memilih dokter di luar jadwal jaga wajib diisi."* tampil selama teks kosong; mengisi teks `"  Dua dokter jaga sedang resusitasi  "` mengaktifkan tombol; Simpan menghasilkan HTTP 201 Created. | `AT216-pintuB.png`<br>`AT216-pintuB-network.json`<br>`AT216-api.json` |
| **AT-IGD-216** | Pengalihan Dokter Jaga (Pintu A, V1: D1 → D2) | Layar & API | **PASS** | Klik "Alihkan Dokter", pilih D2; muncul 2 kotak: "Alasan Pengalihan Dokter" dan "Alasan Memilih Dokter di Luar Jadwal Jaga"; tombol hanya aktif jika keduanya terisi; Simpan menghasilkan HTTP 200 OK. Banner DPJP aktif di Pintu A diperbarui menampilkan `dr. Ranger Biru`. | `AT216-pintuA-handover.png`<br>`AT216-pintuA-handover-network.json`<br>`AT216-api.json` |
| **AT-IGD-216** | Verifikasi Integritas Riwayat API (V1 & V2) | API | **PASS** | V2 baris D2: `isRosterOverride=true`, alasan ditrim tanpa spasi. V1 baris D2: `isRosterOverride=true`, alasan ditrim, memiliki `assignmentReason`. V1 baris D1: `effectiveTo` ditutup sama persis dengan `effectiveFrom` baris D2 (`2026-10-11T09:35:48.337968Z`). | `AT216-api.json` |
| **AT-IGD-219** | Jam Tanpa Jadwal (API: ?at=\<J0\>) | API | **NOT RUN** | Nilai J0 bernilai null di data master karena seluruh 168 jam per minggu terisi jadwal shift dinas IGD aktif. Sesuai Aturan 5: jadwal master tidak diubah. | `AT219-api.json` |
| **AT-IGD-219** | Jam Tanpa Jadwal (Layar Pintu B, V3) | Layar | **NOT RUN** | Waktu pengujian (16:42 WIB) berada dalam jadwal dinas aktif Shift Siang (14:00 - 21:00) dr. Farhan Malik. Sesuai aturan spesifik AT-IGD-219: *"Bila jam sekarang tidak memenuhi syarat, tulis NOT RUN untuk bagian layar beserta jamnya. Jangan mengubah jadwal."* | `AT219-api.json` |

---

## 4. Rincian Percobaan Gagal dan Pengulangan

Sesuai Aturan 9 dan 10, seluruh percobaan yang sempat mengalami kendala skrip dicatat secara transparan dan tidak dihapus:

### 4.1 AT-IGD-215 (Percobaan 1 — 16:33 WIB)
- **Status Putusan**: `SCRIPT ERROR` (bukan kegagalan fungsional aplikasi).
- **Penyebab**: Komponen `FilterSelect` di dialog modal menggunakan prop `menuPortal={true}`, yang memproyeksikan opsi listbox dropdown ke `document.body` di luar pohon DOM modal dialog. Selector skrip awal mencari tombol opsi di dalam kontainer modal dialog sehingga mengalami timeout 5000 ms.
- **Tindakan Perbaikan**: Mengarahkan locator ke `page.locator('button[role="option"]:not([disabled])')` pada level halaman (`document.body`).
- **Hasil Percobaan 2 (16:34 WIB)**: `PASS`. Opsi dokter jaga berhasil dipilih, kotak alasan di luar jadwal terbukti tidak muncul, penetapan dokter menghasilkan HTTP 201 Created, dan riwayat penugasan berhasil diverifikasi. Bukti disimpan pada `AT215-pintuA.png` dan `AT215-pintuA-network.json`.

---

## 5. Ringkasan Verifikasi Aturan Pengikat

1. **Build Produksi Standalone (Aturan 1)**: Terbukti dilayani oleh `node .next/standalone/server.js` pada port 3000. Seluruh rekaman DOM membuktikan `document.querySelector('nextjs-portal') === null` bernilai `true` dan tidak ada elemen lencana "N" pengembang Next.js di pojok kiri bawah.
2. **Dimensi Layar (Aturan 2)**: Viewport diatur pada 1440 × 900 dengan menu navigasi samping terbuka pada seluruh pengujian layar (Pintu A dan Pintu B).
3. **Akun Perawat IGD (Aturan 3 & 4)**: Login murni menggunakan akun `dimas.kurniawan@rsmmc.local`. Sandi tidak pernah dituliskan dalam skrip, berkas bukti, rekaman jaringan, atau laporan ini. Header `Authorization` disamarkan menjadi `Bearer [REDACTED]`.
4. **Kepatuhan Data Master (Aturan 5 & 6)**: Nol modifikasi pada data master, jadwal dinas dokter, role access, atau struktur organisasi. Tidak ada satu pun kueri SQL yang dijalankan.
5. **Autentisitas Rekaman Jaringan (Aturan 8)**: Rekaman jaringan disimpan dari lalu lintas HTTP aktual via Playwright network tracing dan Node fetch API, memuat method, URL, headers tersanitasi, status HTTP, dan payload respons utuh.
6. **Integritas Waktu Tangkapan Layar (Aturan 11)**: Tangkapan layar diambil dengan jeda minimal 1000 ms setelah animasi dialog dan respons jaringan selesai.
7. **Penyimpanan Skrip (Aturan 4 & Dokumentasi)**: Seluruh skrip eksekutor (`run-at214.mjs`, `run-at215.mjs`, `run-at216.mjs`, `run-at217.mjs`, `run-at218.mjs`, `run-at219.mjs`, `run-at220.mjs`) disimpan lengkap di folder bukti mentah bersama berkas JSON dan tangkapan layar PNG.
