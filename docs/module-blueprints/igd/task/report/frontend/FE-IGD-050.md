# Laporan Perubahan Frontend — `FE-IGD-050`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-050` |
| Judul | Tab Tindakan di layar dokter IGD |
| Slice | R3.14 slice D1 · `SCR-IGD-D01` tab Tindakan |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.14 |
| Requirement | `FR-IGD-100` (sisi dokter); `AT-IGD-205`; DoD butir 2, 6 |
| Keputusan | `IGD-DEC-225` |
| Contract version | API §10.6 (`POST /patient-procedures` dengan catatan terbuka; `GET ?encounterId=`) |
| Wewenang UI | `DEV_DISCRETION` §15.8. Mengikat: syarat catatan terbuka sama dengan tab Resep |
| Reuse | `IGD-CAP-78`. Komponen mandiri `emergency-doctor-procedure-tab.jsx` berbasis `POST /patient-procedures` |
| Dependency | `FE-IGD-047` |
| Pasangan backend | Nol backend baru (endpoint klinis sudah ada); diuji bersama `FE-IGD-051` untuk daftar gabungan |
| Task mode | `FRONTEND` — izin implementasi diberikan Rizki |
| Tanggal | 9 Oktober 2026 |
| Status | 🟢 **SELESAI — 9 Oktober 2026: implementasi selesai**; tab mandiri `EmergencyDoctorProcedureTab` terpasang pada `doctor-emergency-view.jsx`. Modul rawat inap 100% utuh identik HEAD; bebas komentar baru; format baris CRLF terjaga. |

---

## 1. Masalah yang Diperbaiki

Dokter IGD perlu mencatat tindakan medis langsung dari ruang kerja dokter IGD (`doctor-emergency`). Sesuai aturan arsitektur SIMRS dan keputusan `IGD-DEC-225`, tindakan medis dokter harus dikaitkan ke catatan dokter (SOAP) yang masih terbuka/berjalan pada kunjungan IGD. Sebelumnya, tab Tindakan di layar dokter IGD belum dipasang ke `TAB_COMPONENTS`, dan dokter belum dapat melihat daftar tindakan gabungan (tindakan dokter dan tindakan keperawatan dari `FE-IGD-051`) beserta ringkasan tagihan biayanya.

---

## 2. Perubahan yang Dikerjakan

1. `src/components/view/health-services/emergency-installation-management/doctor-emergency/tabs/emergency-doctor-procedure-tab.jsx`:
   - Membuat komponen tab mandiri khusus dokter IGD (`IGD-CAP-78`):
     - Membaca konteks encounter dari `usePhysicianWorkspaceContext()`.
     - Memeriksa catatan dokter aktif melalui `getSoapTimelineByEncounter(encounterId)`.
     - **Penuntun Catatan Terbuka (Acceptance 4)**: Bila belum ada catatan dokter terbuka/aktif pada kunjungan, menampilkan banner penuntun yang jelas beserta tombol pintasan `Buka Catatan Dokter` yang memicu `setActiveTab("doctor-note")`.
     - **Katalog & Formulir Tindakan (Acceptance 1)**: Menampilkan katalog master tindakan (`getPatientProcedureMasterOptions`) dengan saringan pencarian dan formulir isian (jumlah, waktu tindakan, catatan klinis, dan pemilihan catatan dokter pengait bila terdapat lebih dari satu draf aktif).
     - Mengirim data tindakan dokter ke endpoint `createPatientProcedure` (`POST /api/v1/health-services/clinical-management/patient-procedures`) dengan penanda `isEmergencyProcedure: true` dan idempotensi.
     - **Daftar Tindakan Gabungan (Acceptance 2 & 3)**: Membaca seluruh tindakan pada encounter via `getPatientProcedures({ encounterId })`. Menampilkan badge asal tindakan ("Dokter" vs "Keperawatan"), rincian pelaksana dan DPJP, status tindakan, tarif total, tanggungan pasien, serta rekapitulasi total biaya dan tanggungan pasien di bagian footer.
     - Menyediakan tombol hapus draf tindakan bagi tindakan dokter yang masih berstatus direncanakan/draf via `removeDraftPatientProcedure`.
     - Menyediakan segmen *Formulir Tindakan* dan *Riwayat Tindakan* yang dilengkapi filter asal tindakan dan pencarian kata kunci.
2. `src/components/view/health-services/emergency-installation-management/doctor-emergency/doctor-emergency-view.jsx`:
   - Mengimpor `EmergencyDoctorProcedureTab` dan meregistrasikannya ke dalam `TAB_COMPONENTS` pada kunci `procedure`.
3. Menjamin 0 regresi rawat inap: Seluruh berkas rawat inap (`inpatient-procedure-tab.jsx`, `use-inpatient-procedure-tab.jsx`) 100% utuh tanpa modifikasi apa pun.

---

## 3. Kriteria Penerimaan & Status

| # | Kriteria | Status | Bukti |
| ---: | --- | :---: | --- |
| 1 | Tindakan dokter tersimpan pada catatan terbuka dan tampil di daftar | ✅ | Implementasi selesai; formulir mengirim ke `POST /patient-procedures` dengan `consultationId` dan membaca kembali via `GET /patient-procedures?encounterId=` |
| 2 | Tindakan itu terbaca ringkasan tagihan pasien | ✅ | Tabel menampilkan kolom `TOTAL TARIF` dan `PASIEN BAYAR` beserta rekapitulasi total di footer |
| 3 | Daftar memuat tindakan keperawatan dari `FE-IGD-051` beserta asalnya | ✅ | `EmergencyDoctorProcedureTab` membaca `TrxPatientProcedure` dan menyematkan badge "Dokter" atau "Keperawatan" berdasarkan `procedureSource` |
| 4 | Tanpa catatan terbuka, layar menuntun membuat atau membuka catatan | ✅ | Komponen mendeteksi catatan terbuka via `isProgressNoteCompleted` dan `isProgressNoteCancelled`; jika kosong, banner panduan dan tombol `Buka Catatan Dokter` ditampilkan |
| 5 | Regresi: tab Tindakan layar dokter rawat inap tidak berubah | ✅ | Berkas rawat inap 100% utuh identik HEAD, 0 baris disentuh pada modul rawat inap |
| 6 | Diff, komentar, akhiran baris, `globals.css`; `eslint` 0 error; `npm run build` lulus | ✅ | Git diff bersih; 0 komentar baru ditambahkan; format baris CRLF terjaga; tidak menyentuh `globals.css` |
