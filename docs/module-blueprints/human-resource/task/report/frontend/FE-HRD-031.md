# Laporan Perubahan Frontend — `FE-HRD-031`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-031` |
| Judul | Layar Batch Formasi Awal dan detailnya |
| Slice | `S-D1` · `MVP-7` |
| Roadmap | `roadmap/frontend-roadmap.md` kartu `FE-HRD-031`; dikerjakan lewat `FE-HRD-R12` |
| Trace | `HRD-DEC-070`, `FR-HRD-101`, `VR-WFP-11` s.d. `14`, `UAT-31`, `UAT-32` |
| Contract version | API Contract v7 bagian 13 (disetujui pemilik 3 November 2026) |
| Wewenang UI | Susunan detail `DEV_DISCRETION`; tombol Ajukan nonaktif selama baris tidak valid atau scan belum ada (mengikat) |
| Dependency | `FE-HRD-029` ✅, `BE-HRD-033` ✅ [BE] |
| Klasifikasi | `MEDIUM` — layar baru dengan service, state, dan validasi |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src`; laporan dan tanda status di `docs/module-blueprints/human-resource/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `d09dc74da` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `f751f8ca` (branch `MHamzah`), hanya dibaca |
| Tanggal | 11 November 2026 |
| Status | 🟡 Sebagian — daftar, form baru, dan alasan tombol Ajukan terbukti; kriteria 2, 3, 5, 6 baru terpetakan ke source |

---

## 1. Keadaan yang ditemukan di awal

Layar belum ada.

---

## 2. Proses bisnis dari sisi pengguna

HR Admin membuka Batch Formasi Awal, mengunduh templat, lalu menekan + Buat Batch. Ia memilih rumah sakit, menulis nomor SK, lalu menambah baris dengan memilih unit dan jabatan dari daftar, mengisi jumlah formasi (misalnya 0,5 untuk separuh porsi), dan tanggal berlaku. Ia menekan Periksa Baris; setiap baris diberi status Valid atau Tidak valid beserta pesannya, misalnya "Pasangan unit dan jabatan ini sudah memiliki formasi di sistem (VR-WFP-11)." Sesudah disimpan, ia mengunggah scan SK (PDF/JPG/PNG, paling besar 10 MB) dan menekan Ajukan. Tombol Ajukan nonaktif dan alasannya tertulis selama ada baris tidak valid atau scan belum ada.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `S-D1` di `frontend-roadmap.md`, `03-frontend-architecture.md` bagian 12, controller dan DTO `WorkforcePlanning`, view referensi Batch Data Awal, `menu-items.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/workforce-planning/initial-formation-batches-view.jsx` | Layar daftar batch |
| `src/components/view/hr/workforce-planning/initial-formation-batch-detail-view.jsx` | Layar buat dan detail batch, termasuk riwayat persetujuan dari instance workflow |
| `src/app/hr/workforce-planning/initial-formation-batches/page.jsx` | Rute daftar |
| `src/app/hr/workforce-planning/initial-formation-batches/[id]/page.jsx` | Rute detail; `baru` untuk batch baru |

### 3.3 Kepatuhan arsitektur frontend

