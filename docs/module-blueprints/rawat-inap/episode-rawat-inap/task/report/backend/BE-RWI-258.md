# Laporan Perubahan Backend — `BE-RWI-258`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-258` |
| Judul | Guard ketersediaan registrasi service unit pada kueri denah kamar admisi, dekomposisi pesan PostgresException, dan sinkronisasi urutan lahir terhadap MstPatient |
| Slice | Admisi Rawat Inap (`FE-INP-03` / `BE-INP-03`) — Penempatan & Pendaftaran Bayi Baru Lahir |
| Roadmap | [roadmap-repair-044.md](../../roadmap/roadmap-repair-044.md) |
| Trace | [ISSUE-EPS-044](../../docs/issue/issue-044-temuan-pengujian-admisi-bayi-baru-lahir.md), [PLAN-REPAIR-EPS-044](../../docs/plan-repair/plan-repair-044-temuan-pengujian-admisi-bayi-baru-lahir.md) (`FIX-EPS-044-01`, `FIX-EPS-044-02`) |
| Contract version | Selaras `POST /patient-encounters/admin` dan `POST /patients/newborns` |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — 2 berkas backend (`InpBedOccupancyService.cs`, `NewbornPatientRegistrationService.cs`) + DML perbaikan data master service unit |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs`, `NewQuilvianSystemBackend/Areas/HealthServices/PatientManagement/MasterData/Services/NewbornPatientRegistrationService.cs` |
| Branch | `MHamzah` (SHA basis `396c8363`) |
| Tanggal | 2026-11-02 |
| Status | ✅ SELESAI (Kode backend selesai, DML Master DB sukses dieksekusi, instruksi kompilasi mandiri diberikan ke pemilik) |

---

## 1. Ringkasan Perubahan

1. **Aktivasi Unit Layanan di Database:**
   - Ditemukan bahwa pada database tim `QuilvianNewDevMaster`, baris `SU-BABY-001` (Ruang Bayi) dan unit rawat inap khusus lainnya memiliki bendera `IsAvailableForRegistration = false`.
   - Dijalankan perintah update DML resmi berizin pada tabel `public."MstServiceUnit"` di database `QuilvianNewDevMaster` untuk mengaktifkan `IsAvailableForRegistration = true` pada unit `SU-BABY-001`, `SU-NICU-001`, `SU-ICU-001`, dan `SU-HCU-001`.
2. **Backend Guard pada Evaluasi Kelayakan Bed (`InpBedOccupancyService.cs`):**
   - Menambahkan konstanta kegagalan `SERVICE_UNIT_REGISTRATION_UNAVAILABLE = "SERVICE_UNIT_REGISTRATION_UNAVAILABLE"`.
   - Pada metode `EvaluatePlacementEligibilityCore`, bila konteks penempatan adalah reservasi admisi (`InpPlacementContext.Reservation`), dilakukan validasi terhadap `room.ServiceUnit.IsAvailableForRegistration`. Bila unit tidak aktif/tersedia untuk registrasi, bed ditandai tidak eligible dengan pesan penolakan yang jelas sebelum petugas mencoba memesan bed.
3. **Dekomposisi Galat Database & Sinkronisasi Urutan Lahir (`NewbornPatientRegistrationService.cs`):**
   - Memperbaiki penentuan sekuens kelahiran (`birthOrder`): Menghitung `maxExistingBirthOrder` langsung dari tabel `_dbContext.MstPatients` berdasarkan `DeliveryCaseId` sebelum penetapan nomor urut, mencegah terjadinya tabrakan indeks unik `UX_MstPatient_DeliveryCase_BirthOrder`.
   - Mengganti penanganan `DbUpdateException`: Menginspeksi `ex.InnerException as Npgsql.PostgresException`. Jika constraint unik spesifik (`UX_MstPatient_DeliveryCase_BirthOrder`, `UX_MstPatient_NewbornRegistrationKeyHash`, `UX_MstPatient_MedicalRecordNumber`) terlanggar, pesan error deskriptif dalam Bahasa Indonesia yang akurat dikembalikan ke klien, bukan pesan generik 409 persaingan modifikasi.

---

## 2. Berkas yang Diubah

1. `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs`
   - Penambahan kode failure `SERVICE_UNIT_REGISTRATION_UNAVAILABLE`.
   - Pengecekan `room.ServiceUnit.IsAvailableForRegistration` pada context `Reservation`.
2. `NewQuilvianSystemBackend/Areas/HealthServices/PatientManagement/MasterData/Services/NewbornPatientRegistrationService.cs`
   - Penyelarasan kueri `maxExistingBirthOrder` dari `MstPatients`.
   - Penanganan khusus `Npgsql.PostgresException` untuk constraint unik database.

---

## 3. Bukti Verifikasi

1. **Verifikasi Database Master (`QuilvianNewDevMaster`):**
   - Kueri verifikasi pasca-update:
     - `SU-BABY-001` (Ruang Bayi): `IsAvailableForRegistration = true`
     - `SU-NICU-001` (Ruang Perawatan Intensif Neonatus): `IsAvailableForRegistration = true`
     - `SU-ICU-001` (Ruang Perawatan Intensif Dewasa): `IsAvailableForRegistration = true`
     - `SU-HCU-001` (Ruang Rawat Menengah): `IsAvailableForRegistration = true`
2. **Kompilasi .NET:**
   - Sesuai instruksi khusus dari pemilik sistem ("skip dotnet build nanti saya lakukan secara mandiri saja"), kompilasi `dotnet build` dilewati oleh agen AI dan diserahkan secara mandiri kepada pengembang/pemilik sistem.

---

## 4. Kesimpulan & Status

Task `BE-RWI-258` berstatus **✅ SELESAI**.
Akar penyebab blocker HTTP 400 pada unit `SU-BABY-001` telah diatasi di tingkat master data dan backend guard, serta penanganan konkurensi/constraint unik pendaftaran bayi telah didekomposisi secara transparan.

