# Laporan Perubahan Frontend — `FE-KSK-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-005` |
| Judul | Inactivity timeout |
| Slice | EPIC KSK-05 — Pembersihan sesi, gelombang `MVP-1`, gelombang eksekusi 3 |
| Roadmap | `docs/module-blueprints/kiosk/roadmap/frontend-roadmap.md` — kartu `FE-KSK-005` |
| Trace | `FR-KSK-040..043`; `KSK-DEC-010`; `KSK-GAP-008/009`; `KSK-UI-006`; SEC-KSK-007; `KSK-AC-005`; UAT-15/16; `contracts/state-transition-matrix.md` §1–2 (`SESSION_CLEARED`); `data/data-dictionary.md` (masa simpan data) |
| Contract version | `KSK-CONTRACT-v1` — `approved` (Sukma Giri Pratama, 2026-09-30) |
| Wewenang UI | `KSK-UI-006` `DEV_DISCRETION` (modal/banner hitung mundur 15 detik + "Lanjutkan"); gaya = keputusan gerbang UI kiosk opsi A dari `FE-KSK-004` |
| Dependency | `FE-KSK-004` ✅ |
| Klasifikasi | `MEDIUM` — 4 berkas baru + 4 berkas diperbarui; interceptor HTTP sementara; tanpa perubahan base component maupun `InstanceAxios` |
| Task mode | `FRONTEND` — pengguna 2026-09-30 ("oke lanjutkan" setelah `FE-KSK-004`) |
| Target tulis | `V2QuilvianSystemFrontendDev`: hook baru + pemasangan di Cek No. RM dan Pasien Lama. Repository backend: laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `4ec51b0bf` (branch `sukmagpV2`), belum di-commit |
| Commit backend yang dijadikan rujukan | `051a658a` (branch `sukmagp`) + perubahan `BE-KSK-003` yang belum di-commit |
| Tanggal | 30 September 2026 |
| Status | ✅ SELESAI — 6 dari 6 acceptance criteria terbukti |

---

## 1. Keadaan yang ditemukan di awal

Kiosk tidak punya inactivity timeout (`KSK-CAP-025`). Data pasien di layar Pasien Lama hanya dibersihkan saat pasien menekan Selesai/Home atau halaman ditutup. Jadi bila pasien pergi di tengah pendaftaran, nama, No. RM, dan data pasiennya tetap terbuka bagi orang berikutnya di depan Kiosk. Layar Cek No. RM (`FE-KSK-004`) juga belum punya timeout.

Request di alur Pasien Lama tersebar di banyak komponen step (pencarian, detail pasien, pembuatan kunjungan), sehingga "timeout ditunda selama request berjalan" (`KSK-GAP-008`) tidak dapat dibaca dari satu penanda loading.

---

## 2. Proses bisnis dari sisi pengguna

1. Pasien memakai layar Cek No. RM atau Pendaftaran Pasien Lama (semua step, termasuk Cetak Antrean).
2. Setiap sentuhan, ketukan tombol, ketikan, atau gulir mengulang hitungan dari nol.
3. Setelah **105 detik** tanpa aktivitas, muncul dialog **"Sesi akan berakhir"** dengan angka hitung mundur besar (15, 14, …), kalimat "Tidak ada aktivitas. Layar akan kembali ke Beranda dalam N detik dan data yang sedang diisi akan dihapus.", dan tombol **Lanjutkan** yang langsung terfokus.
4. Menekan **Lanjutkan** (atau menyentuh layar di mana saja) menutup dialog dan mengulang hitungan 120 detik.
5. Bila tidak ada sentuhan sampai **120 detik**, layar kembali ke **Beranda Kiosk**. Pasien terpilih, hasil pindai/sesi kiosk, draf pendaftaran (jenis kunjungan, pembayaran, layanan), tujuan layanan, nomor yang diketik, hasil Cek No. RM, dan handoff `patientId` dibuang.
6. Menekan tombol Back browser setelah itu tidak memunculkan data pasien lagi.

Jalur tidak normal: selama Kiosk masih menunggu jawaban server (misalnya menyimpan kunjungan), hitungan ditahan di nol. Setelah jawaban datang, hitungan 120 detik dimulai lagi dari awal, sehingga kunjungan tidak pernah terpotong setengah jadi.