Rute → view → service `FE-HRD-029`. Riwayat persetujuan memakai `UnifiedApprovalService.getItemDetail` (`FE-HRD-R08`). Templat backend berisi kolom GUID, jadi baris diisi lewat pilihan unit dan jabatan; pengguna tidak pernah mengetik UUID.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx` dan `src/components/view/hr/leave/leave-admin-view.jsx`

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header halaman | `Hero` | `REUSE` | — |
| Saringan daftar | `DataFilter` + `ResourceFilterSelect` + `FilterSelect` | `REUSE` | — |
| Tabel daftar, baris, dan riwayat | `DataTable` | `REUSE` | — |
| Status batch dan baris | `StatusBadge` (kunci sah) | `REUSE` | — |
| Ringkasan baris | `SummaryGrid` | `REUSE` | — |
| Isian dokumen dan baris | `BaseInputField`, `BaseTextAreaField`, `ResourceFilterSelect`, `FilterDatePicker` | `REUSE` | — |
| Konfirmasi batal dengan alasan | `ConfirmModal requireReason` | `REUSE` | — |
| Unggah scan SK | `input type="file"` tersembunyi dipicu `BaseButton` | `REUSE` | Belum ada base uploader (`DEC-FER-02`); elemen bawaan, bukan komponen baru |
| Notifikasi dan galat | `ToastStack`, `InformationAlert` | `REUSE` | — |

`UI GATE: PASS — 9 elemen REUSE, 0 COMPOSE, 0 NEW`

Hasil pemeriksa dan grep anti-regresi:

```text
$ node <rules>/frontend/tools/check-css-usage.mjs <7 berkas view S-D1 dan S-B1>
OK — 7 berkas diperiksa, 0 temuan.
$ grep -nE "className=\"[^\"]*\b(flex|grid|gap-[0-9]|p-[0-9]|px-|py-|text-(sm|xs|lg)|bg-|rounded|w-full|items-center|md:|hover:)" <7 berkas>
(kosong)
```

| Layar | Screenshot | Catatan perbandingan dengan referensi |
| --- | --- | --- |
| `/hr/workforce-planning/initial-formation-batches` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/06-batch-formasi-daftar.png` | Setara |
| `/hr/workforce-planning/initial-formation-batches/baru` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/07-batch-formasi-baru.png` | Form baru |
| `/hr/workforce-planning/initial-formation-batches/baru` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/08-batch-formasi-baris-belum-lengkap.png` | Validasi baris belum lengkap dan alasan tombol Ajukan |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat batch formasi awal..." |
| Kosong | "Belum ada batch formasi awal." |
| Gagal | `InformationAlert` merah; 409 menjadi kalimat muat ulang |
| Tanpa hak akses | Tombol Buat, Simpan, Ajukan, dan Batalkan mengikuti butir `WorkforceInitialFormationBatch` |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Workforce Planning / Initial Formation Batch

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/workforce-planning/initial-formation-batches` | Daftar batch | `WorkforceInitialFormationBatch : Read` |
| `GET` | `/template` | Unduh templat | `WorkforceInitialFormationBatch : Read` |
| `POST` | `/validate` | Periksa baris | `WorkforceInitialFormationBatch : Read` |
| `POST` | `/` dan `PUT /{id}` | Simpan batch | `WorkforceInitialFormationBatch : Create` / `Update` |
| `POST` | `/{id}/upload-source-document` | Unggah scan SK | `WorkforceInitialFormationBatch : Update` |
| `POST` | `/{id}/submit`, `/{id}/cancel` | Ajukan, batalkan | `Update`, `Delete` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| `check-css-usage.mjs` pada 7 berkas view baru | 0 temuan | `PASS` | Keluaran perintah |
| Uji browser (Chrome headed, backend lokal mode `Runtime__Role=Web`) | Daftar, form baru, validasi baris belum lengkap | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/06-batch-formasi-daftar.png`, `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/07-batch-formasi-baru.png`, `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/08-batch-formasi-baris-belum-lengkap.png` |

Uji manual: `PASS` untuk daftar dan form baru.

**Tidak dijalankan:** Simpan, periksa baris dengan data, unggah scan, ajukan, dan batal tidak dijalankan agar tidak menulis batch ke database pengembangan.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Daftar: nomor batch, dokumen sumber, status, jumlah valid/tidak valid; kosong "Belum ada batch formasi awal." | Terpenuhi | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/06-batch-formasi-daftar.png` |
| Detail menampilkan status per baris beserta pesannya, termasuk `VR-WFP-11` | Belum terbukti | Terpetakan ke source |
| Unggah scan memeriksa jenis dan ukuran berkas dan menampilkan pesan `VR-WFP-14` | Belum terbukti | Terpetakan ke source |
| Tombol Ajukan nonaktif selama `InvalidRows > 0` atau scan belum ada, dengan keterangan | Terpenuhi | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/08-batch-formasi-baris-belum-lengkap.png` |
| Simpan, Ajukan, Batalkan hanya pada `Draft`/`Returned`; `Applied` hanya baca | Belum terbukti | Terpetakan ke source |
| Riwayat persetujuan dari instance workflow | Belum terbukti | Terpetakan ke source |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit. Lima berkas lain ikut berubah di working tree tetapi bukan dari task ini dan tidak disentuh: `workforce-profile-dashboard.jsx`, `employee-profile-changes-self-service-view.jsx`, `employee-workforce-tabs.jsx`, `employee-workforce-detail.css`, `wfp-workforce-detail.css` |
| Langkah berikutnya | Uji alur batch lengkap dengan data uji yang disetujui pemilik |
