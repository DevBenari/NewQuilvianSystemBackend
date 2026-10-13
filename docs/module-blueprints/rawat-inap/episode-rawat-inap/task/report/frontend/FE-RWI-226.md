# Laporan Perubahan Frontend — `FE-RWI-226`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-226` |
| Judul | Penyelarasan Cetak Persetujuan Pasien Rawat Inap & General Consent Workspace PPRI (Murni Siap Cetak Tanpa Input Ulang) |
| Slice | Slice 2 (UI Pendaftaran Admisi Pasien Baru & Dokter); Alur Admisi Pendaftaran |
| Roadmap | [`../../../roadmap/frontend-roadmap-admisi-pendaftaran.md`](../../../roadmap/frontend-roadmap-admisi-pendaftaran.md) — kartu `FE-RWI-226` |
| Trace | `FR-RI-183`; `RWI-DEC-272`; `RWI-AC-395` |
| Contract version | `0.11.0` **`approved`** (10 Oktober 2026) |
| Dependency | `FE-RWI-225` ✅, `BE-RWI-205` ✅ |
| Klasifikasi | `MEDIUM` — Render citra PNG tanda tangan digital, penyelarasan format lembar cetak admisi dan workspace PPRI print-ready |
| Task mode | `FRONTEND` |
| Target tulis | `inpatient-consent-form.jsx`, `inpatient-consent-print.module.css`, `inpatient-consent-utils.jsx`, `inpatient-admission-print-steps.jsx`, `use-admission-general-consent.js`, `general-consent-section.jsx`, `general-consent-form-print.jsx`, `inpatient-admission-print-utils.js`, `inpatient-admission-consent.test.mjs` |
| Tanggal | 10 Oktober 2026 |
| Status | ✅ Selesai. Kriteria penerimaan `RWI-AC-395` terimplementasi bersih dan tervalidasi. |

---

## 1. Kebutuhan Bisnis

Berdasarkan revisi Tim Analisis Bisnis (Mba Ilma):
1. **Lembar Cetak Persetujuan Admisi (`RWI-AC-395`):**
   - Pada langkah cetak lembar persetujuan admisi (Langkah 11 PB / Langkah 10 PL), dokumen otomatis menampilkan tanda tangan digital pasien / wali yang baru saja dibubuhkan pada langkah form persetujuan sebelumnya.
   - Footer tanda tangan merender citra gambar tanda tangan digital (`<img src={signatureImageBase64} />`) di atas kurung nama penanda tangan.
   - Nama penanda tangan dicetak jelas di dalam kurung: `( ${signerName} )`, disertai hubungan penanda tangan dan keterangan penandatanganan digital.
   - Jika dokumen belum bertanda tangan digital (alur cetak manual lama), tetap menampilkan garis bawah kosong `( __________________________ )` sebagai fallback.
2. **Workspace PPRI Murni Siap Cetak (*Print-Ready*, Tanpa Re-Input, `RWI-AC-395`):**
   - Pada Workspace PPRI menu General Consent:
     - Membaca data persetujuan & TTD digital yang telah dibuat saat admisi melalui `fetchAdmissionConsent(episodeId)`.
     - Ketika episode telah memiliki tanda tangan digital, panel penanda tangan beralih ke status **murni siap cetak (*read-only / print-ready*)**.
     - Form input nama/alamat manual dan radio group disembunyikan/digantikan oleh kartu ringkasan identitas penanda tangan yang telah terverifikasi saat admisi.
     - Tidak ada kanvas tanda tangan ganda dan tidak ada tombol simpan yang membingungkan petugas PPRI; petugas hanya perlu menekan tombol cetak (`FaPrint`).

---

## 2. Rincian Perubahan Source Code

### 2.1 `inpatient-consent-form.jsx` & `inpatient-consent-print.module.css`
- Memperbarui blok tanda tangan pasien / wali:
  - Jika `consentDocument.signatureImageBase64` tersedia, merender `<img src={signatureImageBase64} className={consentStyles.signatureImage} />`.
  - Merender nama terang penanda tangan di dalam kurung: `( {consentDocument.signerName} )`.
  - Merender metadata hubungan dan waktu/status tanda tangan digital.
- Menambahkan kelas CSS `.signatureImage`, `.signatureMeta`, `.signerRelationshipText`, dan `.signedAtText` pada modul print CSS.

