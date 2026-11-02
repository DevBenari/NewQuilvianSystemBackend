# PLAN-REPAIR-033 — Perbaikan Transisi Sub-Langkah Pemilihan dan Pemesanan Tempat Tidur Bayi (Langkah 4)

```yaml
plan_id: PLAN-REPAIR-EPS-033
issue: ../issue/issue-033-langkah-penempatan-bayi-terjebak-di-booking-tanpa-bed.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-22"
diputuskan_oleh: "Pemilik Sistem"
tanggal_keputusan: "2026-10-22"
tanggal_selesai: "2026-10-22"
basis_source_backend: "6406fc76 (MHamzah)"
basis_source_frontend: "381ec25fa (HamzahV2)"
target_database: "QuilvianNewDevHamzah"
```

Dokumen ini memuat rencana perbaikan frontend untuk mengatasi masalah petugas terjebak pada sub-langkah booking kosong ("Tempat tidur belum dipilih") saat melanjutkan admisi bayi pada Langkah 4 Penempatan dan Pemesanan.

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Tindakan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :--- | :---: | :--- |
| `FIX-EPS-033-01` | `ISS-EPS-033` | Restrukturisasi state `inBookingView` dan navigasi `handleBookingBack` pada `NewbornPlacementStep` agar tahap pemilihan bed wajib terbuka jika tidak ada reservasi aktif atau bed belum dipilih | Frontend | P0 (Kritis) | `FIX-EPS-033` | ✅ SELESAI | `inpatient-newborn-admission-steps.jsx` diperbarui; 14/14 unit test otomatis lolos pada `tests/unit/inpatient-newborn-admission.test.mjs` |

**Ringkasan: 1 dari 1 perbaikan selesai (100%).**

---

## 2. Rincian Perbaikan Kode Frontend

Pada `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx`:

1. Hapus variabel rentan `const ready = Boolean(preparedId || flow.resumeEpisodeId || bed.hasActiveReservation);`.
2. Gunakan state `const [inBookingView, setInBookingView] = useState(false);`.
3. Hitung visibilitas sub-langkah booking:
   ```javascript
   const isBookingReady = Boolean(
     bed.hasActiveReservation || (inBookingView && bed.selectedBed)
   );
   ```
4. Tambahkan pengendali navigasi kembali internal:
   ```javascript
   const handleBookingBack = () => {
     if (!bed.hasActiveReservation) {
       setInBookingView(false);
       return;
     }
     bed.guardBack(() => {
       setInBookingView(false);
     });
   };
   ```
5. Pada JSX `NewbornPlacementStep`:
   - Jika `!isBookingReady`: Tampilkan `<InpatientAdmissionBedSelectionStep bed={bed} onBack={flow.goBack} onNext={handleSelectionNext} />`.
   - Jika `isBookingReady`: Tampilkan `<InpatientAdmissionBedBookingStep bed={{ ...bed, canExtendReservation: false }} onBack={handleBookingBack} onNext={flow.goNext} nextLabel="Lanjut ke Persetujuan Wali" onCancelAdmission={onCancelAdmission} />`.

---

## 3. Acceptance Criteria & Verifikasi

1. **Acceptance Criteria**:
   - Membuka Langkah 4 pada episode lanjutan tanpa reservasi aktif (`flow.resumeEpisodeId` ada, `bed.selectedBed` null) menampilkan denah lantai dan daftar bed (`InpatientAdmissionBedSelectionStep`), bukan layar booking kosong.
   - Petugas dapat mengklik kamar (`BAYI-1`, `BAYI-2`, atau `NICU-1`), memilih bed, lalu menekan "Lanjut ke Pemesanan".
   - Di sub-langkah booking, jika petugas menekan "Kembali", layar kembali ke pemilihan bed di Langkah 4 (bukan terlempar ke Langkah 3).
   - Seluruh 14 unit test otomatis pada `tests/unit/inpatient-newborn-admission.test.mjs` tetap lulus 100%.

---

## 4. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-22 | Dokumen rencana perbaikan dibuat untuk restrukturisasi transisi sub-langkah Langkah 4. | `diagnose-module-issue` |
| 2026-10-22 | Perbaikan selesai diterapkan dan diverifikasi (100% SELESAI). 14/14 unit test otomatis lulus. | `build-module-frontend` |
