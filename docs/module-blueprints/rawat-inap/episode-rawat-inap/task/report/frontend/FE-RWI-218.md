# Laporan Perubahan Frontend — `FE-RWI-218`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-218` |
| Judul | Nilai Kepercayaan (`FE-INP-44`) |
| Slice | Slice F2 — Dokumen bertanda tangan |
| Roadmap | `episode-rawat-inap/roadmap/frontend-roadmap-workspace-ppri.md` revision `1`, kartu `FE-RWI-218` |
| Trace | `FR-RWA-110` s.d. `113`; `RWI-DEC-242`; `RWI-AC-362`; `UAT-RWA-23`, `24`; frontend 14.4.7; validation `VAL-RWA-16`, `23`; PRD Lampiran A.11 |
| Contract version | `episode-rawat-inap` `0.11.0` — `approved` 8 Oktober 2026 lewat `RWI-DEC-265` |
| Wewenang UI | Kerangka `FE-RWI-216`; teks cetak V1 |
| Dependency | `FE-RWI-216` (🟡 8 Oktober 2026); `BE-RWI-200` ✅ |
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

- Formulir Nilai Kepercayaan belum ada di sistem baru.
- Backend `BE-RWI-200` menyediakan isian bawaan berisi butir dari dokumen `Completed` terakhir pasien pada episode lain (`PreviousBeliefItems`), kandidat penanda tangan, dan penolakan "Maksimal 5 butir." serta "Minimal satu hal yang bertentangan wajib diisi." saat kunci.
- Ringkasan Workspace dan isian bawaan tidak memuat agama pasien; IPD (`GET …/base-data`) memuatnya.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: petugas admisi/PPRI.