Beranda Kiosk sendiri tidak memasang timeout, karena `KSK-DEC-010` hanya berlaku untuk Cek No. RM dan Pasien Lama.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu task; `00-interview-decisions.md` (`KSK-DEC-010`, `KSK-UI-006`); `02-requirement-completeness-assessment.md` (`KSK-GAP-008/009`); `03-frontend-architecture.md` §3–5; `state-transition-matrix.md` §1–2; `data-dictionary.md`; `use-kiosk-old-patient-registration.jsx` (`resetRegistrationDraft`, `handleResetAll`, deep link); `kiosk-old-patient-view.jsx`; `kiosk-old-patient-step-find.jsx` (pencarian tanpa sesi kiosk); `confirm-modal.jsx`; `InstanceAxios.jsx` (interceptor).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/kiosk/use-kiosk-inactivity-timeout.jsx` (baru) | Hitungan per detik; aktivitas `pointerdown`/`keydown`/`touchstart`/`wheel` mengulang hitungan; peringatan pada 15 detik terakhir; `onTimeout` dipanggil tepat sekali; penghitung request berjalan lewat interceptor `InstanceAxios` yang dipasang saat aktif dan dilepas saat halaman ditutup |
| `src/lib/constants/kiosk/kiosk-inactivity.constants.js` (baru) | Bawaan 120/15 detik, batas minimum 10 detik, daftar event aktivitas, teks dialog, `resolveKioskIdleSeconds` untuk `NEXT_PUBLIC_KIOSK_IDLE_SECONDS` |
| `src/components/view/kiosk/kiosk-inactivity-warning.jsx` (baru) | Dialog `role="alertdialog"` + `aria-modal`, hitung mundur, fokus otomatis ke Lanjutkan; dipakai kedua halaman |
| `src/style/kiosk/kiosk-inactivity-warning.module.css` (baru) | Gaya dialog, token global saja |
| `src/lib/hooks/kiosk/registration/medical-record-check/use-kiosk-medical-record-check.jsx` | `handleSessionTimeout`: batalkan request, kosongkan nomor/hasil/error, `clearHandoff`, `router.replace("/kiosk")`; ekspos `inactivity` |
| `src/components/view/kiosk/registration/medical-record-check/kiosk-medical-record-check-view.jsx` | Render `KioskInactivityWarning` |
| `src/lib/hooks/kiosk/registration/use-kiosk-old-patient-registration.jsx` | `handleSessionTimeout`: `handleResetAll` existing (pasien, sesi pindai, draf, tujuan layanan) + `clearHandoff` + `router.replace("/kiosk")`; ekspos `inactivity`. Tidak ada logika step yang diubah |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-view.jsx` | Render `KioskInactivityWarning` |

**Delta dari kartu task:** kartu hanya menyebut hook baru + pemasangan. Dialog peringatan (`KSK-UI-006`) membutuhkan satu komponen dan satu CSS module bersama, serta pemasangan di view Pasien Lama (kartu menyebut "hook Pasien Lama"). Komponen dibuat sekali agar tidak diduplikasi di dua halaman.

### 3.3 Kepatuhan arsitektur frontend

- Alur: `view → hook halaman → use-kiosk-inactivity-timeout → InstanceAxios` (interceptor saja). View tidak memanggil HTTP.
- `InstanceAxios.jsx` tidak diubah. Interceptor penghitung hanya hidup selama halaman kiosk terbuka, dan hanya menghitung (tidak mengubah request/response).
- `router.replace` (bukan `push`) supaya entri riwayat halaman berisi data tergantikan Beranda.
- Pola React Compiler: tanpa `Date.now()` saat render, tanpa `setState` sinkron di effect (dua warning lint awal sudah diperbaiki).

**Gerbang keputusan base component**

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
|---|---|---|---|---|
| Dialog peringatan hitung mundur | `ConfirmModal` | `base-features/confirm-modal.jsx`: bisa `hideCancel`, `confirmLabel`, `backdrop="static"`, tetapi bergaya admin (react-bootstrap, tombol `region-btn` ±42px); 0 layar kiosk memakainya; dialog kiosk lain dibuat sendiri (`kiosk-old-patient-step-find.jsx:497`, `kiosk-new-patient-step-payment.jsx:311`) | NEW | Dialog kiosk kecil yang dipakai bersama, gaya opsi A |

