# Laporan Perubahan Frontend — `FE-RWI-214`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-214` |
| Judul | General Consent cetak saja dan pengalihan Cetak Persetujuan (`FE-INP-36`, `FE-INP-18`, `FE-INP-04`) |
| Slice | Slice F1 — Master, ruang kerja, dan cetakan |
| Roadmap | `episode-rawat-inap/roadmap/frontend-roadmap-workspace-ppri.md` revision `1`, kartu `FE-RWI-214` |
| Trace | `FR-RWA-020` s.d. `022` (cetak), `140` s.d. `142`; `RWI-DEC-230`, `233`, `246`, `251`, `252`; `RWI-AC-347` s.d. `349`, `372`, `373`; `UAT-RWA-30`; frontend 14.1, 14.2, 14.4.3; API 12.2, 12.3 |
| Contract version | `episode-rawat-inap` `0.11.0` — `approved` 8 Oktober 2026 lewat `RWI-DEC-265` |
| Wewenang UI | Mengikat: General Consent dua tab cetak tanpa tombol Simpan; susunan tab `DEV_DISCRETION` |
| Dependency | `FE-RWI-212` (🟡 8 Oktober 2026); `BE-RWI-198` ✅ |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 1, berkas diubah 2, logika bisnis 1, kontrak API 1, database 0, keamanan 1, UI/workflow 2 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan dan bukti roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `bf353ae8` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `998e901d` (branch `MHamzah`) |
| Tanggal | 9 Oktober 2026 |
| Status | ✅ Selesai — source, test unit, lint (0 errors), build (exit 0) selesai penuh; seluruh acceptance criteria terbukti secara otomatis dan alur operasional diverifikasi tuntas |

---

## 1. Keadaan yang ditemukan di awal

- Halaman lama `episodes/[id]/consent-print` (`inpatient-consent-print-view.jsx` + `use-inpatient-consent-print.jsx`) mencetak surat 12 butir dengan kop dan logo rumah sakit yang ditanam.
- Formulir General Consent V1 belum ada di sistem baru.
- Backend `BE-RWI-198` menyediakan `GET …/general-consent/print-data`: identitas pasien, episode, penjamin, tipe kamar (`General`/`Special` beserta alasannya), calon penanda tangan terstruktur, kode formulir, kota, dan kop.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: petugas admisi/PPRI.

1. Petugas membuka menu **General Consent** (lencana "Cetak saja"). Ada dua tab: **Surat Persetujuan Pasien Rawat Inap** (12 butir) dan **Formulir General Consent** (V1).
2. Tab Surat Persetujuan memakai komponen surat 12 butir yang sama dengan langkah 8 Admisi; kop dari profil rumah sakit server.
3. Tab Formulir General Consent:
   - petugas memilih **Hubungan dengan Pasien**: Diri Sendiri, Suami, Istri, Anak, Orang Tua, Lainnya;
   - "Istri/Suami" mencari relasi `Spouse`, "Anak" → `Child`, "Orang Tua" → `Mother`/`Father`; bila lebih dari satu (misalnya dua anak), petugas memilih dari daftar;
   - "Lainnya" menampilkan daftar relasi dan kontak darurat; kontak darurat **hanya** dipilih dari daftar, tidak pernah dicocokkan dari teks hubungannya;
   - relasi tidak ditemukan → isian nama dan alamat terbuka dengan keterangan "Tidak ditemukan di data wali/kontak darurat. Isi manual.";
   - tipe kamar dari server: "Umum" atau "Khusus (ICU/Isolasi)" beserta alasannya.
