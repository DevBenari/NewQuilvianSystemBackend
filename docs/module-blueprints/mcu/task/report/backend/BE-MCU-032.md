# Laporan Perubahan Backend — `BE-MCU-032`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-032` |
| Judul | Ganti dokter MCU |
| Slice | `MVP-3` — `EPIC MCU-05` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-032`* |
| Trace | `FR-MCU-045`; `MCU-DEC-040`, `032`; `MCU-COORD-008` ✅; `MCU-AC-025`; `AT-MCU-39` bagian ketiga |
| Contract version | `MCU-API-v1` grup Mcu Check In (`PATCH orders/{id}/mcu-doctor`, `ReassignMcuDoctorRequest`); `MCU-VAL-v1` `VAL-MCU-40`, `45`, `46`; `MCU-PERM-v1` (`McuOrder : ReassignDoctor`); `MCU-INT-v1` `INT-MCU-06` baris *Ganti dokter MCU* — seluruhnya `approved` 2026-10-21 (sisi Lab/Rad disetujui pemiliknya, `MCU-EVD-002` bagian 1) |
| Dependency | `BE-MCU-020` ✅, `BE-MCU-030` ✅ |
| Klasifikasi | `HEAVY` — skor 9: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), memakai kontrak yang sudah ada (1), perilaku persistence modul mitra (1), keamanan — wewenang klinis pindah tangan (2), lintas tiga modul (2) |
| Task mode | `BACKEND` |
| Target tulis | MCU: `McuCheckInService.cs`, `McuCheckInController.cs`, `McuCheckInDtos.cs`, `McuOperationResult.cs`. Mitra: `LabOrderService.cs` (Lab), `RadOrderService.cs` (Rad). Laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-23 |
| Status | Selesai — `AT-MCU-39` bagian ketiga terbukti di harness integrasi |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE`; modul mitra `LaboratoryManagement` (`Lab`), `RadiologyManagement` (`Rad`) |
| Keberlakuan | `NEW CODE` (endpoint, service MCU, dua method internal baru di Lab/Rad) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (`McuOrder : ReassignDoctor`), `QBE-LOG-001` (alasan tidak masuk log), `QBE-VAL-001`, `QBE-TXN-001` (penugasan, order, pesanan Lab/Rad satu transaksi), `QBE-DTO-001` |
| Checker QBE | `PASS` — 6 berkas task nol temuan; kontrol negatif `ZzzKontrol` terkena `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`, lalu dihapus. Tiga run pertama berjalan bersamaan dengan `dotnet test` dan tidak menangkap kontrol; dijalankan sendirian, semua varian daftar menangkapnya |

---

## 1. Masalah yang diperbaiki

Dokter MCU yang ditetapkan saat check-in tidak bisa diganti. Bila dokter itu pulang sebelum resume selesai, tidak
ada dokter lain yang berhak menyusun dan menyetujui resume, dan nilai kritis pesanan Lab/Rad peserta tetap dikirim
ke dokter yang sudah tidak bertugas.

**Contoh.** dr. Rina pulang pukul 14.00 sebelum resume Siti disusun. Kepala unit mengganti ke dr. Bima dengan alasan
*dokter selesai jaga*. Pesanan Lab dan Thorax Siti yang masih berjalan kini atas nama dr. Bima; dr. Bima dapat
menyusun dan approve resume; dr. Rina ditolak `403`.

---

## 2. Proses bisnis

