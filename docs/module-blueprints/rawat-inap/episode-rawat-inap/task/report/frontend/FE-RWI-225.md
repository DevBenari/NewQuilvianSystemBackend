# Laporan Perubahan Frontend — `FE-RWI-225`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-225` |
| Judul | Langkah Dedicated Form Persetujuan & Kanvas TTD Digital Interaktif (`react-signature-canvas`) Serta Penegakan Invariant Bayi Baru Lahir |
| Slice | Slice 2 (UI Pendaftaran Admisi Pasien Baru & Dokter); Alur Admisi Pendaftaran |
| Roadmap | [`../../../roadmap/frontend-roadmap-admisi-pendaftaran.md`](../../../roadmap/frontend-roadmap-admisi-pendaftaran.md) — kartu `FE-RWI-225` |
| Trace | `FR-RI-183`; `RWI-DEC-272`; `RWI-DEC-273`; `RWI-AC-393`; `RWI-AC-394`; `VAL-ADM-04`; `VAL-ADM-05` |
| Contract version | `0.11.0` **`approved`** (10 Oktober 2026) |
| Dependency | `FE-RWI-224` ✅, `BE-RWI-205` ✅ |
| Klasifikasi | `HIGH` — Komponen kanvas tanda tangan digital interaktif, 12 butir klausul persetujuan, penegakan hukum medis invariant bayi baru lahir |
| Task mode | `FRONTEND` |
| Target tulis | `inpatient-admission-consent-form-step.jsx` (baru), `inpatient-admission-view.jsx`, `inpatient-admission-workspace.service.js`, `inpatient-admission-flow-constants.jsx`, `inpatient-admission-consent.test.mjs` (baru) |
| Tanggal | 10 Oktober 2026 |
| Status | ✅ Selesai. Kriteria penerimaan `RWI-AC-393` dan `RWI-AC-394` terimplementasi bersih dan tervalidasi. |

---

## 1. Kebutuhan Bisnis

Berdasarkan revisi Tim Analisis Bisnis (Mba Ilma) pada admisi pendaftaran rawat inap:
1. **Langkah Dedicated Form Persetujuan (`RWI-DEC-272`, `RWI-AC-393`):**
   - Sebelum layar cetak lembar persetujuan ditampilkan, petugas dan pasien/keluarga wajib melalui langkah mandiri pengisian persetujuan rawat inap.
   - Menyajikan 12 butir klausul persetujuan umum rawat inap (*General Consent*) bilingual dengan tombol pintas "Pilih Semua (12 Butir)". Seluruh 12 klausul wajib disetujui (`VAL-ADM-04`).
   - Menyediakan pilihan subjek penanda tangan: Pasien Sendiri (`Self`), Orang Tua (`Parent`), Wali yang Sah (`Guardian`), Suami / Istri (`Spouse`), atau Keluarga Lainnya (`Family`).
   - Isian identitas penanda tangan: Nama Lengkap (min 3 karakter), Nomor Identitas (NIK/Paspor), Hubungan dengan Pasien, dan Nomor Telepon (maks 13 digit numerik). Pilihan "Pasien Sendiri" meng-autofill data pasien secara otomatis.
2. **Kanvas Tanda Tangan Digital (`react-signature-canvas`, `RWI-AC-393`):**
   - Area kanvas interaktif dengan latar belakang bersih, placeholder instruksi `"Bubuhkan tanda tangan pasien / wali di dalam kotak ini"`.
   - Tombol "Bersihkan Tanda Tangan" untuk mereset goresan.
   - Tombol "Kunci Tanda Tangan" untuk mengonversi citra goresan ke Data URL PNG Base64 dan mengunci kanvas agar tidak tercoret kembali.
   - Tombol "Buka Kunci Tanda Tangan" jika ingin merevisi tanda tangan.
3. **Penegakan Invariant Bayi Baru Lahir (`INV-ADM-01`, `VAL-ADM-05`, `RWI-DEC-273`, `RWI-AC-394`):**
   - Jika pasien berkategori Bayi Baru Lahir (`newborn`), opsi radio "Pasien Sendiri" dinonaktifkan (`disabled = true`).
   - Radio otomatis dikunci ke "Orang Tua" atau "Wali yang Sah".
   - Tampil banner peringatan hukum medis: *"Pasien terdaftar sebagai Bayi Baru Lahir. Penandatanganan persetujuan rawat inap wajib dilakukan oleh Orang Tua atau Wali yang sah. Pilihan 'Pasien Sendiri' dinonaktifkan."*
