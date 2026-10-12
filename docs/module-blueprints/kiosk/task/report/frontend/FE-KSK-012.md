# Laporan Perubahan Frontend — `FE-KSK-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-012` |
| Judul | Pilih Layanan Tujuan lebih jelas; No. HP Pasien Baru dibatasi |
| Slice | Amandemen 8 Oktober 2026 — Revisi Pendaftaran Pasien manual |
| Roadmap | [frontend-roadmap.md](../../../roadmap/frontend-roadmap.md#amandemen-8-oktober-2026--revisi-pendaftaran-pasien-manual) |
| Trace | `KSK-DEC-022`, `KSK-DEC-024`; sisi petugas RJ `RJ-DOC-REV-FE-017` |
| Contract version | Tanpa endpoint baru |
| Wewenang UI | Pesan pilih poliklinik merah; dokter dapat dicari dengan nama + info praktik; No. HP Pasien Baru 13 angka |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — 8 berkas |
| Task mode | `FRONTEND` (`KSK-DEC-024`) |
| Target tulis | `V2QuilvianSystemFrontendDev/src/**`; laporan dan baris status di blueprint ini |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `de323430` (`sukmagpV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `77caf434` (`sukmagp`) |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ `COMPLETE` — uji browser dengan akun perangkat Kiosk `11/11 PASS` (8 Okt 2026) |

## 1. Keadaan yang ditemukan di awal

- Layar *Layanan & Dokter* (Pasien Lama dan Baru) menampilkan "Silakan pilih poliklinik terlebih dahulu." dalam kotak abu-abu bergaris putus.
- Layar dokter **sudah** punya kolom *Cari dokter* ("Ketik nama dokter atau spesialis...") dan kartu dokter berisi jadwal praktik. Kebutuhan "dokter dapat dicari dengan nama dan info praktik" sudah terpenuhi oleh source, sehingga tidak diubah.
- No. HP/WhatsApp Pasien Baru dinormalkan oleh fungsi lokal Kiosk `normalizeIndonesianPhoneNumber` (salinan dari normalizer bersama, batas 15 digit bentuk `+62…`).

## 2. Proses bisnis dari sisi pengguna

1. Pasien sampai di *Layanan & Dokter*. Layar pertama adalah daftar poliklinik; selama poliklinik belum dipilih, catatan footer "Pilih poliklinik tujuan pasien terlebih dahulu." tampil merah. Pesan "Silakan pilih poliklinik terlebih dahulu." di area dokter (keadaan pinggir) juga merah. Setelah poliklinik disentuh, Kiosk pindah ke layar dokter dan catatan footer kembali berwarna normal.
2. Setelah poliklinik dipilih, pasien mencari dokter lewat kolom *Cari dokter*, lalu menyentuh jadwal pada kartu dokter (perilaku existing).
3. Pada form Pasien Baru, kolom *Nomor WhatsApp* (juga mengisi No. HP) berhenti di 13 angka lokal. Contoh: mengetik `0812345678901234` tersimpan `+62812345678901` (13 angka lokal `0812345678901`). Kontak darurat tetap 15 digit.

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-service.jsx` | Pesan pilih poliklinik memakai kelas `errorAlert` + `role="alert"` |
| `src/components/view/kiosk/registration/new-patient/kiosk-new-patient-step-service.jsx` | Sama, untuk Pasien Baru |
| `src/lib/helpers/kiosk/registration/kiosk-new-patient-registration.helpers.jsx` | Normalizer HP lokal diganti delegasi ke normalizer bersama `utils/shared/input-normalizer-utils`; `phoneNumber`/`whatsAppNumber` dibatasi `KIOSK_PATIENT_PHONE_MAX_DIGITS` |
| `src/lib/constants/kiosk/registration/kiosk-new-patient-registration.constants.js` | `KIOSK_PHONE_MAX_DIGITS = 15`, `KIOSK_PATIENT_PHONE_MAX_DIGITS = 14` |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-view.jsx` | Catatan footer layanan diberi kelas merah + `role="alert"` bila `noteTone === "danger"` |
| `src/components/view/kiosk/registration/new-patient/kiosk-new-patient-view.jsx` | Sama, untuk footer Pasien Baru |
| `src/style/kiosk/registration/kiosk-old-patient-view.module.css`, `kiosk-new-patient-view.module.css` | Varian `…FooterNoteDanger` memakai `var(--color-danger)` |

Kedua step layanan juga mengirim `noteTone: "danger"` pada konfigurasi footer layar poliklinik. Helper No. HP juga memperbaiki bug lama: mengetik `+62…` per karakter dulu menjadi `+626…`, karena `+6` dinormalkan menjadi `+626`.

### 3.3 Kepatuhan arsitektur frontend

**UI GATE: 2 elemen — REUSE 2, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0**

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Pesan merah pilih poliklinik | `.errorAlert` stylesheet Kiosk | Sudah dipakai untuk galat di layar yang sama | REUSE | Ganti kelas |
| Dokter dapat dicari + info praktik | Kolom *Cari dokter* + `DoctorCard` existing | `kiosk-*-step-service.jsx` | REUSE | Tanpa perubahan |

## 4. State yang ditangani di layar

Tidak berubah selain warna pesan pilih poliklinik. Memuat, kosong, dan gagal memakai pesan existing.

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru atau berubah.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 4 berkas | 0 error, 7 warning lama (`<img>`, `set-state-in-effect`, refs saat render) di baris yang tidak diubah | `PASS` | Keluaran perintah |
| `npm run build` | exit 0 | `PASS` | Log build scratchpad |
| Uji fungsi normalizer (delegasi yang sama dengan helper) | 5/5: HP pasien 16 digit → `+62812345678901`; 13 angka lokal utuh; kontak darurat tetap 15 digit; awalan `8…`/`62…` sama dengan perilaku lama; field non-HP tidak diubah | `PASS` | Skrip scratchpad (berkas helper memuat JSX sehingga logika delegasinya diuji lewat salinan satu baris) |
| Lint ulang 6 berkas view/helper | 0 error; jumlah warning sama dengan `HEAD` per berkas | `PASS` | `eslint --stdin` versi `HEAD` vs sekarang |
| `npm run build` (sesudah footer merah dan perbaikan `+6`) | exit 0 | `PASS` | Log build scratchpad |
| Uji browser Playwright, akun perangkat `kiosk-test` (kredensial dari pengguna, tidak dicatat), viewport 1080×1920 | 11/11 | `PASS` | Tabel di bawah + 5 screenshot scratchpad |

Lingkungan uji: dev server `localhost:3000` dan backend `localhost:7184`. Agent Plustek `127.0.0.1:9100` ditiru dengan OCR KTP samaran `9999000000000099`. **Seluruh request tulis ke API diblok**, kecuali `POST …/scan-result` yang dibalas sesi tiruan. Tercatat 0 request tulis, sehingga tidak ada data tersimpan.

| ID | Skenario | Hasil |
| --- | --- | --- |
| K1 | Pasien Baru: ketik `0812345678901234` → `+62812345678901` (13 angka lokal) | PASS |
| K2 | Ketik `+62 812-3456-7890` → `+6281234567890` (sebelum perbaikan `+6`: `+62628123456789`) | PASS |
| K3 | Kontak darurat `08123456789012345` → `+628123456789012` (tetap 15 digit) | PASS |
| K4 | Pasien Baru, layar poliklinik: catatan merah `rgb(180,35,24)` = `--color-danger`, `role="alert"` | PASS |
| K5–K7 | Setelah Poli Penyakit Dalam: catatan normal; kolom *Cari dokter*; kartu *dr. Bagus Purnama Sanjaya* dengan jam; kata tak cocok mengosongkan daftar | PASS |
| K8–K9 | Pasien Lama (RM `00-00-00-15`, Tunai): catatan merah sebelum poli, normal sesudahnya | PASS |
| K10 | Pasien Lama: kolom *Cari dokter* + nama dan jam praktik | PASS |
| K11 | Tidak ada request tulis yang lolos | PASS |

`AUTOMATED TEST: SKIPPED (opsional) — tidak ada test baru di repository.`

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Sebelum poliklinik dipilih, pesan tampil merah (Pasien Lama dan Baru) | Terpenuhi | K4, K8, K9 |
| 2. Dokter dapat dicari; tiap pilihan menampilkan nama dan info praktik | Terpenuhi (source existing) | K5–K7, K10 |
| 3. Input No. HP Pasien Baru berhenti di 13 angka | Terpenuhi | K1–K3, uji fungsi 5/5 |
| 4. Base component dan util normalizer yang sudah ada dipakai | Terpenuhi | Normalizer lokal diganti normalizer bersama |
| 5. Lint tanpa error baru, build `PASS` | Terpenuhi | Bagian 6 |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Normalizer bersama juga merapikan awalan `620…` menjadi `62…`; perilaku lain sama |
| Masalah yang diketahui | Perilaku lama, tidak diubah: pada Pasien Baru, kata kunci dokter yang tidak cocok menampilkan "Belum ada doctor schedule aktif…", bukan "Tidak ada dokter yang cocok…" seperti Pasien Lama |
| Dependency backend | Tidak ada |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
