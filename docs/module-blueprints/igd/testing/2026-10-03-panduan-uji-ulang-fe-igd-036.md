# Panduan Uji Ulang `FE-IGD-036` — Putaran 3 Oktober 2026 (sore)

| Field | Nilai |
| --- | --- |
| Tanggal disusun | 3 Oktober 2026 |
| Untuk | Agen penguji (Antigravity) yang dijalankan pemilik modul (Rizki) |
| Tujuan | Membuktikan pengerjaan ulang `FE-IGD-036` bagian 10 (pesan galat dialog Mulai Triage digulir otomatis; tanggal bawaan waktu tiba) dan memastikan langkah Verifikasi loket sudah pulih dari kemunduran commit `4520abe64` |
| Jumlah skenario | 10 uji layar |
| Source yang diuji | Frontend `RizkiV2` — dicatat agen sendiri pada langkah P1. Backend `rizkiG` dijalankan pemilik; **tidak ada perubahan backend** pada putaran ini |
| Sumber skenario | [Laporan `FE-IGD-036`](../task/report/frontend/FE-IGD-036.md) bagian 10.5, [`FE-IGD-038`](../task/report/frontend/FE-IGD-038.md), [`FE-IGD-034`](../task/report/frontend/FE-IGD-034.md). Bila berbeda, **laporan task yang berlaku** |
| Status | Panduan. Belum ada skenario yang dijalankan |

Berkas ini tidak diisi hasil. Hasil ditulis pada laporan baru — lihat bagian 6.

---

## 1. Aturan yang mengikat agen

Putaran sebelumnya ditolak sebagian karena melanggar aturan di bawah. Patuhi seluruhnya.

| No | Aturan | Contoh pelanggaran yang pernah terjadi |
| ---: | --- | --- |
| 1 | **Layar dilayani hasil build** (`npm run start`), bukan `npm run dev`. Bukti: `document.querySelector('nextjs-portal')` bernilai `null` pada setiap skenario, dicatat di JSON. Lencana "N" di pojok kiri bawah tangkapan layar berarti `next dev` → skenario tidak sah | Kelima tangkapan layar 3 Oktober pagi memperlihatkan lencana "N" |
| 2 | **Tangkapan layar diambil sesudah respons tiba dan layar selesai berubah**: tunggu respons permintaan yang diuji, lalu tunggu 1000 ms | `036-U7` 2 Oktober diambil saat tombol masih *Memulai…* |
| 3 | **Jangan menggulir, `scrollIntoView`, atau `scrollIntoViewIfNeeded` pada `036-U7`.** Yang diuji justru apakah layar menggulir sendiri | `036-U7` 3 Oktober putaran kedua menggulir lewat skrip |
| 4 | **Catat semua permintaan ke backend** (`https://localhost:7184/api/**`) per skenario, bukan hanya satu endpoint: method, URL, badan permintaan, kode status, badan respons, waktu. Beberapa pemeriksaan justru membuktikan *tidak adanya* permintaan | Catatan `036-U7` hanya merekam `start-triage`, sehingga pemuatan ulang daftar tidak terbukti |
| 5 | **Hasil ditulis dari eksekusi.** Status dan kalimat diambil dari respons dan DOM yang benar-benar teramati, bukan teks tetap di skrip | — |
| 6 | **Setiap percobaan dilaporkan**, termasuk yang gagal atau diulang. Bukti percobaan lama tidak boleh ditimpa — beri akhiran `-try2`, `-try3` | Percobaan pertama `040-U6` tertimpa dan tidak dilaporkan |
| 7 | **Narasi laporan sama dengan bukti mentah.** Tulis "berurutan" bila berurutan, "kunjungan lain" bila memakai data lain | `036-U8` ditulis "bersamaan" padahal berjeda 2 detik |
| 8 | **Tidak ada penulisan ke basis data.** Kueri `SELECT` boleh untuk mencari data uji dan memeriksa hasil | — |
| 9 | **Kredensial tidak ditulis di skrip maupun laporan.** Baca dari variabel lingkungan `QUILVIAN_TEST_EMAIL`, `QUILVIAN_TEST_PASSWORD`, dan `QUILVIAN_DEV_DB_URL`. Laporan tidak memuat host, port, sandi, token, atau connection string | Skrip 3 Oktober menulis sandi basis data dalam teks biasa; laporan memuat alamat host |
| 10 | **Source tidak diubah**, frontend maupun backend. **Backend tidak di-build** oleh agen. Skenario yang gagal dilaporkan `FAIL` apa adanya | — |
| 11 | Skenario yang tidak dapat dijalankan ditulis **`NOT RUN` beserta alasannya**, bukan `PASS` | — |
| 12 | Ini verifikasi pengembang. **Tidak ada klaim UAT** | — |

