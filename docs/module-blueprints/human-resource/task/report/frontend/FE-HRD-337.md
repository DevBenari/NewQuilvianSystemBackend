# Laporan Perubahan Frontend — `FE-HRD-337`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-337` |
| Judul | Daftar dan formulir SPPD memakai API, bukan data contoh |
| Slice | `S-D5` amandemen / `MVP-71` — gelombang 1 · `EPIC HRD-123` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 28, kartu `FE-HRD-337` |
| Trace | `HRD-TF-079`; `HRD-Q-221`; kontrak `v25` bagian 31 + `v28` 34.4 |
| Contract version | `v28` — approved 19 November 2026 |
| Wewenang UI | Verifikasi saja; tidak ada perubahan source |
| Dependency | Endpoint `v25` (sudah ada); backend amandemen `BE-HRD-374` s.d. `380` ✅ |
| Klasifikasi | `LIGHT` — tanpa perubahan source; uji browser baca saja |
| Task mode | `FRONTEND` (instruksi pengguna 19 November 2026: "lanjut") |
| Target tulis | Laporan ini, roadmap, dan traceability di repository backend. Tidak ada berkas frontend yang ditulis |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `564365595` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `815ce085` (branch `yoga`; memuat `BE-HRD-374` s.d. `380`) |
| Tanggal | 19 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria **sudah dipenuhi source as-is** (commit `2ce7a5911`, gelombang perbaikan frontend `FE-HRD-R22`/`R34`), dibuktikan di browser terhadap data asli devYoga |

---

## 1. Keadaan yang ditemukan di awal

Saat desain dan perencanaan (frontend `e01411158`), daftar SPPD membaca `MOCK_TRAVEL_REQUESTS` dari Redux dan formulir hanya menyimpan ke Redux (`HRD-TF-079`). Sejak itu commit `2ce7a5911` ("add performance, relations, services, travel constants and hooks") yang masuk lewat merge sudah:

| Bagian | Keadaan sekarang |
| --- | --- |
| Daftar SPPD `unified-travel-requests-view.jsx` | `useHrRemoteData` → `BusinessTravelService.getTravelRequests` dengan filter status/tanggal dan paging; detail, unduh surat tugas, dan batal lewat API |
| Formulir `unified-travel-create-view.jsx` | `useHrAction` → `BusinessTravelService.createTravelRequest`; validasi per langkah; zona/tarif dari API |
| Redux `businessTravelSlice.js`, `travel-display-utils.js` | Sudah tidak ada; status dari `src/lib/constants/hr/travel/travel-constants.js` |
| Langkah "pengajuan uang muka" di formulir (butir `FE-HRD-339`) | Sudah tidak ada; formulir mengarahkan uang muka ke layar Uang Muka |

Fakta `HRD-TF-079` sudah usang; `HRD-Q-221` tidak lagi menentukan pekerjaan (penyambungan sudah ada).

---

## 2. Verifikasi

`next dev` (3000) → backend lokal 7184 (`815ce085`) → devYoga; login superadmin dari konfigurasi (tidak dicetak); **semua permintaan non-GET dicegat** (tidak ada yang dicegat karena uji tidak menekan tombol tulis).

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Muat `/hrd/travel/requests` | 10 baris SPPD asli (`SPPD/RSQ/2026/11/0010` … `0001`), permintaan pertama `GET …/requests?pageNumber=1&pageSize=10` | `PASS` (AC1) | `337-hrd-travel-requests.png` |
| Muat `/corporate/hrd/travel/requests` | Sama persis (view yang sama) | `PASS` (AC3) | `337-corporate-hrd-travel-requests.png` |
| Filter status "Disetujui" | Opsi: Semua Status, Diajukan, Disetujui, Uang Muka Cair, Settlement Diajukan/Diverifikasi, Selesai, Ditolak, Dibatalkan; permintaan `status=Approved`; satu baris `0003` | `PASS` | Keluaran skrip |
| Filter "Selesai" (tidak ada data) | 0 baris, "Belum ada SPPD untuk saringan ini.", permintaan `status=Completed` | `PASS` (keadaan kosong) | idem |
| Daftar dijawab `500` (pencegat) | Pesan galat dari server tampil | `PASS` (keadaan galat) | `337-galat.png` |
| Formulir `/hrd/travel/requests/create` | Termuat ("Ajukan SPPD"); penyimpanan lewat `BusinessTravelService.createTravelRequest` (source); dibuktikan nyata bersama alur Ajukan di `FE-HRD-338`/`340` | `PASS` (AC2, source) | `337-form.png` |
| `MANUAL TEST` penyimpanan formulir | Tidak ditekan pada task ini (baca-saja); tidak ada kontrol yang diubah | `NOT FEASIBLE` di task ini — dilakukan di `FE-HRD-338` | — |

**Temuan untuk task berikutnya (tidak dikerjakan di sini):**

| Temuan | Task |
| --- | --- |
| SPPD `Draft` tampil sebagai kode mentah "Draft"; `NeedRevision` belum dikenal `TRAVEL_STATUS` | `FE-HRD-338` |
| Kolom nomor menulis "Diajukan {tanggal}" dari `createdAt`, juga untuk Draf | `FE-HRD-338` |
| Layar sukses formulir "SPPD Terkirim — sudah masuk antrean persetujuan", padahal v28 menyimpan Draf | `FE-HRD-338` |
| `submitTravelRequest` melempar "tidak diperlukan; SPPD langsung diajukan" | `FE-HRD-338` |
| Layar uang muka menganggap SPPD `Submitted` layak (`ADVANCE_ELIGIBLE_TRAVEL`) | `FE-HRD-339` |

---

## 3. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Daftar menampilkan SPPD dari server dengan keadaan memuat, kosong, dan galat | Terpenuhi (as-is) | Browser |
| 2. Simpan formulir memanggil API; galat `400` tampil; tidak ada kirim ganda | Terpenuhi (as-is, source `useHrAction`) | Source; uji tekan di `FE-HRD-338` |
| 3. Kedua rute memakai view yang sama | Terpenuhi | Browser |
| Perubahan source | Tidak ada | — |

---

## 4. Catatan penutup

| Hal | Isi |
| --- | --- |
| Penyegaran blueprint | `HRD-TF-079` dan `03-frontend-architecture.md` 32.1 usang; `HRD-Q-221` dapat ditutup ("sudah tersambung lewat `FE-HRD-R22`") pada pass blueprint |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy |
| Task berikutnya | `FE-HRD-338` (Ajukan, label status, batal) — backend `BE-HRD-375`/`377` ✅ |
