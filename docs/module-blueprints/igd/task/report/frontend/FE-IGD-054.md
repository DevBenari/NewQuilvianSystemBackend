# Laporan Perubahan Frontend — `FE-IGD-054`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-054` |
| Judul | Tab Tindak Lanjut di layar dokter IGD: buat draf, konfirmasi, batalkan |
| Slice | R3.14 slice D2 · `SCR-IGD-D01` tab Tindak Lanjut |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.14 |
| Requirement | `FR-IGD-103`, `FR-IGD-104`; `AT-IGD-211`; DoD butir 2, 3 |
| Keputusan | `IGD-DEC-223`, `IGD-DEC-226` |
| Contract version | API §10.3 (`POST /emergency-dispositions`, `PATCH /emergency-dispositions/{id}/disposition-status`); validation §12.5 aturan 14–15; state §10.1 |
| Wewenang UI | `DEV_DISCRETION` §15.8. Mengikat: pesan `409` tampil di modal konfirmasi yang masih terbuka, terbaca tanpa kursor (pola `FE-IGD-043`) |
| Reuse | `emergency-assessment-disposition-tab.jsx` & `DISPOSITION_STATUS_ACTIONS`; thunk `createDisposition`, `fetchDispositions`, `updateDispositionStatus` di `emergency-assessment-slice.jsx` |
| Dependency | `FE-IGD-045`, `BE-IGD-070` |
| Pasangan backend | `BE-IGD-070` |
| Task mode | `FRONTEND` — izin implementasi diberikan Rizki |
| Tanggal | 9 Oktober 2026 |
| Status | 🟢 **SELESAI — 9 Oktober 2026: implementasi selesai**; tab mandiri `EmergencyDoctorDispositionTab` terpasang pada `doctor-emergency-view.jsx`; pemisahan konstanta aksi per peran di `emergency-assessment-constant.jsx`; bebas komentar baru; format baris CRLF terjaga; eslint 0 error. Siap build pemilik dan uji integrasi. |

---

## 1. Masalah yang Diselesaikan

Dokter IGD memerlukan sarana untuk menetapkan tindak lanjut (disposisi) pasien langsung dari Ruang Kerja Dokter IGD (`doctor-emergency`). Berdasarkan konsensus klinis (`IGD-DEC-223`, `IGD-DEC-226`):
1. Setiap pembuatan tindak lanjut baru wajib berstatus awal `Draft` (nilai `1`).
2. Konfirmasi tindak lanjut (`Draft` $\rightarrow$ `Confirmed`) hanya boleh dilakukan jika pasien sudah memiliki diagnosis kerja ICD-10 pada catatan dokter. Jika belum, penolakan server (`409 Conflict`) wajib ditampilkan di dalam modal konfirmasi tanpa menutup modal, disertai tombol/tautan langsung ke tab *Catatan Dokter*.
3. Dokter dapat membatalkan tindak lanjut (`Draft` atau `Confirmed` $\rightarrow$ `Cancelled`) dengan mencantumkan alasan pembatalan.
4. Dokter **tidak** melakukan pelaksanaan tindak lanjut (aksi *Jalankan* / `Executed` tidak tersedia di layar dokter, karena menjadi wewenang perawat pada `FE-IGD-055`).

---

## 2. Perubahan yang Dikerjakan

1. `src/lib/constants/health-services/emergency-installation-management/emergency-assessment-constant.jsx`:
   - Menambahkan `DOCTOR_DISPOSITION_STATUS_ACTIONS`:
     - Pada `DRAFT`: aksi *Konfirmasi* (`target: CONFIRMED`) dan *Batalkan* (`target: CANCELLED`, wajib alasan).
     - Pada `CONFIRMED`: aksi *Batalkan* (`target: CANCELLED`, wajib alasan). Tidak ada aksi *Jalankan*.
   - Menambahkan `NURSE_DISPOSITION_STATUS_ACTIONS` untuk persiapan `FE-IGD-055`:
     - Pada `DRAFT`: tidak ada aksi.
     - Pada `CONFIRMED`: aksi *Jalankan* (`target: EXECUTED`).