### 2.2 `inpatient-consent-utils.jsx` & `inpatient-admission-print-utils.js`
- Memperbarui fungsi `buildInpatientConsentDocument` untuk menerima parameter `consent` dan menyematkan atribut `signatureImageBase64`, `signerName`, `signerType`, `signerRelationship`, `signedAt`, dan `isSigned`.
- Memperbarui `buildConsentDocumentFromPrintData` untuk meneruskan objek `consent` ke pembangun dokumen.

### 2.3 `inpatient-admission-print-steps.jsx`
- Memperbarui komponen `InpatientAdmissionConsentStep`:
  - Menerima prop `consentData`.
  - Menambahkan pemanggilan fallback `fetchAdmissionConsent(episodeId)` saat komponen dimuat ulang (refresh browser di langkah cetak).
  - Meneruskan data tanda tangan ke `buildInpatientConsentDocument`.

### 2.4 `use-admission-general-consent.js` & `general-consent-section.jsx`
- Pada hook `useAdmissionGeneralConsent`:
  - Menambahkan pembacaan `fetchAdmissionConsent(episodeId)`.
  - Mengembalikan `signedConsent` dan penanda `isConsentSigned`.
  - Mengintegrasikan data penanda tangan digital ke objek `signer` dan `consentDocument`.
- Pada komponen `GeneralConsentSection`:
  - Menampilkan badge status `"Bertanda Tangan Digital"` dan alert keberhasilan penandatanganan saat admisi.
  - Mengganti form radio/manual penanda tangan dengan kartu read-only rincian penanda tangan admisi ketika `isConsentSigned === true`.

### 2.5 `general-consent-form-print.jsx`
- Menampilkan citra tanda tangan digital pada lembar cetak V1 formulir General Consent jika `signer.signatureImageBase64` tersedia.

---

## 3. Bukti Verifikasi dan Acceptance Criteria

| ID Kriteria | Kriteria Penerimaan | Status | Bukti Implementasi & Hasil Pengujian |
|---|---|:---:|---|
| `RWI-AC-395` | Lembar persetujuan admisi menampilkan TTD digital; Menu General Consent PPRI berstatus murni print-ready tanpa input ulang form | ✅ LULUS | 1. `InpatientConsentForm` merender gambar citra TTD digital dan nama penanda tangan di footer.<br>2. `InpatientAdmissionConsentStep` otomatis mengikat citra TTD dari admisi.<br>3. `GeneralConsentSection` di PPRI mendeteksi tanda tangan admisi dan menampilkan antarmuka murni siap cetak (read-only) tanpa form input ulang.<br>4. Pengujian unit `tests/unit/inpatient-admission-consent.test.mjs` dan `tests/unit/inpatient-admission-workspace.test.mjs` lulus 100% (pass 20, fail 0). |

---

## 4. Penutupan Modul & Selesai Seluruh Task
- Seluruh 5 task perubahan bisnis dari Mba Ilma telah tuntas dikerjakan secara menyeluruh:
  1. `BE-RWI-204` ✅: Validasi No. HP max 13 digit numerik & auto-bind unit default ranap.
  2. `BE-RWI-205` ✅: Endpoint simpan & ambil formulir persetujuan rawat inap & TTD digital dengan penegakan invariant bayi baru lahir.
  3. `FE-RWI-222` ✅: Pembatasan input No. HP kontak darurat max 13 digit & unit tujuan dokter disembunyikan.
  4. `FE-RWI-223` ✅: Stepper PB 12 langkah dengan Langkah 1 Jenis Kunjungan (Umum & Rujukan 5 isian teks) dan filter 3 kategori pasien.
  5. `FE-RWI-224` ✅: Stepper PL 10 langkah dengan Langkah 1 Split Screen (Pencarian kanan, Verifikasi kiri) dan Langkah 2 Jenis Kunjungan & Kategori.
  6. `FE-RWI-225` ✅: Dedicated Step Form Persetujuan Rawat Inap & Kanvas TTD Digital interaktif (`react-signature-canvas`) dengan penegakan invariant bayi baru lahir.
  7. `FE-RWI-226` ✅: Lembar cetak persetujuan admisi terisi TTD digital & General Consent PPRI berstatus murni *print-ready* tanpa input ulang.