---

## 2. Persiapan

| Langkah | Tindakan | Syarat lanjut |
| --- | --- | --- |
| P1 | Di `QuilvianSystemFrontendDev`: catat `git rev-parse --short HEAD`, `git status --short`, dan waktu ubah dua berkas: `src/components/view/health-services/emergency-installation-management/emergency-management-triage-view/components/emergency-triage-start-dialog.jsx` dan `src/components/view/health-services/registration-management/emergency-registration/verification-step.jsx` | — |
| P2 | Bandingkan waktu ubah `.next/BUILD_ID` dengan kedua berkas pada P1 | `BUILD_ID` **lebih baru** dari keduanya. Bila lebih tua atau tidak ada: jalankan `npm run build` (frontend saja), catat hasilnya, lalu ulangi P2 |
| P3 | Pastikan port 3000 tidak dipakai `next dev`. Bila dipakai proses lain, **berhenti dan minta pemilik menghentikannya** — jangan mematikan proses yang bukan milik agen | Port 3000 bebas |
| P4 | Jalankan `npm run start` (menjalankan `node .next/standalone/server.js`) di port 3000. Port lain tidak dapat dipakai karena backend hanya mengizinkan origin `http://localhost:3000` | Halaman login terbuka; `nextjs-portal` bernilai `null` |
| P5 | Pastikan backend pemilik berjalan di `https://localhost:7184` dengan mencoba login | Login berhasil |
| P6 | Viewport bawaan **1366×768**. `036-U7` dijalankan dua kali: 1366×768 dan **1280×720** | — |

Alamat layar:

| Layar | URL |
| --- | --- |
| Loket pendaftaran IGD | `http://localhost:3000/health-services/registration-management/emergency-registration` |
| Daftar Triage Pasien | `http://localhost:3000/health-services/emergency-installation-management/emergency-triage` |

---

## 3. Data uji

Cari dengan kueri `SELECT` (aturan 8). Bila data contoh sudah tidak memenuhi syarat, pilih data lain yang memenuhi dan
catat penggantinya.

| Kode | Syarat | Contoh yang pernah dipakai |
| --- | --- | --- |
| **D1** | Pasien yang punya kunjungan IGD **belum berakhir** (`EmgVisit.VisitStatus` bukan 8/9, tidak dihapus) **dan** encounter `Emergency` kedua yang belum berakhir dan **belum punya kunjungan** | RM `00-00-01-33`, encounter kedua `ENC-RSMMC-00320`, kunjungan berjalan `IGD-261001092617-190FE0`. Bila tidak lagi memenuhi: buat encounter kedua lewat loket dengan alasan pendaftaran ganda (alur `038-U4`) |
| **D2** | Pasien **bersih**: tanpa kunjungan IGD berjalan dan tanpa encounter `Emergency` yang belum berakhir | Pilih dari data master pasien; catat nama dan RM |
| **D3** | Pasien yang punya kunjungan IGD berjalan (boleh pasien D1) | RM `00-00-01-33` |

Contoh kueri pemeriksaan D1 (baca saja):

```sql
SELECT e."EncounterNumber", e."EncounterStatus", e."CompletedAt", e."CancelledAt", e."NoShowAt",
       (SELECT v."EmergencyVisitNumber" FROM public."EmgVisit" v
         WHERE v."PatientId" = e."PatientId" AND NOT v."IsDelete" AND v."VisitStatus" NOT IN (8, 9) LIMIT 1) AS kunjungan_berjalan,
       (SELECT COUNT(*) FROM public."EmgVisit" v WHERE v."EncounterId" = e."Id" AND NOT v."IsDelete") AS kunjungan_pada_encounter
FROM public."RegPatientEncounter" e
WHERE e."EncounterNumber" = 'ENC-RSMMC-00320';
-- Syarat D1: kunjungan_berjalan terisi, kunjungan_pada_encounter = 0, encounter belum berakhir
```

