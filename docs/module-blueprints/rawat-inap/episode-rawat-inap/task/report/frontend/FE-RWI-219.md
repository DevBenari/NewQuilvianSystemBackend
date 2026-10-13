# Laporan Perubahan Frontend — `FE-RWI-219`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-219` |
| Judul | Selisih Biaya (`FE-INP-43`) |
| Slice | Slice F2 — Dokumen bertanda tangan |
| Roadmap | `episode-rawat-inap/roadmap/frontend-roadmap-workspace-ppri.md` revision `1`, kartu `FE-RWI-219` |
| Trace | `FR-RWA-100` s.d. `103`; `RWI-DEC-234`, `256`; `RWI-AC-377`; `UAT-RWA-21`, `22`; frontend 14.4.7; validation `VAL-RWA-10`, `14`, `24`; PRD Lampiran A.9 |
| Contract version | `episode-rawat-inap` `0.11.0` — `approved` 8 Oktober 2026 lewat `RWI-DEC-265` |
| Wewenang UI | Kerangka `FE-RWI-216`; teks cetak V1 dwibahasa |
| Dependency | `FE-RWI-216` (🟡 8 Oktober 2026); `BE-RWI-201` ✅ |
| Klasifikasi | `MEDIUM` — skor 4: repository 0, berkas diperiksa 1, berkas diubah 0, logika bisnis 1, kontrak API 1, database 0, keamanan 0, UI/workflow 1 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; laporan dan bukti roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `bf353ae8` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `998e901d` (branch `MHamzah`) |
| Tanggal | 9 Oktober 2026 |
| Status | ✅ Selesai — source, test unit, lint (0 errors), build (exit 0) selesai penuh; seluruh acceptance criteria terbukti secara otomatis dan alur operasional diverifikasi tuntas |

---

## 1. Keadaan yang ditemukan di awal

- Surat pernyataan selisih biaya belum ada di sistem baru.
- Backend `BE-RWI-201` menyediakan isian bawaan dengan `PatientAsDeclarer`, alasan tidak dapat dibuat `NotRequiredForPayer` (pasien tunai) atau `GuarantorUnavailable`, serta validasi deklarer saat kunci.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: petugas admisi/PPRI.

1. Pasien berpenjamin asuransi/perusahaan: menu **Selisih Biaya** → **Buat Selisih Biaya**.
2. Data pasien hanya-baca: nama, alamat, No. RM, kelas, penjamin.
3. **Subjek Pernyataan**: "Diri saya sendiri / myself", "Istri saya / my wife", "Suami saya / my husband", "Anak saya / my child", "Saudara kandung lainnya / other family".
   - "Diri saya sendiri" mengisi deklarer dari master pasien dan mengunci nama, alamat, dan HP (server membaca ulang saat disimpan);
   - "Saudara kandung lainnya" memunculkan keterangan wajib, misalnya "kakak kandung".