1. Kepala unit MCU (`McuOrder : ReassignDoctor`) memilih dokter pengganti dan mengisi alasan.
2. Syarat: order sudah check-in dan belum *Diserahkan*/*Dibatalkan*; dokter pengganti aktif dan berbeda dari dokter
   sekarang; versi order sama.
3. Satu transaksi:
   - penugasan dokter lama ditutup (waktu berakhir dicatat);
   - penugasan baru dicatat beserta alasan dan kepala unit pelakunya;
   - order menunjuk dokter baru;
   - pesanan **paket** Lab dan Rad yang masih **terbuka** berpindah dokter penanggung jawab ke dokter baru, masing-masing
     mencatat riwayat `Order.ReassignMcuDoctor` beralasan. Pesanan yang sudah selesai atau batal tetap atas nama dokter
     lama — hasilnya memang dikerjakan di bawah tanggung jawabnya.
4. Sejak itu seluruh tindakan dokter (draf/approve/buka ulang resume, tambah/batal komponen) hanya untuk dokter baru.
   Riwayat order menampilkan kedua penugasan.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Order *Diserahkan* atau *Dibatalkan* | `409` `VAL-MCU-45` *"Dokter MCU tidak dapat diganti pada order yang sudah selesai."* |
| Order belum check-in | `409` *"Dokter MCU ditetapkan saat check-in."* |
| Alasan kosong | `400` `VAL-MCU-40` *"Alasan wajib diisi."* |
| Dokter sama / tidak aktif / tidak dikenal | `400` *"Dokter pengganti sama dengan dokter MCU sekarang."* / *"Dokter pengganti tidak ditemukan atau tidak aktif."* |
| Versi order berbeda; dua penggantian serentak | `409` `VAL-MCU-46` |
| Lab/Rad menolak karena pesanan diubah bersamaan | `409` `MCU-PARTNER-REJECTED`; penugasan dan order kembali seperti semula |
| Tanpa izin `ReassignDoctor` | `403` dari `[AccessPermission]` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/backend-roadmap.md` (`BE-MCU-032`), `contracts/api-contract.md` (grup Mcu Check In), `validation-matrix.md`
(`VAL-MCU-40`, `45`), `integration-contract.md` (`INT-MCU-06`), `permission-audit-matrix.md`, `testing/acceptance-test-matrix.md`
(`AT-MCU-39`), `00-interview-decisions.md` (`MCU-DEC-032`, `040`, `MCU-COORD-008`), laporan `BE-MCU-020`, `McuOrderDoctorAssignment`
dan konfigurasinya (index unik penugasan berlaku), `McuCheckInService` (penugasan saat check-in), `McuCheckInController`,
`LabOrderService` (`CreateForMcuAsync`, `InstructingDoctorId`, `SaveWithConcurrencyGuardAsync`), `LabSpecimenService.AppendHistory`,
`RadOrderService` (`CreateForMcuAsync`, `Touch`, `AddHistory`, penjaga konkurensi).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/Services/McuCheckInService.cs` | `ReassignDoctorAsync`; dependensi `LabOrderService`, `RadOrderService` |
| `MedicalCheckupManagement/Controllers/McuCheckInController.cs` | `PATCH mcu-doctor`, izin `McuOrder : ReassignDoctor`; log tanpa alasan |
| `MedicalCheckupManagement/DTOs/McuCheckInDtos.cs` | `ReassignMcuDoctorRequest` (doctorId, reason, version) |
| `MedicalCheckupManagement/Services/McuOperationResult.cs` | Kode `VAL-MCU-45` |
| `LaboratoryManagement/Services/LabOrderService.cs` | `ReassignMcuDoctorAsync` (`internal`): pesanan paket terbuka → `InstructingDoctorId` baru, versi naik, riwayat beralasan |
| `RadiologyManagement/Services/RadOrderService.cs` | `ReassignMcuDoctorAsync` (`internal`): idem untuk pesanan Rad |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `PATCH /orders/{id}/mcu-doctor` baru, sesuai `MCU-API-v1`. Endpoint Lab/Rad tidak berubah |
| Database | Tanpa migration. **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Auth | Wewenang klinis atas peserta berpindah tangan: dokter lama kehilangan seluruh tindakan dokter, dokter baru mendapatkannya. Izin `McuOrder : ReassignDoctor` diberikan per departemen-posisi di `SysAccessPolicies` — pemberiannya ke jabatan kepala unit MCU di devYoga belum diperiksa |
| Modul mitra | Lab dan Rad: method `internal` baru saja, hanya dipanggil MCU; jalur yang ada tidak berubah |

### 3.4 Delta terhadap kontrak dan roadmap

| Butir | Roadmap/kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Reuse | *Jalur ganti dokter penanggung jawab pesanan terbuka di Lab/Rad (020)* | Jalur itu tidak ada; dibuat `ReassignMcuDoctorAsync` di Lab dan Rad | Perilakunya ditetapkan `INT-MCU-06` yang disetujui pemilik Lab/Rad; dipasang di service pemiliknya, bukan ditulis langsung dari MCU |
| Cakupan | `McuCheckInService`, controller | + `LabOrderService`, `RadOrderService` | Idem |
| Pesanan yang dipindah | *Pesanan terbuka milik order* | Pesanan ber-`CoveringMcuOrderId` = order, belum *Completed*/*Cancelled* | Pesanan di luar paket memakai jalur biasa yang tidak mengisi `InstructingDoctorId` (hanya rawat inap yang mengisinya) |
| Status yang diblokir | `VAL-MCU-45`: *Diserahkan*/*Dibatalkan* | + belum check-in (*Terjadwal*/*Tidak Hadir*) | Belum ada dokter yang ditetapkan |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Check In

Base URL: `/api/v1/health-services/medical-checkup-management/orders/{id}`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `PATCH` | `/mcu-doctor` | Kepala unit mengganti dokter MCU; pesanan paket terbuka ikut pindah | `McuOrder : ReassignDoctor` | `ReassignMcuDoctorRequest` (doctorId, reason, version) | `McuOrderDetailResponse` |

Kode status: `200`, `400`, `403`, `404`, `409`. Log aplikasi: nomor order dan nama dokter baru — tanpa alasan.

---

## 5. Verifikasi