4. **Pengiriman Data Backend (`POST /api/v1/inpatient-admissions/{episodeId}/consent`):**
   - Menghubungkan tombol "Simpan & Lanjut ke Cetak" dengan endpoint `saveAdmissionConsent` (`BE-RWI-205`), dan meneruskan alur ke langkah cetak pratinjau setelah berhasil.

---

## 2. Rincian Perubahan Source Code

### 2.1 `inpatient-admission-consent-form-step.jsx` (Komponen Baru)
- Mengimplementasikan seluruh form persetujuan interaktif:
  - Form data penanggung jawab dengan autofill pasien sendiri.
  - Penegakan invariant bayi baru lahir: disable radio `Self`, default `Parent`, dan banner peringatan hukum.
  - Kartu daftar 12 klausul persetujuan umum dengan seleksi terpadu dan validasi kelengkapan.
  - Kanvas `react-signature-canvas` dengan ref, autofit canvas width/height, clear, lock, unlock, dan penangkapan Data URL PNG Base64.
  - Eksekusi penyimpanan ke backend lewat `saveAdmissionConsent(episodeId, payload)` dengan error handling dan loading state.

### 2.2 `inpatient-admission-workspace.service.js`
- Menambahkan fungsi API service:
  - `saveAdmissionConsent(episodeId, payload, config)`: Memanggil `POST /v1/health-services/inpatient-management/episodes/{episodeId}/consent`.
  - `fetchAdmissionConsent(episodeId, config)`: Memanggil `GET /v1/health-services/inpatient-management/episodes/{episodeId}/consent`.

### 2.3 `inpatient-admission-view.jsx`
- Mengimpor `InpatientAdmissionConsentFormStep`.
- Menambahkan state `consentData` dan callback `onConsentSaved` pada level view admisi.
- Menghubungkan rendering langkah `consent-form` ke `InpatientAdmissionConsentFormStep`.

### 2.4 `tests/unit/inpatient-admission-consent.test.mjs` (Pengujian Unit Baru)
- Memverifikasi keberadaan langkah `consent-form` pada stepper PB (langkah 10) dan PL (langkah 9).
- Memverifikasi kelengkapan dan keutuhan 12 butir klausul `INPATIENT_MMC_CONSENT_ITEMS`.
- Memverifikasi ekspor fungsi service `saveAdmissionConsent` dan `fetchAdmissionConsent`.

---

## 3. Bukti Verifikasi dan Acceptance Criteria

| ID Kriteria | Kriteria Penerimaan | Status | Bukti Implementasi & Hasil Pengujian |
|---|---|:---:|---|
| `RWI-AC-393` | Form isian Surat Persetujuan Rawat Inap & kanvas tanda tangan digital pasien/penanggung jawab langsung di admisi | ✅ LULUS | Komponen `InpatientAdmissionConsentFormStep` menyediakan isian penanda tangan, 12 klausul bilingual, kanvas `react-signature-canvas` dengan fitur kunci/bersihkan tanda tangan, dan pengiriman payload ke endpoint backend `POST .../consent`. |
| `RWI-AC-394` | Invariant penanda tangan Bayi Baru Lahir: dilarang diri sendiri, wajib orang tua atau wali sah | ✅ LULUS | Jika `patientType === "newborn"`, radio opsi "Pasien Sendiri" berstatus `disabled = true`, pilihan diarahkan ke "Orang Tua / Wali", dan banner peringatan hukum medis tampil secara tegas. |

---

## 4. Handoff ke Task Berikutnya
- Task frontend `FE-RWI-225` telah selesai penuh.
- Siap dilanjutkan ke task **`FE-RWI-226`**:
  - Penyelarasan lembar cetak admisi agar merender citra TTD digital pasien/wali.
  - Penyesuaian tab General Consent Workspace PPRI berstatus murni siap cetak (*print-ready*) tanpa form input ulang.