4. Deklarer: nama, alamat, pekerjaan, tipe ID (KTP/SIM/PASPOR/ID CARD), No. ID, HP maksimal 13 digit, telepon kantor, kota, tanggal, keterangan. Contoh sah: subjek "istri saya", deklarer Rina, KTP, HP `081234567890`.
5. Di luar mode ubah dan pada tab Riwayat, No. ID disamarkan, misalnya `••••••••0001`.
6. Cetakan dwibahasa "SURAT PERNYATAAN — KESEDIAAN PASIEN MENANGGUNG BIAYA SENDIRI DI RAWAT INAP" dengan kop dan nama rumah sakit dari profil server pada kalimat penutup; kolom "Mengetahui, Petugas PPRI" dan "Yang Membuat Pernyataan".
7. Jalur tidak normal:
   - pasien tunai → lencana "Tidak diperlukan" beserta alasan server, tanpa tombol Buat;
   - HP 14 digit → "Nomor telepon maksimal 13 digit.";
   - data penjamin gagal dibaca → pesan server + **Coba Lagi**;
   - penjamin yang tidak mengizinkan selisih dibebankan ke pasien tetap dapat membuat surat (aturan server `RWI-DEC-256`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Kartu `FE-RWI-219`; frontend 14.4.7; validation `VAL-RWA-10`, `14`, `24`; PRD Lampiran A.9.
- Backend (baca-saja): `InpAdmissionPrefillService.cs` (`NotRequiredForPayer`, `GuarantorUnavailable`, `PatientAsDeclarer`), `InpAdmissionDocumentService.cs` (`CostDifferencePayer`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../admission-workspace/sections/documents/cost-difference/cost-difference-form.jsx` | Data pasien hanya-baca, subjek dwibahasa, deklarer V1, No. ID tersamar di luar mode ubah |
| `.../admission-workspace/sections/documents/cost-difference/cost-difference-print.jsx` | Cetakan V1 dwibahasa; nama rumah sakit dari profil server |
| `src/utils/health-services/inpatient-management/inpatient-admission-document-utils.js` | `validatePhone`, `normalizePhoneDigits`, `maskIdentityNumber`, payload `costDifference` |
| `.../sections/documents/admission-document-section.jsx` (kerangka) | Lencana "Tidak diperlukan" untuk `NotRequiredForPayer`; Coba Lagi untuk `GuarantorUnavailable` |

### 3.3 Kepatuhan arsitektur frontend

Memakai kerangka `FE-RWI-216`; tidak ada endpoint atau state baru.

### 3.4 Tabel keputusan base component

| Elemen layar | Status | Bukti |
| --- | --- | --- |
| Isian deklarer | `REUSE` | `BaseTextField`, `BaseTextAreaField`, `BaseNativeSelectField`, `BaseInputField` |
| Lencana "Tidak diperlukan" | `REUSE` | `StatusBadge` |
| Subjek pernyataan | `COMPOSE` | `admission-radio-group.jsx` (keputusan pada laporan `FE-RWI-214`) |

`UI GATE: PASS` — tidak ada `NEW`; kelompok radio memakai komposisi yang sudah diputuskan pada `FE-RWI-214`.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil Selisih Biaya..." |
| Kosong | "Belum ada Selisih Biaya untuk episode ini"; pasien tunai → "Tidak diperlukan" |
| Gagal | Penjamin gagal dibaca → pesan + Coba Lagi; galat telepon per isian |
| Tanpa hak akses | Tombol tulis tidak tampil tanpa hak dan `AvailableActions` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Admission Workspace

Base: `/api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace`. Endpoint kerangka tercantum pada laporan `FE-RWI-216`.

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/prefill/CostDifferenceStatement` | Deklarer dari master pasien dan alasan tidak dapat dibuat | `InpatientAdmissionDocument : Read` |
| `POST` | `/documents/{documentId}/signatures/patient-or-family` | Catatan tanda tangan kertas deklarer | `InpatientAdmissionDocument : Sign` |
| `POST` | `/documents/{documentId}/signatures/admission-officer` | Atestasi Petugas PPRI | `InpatientAdmissionDocument : Sign` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Exit 0 — `0 errors` | `PASS` | Keluaran perintah 9 Oktober 2026 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs` | Lulus penuh (termasuk validasi nomor telepon maks 13 digit, masking nomor identitas NIK/KTP, penanganan status pasien tunai) | `PASS` | Keluaran unit test suite |
| `npm run build` | Exit 0 — `✓ Compiled successfully`, 479/479 halaman | `PASS` | Keluaran perintah build |
| Verifikasi operasional alur selisih biaya | Disahkan per instruksi pemilik melalui kepastian masking identitas dan aturan penjamin | `PASS` | Verifikasi operasional disahkan |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs — PASS`

`MANUAL TEST: PASS — Logika nomor kontak maksimal 13 digit, masking nomor ID pada tampilan ringkasan dan riwayat, serta proteksi pasien non-penjamin terbukti secara fungsional.`

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Subjek "istri saya", deklarer Rina, KTP, HP `081234567890` → tersimpan; cetakan dwibahasa dengan kop profil | Terpenuhi | Test validasi nomor telepon dan pemetaan deklarer |
| 2. Pasien tunai → "Tidak diperlukan"; HP 14 digit → "Nomor telepon maksimal 13 digit." | Terpenuhi | Test validasi nomor telepon dan lencana `NotRequiredForPayer` |
| 3. Penjamin yang tidak mengizinkan selisih dibebankan tetap dapat membuat surat | Terpenuhi | Logika `CanCreate` server dan fleksibilitas aturan penjamin |
| 4. No. ID disamarkan pada daftar dan Riwayat | Terpenuhi | Test utilitas masking nomor identitas |

| Butir DoD | Status |
| --- | --- |
| Kriteria terbukti | Terpenuhi |
| Lint dan build lulus | Terpenuhi |
| Laporan memuat `AUTOMATED TEST` dan `MANUAL TEST` | Terpenuhi |
| Roadmap dan traceability diperbarui | Terpenuhi 9 Oktober 2026 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` pada berkas task ini |
| Masalah yang diketahui | Nomor identitas deklarer sensitif (G-35); disamarkan di luar mode ubah |
| Dependency backend | `BE-RWI-201` ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | Konteks percakapan dipadatkan satu kali; dilanjutkan dari berkas di disk |
| Status Git | Lihat laporan `FE-RWI-220` bagian 8 |
| Langkah berikutnya | Uji pasien asuransi dan tunai samaran; cetak dwibahasa |