Harness uji **sementara** dipulihkan dari salinan `BE-MCU-031` (SQLite, service produksi MCU/Lab/Rad/Billing/Registrasi;
subclass seam check-in disesuaikan dengan konstruktor baru). Harness **dihapus** sesudahnya.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 35 lulus (6 ganti dokter + 29 regresi resume/buka ulang/batal komponen) | `PASS` | `Passed! - Failed: 0, Passed: 35` |
| `AT-MCU-39` bagian ketiga | Ganti ke dr. Bima: dua penugasan (lama berakhir, baru beralasan oleh kepala unit); pesanan Lab dan Rad terbuka → dr. Bima, masing-masing satu riwayat `Order.ReassignMcuDoctor` beralasan; dr. Rina draf/approve `403` `VAL-MCU-38`; dr. Bima draf dan approve berhasil | `PASS` | `AT_MCU_39_sesudah_ganti_dokter_…` |
| Pesanan selesai | Lab/Rad *Completed* sebelum penggantian tetap atas nama dr. Rina; tanpa riwayat Lab; order → dr. Bima | `PASS` | `Pesanan_yang_sudah_selesai_…` |
| `VAL-MCU-45` | Order *Diserahkan* dan *Dibatalkan* → `409` pesan persis; penugasan dan dokter tidak berubah | `PASS` | `Order_Diserahkan_atau_Dibatalkan_409` |
| Validasi | Alasan kosong `VAL-MCU-40`; dokter sama; dokter tidak dikenal; versi lama `VAL-MCU-46`; nol perubahan | `PASS` | `Validasi_alasan_dokter_dan_versi` |
| Ganti dua kali | 3 penugasan, tepat satu berlaku; pesanan Rad kembali ke dr. Rina | `PASS` | `Ganti_dokter_dua_kali_…` |
| Kontrak endpoint | `PATCH mcu-doctor`, izin `McuOrder : ReassignDoctor` | `PASS` | `Endpoint_PATCH_mcu_doctor_…` |
| Uji mutasi MCU | Panggilan Lab, `VAL-MCU-45`, penutupan penugasan lama, dan pemindahan dokter di order dilepas → 4 uji **gagal**; dipulihkan identik (`cmp`) | `PASS` | Keluaran `dotnet test` |
| Uji mutasi Rad | Filter *terbuka* di Rad dilepas → `Pesanan_yang_sudah_selesai_…` **gagal** (pesanan selesai ikut pindah); dipulihkan identik | `PASS` | Keluaran `dotnet test` |
| Checker QBE `-Path` (6 berkas + kontrol) | Nol temuan; kontrol terkena 3 aturan | `PASS` | Keluaran checker |
| PostgreSQL (index unik penugasan berlaku), `403` lewat HTTP, nilai kritis sampai ke dokter baru | Tidak dijalankan; pemberitahuan nilai kritis Lab masih menunggu `LAB-COORD-017` | `NOT RUN` | — |

Uji manual: `NOT FEASIBLE` di sesi ini.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MCU-39` bagian ketiga — sesudah penggantian, dokter pengganti approve berhasil dan dokter lama `403` | Terpenuhi | §5 |
| Order *Diserahkan* → `409` *"Dokter MCU tidak dapat diganti pada order yang sudah selesai."* | Terpenuhi | §5 |
| Riwayat dokter bertambah; pesanan Lab/Rad terbuka berpindah `InstructingDoctorId` | Terpenuhi | §5 |
| DoD: kriteria terbukti | Terpenuhi | §5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Checker QBE yang berjalan bersamaan dengan `dotnet test` tidak menangkap kontrol negatif — sebab inilah yang juga terjadi di `BE-MCU-030`/`031`; laporan keduanya sudah dikoreksi |
| Masalah yang diketahui | Resume yang sudah disetujui dokter lama tetap atas nama dokter lama; bila perlu diubah, dokter baru membuka ulang (`BE-MCU-031`) |
| Risiko tersisa | Index unik penugasan berlaku hanya teruji lewat urutan kode (dua `SaveChanges`), bukan PostgreSQL. Hak akses kepala unit di devYoga belum diperiksa |
| Perubahan sampingan | Folder uji sementara dipulihkan lalu dihapus (salinan diperbarui di scratchpad); kontrol negatif QBE dibuat lalu dihapus. Laporan `BE-MCU-030`/`031` dikoreksi pada butir sebab checker |
| Interupsi | Skrip mutasi gabungan berhenti di penanda Rad (CRLF) sesudah mutasi MCU terpasang; uji dijalankan, kedua berkas dipulihkan identik, lalu mutasi Rad diulang terpisah |
| Status Git | Baru dari task ini: berkas di folder `MedicalCheckupManagement/` (`??`); hunk baru di `LabOrderService.cs` dan `RadOrderService.cs` (` M` sejak `BE-MCU-020`); laporan ini; koreksi laporan 030/031; tanda roadmap/traceability |
| Langkah berikutnya | `BE-MCU-033` (dokumen hasil lengkap dan ringkasan kelayakan) siap |