---

## 4. Skenario

Kerjakan berurutan. `036-U9` dan `038-U2` memakai pasien hasil `036-U1`.

### 4.1 Dialog Mulai Triage — pengerjaan ulang `FE-IGD-036` bagian 10

| ID | Langkah | Yang diharapkan | Bukti wajib |
| --- | --- | --- | --- |
| `036-U7a` | Viewport 1366×768. Daftar Triage Pasien → cari RM pasien D1 → baris encounter keduanya → **Aksi → Mulai Triage** → tanpa mengubah isian, tekan **Mulai Triage** di dialog. **Jangan menggulir apa pun** (aturan 3) | (1) `POST …/emergency-visits/start-triage` → `409` dengan kalimat *"Pasien ini masih memiliki kunjungan IGD aktif bernomor …"*. (2) Kotak pesan merah **seluruhnya berada di area terlihat** isi dialog tanpa digulir manual. (3) Dialog tetap terbuka. (4) Ada `GET …/emergency-visits/triage-queue…` **sesudah** respons `409` (daftar dimuat ulang) | PNG sesudah respons + 1000 ms. JSON: catatan jaringan lengkap; hasil `getBoundingClientRect()` kotak pesan **dan** isi dialog (`.modal-body`), beserta nilai `pesanTerlihatUtuh = top >= bodyTop && bottom <= bodyBottom`; `scrollTop` isi dialog sebelum menekan tombol dan sesudah 1000 ms |
| `036-U7b` | Sama dengan `036-U7a`, viewport **1280×720** | Sama dengan `036-U7a` | Sama dengan `036-U7a` |
| `036-R1` | Daftar Triage Pasien → baris encounter **tanpa kunjungan** (boleh encounter pasien D2 hasil `036-U1`, **sebelum** `036-U9`) → **Mulai Triage**. Catat tanggal dan jam yang terisi awal. Tekan **×** pada isian **tanggal** tiba, lalu pilih jam apa saja pada isian jam (bukan tombol *Sekarang*). Tekan **Batal** | (1) Isian awal = waktu terdaftar baris itu (sama dengan teks *Terdaftar: hh.mm, dd Mmm yyyy* di kepala dialog). (2) Sesudah × lalu pilih jam: isian tanggal kembali berisi **tanggal terdaftar**, bukan tanggal hari ini (bila kebetulan sama, catat dan tetap `PASS` hanya bila tanggal = tanggal terdaftar). (3) **Nol** `POST start-triage` | PNG sesudah memilih jam. JSON: tanggal terdaftar, tanggal sebelum ×, tanggal sesudah pilih jam, jam yang dipilih, catatan jaringan |

### 4.2 Loket — langkah Verifikasi sesudah pemulihan `verification-step.jsx`