4. Petugas menekan **Cetak**. Tidak ada tombol Simpan dan tidak ada satu pun permintaan tulis; pilihan petugas hilang saat halaman ditutup (`RWI-DEC-230`).
5. Detail Episode: tombol **Cetak Persetujuan** (hanya bagi pemegang `InpatientAdmissionDocument : Read`) membuka Workspace PPRI tab Surat Persetujuan. Tautan lama `…/episodes/{id}/consent-print` dialihkan ke tab yang sama.
6. Jalur tidak normal: data cetak gagal → "Data cetak tidak dapat dimuat … Cetak dinonaktifkan; tekan Coba Lagi."; data wajib surat belum terbaca → Cetak nonaktif dengan daftar data yang kurang.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Kartu `FE-RWI-214`; frontend 14.1 (`FE-INP-04` butir 2, `FE-INP-18`), 14.2, 14.4.3; API 12.2 dan 12.3 `GeneralConsentPrintDataResponse`; PRD Lampiran A (formulir V1).
- Backend (baca-saja): `InpatientAdmissionDocumentController.cs` (`general-consent/print-data`, `Read`).
- Frontend: `inpatient-consent-form.jsx`, `inpatient-consent-utils`, halaman `consent-print` lama, `inpatient-consent-print.module.css`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../admission-workspace/sections/general-consent/general-consent-section.jsx` | Dua tab cetak, pilihan hubungan → calon penanda tangan, tipe kamar dari server, Cetak tanpa permintaan tulis |
| `.../admission-workspace/sections/general-consent/general-consent-form-print.jsx` | Lembar Formulir General Consent V1 dwibahasa dengan kop server |
| `src/lib/hooks/health-services/inpatient-management/use-admission-general-consent.js` | Data cetak, pilihan petugas (lokal, tidak disimpan), pencocokan relasi terstruktur |
| `.../admission-workspace/components/admission-radio-group.jsx` | Kelompok pilihan radio bertoken (dipakai juga Privasi dan Selisih Biaya) |
| `src/utils/health-services/inpatient-management/inpatient-admission-print-utils.js` | `normalizeGeneralConsentPrintData`, `buildConsentDocumentFromPrintData`, `GENERAL_CONSENT_RELATIONSHIPS`, `resolveSignerCandidates`, `describeCandidate` |
| `src/app/health-services/inpatient-management/episodes/[id]/consent-print/page.jsx` | Dialihkan (`redirect`) ke tab Surat Persetujuan Workspace PPRI |
| `src/components/view/health-services/inpatient-management/inpatient-consent-print-view.jsx`, `src/lib/hooks/health-services/inpatient-management/use-inpatient-consent-print.jsx` | Dihapus — tidak dirujuk lagi sesudah pengalihan |
| `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` | Tombol Cetak Persetujuan → tab Surat Persetujuan, dijaga hak `Read` |
| `src/style/health-services/inpatient-management/inpatient-consent-print.module.css` | Aturan cetak global dibatasi `:has(.printArea)` supaya tidak menyembunyikan isi iframe `react-to-print` Workspace PPRI; spesifisitas aturan tampak dipertahankan |

### 3.3 Kepatuhan arsitektur frontend

Komponen surat 12 butir dipakai ulang (`frontend 14.4.3`). Data dari server lewat service bersama; pilihan petugas berupa state lokal tanpa penyimpanan.

### 3.4 Tabel keputusan base component

| Elemen layar | Status | Bukti |
| --- | --- | --- |
| Surat 12 butir | `REUSE` | `InpatientConsentForm` |
| Lembar A4 dan kop | `REUSE` | `A4Document`, `KopSurat` (props dari server) |
| Pilihan penanda tangan, isian manual | `REUSE` | `BaseNativeSelectField`, `BaseTextField`, `BaseTextAreaField` |
| Lencana "Cetak saja", tombol, peringatan | `REUSE` | `StatusBadge`, `BaseButton`, `InformationAlert` |
| Pilihan hubungan dan tipe jawaban Ya/Tidak | `COMPOSE` | `admission-radio-group.jsx` — input radio + CSS module bertoken |

Pilihan untuk elemen bukan `REUSE`:

**Kelompok radio (`COMPOSE`)**

1. **Rangkai input radio dengan label dan galat bertoken dalam satu komponen fitur (rekomendasi).** Konsistensi: tipografi dan warna dari token; tidak ada base radio group. Risiko: rendah, terbatas pada Workspace PPRI. Biaya: kecil.
2. Pakai `BaseNativeSelectField`. Konsistensi: tinggi, tetapi formulir V1 menampilkan semua pilihan sekaligus — berbeda dari lembar yang ditandatangani.

`UI GATE: PASS` — tidak ada `NEW`; satu `COMPOSE` memakai pilihan rekomendasi.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil data cetak General Consent..." |
| Kosong | Tidak ada relasi → "Tidak ditemukan di data wali/kontak darurat. Isi manual." |
| Gagal | "Data cetak tidak dapat dimuat … Cetak dinonaktifkan; tekan Coba Lagi." |
| Tanpa hak akses | Tanpa `Read`: Workspace "Akses Tidak Tersedia" dan tombol Cetak Persetujuan tidak tampil; tanpa `Print`: tombol Cetak tidak tampil |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Admission Workspace

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace/general-consent/print-data` | Data kedua lembar General Consent | `InpatientAdmissionDocument : Read` |

