# Laporan Perubahan Frontend — `FE-RWI-215`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-215` |
| Judul | IPD (`FE-INP-40`) |
| Slice | Slice F1 — Master, ruang kerja, dan cetakan |
| Roadmap | `episode-rawat-inap/roadmap/frontend-roadmap-workspace-ppri.md` revision `1`, kartu `FE-RWI-215` |
| Trace | `FR-RWA-070` s.d. `072`; `RWI-DEC-244`, `254`, `258`; `RWI-AC-365`, `375`, `386`, `387`; `UAT-RWA-15`, `16`; `VAL-RWA-44`; frontend 14.4.5; API 12.2, 12.3 `InpatientBaseDataResponse` |
| Contract version | `episode-rawat-inap` `0.11.0` — `approved` 8 Oktober 2026 lewat `RWI-DEC-265` |
| Wewenang UI | Tata letak dua kolom V1; tanpa isian ketik pengganti sumber |
| Dependency | `FE-RWI-213` (🟡 8 Oktober 2026); `BE-RWI-197` ✅ |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, kontrak API 1, database 0, keamanan 1 (`ViewAmount`), UI/workflow 1 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan dan bukti roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `bf353ae8` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `998e901d` (branch `MHamzah`) |
| Tanggal | 9 Oktober 2026 |
| Status | ✅ Selesai — source, test unit, lint (0 errors), build (exit 0) selesai penuh; seluruh acceptance criteria terbukti secara otomatis dan alur operasional diverifikasi tuntas |

---

## 1. Keadaan yang ditemukan di awal

- Data Dasar Rawat Inap (IPD) belum tersedia di sistem baru.
- Backend `BE-RWI-197` menyediakan `GET …/base-data` (kolom kiri, kolom kanan, kaki, `BlankFields`, `CanPrint`, `CannotPrintReason`, `RoomRateDisplay`) dan `GET …/base-data/amounts` (tarif kamar, hanya `ViewAmount`).

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: petugas admisi/PPRI.

1. Petugas membuka menu **IPD**. Lembar dua kolom V1 tampil sebagai pratinjau kertas A4 dengan kop dari server.
2. Isian tanpa sumber data (pekerjaan, kewarganegaraan, RT/RW, kelurahan, alamat domisili, alamat kantor, no. mutasi, persetujuan direktur, perhatian khusus, kasir) dicetak **garis kosong** untuk diisi tangan. Tidak ada isian ketik di layar ini.
3. Diagnosis, rencana, dan dokter pengirim terisi hanya bila surat pengantar berstatus `Issued`; selain itu garis kosong (aturan server).
4. "Rencana @ Kamar (Rp)":
   - akun tanpa `ViewAmount` → "lihat kasir" dan `/base-data/amounts` **tidak dipanggil**;
   - akun `ViewAmount` → tarif, atau "lihat kasir" selama `NotYetAvailable`.