2. `src/components/view/health-services/emergency-installation-management/doctor-emergency/tabs/emergency-doctor-disposition-tab.jsx`:
   - Membuat komponen tab mandiri dokter IGD:
     - Membaca konteks dokter dari `usePhysicianWorkspaceContext()` (`visitId`, `encounterId`, `patientId`, `writeAccess`, `canWrite`, `setActiveTab`, `refreshVisit`).
     - Mengambil data disposisi dan master options melalui Redux `emergencyAssessment` slice (`dispositions`, `masterOptions`).
     - **Formulir Simpan Draf (Acceptance 1)**: Selalu mengirim `dispositionStatus: DISPOSITION_STATUS.DRAFT` (nilai `1`).
     - **Modal Konfirmasi & Penanganan 409 (Acceptance 2)**:
       - Modal konfirmasi tetap terbuka jika server menolak permintaan status.
       - Pesan galat dari server ditampilkan apa adanya via `InformationAlert variant="danger"`.
       - Jika pesan penolakan menyebutkan diagnosis kerja, menyajikan tombol aksi `Buka Tab Catatan Dokter →` yang secara otomatis menutup modal dan mengalihkan tab aktif ke `doctor-note` (`setActiveTab("doctor-note")`).
     - **Pembaruan Status Real-time (Acceptance 3)**: Saat konfirmasi berhasil, modal tertutup, data riwayat dimuat ulang (`reload()`), dan status langsung terbaca *Dikonfirmasi* tanpa memuat ulang seluruh halaman.
     - **Pembatalan & Pencegahan Aksi Tidak Sah (Acceptance 4)**: Aksi *Batalkan* mewajibkan input alasan; aksi *Jalankan* tidak ditampilkan pada layar dokter.
3. `src/components/view/health-services/emergency-installation-management/doctor-emergency/doctor-emergency-view.jsx`:
   - Mengimpor `EmergencyDoctorDispositionTab` dan meregistrasikannya ke `TAB_COMPONENTS` pada kunci `disposition`.
   - Tab *Tindak Lanjut* otomatis tampil pada bilah navigasi tab dokter IGD.

---

## 3. Gerbang Base Component (UI Decision Gate)

```
UI GATE: 4 elemen — REUSE 3, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat Base | Bukti | Status | Rekomendasi |
| :--- | :--- | :--- | :---: | :--- |
| Modal konfirmasi status tindak lanjut | `ConfirmModal` | `src/components/features/base-features/confirm-modal.jsx` | REUSE | Dipakai apa adanya |
| Pesan penolakan 409 di dalam modal | `InformationAlert` | `src/components/features/base-features/information-alert.jsx` | REUSE | `variant="danger"` |
| Formulir & field tindak lanjut | `BaseSelectField`, `BaseTextField`, `BaseTextAreaField`, `BaseSimpleCheckbox` | `src/components/features/base-features/` | REUSE | Standar base component |
| Rangkaian modal konfirmasi + alert error + tombol pintas | `ConfirmModal` + `InformationAlert` + `BaseButton` | Rangkaian pola `FE-IGD-043` | COMPOSE | Dirangkai di dalam `EmergencyDoctorDispositionTab` |

---

## 4. Kriteria Penerimaan & Status

| # | Kriteria | Status | Bukti |
| ---: | --- | :---: | --- |
| 1 | Tindak lanjut baru tersimpan sebagai draf (layar selalu mengirim `dispositionStatus` `1`) | ✅ | Kode `submit` secara eksplisit menetapkan `dispositionStatus: DISPOSITION_STATUS.DRAFT` (nilai `1`) |
| 2 | Konfirmasi tanpa diagnosis: pesan `409` apa adanya di modal; status tetap draf; tautan ke tab Catatan Dokter bekerja | ✅ | `runAction` tidak menutup modal saat gagal; pesan `result.payload?.message` tampil di `InformationAlert`; tombol `Buka Tab Catatan Dokter →` memicu `setActiveTab("doctor-note")` |
| 3 | Sesudah diagnosis ditambahkan, konfirmasi berhasil dan status terbaca *Dikonfirmasi* tanpa memuat ulang halaman | ✅ | Pada `updateDispositionStatus.fulfilled`, modal ditutup, `reload()` dipanggil, data termutakhir langsung ter-render dengan badge `Dikonfirmasi` |
| 4 | Batalkan meminta alasan; aksi *Jalankan* tidak tersedia di layar dokter | ✅ | `requiresReason: true` pada aksi `Batalkan`; `DOCTOR_DISPOSITION_STATUS_ACTIONS` tidak memiliki entri `EXECUTED` |
| 5 | Diff, komentar, akhiran baris, `globals.css`; `eslint` 0 error; `npm run build` siap diuji | ✅ | Diff 3 berkas bersih; 0 komentar baru ditambahkan; format baris CRLF konsisten; `npx eslint` lulus 0 error |

---

## 5. Verifikasi Engineering

| Pemeriksaan | Hasil | Keterangan |
| :--- | :---: | :--- |
| **Cakupan Berkas** | **PASS** | 3 berkas (`emergency-assessment-constant.jsx`, `emergency-doctor-disposition-tab.jsx`, `doctor-emergency-view.jsx`) |
| **Komentar Baru** | **PASS** | 0 baris komentar baru (`//`, `///`, `/* */`) |
| **Line Endings & Whitespace** | **PASS** | `git diff --check` lulus 0 warning/error |
| **ESLint Check** | **PASS** | `npx eslint` lulus 0 error pada seluruh berkas terdampak |
