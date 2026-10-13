# Laporan Perubahan Frontend — `FE-KSK-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-014` |
| Judul | Step Data Rujukan Kiosk (Pasien Lama dan Baru) |
| Roadmap | [frontend-roadmap.md](../../../roadmap/frontend-roadmap.md) — *Amandemen 8 Oktober 2026 (B)* |
| Trace | `RJ-DOC-DEC-072`, `076`, `077`, `081`, `082`; `KSK-DEC-025`; `03` *PM-FE.4*; `RJ-AC-PM-09` |
| Contract version | `RJ-DOC-REFERRAL-001@1.0.0` (**approved**) |
| Dependency | `RJ-DOC-REV-BE-019` ✅ |
| Task mode | `TASK MODE: FRONTEND` (`RJ-DOC-DEC-083`) |
| Commit frontend | `de323430` (`sukmagpV2`), belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 9 Oktober 2026 |
| Status | ✅ `COMPLETE` |

## 1. Keadaan awal

| Alur | Rujukan sebelumnya |
| --- | --- |
| Pasien Lama | Tidak ada isian. Kunjungan hanya `isReferral` + `referralNumber` kosong |
| Pasien Baru | Blok teks bebas (asal rujukan, jenis, No., tanggal, dokter, diagnosa teks) di dalam Layanan & Dokter. Asal rujukan hanya masuk ke `notes` kunjungan |

## 2. Proses dari sisi pengguna

1. **Jenis Kunjungan** tetap step sendiri (`KSK-DEC-025`). Bila **Pasien Rujukan**, bar step bertambah satu: **Data Rujukan** sesudah Pembayaran dan sebelum Layanan & Dokter. Pasien Umum dan jalur Laboratorium tidak berubah.
2. **Data Rujukan** (komponen bersama Pasien Lama dan Baru):
   - No. Rujukan (wajib).
   - Tanggal & jam rujukan, terisi waktu saat step dibuka dan tidak boleh di masa depan.
   - **Fasilitas Perujuk** (wajib): daftar dapat dicari dari `referral-institutions/kiosk/options`, badge *Mitra*, dan alert **"Fasilitas Perujuk Bermitra dengan Rumah Sakit"** bila mitra.
   - Dokter Perujuk (opsional): pilihan disaring menurut fasilitas.
   - **Scan Surat Rujukan** lewat scanner Kiosk (`/scanner/scan`), dengan pratinjau tiap halaman, *Tambah Halaman*, *Scan Ulang*, dan *Hapus Hasil Scan*. Scan tidak wajib; bila scanner gagal, pesan tampil dan pasien tetap dapat lanjut.
   - Diagnosa dan alasan **tidak ditanyakan** (`RJ-DOC-DEC-076`).
3. Poli yang dipilih di Layanan & Dokter menjadi unit tujuan.
4. **Konfirmasi** (Pasien Lama) menampilkan Fasilitas Perujuk, Dokter Perujuk, dan jumlah halaman surat.
5. Kunjungan dikirim dengan `referralNumber`, `referralInstitutionId`, `referralDoctorId`, dan blok `referral` (`targetUnitType` 1, tanpa diagnosa). Sesudah kunjungan terbentuk, halaman scan diunggah lewat `POST /patient-encounters/kiosk/{id}/referral/documents`.
6. Bila surat tidak di-scan atau unggah gagal, **tiket tetap terbit** dan menampilkan **"Tunjukkan surat rujukan ke petugas"**. Kunjungan tampil "Rujukan belum lengkap" di Daftar RJ petugas (`RJ-AC-PM-09`).

