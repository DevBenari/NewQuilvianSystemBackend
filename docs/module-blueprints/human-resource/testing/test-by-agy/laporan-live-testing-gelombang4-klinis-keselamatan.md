# Laporan Live Browser Testing — Layar Klinis & Keselamatan (Gelombang 4, `FE-HRD-R44`)

| Item | Keterangan |
| --- | --- |
| Menu yang diuji | **Kesehatan & Keselamatan Kerja RS (K3RS)** (`S-C6`, 8 rute), **Hubungan Industrial & Terminasi** (`S-C5`, 7 rute), **Kredensialing & Lisensi Nakes** (`S-C1`, 10 rute), **OPPE/FPPE & Rekredensial** (`S-C1-OPPE`, 7 rute) |
| Task roadmap | `FE-HRD-R44` — Verifikasi layar klinis dan keselamatan |
| Tanggal uji | Rabu, 18 November 2026, pagi hingga 11.45 WIB |
| Penguji | AI Agent (Claude Code, skill `live-browser-testing`) dengan Google Chrome (Chromium) mode visual/headed |
| Lingkungan | Frontend `http://localhost:3000` (branch `HamzahV2`); backend `https://localhost:7184` (branch `MHamzah`, build scratch, `Runtime__Role=Web`) |
| Database | `QuilvianNewDevHamzah` |
| Akun | `superadmin@admin.com` (admin) dan `dewi.anggraeni@rsmmc.local` (pegawai non-admin) |
| Persetujuan pemilik | 18 Nov 2026: backend dinyalakan apa adanya; uji membaca; galat server disimulasikan di browser (`page.route`), sehingga tidak ada permintaan tiruan yang sampai ke server |

---

## 1. Ringkasan Eksekutif

**Kesimpulan: 32 layar klinis dan keselamatan termuat tanpa galat, dan kegagalan server tampil jujur di layar setelah satu perbaikan.** Skenario keberhasilan (memuat data) dan kegagalan server (500 dan 403) sudah diuji. Alur **tulis** klinis (memutus kelaikan kerja, mencatat insiden, memverifikasi lisensi) belum diuji karena di luar izin penulisan data. Karena itu `FE-HRD-R44` berstatus 🟡.

Contoh di rumah sakit: sebelum perbaikan, bila server gagal memuat radar lisensi, layar **Verifikasi Sumber Primer (PSV)** menampilkan "0 lisensi dalam radar". Petugas kredensial bisa mengira tidak ada STR/SIP yang hampir kedaluwarsa. Sekarang ringkasan menampilkan "-" disertai pesan galat merah.

| Ringkasan | Jumlah |
| --- | ---: |
| Test case | 76 (32 admin + 32 non-admin + 10 simulasi galat + 1 verifikasi ulang PSV + 1 keterlihatan menu non-admin) |
| ✅ Lulus | 73 |
| ❌ Gagal → ✅ diperbaiki di sesi ini | 2 (PSV saat `500` dan `403`, LBT-W4-KL-01) |
| ⚠️ Temuan terbuka | 1 (LBT-W4-KL-02, P1) |
| ⏸️ Tidak dapat diuji | Alur tulis (bagian 7) |

**Yang sudah berjalan baik:**
- Admin: 32/32 layar termuat tanpa HTTP 500, crash JavaScript, layar kosong, atau pesan Axios.
- Non-admin: 18 layar menolak dengan pesan "belum punya hak", dan tidak ada data klinis rahasia yang bocor.
- Simulasi 500/403 di Insiden HI, Pemicu FPPE, dan Sinkron SIMRS menampilkan pesan yang dimengerti pengguna, tanpa data contoh.

---

## 2. Alur Proses Bisnis yang Diuji