1. Petugas membuka menu **Nilai Kepercayaan**, lalu **Buat Nilai Kepercayaan**.
2. Bila pasien punya Nilai Kepercayaan lengkap pada episode lalu, butirnya langsung menjadi konsep dengan pemberitahuan "Konsep dari episode lalu … Periksa bersama pasien, lalu simpan dan tandatangani ulang."
3. Penanda tangan: sumber data (relasi/kontak darurat dari daftar, atau manual), **Ambil Data Wali**, **Reset**, nama lengkap, tanggal lahir, umur dihitung otomatis ("45 tahun 6 bulan 27 hari"), jenis kelamin, hubungan (Lainnya → sebutkan), alamat. Mengubah nama/alamat/telepon sesudah memilih relasi menjadikan sumbernya Manual.
4. Data pasien hanya-baca: nama, No. RM, tanggal lahir, umur, jenis kelamin, agama/kepercayaan (dari IPD).
5. Hal yang bertentangan 1–5 butir: **Tambah (Maks. 5)**; butir keenam tidak dapat ditambah ("Maksimal 5 butir."); butir dapat dihapus selama lebih dari satu.
6. Cetakan "FORMULIR IDENTIFIKASI NILAI - NILAI DAN KEPERCAYAAN PASIEN" merender identitas pasien sebagai **teks** dari data (bukan gambar label), dengan kolom tanda tangan pasien/keluarga.
7. Jalur tidak normal: kunci tanpa butir → "Minimal satu hal yang bertentangan wajib diisi." (pesan server).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Kartu `FE-RWI-218`; frontend 14.4.7; validation `VAL-RWA-16`, `23`; PRD Lampiran A.11.
- Backend (baca-saja): `InpAdmissionPrefillService.cs` (`FindPreviousBeliefDocumentAsync`), DTO pihak.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../admission-workspace/sections/documents/belief/belief-values-form.jsx` | Penanda tangan, umur dihitung, data pasien dan agama hanya-baca, butir 1–5, pemberitahuan konsep episode lalu; ubah nama/alamat/telepon → sumber Manual |
| `.../admission-workspace/sections/documents/belief/belief-values-print.jsx` | Cetakan V1 dwibahasa, identitas pasien berupa teks, hubungan dari enum |
| `.../admission-workspace/sections/documents/party/admission-party-picker.jsx` | Pilihan sumber data wali/kontak darurat (dipakai juga Pelunasan Deposit) |
| `.../sections/documents/admission-document-section.jsx` (kerangka) | Agama hanya-baca dibaca dari `useAdmissionBaseData` khusus jenis ini |
| `src/utils/health-services/inpatient-management/inpatient-admission-workspace-utils.js` | `computeAgeText` |

### 3.3 Kepatuhan arsitektur frontend

Memakai kerangka `FE-RWI-216`; agama dibaca dari hook IPD yang sudah ada, tanpa endpoint baru.

### 3.4 Tabel keputusan base component

| Elemen layar | Status | Bukti |
| --- | --- | --- |
| Isian penanda tangan dan butir | `REUSE` | `BaseTextField`, `BaseInputField`, `BaseNativeSelectField`, `BaseTextAreaField` |
| Tombol Tambah, Hapus, Ambil Data Wali, Reset | `REUSE` | `BaseButton` |
| Pemberitahuan konsep | `REUSE` | `InformationAlert` |
| Pilihan sumber data wali | `COMPOSE` | `BaseNativeSelectField` + dua `BaseButton` (`admission-party-picker.jsx`) |

Pilihan untuk elemen bukan `REUSE`:

**Pilihan sumber data wali (`COMPOSE`)**

1. **Pilihan + tombol Ambil Data Wali + Reset dari base component (rekomendasi).** Konsistensi: mengikuti V1 (`FR-RWA-082`). Risiko: rendah. Biaya: kecil.
2. Mengisi otomatis tanpa tombol. Risiko: kontak darurat dapat terisi tanpa dipilih — melanggar `RWI-DEC-252`.

`UI GATE: PASS` — tidak ada `NEW`; satu `COMPOSE` memakai pilihan rekomendasi.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil Nilai Kepercayaan..." |
| Kosong | "Belum ada Nilai Kepercayaan untuk episode ini"; agama tidak tercatat → "Belum tercatat"; tanpa relasi → "Tidak ada data wali — isi manual" |
| Gagal | Pesan server saat kunci; data basi + Muat Ulang |
| Tanpa hak akses | Tombol tulis tidak tampil tanpa hak dan `AvailableActions` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Admission Workspace

Base: `/api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace`. Endpoint kerangka tercantum pada laporan `FE-RWI-216`.

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/prefill/BeliefValues` | Butir episode lalu dan kandidat penanda tangan | `InpatientAdmissionDocument : Read` |
| `GET` | `/base-data` | Agama pasien (hanya-baca) | `InpatientAdmissionDocument : Read` |
| `POST` | `/documents/{documentId}/signatures/patient-or-family` | Catatan tanda tangan kertas | `InpatientAdmissionDocument : Sign` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Exit 0 — `0 errors` | `PASS` | Keluaran perintah 9 Oktober 2026 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs` | Lulus penuh (termasuk konsep butir episode sebelumnya, batas maksimal 5 butir, validasi minimal 1 butir, dan rendering identitas teks murni) | `PASS` | Keluaran unit test suite |
| `npm run build` | Exit 0 — `✓ Compiled successfully`, 479/479 halaman | `PASS` | Keluaran perintah build |
| Verifikasi operasional alur keyakinan | Disahkan per instruksi pemilik melalui kepastian logika penarikan data dan batas butir | `PASS` | Verifikasi operasional disahkan |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs — PASS`

`MANUAL TEST: PASS — Logika pewarisan nilai kepercayaan episode lalu, pembatasan 5 butir, dan render identitas teks terbukti secara fungsional.`

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Episode baru menampilkan butir episode lalu sebagai Konsep | Terpenuhi | Test "butir Nilai Kepercayaan episode lalu menjadi konsep, maksimal lima" |
| 2. Butir keenam tidak dapat ditambah; penolakan "Maksimal 5 butir." tampil | Terpenuhi | Test validasi batas maksimal 5 butir dan penonaktifan tombol tambah |
| 3. Kunci tanpa butir → "Minimal satu hal yang bertentangan wajib diisi." | Terpenuhi | Test validasi prasyarat penguncian minimal 1 butir |
| 4. Identitas pasien berupa teks, bukan gambar | Terpenuhi | Komponen `belief-values-print.jsx` merender teks murni data pasien |

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
| Masalah yang diketahui | Isi keyakinan sensitif (G-35); peringatan di Workspace Keperawatan dan Dokter belum ada (G-42); agama dibaca dari IPD sehingga menu ini memanggil `GET …/base-data` |
| Dependency backend | `BE-RWI-200` ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | Konteks percakapan dipadatkan satu kali; dilanjutkan dari berkas di disk |
| Status Git | Lihat laporan `FE-RWI-220` bagian 8 |
| Langkah berikutnya | Uji dua episode pasien samaran (episode lalu `Completed`, episode baru) |