`UI GATE: 1 elemen — REUSE 0, EXTEND 0, COMPOSE 0, WRAP 0, NEW 1`

Status NEW ini **merujuk keputusan pengguna yang sama pada modul yang sama**: gerbang UI `FE-KSK-004` opsi A (Sukma Giri Pratama, 30 Sep 2026). Keputusan itu menetapkan elemen kiosk memakai pola kiosk + token global, bukan base component admin (opsi C ditolak). Alternatif `ConfirmModal` tetap mungkin dan murah bila pemilik UI menghendakinya.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Selama request berjalan, peringatan tidak muncul dan hitungan ditahan |
| Kosong | `NOT APPLICABLE` |
| Gagal | `NOT APPLICABLE` — timeout bukan error; hasilnya kembali ke Beranda |
| Tanpa hak akses | Timeout hanya aktif bila akun Kiosk sah (`enabled = canAccessKiosk`) |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — task ini tidak memanggil endpoint baru. Interceptor hanya menghitung request `InstanceAxios` yang sudah ada.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --max-warnings=0` untuk 7 berkas JS/JSX task | 0 error, 0 warning (setelah dua warning React Compiler di hook baru diperbaiki) | `PASS` | Keluaran perintah |
| `npm run build` | Exit `0`, "Compiled successfully in 69s" | `PASS` | Keluaran `next build` |
| Grep anti-regresi (CSS + komponen baru) | 0 hex/rgb/hsl, 0 `px`, 0 `!important`, 0 dark mode, 0 inline style; 1 `<button>` mentah (opsi A); satu literal `z-index: 2000` (tidak ada token z-index) | `PASS` | Keluaran grep |
| `resolveKioskIdleSeconds` (skrip node) | tanpa env / `""` / `abc` / `5` / `9` → 120; `10` → 10; `20` → 20; `300` → 300 | `PASS` | Keluaran skrip |
| Uji browser Playwright, `NEXT_PUBLIC_KIOSK_IDLE_SECONDS=20` (peringatan di detik 5) | 20/20 PASS pada run ketiga | `PASS` | Tabel di bawah + screenshot |

`AUTOMATED TEST: SKIPPED (opsional) — uji browser lewat skrip Playwright sekali pakai di scratchpad; repository tidak memakai Jest`

### Uji browser

Lingkungan: `next dev` port 3000 dengan `NEXT_PUBLIC_KIOSK_IDLE_SECONDS=20` → backend `bin/Release` di `https://localhost:7184` → DB `QuilvianNewDevSukma`. Login akun perangkat Kiosk `kiosk-test` (kredensial dari pengguna, tidak dicatat).

Data:
- Hasil Cek No. RM "ditemukan" untuk P7 disimulasikan lewat `page.route`, jadi KTP P7 tidak diubah.
- Di Pasien Lama, P7 dicari lewat nama lengkap ke backend asli. Pencarian ini baca-saja; sesi kiosk baru di DB tetap 0.

| ID | Skenario | Hasil | Klasifikasi |
| --- | --- | --- | --- |
| R0 | Beranda didiamkan 8 detik | Tidak ada peringatan (Beranda di luar cakupan) | `PASS` |
| AC1/AC6 | Diam setelah membuka Cek No. RM | Peringatan muncul setelah 5,4 detik (= 20 − 15); teks "Sesi akan berakhir … dalam 15 detik …" + Lanjutkan; fokus pada Lanjutkan | `PASS` |
| AC2 | Tekan Lanjutkan | Dialog tertutup; 3 detik kemudian belum muncul; muncul lagi 5,7 detik setelah Lanjutkan (hitungan dari nol) | `PASS` |
| AC3a | Cek No. RM dengan hasil P7 tampil, lalu diam | Pindah ke `/kiosk`; handoff `{ patientId: null }` | `PASS` |
| AC5a | Back setelah AC3a | Nama P7, KTP, dan "Pasien Ditemukan" tidak tampil | `PASS` |
| AC3b | Cek → Lanjut (handoff = P7) → Poliklinik → cari "KSKTEST Karyawan PT Samaran" → Review Data berisi P7, lalu diam | Peringatan muncul di atas Review Data (screenshot); lalu `/kiosk`; handoff `{ patientId: null }` | `PASS` |
| AC5b | Back setelah AC3b | Halaman sebelumnya (Cek No. RM) kosong; nama P7 tidak tampil | `PASS` |
| AC4a | Request lookup ditahan 18 detik (peringatan normalnya di detik 5) | Tetap di layar, peringatan tidak pernah muncul | `PASS` |
| AC4b | Request selesai | Hitungan mulai lagi: peringatan 5,5 detik kemudian, lalu Beranda | `PASS` |
| R-akhir | DB | `MstPatient` 17, `RegPatientEncounter` 173, `TrxKioskScanSession` baru 0, KTP P7 tetap `NULL` | `PASS` |

