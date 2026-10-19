# PLAN-REPAIR-001 — Rencana Perbaikan Penetapan Kelas Kartu Asuransi Pasien dan Resolusi Coverage Lintas Modul

```yaml
plan_id: PLAN-REPAIR-INS-001
issue: ./issue-001-penetapan-kelas-asuransi-dan-kegagalan-coverage.md
status_rencana: MENUNGGU_PERSETUJUAN
tanggal_rencana: "2026-10-16"
diputuskan_oleh: null
tanggal_keputusan: null
basis_source_backend: "ca09cae9 (MHamzah)"
basis_source_frontend: "4d9bf608d (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Perbaikan | Area | Prioritas | Task ID | Status | Bukti / Catatan |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `FIX-INS-001-01` | `ISS-INS-001-01` | Tambahkan `PatientClassId` (FK) pada `MstPatientInsurance` & ubah UI form menjadi dropdown master kelas | BE + FE | 1 | — | BELUM DIKERJAKAN | Menunggu persetujuan K-01 |
| `FIX-INS-001-02` | `ISS-INS-001-02` | Tambahkan mekanisme fallback pencarian tarif asuransi saat kelas kunjungan belum memiliki mapping tarif spesifik | BE | 2 | — | BELUM DIKERJAKAN | Menunggu persetujuan K-03 |
| `FIX-INS-001-03` | `ISS-INS-001-03` | Otomatisasi penetapan kelas pada kunjungan IGD dan auto-populate hak kelas dari polis pasien saat registrasi | BE + FE | 1 | — | BELUM DIKERJAKAN | Menunggu persetujuan K-02 |
| `FIX-INS-001-04` | `ISS-INS-001-04` | Berikan indikator visual dan pesan informatif di UI resep/tindakan/kasir saat terjadi ketidaksesuaian kelas polis | FE | 3 | — | BELUM DIKERJAKAN | Bergantung pada FIX-INS-001-01 & 02 |

**Ringkasan: 0 dari 4 perbaikan selesai (Menunggu Persetujuan Pemilik).**

---

## 2. Solusi Terpilih Per Temuan

### A. ISS-INS-001-01 — Field Kelas pada Kartu Asuransi Pasien
* **Opsi 1**: Mempertahankan teks bebas `ClassName` dan menambahkan pencocokan string (*string fuzzy match*) di backend.
  * *Kelemahan*: Sangat rentan salah cocok (misal typo "kls 1", "Kelas satu"), lambat, dan tidak memenuhi integritas data relasional.
* **Opsi 2 (SOLUSI TERPILIH)**: Menambahkan kolom `PatientClassId (Guid?, FK ke MstPatientClass)` pada tabel `MstPatientInsurance` dan `PatientClassIdSnapshot` pada `RegPatientEncounterGuarantor`. Di frontend, ganti input teks bebas menjadi dropdown terikat ke master data kelas aktif (`MstPatientClass`).
  * *Alasan*: Menjamin integritas referensial 100%, standar arsitektur relasional baku, dan dapat langsung di-join dengan `MstInsuranceTariff`.

### B. ISS-INS-001-02 & 04 — Mesin Pencari Tarif Asuransi (`FindInsuranceTariffAsync`)
* **Opsi 1**: Mewajibkan semua provider memiliki tarif `PatientClassId = NULL`.
  * *Kelemahan*: Mengubah 1,4 juta baris data tarif existing di database rumah sakit.
* **Opsi 2 (SOLUSI TERPILIH)**: Memperbarui logika query di `InsuranceCoverageService.cs` dengan hierarki pencocokan (*cascading resolution*):
  1. Cari tarif kontrak asuransi dengan `PatientClassId == context.PatientClassId`.
  2. Bila tidak ditemukan dan encounter berjenis Outpatient/Emergency, cari tarif kontrak asuransi berkelas `RAWAT JALAN` (`CLS-0`).
  3. Bila tetap tidak ditemukan, fallback ke tarif dengan `PatientClassId IS NULL`.
  * *Alasan*: Menyelamatkan pasien agar tidak langsung divonis *NotCovered* tanpa perlu mengubah struktur 1,4 juta data tarif kontrak.

### C. ISS-INS-001-03 — Kelas Kunjungan IGD
* **SOLUSI TERPILIH**: Saat registrasi IGD memilih penjamin Asuransi:
  1. Jika kartu polis pasien memiliki `PatientClassId`, otomatis isi `RegPatientEncounter.PatientClassId` dengan hak kelas polis tersebut.
  2. Jika kartu polis belum memiliki kelas, backend menetapkan default kelas pelayanan IGD setara kelas Rawat Jalan/Umum alih-alih membiarkannya `NULL`.

---

## 3. Skema Tampilan Sebelum → Sesudah

### Form Kartu Asuransi Pasien (`FE-PAT-INS-01`)

#### Sebelum (As-Is):
```text
┌─────────────────────────────────────────────────────────────────┐
│ Informasi Polis Asuransi Pasien                                 │
├─────────────────────────────────────────────────────────────────┤
│ Nomor Polis : [ 1234567890             ]                        │
│ Nama Plan   : [ Platinum Care          ]                        │
│ Kelas       : [ Kelas 1                ] <-- Input Teks Bebas!  │
│ Kode Benefit: [ PLAN-001               ]                        │
└─────────────────────────────────────────────────────────────────┘
```

#### Sesudah (To-Be):
```text
┌─────────────────────────────────────────────────────────────────┐
│ Informasi Polis Asuransi Pasien                                 │
├─────────────────────────────────────────────────────────────────┤
│ Nomor Polis : [ 1234567890             ]                        │
│ Nama Plan   : [ Platinum Care          ]                        │
│ Hak Kelas   : [ KELAS I              ▼ ] <-- Dropdown Master!   │
│               ├─ KELAS I               │                        │
│               ├─ KELAS II              │                        │
│               ├─ KELAS III             │                        │
│               ├─ VIP                   │                        │
│               └─ VVIP                  │                        │
│ Kode Benefit: [ PLAN-001               ]                        │
└─────────────────────────────────────────────────────────────────┘
```

---

## 4. Rincian Perbaikan

### `FIX-INS-001-01` — Penambahan `PatientClassId` pada `MstPatientInsurance`
1. **Migration Backend**:
   * Tambahkan kolom `PatientClassId (uuid, nullable)` dan FK ke `MstPatientClass(Id)`.
   * Tambahkan kolom `PatientClassIdSnapshot (uuid, nullable)` pada `RegPatientEncounterGuarantor`.
2. **DTO & Service Backend**:
   * Perbarui [`PatientInsuranceDtos.cs`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientInsuranceDtos.cs) untuk menerima dan merespons `patientClassId` dan `patientClassName`.
3. **Frontend Form Editor**:
   * Perbarui [`patient-insurance-constants.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/constants/health-services/patient-management/master-data/patient-insurance-constants.jsx): ubah field `className` menjadi `patientClassId` bertipe `select`, terikat pada `patientClassOptions`.

