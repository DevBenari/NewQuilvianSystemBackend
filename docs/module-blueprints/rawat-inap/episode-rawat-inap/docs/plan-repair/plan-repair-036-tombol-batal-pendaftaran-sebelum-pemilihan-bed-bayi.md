# PLAN-REPAIR-036 — Penambahan Tombol Batal Pendaftaran pada Tahapan Sebelum Pemilihan Bed Admisi Bayi

```yaml
plan_id: PLAN-REPAIR-EPS-036
issue: ../issue/issue-036-tombol-batal-pendaftaran-sebelum-pemilihan-bed-bayi.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-22"
diputuskan_oleh: "Pemilik Sistem"
tanggal_keputusan: "2026-10-22"
tanggal_selesai: "2026-10-22"
basis_source_backend: "6406fc76 (MHamzah)"
basis_source_frontend: "381ec25fa (HamzahV2)"
target_database: "QuilvianNewDevHamzah"
```

Dokumen ini memuat rencana perbaikan untuk menyelaraskan alur pembatalan lokal pada admisi bayi baru lahir agar mengikuti standar admisi rawat inap reguler: tombol **`Batal Pendaftaran`** hadir pada tahapan sebelum pemesanan bed tanpa menuntut alasan pembatalan.

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Tindakan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :--- | :---: | :--- |
| `FIX-EPS-036-01` | `ISS-EPS-036` | Meneruskan prop `onResetAdmission` ke `NewbornContextStep`, `NewbornIdentityStep`, `NewbornCareStep`, dan `NewbornPlacementStep` pada `InpatientAdmissionView` | Frontend | P1 (Tinggi) | `FIX-EPS-036` | ✅ SELESAI | `inpatient-admission-view.jsx` menghubungkan `onResetAdmission` ke seluruh langkah awal bayi |
| `FIX-EPS-036-02` | `ISS-EPS-036` | Memperbarui helper `Actions` dengan tata letak `justifyContent: "space-between"` dan tombol `Batal Pendaftaran` (`variant="ghost"`) di sebelah kiri | Frontend | P1 (Tinggi) | `FIX-EPS-036` | ✅ SELESAI | `inpatient-newborn-admission-steps.jsx` merender tombol batal pada Langkah 1 dan 2 |
| `FIX-EPS-036-03` | `ISS-EPS-036` | Memasang prop `onResetAdmission` pada `InpatientAdmissionDoctorStep` (Langkah 3) dan `InpatientAdmissionBedSelectionStep` (Langkah 4) | Frontend | P1 (Tinggi) | `FIX-EPS-036` | ✅ SELESAI | Tombol `Batal Pendaftaran` terhubung pada Langkah 3 dan Langkah 4 fase pilih bed |
| `FIX-EPS-036-04` | `ISS-EPS-036` | Memperbarui suite pengujian unit dan menambahkan test otomatis verifikasi tombol Batal Pendaftaran | Test | P1 (Tinggi) | `FIX-EPS-036` | ✅ SELESAI | `inpatient-newborn-admission.test.mjs` 17/17 test lulus 100% |

**Ringkasan: 4 dari 4 perbaikan selesai (100%).**

---

## 2. Skema Tampilan Alur Pembatalan

```carousel
### Tampilan Bilah Aksi Langkah 1, 2, 3, dan 4 (Fase Pilih Bed)
```text
+-------------------------------------------------------------------------------+
| [Area Konten Langkah: Pilih Ibu / Identitas Bayi / DPJP / Pemilihan Bed]      |
+-------------------------------------------------------------------------------+
[Batal Pendaftaran] (Ghost)                      [Kembali]  [Simpan / Lanjut ->]
```
<!-- slide -->
### Dialog Konfirmasi Batal Pendaftaran (Lokal — Tanpa Alasan)
```text
+-------------------------------------------------------------------------------+
| [WARNING] Batalkan Pendaftaran?                                               |
| Isian formulir pendaftaran ini akan dihapus dan kembali ke menu awal.         |
| Apakah Anda yakin ingin membatalkan?                                          |
|                                                                               |
| [Kembali]                                                   [Ya, Batalkan]    |
+-------------------------------------------------------------------------------+
*(Tanpa field isian alasan — langsung mereset state dan kembali ke menu awal)*
```
<!-- slide -->
### Dialog Konfirmasi Batalkan Admisi (Server — Pasca-Pemesanan Bed)
```text
+-------------------------------------------------------------------------------+
| [DANGER] Batalkan Admisi Rawat Inap?                                          |
| Alasan Pembatalan:                                                            |
| +---------------------------------------------------------------------------+ |
| | [Textarea alasan pembatalan minimal 10 karakter...]                       | |
| +---------------------------------------------------------------------------+ |
|                                                                               |
| [Kembali]                                                [Batalkan Admisi]    |
+-------------------------------------------------------------------------------+
*(Melepaskan reservasi bed dan membatalkan episode resmi di server)*
```
````

---

## 3. Bukti Pengujian Unit Otomatis

Pengujian dijalankan melalui Node Test Runner:

```text
✔ FE-RWI-257 / FIX-EPS-028: opsi kartu Admisi Bayi Baru Lahir aktif pada menu utama
✔ FE-RWI-257: kandidat ibu memakai endpoint kasus persalinan
✔ FE-RWI-257 & FIX-EPS-035: alur bayi tepat delapan langkah dengan kartu pasien tanpa penjamin dan deposit pada mode newborn
✔ FE-RWI-258: identitas bayi mewajibkan kasus, tanggal, jam, dan jenis kelamin
✔ FE-RWI-258: payload bayi tidak mengirim nama atau urutan manual
✔ FE-RWI-259: konteks episode tidak membawa keputusan invoice/deposit
✔ FE-RWI-259: rawat gabung wajib episode ibu dan semua perawatan wajib DPJP
✔ FE-RWI-259: query bed mempertahankan konteks bayi sebelum episode dibuat
✔ FE-RWI-259: tampilan bayi tidak menyediakan input invoice/deposit
✔ FIX-EPS-030: validasi rawat gabung mendeteksi kamar penempatan ibu closed
✔ FIX-EPS-030: hook bed mendukung pemuatan denah mandiri dan selektivitas kamar bayi
✔ FIX-EPS-031: kartu pilihan jenis perawatan bayi menyediakan 3 opsi eksplisit (Rawat Gabung, Perinatologi, NICU)
✔ FIX-EPS-031: komponen NewbornCareStep merender 3 kartu dan meniadakan centang intensif tersembunyi
✔ FIX-EPS-033: NewbornPlacementStep merender pemilihan bed jika bed belum dipilih dan menangani kembali internal
✔ FIX-EPS-034: NewbornDocumentsStep merender kartu identitas bayi dan nomor rekam medis serta alert sukses
✔ FIX-EPS-035: NewbornDocumentsStep menyediakan tombol Lanjut ke Kartu Pasien dan view merender InpatientAdmissionPatientCardStep untuk bayi
✔ FIX-EPS-036: langkah-langkah sebelum pemilihan bed bayi menyediakan tombol Batal Pendaftaran tanpa memerlukan alasan
ℹ tests 17 | pass 17 | fail 0
```