1. **Petugas K3RS** membuka dasbor, rekam kelaikan kerja, pembatasan tugas, tertusuk jarum, vaksinasi, surveilans, MCU, dan kecelakaan kerja.
2. **Petugas Hubungan Industrial** membuka dasbor, insiden disiplin, sanksi, skorsing, keluhan, bipartit, dan pemutusan hubungan kerja.
3. **Petugas kredensial** membuka PSV, radar lisensi, buku putih, peer review, pleno komite, persetujuan direktur, registri kewenangan, audit KMKP, kewenangan fast-track, dan siklus rekredensial.
4. **Komite medik** membuka OPPE klinis, periode OPPE, pemicu/proctoring/kesimpulan FPPE, skor komposit rekredensial, dan sinkron SIMRS.
5. **Kegagalan server** — untuk lima layar contoh, seluruh `GET` data dijawab tiruan `500` lalu `403` di browser. Hasil yang diharapkan: pesan galat jelas, bukan angka nol atau data contoh.
6. **Peran tidak berhak** — langkah 1–4 diulang dengan pegawai non-admin.

## 3. Persiapan Lingkungan & Data

Sama dengan [laporan R42](laporan-live-testing-gelombang4-perencanaan-tenaga-kerja.md#3-persiapan-lingkungan--data). Tidak ada data uji yang dibuat.

## 4. Endpoint yang Diuji

Auth: cookie sesi JWT + `[AccessPermission]`. Yang tercatat di bawah adalah endpoint utama yang dipanggil saat layar dimuat.

### `[Tags("Corporate - HRD Occupational Health & Medical Privacy")]` dan `[Tags("Corporate - HRD Medical Check-Up & Risk Surveillance")]`

| Method | Path | Deskripsi | Respons yang teramati |
| --- | --- | --- | --- |
| GET | `/api/v1/hrd/occupational-health/...` | Rekam kesehatan, pembatasan, MCU, surveilans | Admin `200`. Rekam K3RS tidak memanggil daftar lintas pegawai: pegawai dipilih dulu, dan layar menjelaskan bahwa daftar lintas pegawai belum tersedia di server |

### `[Tags("Corporate - HRD Employee Disciplinary & Just Culture")]`

| Method | Path | Deskripsi | Respons yang teramati |
| --- | --- | --- | --- |
| GET | `/api/v1/hrd/disciplinary/incidents` | Daftar insiden disiplin | Admin `200`; simulasi `500`/`403` → pesan ramah |

### `[Tags("HR Credentialing - Professional Licenses & PSV Radar")]`

| Method | Path | Deskripsi | Respons yang teramati |
| --- | --- | --- | --- |
| GET | `/api/v1/corporate/hrd/credentialing/licenses/radar/expiring` | Lisensi yang mendekati kedaluwarsa | Admin `200`; simulasi `500` → sebelum perbaikan tampil angka 0, sesudah perbaikan "-" + pesan galat |

### `[Tags("Corporate - HRD FPPE Incident & Focused Evaluation")]` dan `[Tags("Corporate - HRD Recredentialing Quality & Composite Scorecard")]`

| Method | Path | Deskripsi | Respons yang teramati |
| --- | --- | --- | --- |
| GET | `/api/v1/corporate/hrd/clinical-performance/fppe/...` | Pemicu, proctoring, kesimpulan FPPE | Admin `200`; simulasi → pesan ramah |
| GET | `/api/v1/corporate/hrd/credentialing/recredentialing/...` | Skor komposit, sinkron SIMRS | Admin `200`; simulasi → pesan ramah |

## 5. Rincian Test Case

### P0 — Critical / Fatal Functional

| ID | Skenario | Hasil |
| --- | --- | --- |
| KL-P0-01 s.d. -32 | 32 layar dibuka admin | ✅ 32/32 tanpa 5xx, crash, blank, atau pesan Axios |
| KL-P0-33 s.d. -64 | 32 layar dibuka non-admin | ✅ tidak ada crash. 18 layar menolak dengan pesan tanpa hak; 14 layar termuat (data milik sendiri atau daftar kosong) |

### P1 — Business Compliance

| ID | Skenario | Hasil |
| --- | --- | --- |
| KL-P1-01 | PSV saat radar gagal (500 tiruan) tidak boleh menampilkan "0 lisensi" | ❌ → ✅ diperbaiki (LBT-W4-KL-01); bukti `verifikasi-perbaikan/07-psv-radar-500.png` |
| KL-P1-02 | Insiden HI, Pemicu FPPE, Sinkron SIMRS saat 500 dan 403 | ✅ 6/6 pesan ramah, tanpa data contoh |
| KL-P1-03 | Rekam K3RS saat 500/403 | ✅ tidak ada panggilan daftar saat dimuat; pesan info benar |
| KL-P1-04 | Menu klinis untuk non-admin | ⚠️ Menu K3RS (8/8) dan Hubungan Industrial (7/7) tetap terlihat oleh pegawai biasa (LBT-W4-KL-02). Menu Kredensial (0/10) tersembunyi dengan benar; OPPE 2/7 terlihat |

## 6. Daftar Temuan & Rekomendasi

### P1 — Kepatuhan bisnis

| ID | Temuan | Contoh dampak | Status / rekomendasi |
| --- | --- | --- | --- |
| LBT-W4-KL-01 | PSV menelan galat radar lisensi: `radarError` tidak dibaca, sehingga ringkasan menampilkan 0 | Petugas kredensial menyimpulkan tidak ada STR/SIP yang hampir habis, padahal server gagal | ✅ **Diperbaiki** di `credentialing-psv-dashboard-view.jsx`: ringkasan "-" dan `InformationAlert` merah saat radar gagal |
| LBT-W4-KL-02 | Butir menu K3RS dan Hubungan Industrial tanpa `requiredPermission` terlihat oleh semua pegawai | Perawat melihat menu "Insiden Disiplin", membukanya, lalu ditolak. Halaman memang aman, tetapi navigasi membingungkan dan memberi kesan modul terbuka. | Terbuka. Tambahkan `requiredPermission` yang memetakan resource/action backend (misalnya `HealthRecord : Read`, `EmployeeDisciplinary : Read`) di `menu-items.jsx`. Butir layanan mandiri seperti "Kesehatan Saya" tetap tanpa gerbang. |

## 7. Tidak Dapat Diuji / Tertahan

| Hal | Alasan |
| --- | --- |
| Alur tulis klinis: memutus kelaikan kerja, mencatat insiden/sanksi, memverifikasi PSV, keputusan FPPE, eksekusi sinkron SIMRS | Penulisan data di luar izin Gelombang 4 (hanya satu delegasi yang diizinkan). Risiko yang tercatat di roadmap: "butuh data uji". |
| Skenario keberhasilan dengan data klinis nyata | Data K3RS/HI/kredensial di DB dev sedikit; kebanyakan daftar kosong |

## 8. Data Uji yang Tertinggal di `QuilvianNewDevHamzah`

Tidak ada.

## 9. Lampiran Teknis

| Berkas | Isi |
| --- | --- |
| `test-with-agy/scripts/hrw4-1118-server-failure.mjs` | Simulasi `500`/`403` untuk semua `GET` data (kecuali auth/menu/izin/opsi) |
| `test-with-agy/scripts/hrw4-1118-verify-fixes.mjs` | Verifikasi ulang PSV setelah perbaikan |
| `test-with-agy/screenshots/hrw4-1118/R44-admin/`, `R44-nonadmin/` | 32 + 32 screenshot dan `hasil.jsonl` |
| `test-with-agy/screenshots/hrw4-1118/R44-kegagalan-server/` | 10 screenshot simulasi |
| `test-with-agy/screenshots/hrw4-1118/verifikasi-perbaikan/07-psv-radar-500.png` | PSV setelah perbaikan |

## 10. Tindak Lanjut

1. Frontend: tambahkan `requiredPermission` untuk menu K3RS dan Hubungan Industrial (LBT-W4-KL-02).
2. Uji alur tulis klinis per peran (petugas K3RS, dokter Hiperkes, komite medik) setelah pemilik mengizinkan data uji.