| ID | Langkah | Yang diharapkan | Bukti wajib |
| --- | --- | --- | --- |
| `036-U1` | Loket → pilih pasien **D2** → isi sampai langkah 4 *Verifikasi & Konfirmasi* → ambil tangkapan layar langkah ini → centang konfirmasi → **Selesaikan Pendaftaran** | Langkah Emergency Visit hanya berisi kategori kunjungan, keluhan utama, dan alasan pendaftaran ganda — **tanpa** isian waktu tiba. Langkah Verifikasi menulis **Encounter Type: Emergency (2)**, **Waktu Tiba: Dicatat perawat saat memulai triage**, dan **Proses Penyimpanan: Satu permintaan pendaftaran, lalu pasien menunggu triage.** Jaringan: tepat **satu** `POST …/patient-encounters`, **nol** `POST …/emergency-visits`. Layar Selesai memuat nomor encounter dan *Menunggu Triage* | PNG langkah Emergency Visit, Verifikasi, dan Selesai. JSON: catatan jaringan lengkap, nomor encounter |
| `038-U2` | Loket → pasien `036-U1` (masih menunggu triage) → alasan pendaftaran ganda **kosong** → Verifikasi → **Selesaikan Pendaftaran** | Kotak kuning **"Pasien ini sudah terdaftar di IGD dan masih menunggu triage"** dengan nomor encounter `036-U1` dan waktu daftarnya, serta tombol **Buka Triage Pasien**. **Nol** `POST …/patient-encounters` | PNG kotak kuning. JSON jaringan |
| `038-U3` | Dari `038-U2`, tekan **Buka Triage Pasien** | Berpindah ke daftar Triage Pasien | PNG, URL tujuan |
| `036-U9` | Loket → pasien `036-U1` lagi → isi **alasan pendaftaran ganda** pada langkah Emergency Visit → selesaikan | Tepat **satu** `POST …/patient-encounters` yang badannya memuat `duplicateEpisodeOverrideReason`; **nol** `POST …/emergency-visits`; layar Selesai untuk encounter kedua | PNG Selesai. JSON jaringan |
| `034-U1` | Loket → pasien **D3** (kunjungan berjalan) → alasan kosong → Verifikasi → **Selesaikan Pendaftaran** | Kotak kuning **"Pasien ini masih punya kunjungan IGD yang berjalan"** memuat nomor kunjungan dan statusnya, serta tombol **Buka Kunjungan IGD**. **Nol** `POST …/patient-encounters` | PNG. JSON jaringan |
| `034-U2` | Dari `034-U1`, tekan **Buka Kunjungan IGD** | Status kunjungan 1–2 → layar Triage; status 3–7 → layar Assesmen IGD | PNG, URL tujuan, status kunjungan D3 |
| `034-U6` | Pasien D3, alasan kosong, Verifikasi → tekan **Selesaikan Pendaftaran dua kali cepat** (jeda < 200 ms) | Hanya **satu** rangkaian permintaan pra-cek; tombol nonaktif bertulisan *Memeriksa pendaftaran…* selama pemeriksaan | PNG saat tombol nonaktif bila tertangkap. JSON jaringan dengan stempel waktu |

---

## 5. Bentuk bukti mentah

Simpan di folder **baru** supaya bukti lama tidak tertimpa:
`QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/`

| Berkas | Isi |
| --- | --- |
| `<ID>.png` | Tangkapan layar viewport (bukan halaman penuh), diambil menurut aturan 2 |
| `<ID>.json` | `{ id, percobaan, mulai, selesai, viewport, nextjsPortalNull, langkah[], jaringan[], pemeriksaan{}, putusan, alasan }` — `jaringan[]` berisi seluruh permintaan ke `/api/**` |
| `persiapan.json` | Hasil P1–P5: SHA, `git status --short`, waktu ubah berkas, waktu `BUILD_ID`, hasil build bila dijalankan |
| Skrip | `<ID>.mjs` atau satu skrip per bagian; kredensial dari variabel lingkungan |

---

## 6. Pelaporan

Tulis laporan baru di folder ini: `2026-10-03-laporan-uji-ulang-fe-igd-036-sore.md`. Berkas panduan ini, laporan task,
roadmap, dan dokumen blueprint lain **tidak disunting** — penandaan status dilakukan pengembang sesudah memeriksa bukti
mentah.

Isi laporan:

1. Metadata: tanggal, pelaksana, SHA frontend dan `git status --short` dari P1, waktu `BUILD_ID`, bukti layar dilayani
   hasil build (`nextjsPortalNull` pada semua skenario), basis data dev **tanpa** alamat host.
2. Tabel per skenario: ID, percobaan ke berapa, `PASS` / `FAIL` / `NOT RUN`, kode status dan kalimat yang teramati,
   nama berkas bukti.
3. Data uji: nama pasien, RM, nomor encounter, dan nomor kunjungan yang dipakai atau dibuat.
4. Setiap penyimpangan dari panduan ini, termasuk data pengganti dan percobaan ulang.

Rekap:

| Task | Skenario | Jumlah | `PASS` | `FAIL` | `NOT RUN` |
| --- | --- | ---: | ---: | ---: | ---: |
| `FE-IGD-036` | U7a, U7b, R1, U1, U9 | 5 | | | |
| `FE-IGD-038` | U2, U3 | 2 | | | |
| `FE-IGD-034` | U1, U2, U6 | 3 | | | |
| **Jumlah** | | **10** | | | |