Catatan run:
- **Run pertama:** berhenti di login. Tombol ditekan sebelum JavaScript halaman login selesai dimuat setelah `next dev` baru menyala, sehingga form terkirim secara HTML biasa. Skrip diubah untuk menunggu `networkidle`.
- **Run kedua:** 18 PASS, lalu gagal di AC4b. Request saya tahan 25 detik, melewati batas timeout service lookup (20 detik), sehingga axios membatalkannya lebih dulu. Ini kesalahan desain uji, bukan aplikasi. Penahanan diubah menjadi 18 detik; itu tetap membuktikan penundaan karena tanpa penundaan peringatan muncul di detik ke-5.
- **Run ketiga:** 20/20.

Uji manual oleh orang: `NOT RUN` — digantikan uji browser otomatis dan pemeriksaan visual screenshot peringatan di layar Review Data.

**Tidak dijalankan:**
- Uji dengan nilai bawaan 120 detik penuh di browser. Nilai bawaan dibuktikan lewat `resolveKioskIdleSeconds` dan build tanpa env.
- Uji timeout pada step Konfirmasi/Cetak Antrean. Mekanismenya sama untuk semua step karena dipasang di hook halaman.
- Perangkat kiosk fisik.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (roadmap) | Status | Bukti |
| --- | --- | --- |
| 1. 105 detik tanpa sentuhan → peringatan 15 detik | Terpenuhi (diuji dengan skala 20/15) | AC1 |
| 2. Lanjutkan mengulang hitungan | Terpenuhi | AC2 |
| 3. 120 detik → state pasien, hasil pindai, draf, dan handoff kosong, lalu layar ke Beranda | Terpenuhi | AC3a, AC3b; `handleResetAll` + `clearHandoff` |
| 4. Selama request berjalan timeout tidak terpicu | Terpenuhi | AC4a, AC4b |
| 5. Back browser setelah timeout tidak menampilkan data pasien | Terpenuhi | AC5a, AC5b |
| 6. Nilai dapat ditimpa `NEXT_PUBLIC_KIOSK_IDLE_SECONDS` untuk uji | Terpenuhi | Uji browser 20 detik; skrip `resolveKioskIdleSeconds` |
| DoD: AC 1–6 di `FE-KSK-005.md` | Terpenuhi | Laporan ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` baru |
| Masalah yang diketahui | (1) `NEXT_PUBLIC_KIOSK_IDLE_SECONDS` dibaca saat build; mengubahnya di produksi butuh build ulang (sesuai `04-prd-to-mvp.md`: satu nilai untuk seluruh Kiosk). (2) Request latar `InstanceAxios` (misalnya refresh sesi perangkat tiap 10 menit) ikut menahan hitungan sesaat. Efeknya hanya memperpanjang, tidak pernah memotong |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` di source. Dua proses `next dev` sisa uji `FE-KSK-004` yang menahan port 3000 ditemukan dan dihentikan (proses milik sesi agent sendiri, dibuat 21:04) |
| Interupsi | `NONE` |
| Status Git | Frontend (task ini): ` M src/components/view/kiosk/registration/old-patient/kiosk-old-patient-view.jsx`, ` M src/lib/hooks/kiosk/registration/use-kiosk-old-patient-registration.jsx`, `?? src/components/view/kiosk/kiosk-inactivity-warning.jsx`, `?? src/lib/constants/kiosk/kiosk-inactivity.constants.js`, `?? src/lib/hooks/kiosk/use-kiosk-inactivity-timeout.jsx`, `?? src/style/kiosk/kiosk-inactivity-warning.module.css`, serta perubahan di berkas `FE-KSK-004` (hook + view Cek No. RM) |
| Langkah berikutnya | `FE-KSK-006` (Step 1 tanpa KTP/HP di URL), lalu `FE-KSK-007` setelah `KSK-OQ-004` ditutup |
