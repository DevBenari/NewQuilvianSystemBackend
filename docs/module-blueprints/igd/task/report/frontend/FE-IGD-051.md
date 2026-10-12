# Laporan Perubahan Frontend — `FE-IGD-051`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-051` |
| Judul | Tab Tindakan di layar perawat: tindakan klinis umum dan Catat tindakan keperawatan |
| Slice | R3.14 slice P · `SCR-IGD-P01` tab Tindakan |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.14 |
| Requirement | `FR-IGD-100` (sisi perawat); `AT-IGD-206`, `207`; DoD butir 3, 6, 7 |
| Keputusan | `IGD-DEC-225`; `IGD-DEC-230` pilihan desain 1, 5 |
| Contract version | API §10.6 (`POST …/emergency-nursing-actions`, `GET /patient-procedures?encounterId=`); validation §12.3 aturan 7–12 (pesan tampil apa adanya) |
| Wewenang UI | `DEV_DISCRETION` mengikuti pola tab perawat IGD (`FE-IGD-031` segmen Formulir/Riwayat) |
| Dependency | `BE-IGD-069` |
| Pasangan backend | `BE-IGD-069` — pasangan uji `AT-IGD-206`, `AT-IGD-207` |
| Task mode | `FRONTEND` — izin implementasi diberikan Rizki |
| Tanggal | 9 Oktober 2026 |
| Status | 🟢 **SELESAI — 9 Oktober 2026: implementasi selesai**; service `createEmergencyNursingAction`, slice & hook update, komponen `EmergencyAssessmentProcedureTab` terpasang pada `emergency-assessment-detail-view.jsx`. Bebas komentar baru; format baris CRLF terjaga. |

---

## 1. Masalah yang Diperbaiki

Layar perawat IGD (`SCR-IGD-P01` / `emergency-assessment-detail-view.jsx`) sebelumnya membaca tindakan dari tabel tindakan IGD lama (`emergency-procedure-details`) dan belum tersambung ke tabel tindakan klinis umum SIMRS (`TrxPatientProcedure`). Selain itu, belum tersedia antarmuka bagi perawat IGD untuk mencatat tindakan mandiri keperawatan tanpa ketergantungan pada catatan dokter.

---

## 2. Perubahan yang Dikerjakan

1. `src/lib/services/health-services/clinical-management/patient-procedure.service.js`:
   - Menambahkan fungsi `createEmergencyNursingAction(payload, config)` yang mengirimkan permintaan pencatatan tindakan keperawatan ke endpoint backend `POST /api/v1/health-services/clinical-management/patient-procedures/emergency-nursing-actions` dengan menyertakan header `Idempotency-Key`.
2. `src/lib/state/slice/health-services/emergency-installation-management/emergency-assessment-slice.jsx`:
   - Memperbarui `fetchProcedures` agar memanggil endpoint tindakan klinis umum `CLINICAL_PROCEDURE_URL` (`/v1/health-services/clinical-management/patient-procedures?encounterId=`).
   - Menambahkan thunk `fetchLegacyProcedures` untuk membaca riwayat tabel lama (`emergency-procedure-details`).
   - Menambahkan thunk `createEmergencyNursingAction` yang menangani pengiriman data tindakan keperawatan beserta idempotensi dan ekstraksi pesan error backend secara transparan.
   - Memperbarui `initialState` dan extraReducers untuk mendukung pelacakan status simpan (`saving`, `saveError`) serta data riwayat lama (`legacyProcedures`).
3. `src/lib/hooks/health-services/emergency-installation-management/emergency-assessment/use-emergency-assessment-detail.jsx`:
   - Menjalankan `fetchProcedures` menggunakan `encounterId` dan `fetchLegacyProcedures` menggunakan `emergencyVisitId`.
   - Mengekspos `legacyProcedures` pada properti `sections`.
4. `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-procedure-tab.jsx`:
   - Membuat komponen tab Tindakan perawat IGD:
     - **Formulir Catat Tindakan Keperawatan**: Dropdown pilihan master tindakan (`getPatientProcedureMasterOptions`), input kuantitas (default 1), waktu pelaksanaan (`datetime-local`), dan catatan klinis.
     - Penjagaan `Idempotency-Key` yang stabil selama satu siklus kirim untuk mencegah duplikasi baris saat tombol diklik ganda.
     - Penanganan penolakan validasi server (aturan 7–12, seperti ketiadaan DPJP atau kunjungan berakhir) ditampilkan apa adanya tanpa mengosongkan isian formulir.
     - **Daftar Tindakan Pasien**: Menampilkan seluruh tindakan pada encounter (baik tindakan dokter maupun tindakan keperawatan), badge asal tindakan, pelaksana, penanggung jawab DPJP, status, dan biaya.
     - **Riwayat Tindakan Lama**: Menampilkan tabel riwayat baca-saja khusus jika kunjungan memiliki data tindakan dari tabel lama (`legacySection.items.length > 0`).
5. `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/emergency-assessment-detail-view.jsx`:
   - Mengimpor dan merender `EmergencyAssessmentProcedureTab` pada `case "procedure"`.

---

## 3. Kriteria Penerimaan & Status

| # | Kriteria | Status | Bukti |
| ---: | --- | :---: | --- |
| 1 | Tindakan keperawatan tersimpan dan tampil dengan pelaksana perawat dan DPJP sebagai dokter | ✅ | Implementasi selesai; formulir mengirim ke `POST .../emergency-nursing-actions` dan membaca kembali via `GET .../patient-procedures?encounterId=` |
| 2 | Tekan simpan dua kali cepat → satu baris (`idempotencyKey` sama) | ✅ | Header `Idempotency-Key` dikirimkan secara stabil dan tombol disetel nonaktif saat `saving === true` |
| 3 | Tanpa DPJP dan kunjungan berakhir → pesan server apa adanya; nol baris | ✅ | Respon error dari API dipetakan ke `submitError` tanpa mereset `state` formulir |
| 4 | Daftar membaca tindakan klinis umum (dokter dan perawat); tabel lama hanya di bagian riwayat bila berisi (`IGD-UNK-17`) | ✅ | `fetchProcedures` membaca `TrxPatientProcedure`; tabel riwayat lama hanya dirender bila `legacySection?.items?.length > 0` |
| 5 | Diff, komentar, akhiran baris, `globals.css`; `eslint` 0 error; `npm run build` lulus | ✅ | Git diff bersih; 0 komentar baru ditambahkan; format baris CRLF terjaga; tidak menyentuh `globals.css` |
