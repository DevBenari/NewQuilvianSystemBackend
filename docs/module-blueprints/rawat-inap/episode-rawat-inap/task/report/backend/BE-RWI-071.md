# Laporan Perubahan Backend — `BE-RWI-071`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-071` |
| Judul | Daftar pantau kekurangan deposit |
| Slice | `S11` — Deposit dapat diterima dan ditelusuri ke episodenya; `EPIC RI-35a`, gelombang `MVP-1` |
| Roadmap | `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/backend-roadmap.md` bagian 4, kartu `BE-RWI-071` |
| Trace | `RWI-DEC-096`; `FR-RI-177`; `api-contract.md` `0.6.0` `GET /monitoring/deposit-shortfall` |
| Contract version | API `0.6.1` berlaku. Status endpoint `GET /monitoring/deposit-shortfall` kini **✅ Tersedia** |
| Dependency | `BE-BKC-040` (selesai di `BillingManagement`); `BE-RWI-070` (selesai) |
| Klasifikasi | `MEDIUM` — satu operasi baca, satu DTO query, satu DTO item paged result, satu adapter integrasi Billing, satu endpoint monitoring |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/InPatientManagement/Services/IInpBillingDepositAdapter.cs`, `InpBillingDepositAdapter.cs`, `DTOs/InpatientMonitoringDtos.cs`, `Services/InpCensusQueryService.cs`, `Controllers/InpatientMonitoringController.cs`, `Program.cs` |
| Tanggal | 17 September 2026 |
| Status | ✅ **SELESAI.** Kelima acceptance criteria terbukti terpenuhi pada kode implementasi. `dotnet build` sengaja dikecualikan (**NOT RUN**) sesuai instruksi mandiri pengguna |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, petugas rawat inap dan kasir rumah sakit tidak memiliki daftar kerja terpusat untuk memantau pasien rawat inap mana saja yang uang mukanya (deposit) masih kurang dan telah melampaui batas waktu tindak lanjut berkala. Penagihan berkala yang diwajibkan dalam kebijakan rumah sakit (`RWI-DEC-096`) tidak dapat berjalan efektif karena petugas baru mengetahui adanya kekurangan uang muka saat pasien hendak pulang (penyusunan tagihan final).

Dengan adanya perubahan ini, sistem menyediakan endpoint monitoring operasional yang secara otomatis menampilkan episode rawat inap aktif yang masih memiliki kekurangan deposit dan lama perawatannya telah melewati ambang batas hari tindak lanjut (`DepositFollowUpIntervalDays`), lengkap dengan angka kekurangan yang ditarik langsung secara konsisten dari modul Billing tanpa rekalkulasi terpisah.

**Contoh Kasus Nyata di Rumah Sakit:**
Pasien Tn. Hendra dirawat di Bangsal Melati sejak tanggal 10 September. Berdasarkan kebijakan penjamin umum dan kelas kamar, minimum deposit awal adalah Rp 5.000.000. Saat admisi, keluarga baru menyetor uang muka Rp 2.000.000 (terdapat kekurangan deposit Rp 3.000.000). Rumah sakit menetapkan ambang penagihan berkala setiap 3 hari. Pada tanggal 13 September (hari ke-3), sistem secara otomatis memunculkan Tn. Hendra pada daftar pantau kekurangan deposit beserta rincian kekurangan Rp 3.000.000. Petugas administrasi/kasir dapat langsung menghubungi keluarga untuk pelunasan deposit berkala tanpa menunggu pasien dipulangkan.

---

## 2. Proses bisnis

**Tujuan:** Memberikan daftar kerja harian kepada kasir dan staf rawat inap untuk menindaklanjuti kekurangan uang muka pasien aktif secara berkala.

**Pelaku:** Petugas kasir rawat inap, petugas administrasi bangsal, atau manajer keuangan pemegang izin `InpatientMonitoring : Read`.

**Pemicu:** Petugas membuka layar atau memanggil API monitoring kekurangan deposit.

**Alur Langkah Runtut:**
1. Sistem menerima kueri penyaringan dari pemanggil (dapat difilter per unit layanan/bangsal serta didukung penomoran halaman/paging).
2. Sistem membaca ambang batas hari tindak lanjut penagihan dari pengaturan rawat inap (`InpSettingService.GetEffectiveSettingAsync`), yang memiliki nilai bawaan 3 hari dan dapat disesuaikan oleh administrator rumah sakit (`BE-RWI-070`).
3. Sistem mengambil daftar episode rawat inap aktif (`Admitted` dan `DischargePending`) yang belum dihapus (`!IsDelete`).
4. Untuk setiap episode aktif, sistem menghitung lama hari rawat inap (`LengthOfStayDays`) menggunakan perhitungan selisih tanggal kalender (`InpCensusQueryService.CalculateLengthOfStayDays`).
5. **Penyaringan Ambang Waktu (Kriteria 2):** Episode yang lama rawatnya belum mencapai ambang hari tindak lanjut (`LengthOfStayDays < ThresholdDays`) langsung disaring keluar dari daftar.
6. **Integrasi Otoritatif Billing (Kriteria 4 & 5):** Untuk episode yang melewati ambang, sistem memanggil `IInpBillingDepositAdapter.GetDepositSummaryAsync` yang membaca data ringkasan deposit dari `BillingDepositService.GetEpisodeDepositSummaryAsync`.
   - **Jalur Gagal-Aman (Kriteria 5):** Jika data Billing tidak dapat diakses atau terjadi kegagalan jaringan/layanan, sistem menandai `BillingDataAvailable = false` dan menyertakan pesan kendala `BillingUnavailableReason`, serta **tidak** menampilkan angka nol rupiah yang dapat menyesatkan petugas seolah-olah deposit sudah lunas.
   - **Jalur Normal (Kriteria 1 & 3):** Bila data berhasil dibaca, sistem memeriksa nilai `PolicyShortfallAmount`. Jika kekurangan deposit > 0, episode ditampilkan pada daftar pantau. Sebaliknya, jika kekurangan sudah dilunasi (`shortfall <= 0`), episode secara otomatis hilang dari daftar tanpa mengubah catatan transaksi lama.
7. Sistem mengembalikan data terstruktur dalam format bertingkat (*paged result*) lengkap dengan metadata total data dan halaman.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Tujuan pemeriksaan |
| --- | --- |
| `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/backend-roadmap.md` | Memverifikasi scope, dependency, dan kelima acceptance criteria `BE-RWI-071` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingDepositService.cs` | Memastikan ketersediaan dan kontrak `GetEpisodeDepositSummaryAsync` yang mendarat dari `BE-BKC-040` |
| `Areas/HealthServices/InPatientManagement/Services/InpSettingService.cs` | Memverifikasi pembacaan kolom ambang hari `DepositFollowUpIntervalDays` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientMonitoringController.cs` | Memeriksa pola route, atribut akses, dan struktur controller monitoring |
| `Areas/HealthServices/InPatientManagement/Services/InpCensusQueryService.cs` | Memeriksa pola query census dan integrasi kalkulasi lama dirawat |

### 3.2 Berkas yang dibuat dan diubah

| Berkas | Jenis | Perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/IInpBillingDepositAdapter.cs` | Baru | Kontrak interface adapter pembacaan deposit dan DTO `InpEpisodeDepositSummaryDto` dengan penanda fail-safe `IsDataAvailable` |
| `Areas/HealthServices/InPatientManagement/Services/InpBillingDepositAdapter.cs` | Baru | Implementasi adapter yang memanggil `BillingDepositService.GetEpisodeDepositSummaryAsync` dengan penanganan exception terstruktur |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientMonitoringDtos.cs` | Diubah | Penambahan DTO `DepositShortfallQuery`, `DepositShortfallItemResponse`, dan `DepositShortfallPagedResult` |
| `Areas/HealthServices/InPatientManagement/Services/InpCensusQueryService.cs` | Diubah | Injeksi adapter pada constructor dan penambahan method bisnis `GetDepositShortfallAsync` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientMonitoringController.cs` | Diubah | Penambahan endpoint `GET deposit-shortfall` beranotasi Swagger dan izin hak akses `InpatientMonitoring : Read` |
| `Program.cs` | Diubah | Pendaftaran scoped service untuk `IInpBillingDepositAdapter` dan `InpBillingDepositAdapter` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** Endpoint baru `GET /api/v1/health-services/inpatient-management/monitoring/deposit-shortfall` sesuai kontrak `0.6.0` yang kini aktif (`Tersedia`). Tidak merusak endpoint yang sudah ada |
| Database | **Nol migration.** Tidak ada tabel atau kolom baru di database Rawat Inap maupun Billing. Seluruh data diturunkan dari tabel yang sudah ada (`InpEpisode`, `MstInpatientSetting`, `BilDepositAccount`, `BilDepositMovement`) |
| Keamanan/Auth | Terproteksi atribut `[Authorize]`, `[AccessAction("Read", ...)]`, dan `[AccessPermission("InpatientMonitoring", "Read")]` sesuai matriks otorisasi baku sistem Quilvian |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Monitoring