### `FIX-INS-001-02` — Logika *Cascading Fallback* Tarif Asuransi
1. Perbarui metode `FindInsuranceTariffAsync` di [`InsuranceCoverageService.cs`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs#L585-L617):
   * Tambahkan pencarian berjenjang jika pencocokan kelas persis tidak menghasilkan baris tarif.

### `FIX-INS-001-03` — Auto-Populate Kelas saat Registrasi Kunjungan
1. Di [`PatientEncounterController.cs`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs#L2059-L2100):
   * Jika pendaftaran berpenjamin Asuransi dan `request.PatientClassId` tidak dikirim:
     * Ambil `PatientClassId` dari kartu `MstPatientInsurance` yang dipilih.
     * Jika kartu tidak memiliki kelas dan `EncounterType == Emergency`, pasangkan ke kelas Rawat Jalan / Standar.

---

## 5. Urutan Pengerjaan & Dependency

```text
FIX-INS-001-01 (Backend Migration & DTO PatientInsurance)
      │
      ├─────────────────────────────────────────┐
      ▼                                         ▼
FIX-INS-001-01 (Frontend Dropdown Kelas)   FIX-INS-001-03 (Auto-Populate Registrasi Kunjungan)
      │                                         │
      └──────────────────┬──────────────────────┘
                         ▼
                  FIX-INS-001-02 (Cascading Fallback Tarif Asuransi di Backend)
                         │
                         ▼
                  FIX-INS-001-04 (Frontend Validasi & Alert Kasir/Pelayanan)
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan | Pilihan | Rekomendasi | Menahan Perbaikan |
| :---: | :--- | :--- | :--- | :--- |
| **K-01** | Kebijakan isian kelas pada pendaftaran kartu asuransi pasien | Wajib Dropdown Master / Tetap Opsional | **Wajib Dropdown Master** | `FIX-INS-001-01` |
| **K-02** | Kelas bawaan pasien IGD penjamin asuransi | Rawat Jalan / Hak Kelas Polis | **Hak Kelas Polis (Fallback ke Rawat Jalan)** | `FIX-INS-001-03` |
| **K-03** | Penerapan cascading fallback buku tarif asuransi | Ya (Izinkan Fallback) / Tidak (Ketat Tolak) | **Ya (Izinkan Fallback)** | `FIX-INS-001-02` |

---

## 7. Verifikasi Menyeluruh (End-to-End Test Scenario)

1. **Uji Tambah Kartu Asuransi Pasien**:
   * Buka menu Master Data Asuransi Pasien.
   * Pilih Pasien, pilih Provider (misal BPJS / Allianz), pilih Hak Kelas: `KELAS I`.
   * Simpan. Verifikasi di basis data bahwa `PatientClassId` tersimpan sebagai UUID valid.
2. **Uji Registrasi Kunjungan Pasien**:
   * Daftarkan kunjungan pasien tersebut di loket Rawat Jalan dan IGD dengan penjamin kartu di atas.
   * Pastikan `PatientClassId` pada `RegPatientEncounter` terisi otomatis dengan ID `KELAS I`.
3. **Uji Evaluasi Coverage pada Order Tindakan & Obat**:
   * Dokter melakukan input tindakan (misal Nebulisasi) dan resep obat.
   * Panggil mesin penilai coverage. Pastikan sistem berhasil menemukan tarif kontrak `MstInsuranceTariff` yang cocok dengan kelas tersebut.
   * Status coverage harus bernilai **`Covered` (100% atau sesuai limit)** dan **BUKAN** `NotCovered`.
4. **Uji Billing & Kasir**:
   * Kasir membuka tagihan encounter tersebut.
   * Pastikan porsi biaya penjamin asuransi (*PrimaryAmount*) terisi sesuai persentase tanggungan dan tidak jatuh seluruhnya ke pasien.

---

## 8. Riwayat Dokumen

| Tanggal | Versi | Catatan Perubahan | Oleh |
| :---: | :---: | :--- | :--- |
| 2026-10-16 | 1.0.0 | Dokumen rencana perbaikan resmi diterbitkan berpasangan dengan ISSUE-INS-001. | AI Engineering Agent (Antigravity) |