## 3. Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/kiosk/registration/kiosk-referral.constants.js` | **Baru** — URL `KioskRead`, bentuk form bersama, batas, teks |
| `src/utils/kiosk/registration/kiosk-referral-utils.jsx` | **Baru** — validasi, ruas kunjungan rujukan, `datetime-local` → ISO, data URL → File |
| `src/lib/services/kiosk/registration/kiosk-referral.service.js` | **Baru** — opsi fasilitas/dokter Kiosk, scan surat, unggah halaman |
| `src/components/view/kiosk/registration/shared/kiosk-referral-step.jsx` | **Baru** — step Data Rujukan |
| `src/components/view/kiosk/registration/shared/kiosk-referral-ticket-notice.jsx` | **Baru** — catatan tiket |
| `src/style/kiosk/registration/kiosk-referral-step.module.css` | **Baru** — token global saja |
| `src/lib/hooks/kiosk/registration/use-kiosk-old-patient-registration.jsx` | Step `referral`, bar step rujukan, transisi Pembayaran → Data Rujukan → Layanan, kembali dari Layanan, `handleReferralChange` menerima patch |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-view.jsx` | Render step + footer |
| `src/lib/services/kiosk/registration/kiosk-old-patient-registration.service.js` | Payload kunjungan memakai `buildKioskReferralEncounterFields` |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-confirm.jsx` | Unggah halaman sesudah kunjungan dibuat; ringkasan rujukan |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-ticket.jsx` | Catatan surat di akhir kartu tiket |
| `src/lib/hooks/kiosk/registration/use-kiosk-new-patient-registration.jsx` | Step `referral`, transisi, unggah sesudah submit, `referralLetterNotice` |
| `src/components/view/kiosk/registration/new-patient/kiosk-new-patient-view.jsx` | Bar step dinamis, render step + footer, catatan pada layar Cetak |
| `src/lib/helpers/kiosk/registration/kiosk-new-patient-submit.helpers.jsx` | Draft kunjungan memakai ruas rujukan baru; `notes` memakai nama fasilitas |
| `src/components/view/kiosk/registration/new-patient/kiosk-new-patient-step-service.jsx` | Blok rujukan teks bebas dan validasi "Asal rujukan" dihapus (digantikan step baru) |

**UI GATE: 6 elemen — REUSE 1, COMPOSE 5, NEW 0**

| Kebutuhan UI | Keputusan | Status |
| --- | --- | --- |
| Bar step | Bar step Kiosk yang ada (item ditambah) | REUSE |
| Input sentuh, pencarian fasilitas, chip dokter, panel scan + pratinjau, catatan tiket | Komponen Kiosk bertoken mengikuti pola layar Kiosk | COMPOSE |

Pilihan komposisi:

1. **(Rekomendasi, dipakai)** Satu komponen step Kiosk bersama untuk Pasien Lama dan Baru, bergaya token global. Konsisten dan sekali kerja.
2. Pakai komponen admin (`FilterSelect`, `EmergencySelectField`). Ukurannya tidak cocok untuk layar sentuh Kiosk.

## 4. Verifikasi

| Skenario / perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` seluruh berkas Kiosk yang berubah dan baru | 0 error. Tidak ada warning baru dibanding `HEAD`; satu berkas berkurang (2 → 1) | `PASS` |
| `npm run build` | exit 0 (diulang sesudah perbaikan tata letak tiket) | `PASS` |
| Uji browser akun perangkat Kiosk (build 3100 → backend uji 7185, agent tiruan) | **14/14 PASS** | `PASS` |

| ID | Skenario | Hasil |
| --- | --- | --- |
| R1 | Pasien Baru Umum: bar step 6 tanpa Data Rujukan | PASS |
| R2 | Pasien Baru Rujukan: bar step 7, Data Rujukan sesudah Pembayaran | PASS |
| R3 | Tanggal rujukan bawaan = sekarang | PASS |
| R4 | Tanpa No. rujukan/fasilitas → pesan merah, tidak lanjut | PASS |
| R5 | `PMTEST-KSS` → alert mitra; dokter tersaring (Rina, tanpa Budi) | PASS |
| R6 | Lanjut ke Layanan & Dokter tanpa blok rujukan lama | PASS |
| R7 | Pasien Lama Umum tanpa step Data Rujukan | PASS |
| R8 | Pasien Lama Rujukan → step Data Rujukan sesudah Pembayaran | PASS |
| R9 | Scan surat → pratinjau 2 halaman | PASS |
| R10 | Konfirmasi memuat fasilitas, dokter, "2 halaman" | PASS |
| R11 | Kunjungan terbentuk dengan blok `referral` tanpa diagnosa; **2 surat tersimpan** lewat jalur Kiosk (`captureSource` Kiosk); tiket tanpa catatan | PASS |
| R12 | `RJ-AC-PM-09`: rincian `isComplete = false`, `missingFields` `diagnosis`, `referralReason`; tampil di filter "Belum lengkap" | PASS |
| R13 | Scanner gagal → pesan, tetap lanjut; tiket "Tunjukkan surat rujukan ke petugas" | PASS |
| R14 | Jalur Laboratorium Kiosk tanpa step Data Rujukan | PASS |

Skenario Pasien Baru memblok request tulis, sehingga tidak ada pasien baru yang terbentuk. Kunjungan uji Pasien Lama dibatalkan lewat akun admin (`PATCH …/admin/{id}/cancel`, `200`). Kredensial akun Kiosk lewat variabel lingkungan, tidak ditulis ke berkas.

`AUTOMATED TEST: SKIPPED (opsional) — tidak ada test baru di repository.`

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pasien Umum tidak melihat step baru | Terpenuhi | R1, R7 |
| 2. Pasien Rujukan: bar step +1, isian tersimpan, alert mitra | Terpenuhi | R2, R5, R8, R10, R11 |
| 3. Kunjungan terbentuk, surat 2 halaman tersimpan; `RJ-AC-PM-09` | Terpenuhi | R11, R12 |
| 4. Scanner gagal → dapat dilewati; tiket "Tunjukkan surat rujukan ke petugas" | Terpenuhi | R13 |
| 5. Jalur Laboratorium Kiosk tidak berubah | Terpenuhi | R14 |
| 6. Lint, build, uji browser akun Kiosk | Terpenuhi | Bagian 4 |

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Belum diuji sampai submit | Pasien Baru Rujukan sampai submit (submit membuat master pasien baru). Payload dan unggahnya memakai util yang sama dengan Pasien Lama (R11) |
| Asumsi agent | Profil scan surat `scanType: "DOCUMENT"`, `documentType: "REFERRAL_LETTER"` (200 dpi). Profil sebenarnya perlu dikonfirmasi tim agent; tanpa dukungan profil, agent memakai bawaan |
| Coverage gap | Scanner Kiosk fisik — UAT pemilik |
| Perubahan sampingan | Blok rujukan teks bebas lama di Layanan Pasien Baru dihapus karena digantikan step baru (`03` PM-FE.4) |