| Method | Path | Deskripsi | Hak Akses | Request Query | Response Payload |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/health-services/inpatient-management/monitoring/deposit-shortfall` | Menampilkan daftar pantau episode aktif yang depositnya masih kurang dan telah melewati ambang hari | `InpatientMonitoring : Read` | `DepositShortfallQuery` | `ApiResponse<DepositShortfallPagedResult>` |

**Parameter Query:**
- `serviceUnitId` (Guid, opsional): Penyaringan berdasarkan unit layanan/bangsal rawat inap tertentu.
- `pageNumber` (int, default: 1): Nomor halaman yang diminta.
- `pageSize` (int, default: 25): Jumlah data per halaman.

**Contoh Respons Sukses (JSON):**
```json
{
  "status": 200,
  "message": "Daftar pantau kekurangan deposit berhasil diambil.",
  "data": {
    "pageNumber": 1,
    "pageSize": 25,
    "totalData": 1,
    "totalPage": 1,
    "items": [
      {
        "episodeId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
        "episodeNumber": "RI-202609-0001",
        "patientId": "1a2b3c4d-0000-0000-0000-000000000001",
        "patientName": "Tn. Hendra",
        "medicalRecordNumber": "RM-00129",
        "serviceUnitId": "2b3c4d5e-0000-0000-0000-000000000002",
        "serviceUnitName": "Bangsal Melati",
        "bedName": "Bed Melati 01",
        "roomName": "Kamar 101",
        "admittedAt": "2026-09-10T08:00:00Z",
        "lengthOfStayDays": 7,
        "thresholdDays": 3,
        "minimumPolicyAmount": 5000000.0,
        "totalReceived": 2000000.0,
        "shortfallAmount": 3000000.0,
        "billingDataAvailable": true,
        "billingUnavailableReason": null,
        "followUpDue": true
      }
    ]
  }
}
```

**Contoh Respons Jalur Gagal-Aman (Billing Tidak Tersedia):**
```json
{
  "status": 200,
  "message": "Daftar pantau kekurangan deposit berhasil diambil.",
  "data": {
    "pageNumber": 1,
    "pageSize": 25,
    "totalData": 1,
    "totalPage": 1,
    "items": [
      {
        "episodeId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
        "episodeNumber": "RI-202609-0001",
        "patientName": "Tn. Hendra",
        "lengthOfStayDays": 7,
        "thresholdDays": 3,
        "shortfallAmount": null,
        "billingDataAvailable": false,
        "billingUnavailableReason": "Data deposit episode tidak ditemukan di Billing: Episode rawat inap belum memiliki akun deposit.",
        "followUpDue": true
      }
    ]
  }
}
```

---

## 5. Verifikasi

| Skenario | Hasil | Klasifikasi | Bukti |
| :--- | :--- | :--- | :--- |
| Kriteria 1: Episode aktif dengan kekurangan deposit > 0 muncul pada daftar | Terpenuhi | `PASS` | Logika `activeEpisodes` menyaring `Admitted` & `DischargePending`, mengecek `shortfall > 0`, lalu menambahkan item ke daftar |
| Kriteria 2: Episode dengan lama rawat < ambang batas hari TIDAK muncul | Terpenuhi | `PASS` | Pengecekan eksplisit `if (losDays < thresholdDays) continue;` pada iterasi episode |
| Kriteria 3: Episode yang kekurangannya sudah lunas (<= 0) hilang dari daftar tanpa mutasi data lama | Terpenuhi | `PASS` | Hanya episode dengan `shortfall > 0` yang dimasukkan; operasi murni read-only tanpa mengubah ledger Billing |
| Kriteria 4: Angka kekurangan sama persis dengan ringkasan Billing | Terpenuhi | `PASS` | Nilai `ShortfallAmount` diambil langsung dari `depositSummary.PolicyShortfallAmount` dari `BillingDepositService` |
| Kriteria 5: Bila ringkasan Billing gagal dibaca, status ditandai tidak tersedia dan tidak menampilkan nol | Terpenuhi | `PASS` | Percabangan `if (!depositSummary.IsDataAvailable)` menghasilkan `BillingDataAvailable = false` dan `ShortfallAmount = null` |
| `dotnet build` | Sengaja dikecualikan atas instruksi eksplisit pengguna | `NOT RUN` | Pemilik sistem akan melakukan kompilasi mandiri di workstation |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti Implementasi |
| :--- | :--- | :--- |
| 1. Episode aktif yang kekurangannya di atas nol muncul pada daftar | **Terpenuhi** | `InpCensusQueryService.cs` (`matchedItems.Add(...)` saat `shortfall > 0`) |
| 2. Episode yang lama rawatnya belum melewati ambang tidak muncul | **Terpenuhi** | `InpCensusQueryService.cs` (`if (losDays < thresholdDays) continue;`) |
| 3. Episode yang kekurangannya sudah tertutup hilang dari daftar tanpa transaksi lama berubah | **Terpenuhi** | Saringan `shortfall > 0` mengabaikan saldo yang sudah tertutup |
| 4. Angka kekurangan pada daftar sama persis dengan ringkasan Billing | **Terpenuhi** | Ditarik melalui `IInpBillingDepositAdapter` dari `BillingDepositService` |
| 5. Bila ringkasan Billing tidak dapat dibaca, daftar menyatakan datanya tidak tersedia | **Terpenuhi** | Mengembalikan `BillingDataAvailable = false` dengan `ShortfallAmount = null` dan alasan kendala |

**Definition of Done:**
- Endpoint `GET /monitoring/deposit-shortfall` terpasang: ✅
- DTO query dan respons lengkap terdefinisi: ✅
- Integrasi adapter fail-safe terimplementasi: ✅
- Registrasi container DI terpasang di `Program.cs`: ✅
- Kelima kriteria terverifikasi pada kode: ✅

---

## 7. Catatan penutup

Task `BE-RWI-071` telah selesai diimplementasikan secara tuntas pada sisi backend modul Rawat Inap dengan prinsip *zero false zero* (mencegah salah tafsir angka nol saat data tidak tersedia) serta arsitektur adapter yang aman dari kegagalan lintas modul. Modul Rawat Inap siap melanjutkan penyelesaian task penutup kelayakan keuangan `BE-RWI-072`.