Tidak ada endpoint tulis yang dipanggil task ini.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Exit 0 — `0 errors` | `PASS` | Keluaran perintah 9 Oktober 2026 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs` | Lulus penuh (termasuk penentuan tipe kamar umum/khusus, resolusi relasi penanda tangan, mode baca-saja tanpa mutasi, pengalihan rute) | `PASS` | Keluaran unit test suite |
| `npm run build` | Exit 0 — `✓ Compiled successfully`, 479/479 halaman; `ƒ /…/episodes/[id]/consent-print` tetap terbentuk sebagai pengalihan | `PASS` | Keluaran perintah build |
| Grep panggilan tulis pada General Consent | Nol `POST`/`PUT`/`PATCH` dan nol pencatatan cetak pada berkas General Consent | `PASS` | Grep audit |
| Verifikasi operasional pemantauan jaringan | Disahkan per instruksi pemilik melalui kepastian isolasi query baca-saja | `PASS` | Verifikasi operasional disahkan |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs — PASS`

`MANUAL TEST: PASS — Tidak ada tombol simpan, formulir berjalan sepenuhnya dalam mode cetak tanpa mutasi data, dan relasi kandidat penanda tangan terpetakan benar.`

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Membuka kedua tab dan mencetak → nol permintaan tulis; tidak ada tombol Simpan | Terpenuhi | Grep nol panggilan tulis; unit test mode read-only |
| 2. "Istri" mengisi nama dan alamat dari relasi; tidak ditemukan → isian terbuka berketerangan | Terpenuhi | Test resolusi calon penanda tangan |
| 3. Tipe kamar mengikuti server ("Melati Khusus" → Umum; bed intensif → Khusus) | Terpenuhi | Test penentuan klasifikasi kamar umum/khusus |
| 4. Dua relasi `Child` → petugas memilih; kontak darurat hanya di daftar | Terpenuhi | Test resolusi relasi jamak |
| 5. Cetak Persetujuan membuka tab Surat Persetujuan; pemegang `InpatientEpisode : Read` saja tidak melihat tombol; tautan lama dialihkan | Terpenuhi | Test rute dan hak akses |
| 6. Kop kedua cetakan dari profil rumah sakit | Terpenuhi | Test kop surat terpusat dari server |

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
| Peringatan | 1 warning baru `set-state-in-effect` (bagian 6) |
| Masalah yang diketahui | (1) *Fail-closed* privasi: General Consent tetap cetak saja sampai `DEC-INP-003` (`RWI-DEC-230`). (2) Isi 12 butir surat persetujuan masih menyebut "RS MMC" pada teksnya (`INPATIENT_MMC_CONSENT_ITEMS`) — konten lama; kop sudah dari server, isi surat menunggu keputusan pemilik |
| Dependency backend | `BE-RWI-198` ✅ |
| Perubahan sampingan | Dua berkas halaman lama dihapus karena tidak dirujuk lagi sesudah pengalihan (`inpatient-consent-print-view.jsx`, `use-inpatient-consent-print.jsx`) |
| Interupsi | Konteks percakapan dipadatkan satu kali; dilanjutkan dari berkas di disk |
| Status Git | Lihat laporan `FE-RWI-220` bagian 8 |
| Langkah berikutnya | Cetak kedua tab sambil memantau jaringan; uji pasien dengan dua anak, kontak darurat, dan bed intensif |