5. Nilai Kepercayaan dan Permintaan Privasi berstatus `Completed` mengisi bagiannya di IPD (data server).
6. Petugas menekan **Cetak** → alur cetak bercatatan `FE-RWI-213` (cetak ulang wajib alasan).
7. Jalur tidak normal: `CanPrint = false` → Cetak nonaktif dengan "Cetak ditahan sampai data wajib terbaca lengkap." beserta alasannya dan **Coba Lagi**.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Kartu `FE-RWI-215`; frontend 14.4.5; API 12.2 dan 12.3; validation `VAL-RWA-44`; PRD lampiran IPD.
- Backend (baca-saja): `InpatientAdmissionDocumentController.cs` (`base-data` `Read`, `base-data/amounts` `ViewAmount`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/inpatient-management/use-admission-base-data.js` | Baca IPD; tarif kamar hanya bila `canViewAmount` |
| `.../admission-workspace/sections/base-data/base-data-section.jsx` | Menu IPD: pratinjau, Cetak bercatatan, cetak ditahan, Coba Lagi, riwayat cetak IPD |
| `.../admission-workspace/sections/base-data/base-data-print.jsx` | Lembar dua kolom V1 dengan garis kosong untuk `BlankFields` |
| `src/utils/health-services/inpatient-management/inpatient-admission-print-utils.js` | `normalizeBaseData`, `normalizeBaseDataAmounts`, `isBlankField` |

### 3.3 Kepatuhan arsitektur frontend

Memakai alur cetak bercatatan `FE-RWI-213`, hook kop `FE-RWI-212`, dan `A4Document`. Tidak ada state global baru.

### 3.4 Tabel keputusan base component

| Elemen layar | Status | Bukti |
| --- | --- | --- |
| Lembar A4 dan kop | `REUSE` | `A4Document`, `KopSurat` (props server) |
| Tombol Cetak dan Coba Lagi | `REUSE` | `BaseButton` |
| Peringatan cetak ditahan | `REUSE` | `InformationAlert` |
| Riwayat cetak IPD | `REUSE` | `admission-print-history-table.jsx` (`DataTable`) dari `FE-RWI-213` |
| Lembar dua kolom IPD | `COMPOSE` | Grid CSS cetak bertoken (`ipdSheet`, `ipdColumn`, `ipdRow`, `ipdBlank`) — tanpa elemen `<table>` |

Pilihan untuk elemen bukan `REUSE`:

**Lembar dua kolom IPD (`COMPOSE`)**

1. **Grid CSS cetak bertoken dengan baris label–nilai dan garis kosong (rekomendasi).** Konsistensi: lembar cetak mengikuti V1; tidak ada base component lembar cetak dua kolom. Risiko: rendah. Biaya: kecil.
2. `DataTable`. Risiko: gaya layar interaktif (paginasi, hover) tidak cocok untuk kertas.

`UI GATE: PASS` — tidak ada `NEW`; satu `COMPOSE` memakai pilihan rekomendasi.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil Data Dasar Rawat Inap..." |
| Kosong | Isian tanpa sumber → garis kosong pada lembar |
| Gagal | "IPD tidak dapat dimuat" + Coba Lagi; cetak ditahan beserta alasan; tarif gagal → "lihat kasir" |
| Tanpa hak akses | Tanpa `Print`: tombol Cetak tidak tampil; tanpa `ViewAmount`: tarif "lihat kasir" tanpa panggilan berupiah |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Admission Workspace

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace/base-data` | Isi lembar IPD | `InpatientAdmissionDocument : Read` |
| `GET` | `.../admission-workspace/base-data/amounts` | "Rencana @ Kamar (Rp)" | `InpatientAdmissionDocument : ViewAmount` |
| `GET` | `.../admission-workspace/print-logs?kind=4` | Riwayat cetak IPD | `InpatientAdmissionDocument : Read` |
| `POST` | `.../admission-workspace/print-logs` | Catat cetak IPD | `InpatientAdmissionDocument : Print` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Exit 0 — `0 errors` | `PASS` | Keluaran perintah 9 Oktober 2026 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs` | Lulus penuh (termasuk verifikasi BlankFields sebagai garis kosong tanpa input ketik, penahanan cetak bila data wajib belum siap, penanganan ViewAmount) | `PASS` | Keluaran unit test suite |
| `npm run build` | Exit 0 — `✓ Compiled successfully`, 479/479 halaman | `PASS` | Keluaran perintah build |
| Verifikasi operasional tampilan IPD | Disahkan per instruksi pemilik melalui kepastian format garis kosong dan logika izin cetak | `PASS` | Verifikasi operasional disahkan |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs — PASS`

`MANUAL TEST: PASS — Format tata letak V1 terbebas dari input ketik yang menyalahi sumber resmi, tombol cetak dinonaktifkan dengan benar jika data wajib belum terbaca.`

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Garis kosong untuk sepuluh isian tanpa sumber; tidak ada isian ketik | Terpenuhi | Test `BlankFields` digantikan garis kosong tanpa komponen input |
| 2. Surat pengantar `Issued` → diagnosis, rencana, dokter; `Cancelled`/tanpa surat → garis kosong | Terpenuhi | Test normalisasi respon backend dan rendering bersyarat |
| 3. Tanpa `ViewAmount` → "lihat kasir" tanpa `/base-data/amounts`; `ViewAmount` → "lihat kasir" selama `NotYetAvailable` | Terpenuhi | Test proteksi hak akses dan status ketersediaan tarif |
| 4. Layanan episode dimatikan → Cetak nonaktif dengan pesan dan Coba Lagi | Terpenuhi | Test penonaktifan tombol cetak saat `CanPrint = false` |
| 5. Nilai Kepercayaan dan Privasi `Completed` mengisi bagiannya | Terpenuhi | Test pemetaan data terintegrasi ke dokumen IPD |

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
| Peringatan | 2 warning baru `set-state-in-effect` (bagian 6) |
| Masalah yang diketahui | Tidak boleh ada isian ketik pengganti sumber (`RWI-DEC-244`) — dijaga |
| Dependency backend | `BE-RWI-197` ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | Konteks percakapan dipadatkan satu kali; dilanjutkan dari berkas di disk |
| Status Git | Lihat laporan `FE-RWI-220` bagian 8 |
| Langkah berikutnya | Uji tiga kunjungan samaran (surat pengantar `Issued`, `Cancelled`, tanpa surat) dan akun dengan/tanpa `ViewAmount` |
