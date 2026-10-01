# Laporan Perubahan Backend — `BE-RWI-141`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-141` |
| Judul | Tautan SOAP ke deret tanda vital (perawat dan dokter) |
| Slice | Rencana kerja SOAP Dokter Rawat Inap Rev 2.1, gelombang 2 |
| Roadmap | [`rencana-kerja/soap/soap.md`](../../../roadmap/rencana-kerja/soap/soap.md) bagian 7.6; didaftarkan di [`backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) |
| Trace | Permintaan pemilik 2 (tanda vital dari riwayat perawat); keputusan K5 (30-09-2026); `soap.md` bagian 3.2 dan 4.2 |
| Contract version | `0.6.1` ditambah delta aditif `soap.md` bagian 4.2, disetujui pemilik 30-09-2026. **Usulan:** `0.6.2` |
| Dependency | Tidak ada. Dipakai oleh `FE-RWI-140` |
| Klasifikasi | `HEAVY` — skor 11: repository 0, berkas diperiksa 2 (> 20), berkas diubah 2 (12 berkas), logika 2, kontrak API 2, database 2 (kolom + FK + migration), keamanan 1 (penjaga jalur perawat), UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/ClinicalManagement/**`, `Migrations/**`, `Repositories/Configurations/HealthServices/TrxDoctorConsultationConfiguration.cs`, `Program.cs` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `b342ae46` (branch `MHamzah`), perubahan belum di-commit |
| Tanggal | 30 September 2026 |
| Status | ✅ Selesai di tingkat source. `dotnet build` PASS. Migration diterapkan ke `QuilvianNewDevHamzah`. Verifikasi runtime **NOT RUN** |

---

## 1. Masalah yang diperbaiki

Perawat sudah mencatat tanda vital per episode (`TrxPatientVitalSign`), tetapi SOAP dokter rawat inap V2 meminta dokter **mengetik ulang** angka itu. V1 dulu menariknya otomatis. Akibatnya:

- **Entri ganda.** Angka yang sama diketik dua kali, dan salah ketik di salah satunya tidak ketahuan.
- **Asal angka hilang.** Catatan dokter tidak tahu angka itu milik perawat siapa dan jam berapa.
- **Ukuran dokter tidak terlihat perawat.** Saat dokter mengukur sendiri (misalnya pasien baru masuk sebelum perawat sempat mengukur), perawat shift berikutnya tidak melihatnya di grafik tanda vital.

---

## 2. Proses bisnis

**Pelaku:** dokter rawat inap; perawat ruangan sebagai pembaca deret tanda vital.

**Jalur 1 — dokter memakai data perawat.**
1. Layar mengirim `sourceVitalSignId` = baris tanda vital perawat yang dipilih.
2. Backend memeriksa baris itu **sebelum** mengubah apa pun:
   - baris ada dan belum dihapus;
   - milik pasien yang sama, dan pada **episode rawat inap yang sama** (catatan non-rawat-inap: kunjungan yang sama);
   - tidak berstatus `Cancelled` atau `EnteredInError`.
3. Nilai baris itu disalin utuh ke snapshot catatan dokter (TD, nadi, RR, suhu, SpO2, BB, TB, BMI), `IsVitalSignCopiedFromAssessment = true`, dan `SourceVitalSignId` menunjuk baris perawat.
4. Bila perawat kemudian mengoreksi baris itu, snapshot catatan dokter **tidak** berubah: yang dilihat dokter saat menandatangani tetap utuh.

**Jalur 2 — dokter mengukur sendiri (K5).**
1. Layar mengirim `isDoctorMeasuredVitalSign = true` beserta nilai ukuran.
2. Backend mengganti utuh kedelapan nilai snapshot. Nilai yang dikosongkan dokter ikut kosong dan tidak mewarisi angka perawat sebelumnya.
3. Pada catatan rawat inap, backend membuat **satu** baris `TrxPatientVitalSign` milik catatan itu:
   - `VitalSignSource = DoctorConsultation`, `ConsultationId`, dan `InpEpisodeId` diisi server;
   - pengukur = dokter penulis;
   - `ObservationDateTime` = waktu pemeriksaan catatan. Visite pukul 07.40 yang baru diketik pukul 11.00 tetap tercatat 07.40 pada deret;
   - MAP, BMI, EWS, tingkat risiko, dan tanda abnormal/kritis dihitung dengan logika yang **sama** dengan jalur perawat (`PatientVitalSignCalculation`, dipindahkan apa adanya dari `PatientVitalSignController`);
   - nomor baris `VTD-########` diterbitkan `NumberSeriesAllocator` (kunci `CLI_DOCTOR_VITAL_SIGN`), bukan `Count + 1`.
4. Menyimpan draf lagi dengan angka berbeda **memperbarui baris yang sama**, tidak menambah baris.
5. Bila seluruh nilai dikosongkan, baris itu dibatalkan dan rujukan dilepas.

**Jalur tidak normal.**
- Mengirim `sourceVitalSignId` **dan** `isDoctorMeasuredVitalSign = true` sekaligus: 400 *"Pilih salah satu sumber tanda vital: data yang sudah tercatat atau pengukuran dokter."*
- Rujukan tidak ditemukan: 400 *"Tanda vital yang dirujuk tidak ditemukan."*
- Rujukan milik pasien atau perawatan lain: 400 *"Tanda vital yang dirujuk bukan milik pasien atau perawatan ini."*
- Rujukan batal atau salah catat: 400 *"Tanda vital yang dirujuk sudah dibatalkan atau ditandai salah catat."*
- Dokter beralih dari ukurannya sendiri ke data perawat: baris ukuran dokter dibatalkan dengan alasan tercatat, supaya grafik perawat tidak menyimpan ukuran yang sudah ditarik.
- Catatan dokter dibatalkan: baris ukuran dokternya ikut dibatalkan pada `SaveChanges` yang sama. Baris tidak dihapus.
- Perawat mencoba mengubah atau membatalkan baris ukuran dokter lewat layar perawat: 400 *"Tanda vital ini dicatat dari SOAP dokter dan hanya dapat diubah lewat catatan dokter tersebut."* (`code: DOCTOR_VITAL_SIGN_READ_ONLY`).
- Nomor baris gagal diterbitkan: 500 *"Nomor tanda vital dokter tidak dapat diterbitkan. Simpan ulang beberapa saat lagi."* Transaksi dibatalkan, sehingga tidak ada catatan setengah jadi.
- Setelah catatan diselesaikan, baris ukuran dokter **beku**: catatan final tidak dapat di-patch (lifecycle yang sudah ada), dan jalur perawat menolak mengubahnya.

**Contoh berangka.** Dokter menyimpan draf dengan TD 130/85, suhu 36,7. Terbentuk `VTD-00000001` dengan EWS dihitung. Sepuluh menit kemudian dokter mengoreksi suhu menjadi 37,2 dan menyimpan lagi. `VTD-00000001` diperbarui menjadi 37,2 dan tidak lahir baris kedua. Dokter lalu memilih data perawat pukul 06.10. `VTD-00000001` dibatalkan dengan alasan "Catatan dokter beralih ke tanda vital yang sudah tercatat.", dan catatan menunjuk baris perawat.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` backend; `rules/backend/` (`TASK_RULES`, `API_RULES`, `DATABASE_RULES`, `TEST_POLICY`, `engineering/BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `role-access-rules.md`, `transaction-endpoint-standard.md`); `PatientVitalSignController.cs`; `InpatientVitalSignService.cs`; `TrxPatientVitalSign.cs` dan enum `PatientVitalSignSource`, `PatientVitalSignStatus`, `EwsRiskLevel`, `ConsciousnessStatus`; `TrxDoctorConsultation.cs` dan konfigurasinya; `DoctorConsultationController.cs` dan DTO-nya; `NumberSeriesAllocator` beserta `NumberAllocationRequest`; `ApplicationDbContextModelSnapshot.cs`; `Program.cs`; migration terakhir `20260928093849_AddMasterNursingDiagnosisSdki`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Services/PatientVitalSignCalculation.cs` (baru) | Perhitungan BMI, MAP, status MAP, GCS, EWS, tingkat risiko, rekomendasi, abnormal/kritis, serta `Normalize`. Dipindahkan **apa adanya** dari controller perawat supaya kedua jalur memakai satu logika |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs` | Memanggil `PatientVitalSignCalculation` (±278 baris pindah). `PUT` dan `PATCH cancel` menolak baris milik SOAP dokter |
| `Areas/HealthServices/ClinicalManagement/Services/DoctorConsultationVitalSignService.cs` (baru) | `ResolveAsync`, `CopySnapshot`, `RecordDoctorMeasuredAsync`, `CancelOwnRowsAsync`, `GetProvenanceAsync`, `IsDoctorConsultationVitalSign` |
| `Areas/HealthServices/ClinicalManagement/Models/TrxDoctorConsultation.cs` | `Guid? SourceVitalSignId` |
| `Repositories/Configurations/HealthServices/TrxDoctorConsultationConfiguration.cs` | FK `FK_TrxDoctorConsultation_SourceVitalSignId` → `TrxPatientVitalSign(Id)` `Restrict`, index |
| `Migrations/20260930110000_AddDoctorConsultationSourceVitalSign.cs` (baru) | Kolom `uuid NULL`, index `IX_TrxDoctorConsultation_SourceVitalSignId`, FK `RESTRICT`. `Down` membalik urutannya. Tanpa berkas Designer, mengikuti kebijakan repo |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Tiga sisipan pada blok `TrxDoctorConsultation`: properti, index, relasi |
| `Program.cs` | `AddScoped<DoctorConsultationVitalSignService>()` |
| `Areas/HealthServices/ClinicalManagement/DTOs/DoctorConsultationDtos.cs` | Request create dan patch SOAP: `SourceVitalSignId`, `IsDoctorMeasuredVitalSign`. Respons: `SourceVitalSignId`. Lini masa: `VitalSignObservedAt`, `VitalSignObservedByName`, `IsDoctorMeasuredVitalSign` |
| `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` | Create: validasi sumber, snapshot, dua `SaveChanges` dalam satu transaksi (baris tanda vital dan catatan saling menunjuk). Patch SOAP: salin atau ganti snapshot, catat atau batalkan baris dokter. Cancel: batalkan baris dokter. Lini masa: asal-usul tanda vital |
| `Areas/HealthServices/ClinicalManagement/DTOs/PatientVitalSignDtos.cs` | Butir deret: `VitalSignSource`, `ConsultationId` (layar dapat memberi label "Dokter") |
| `Areas/HealthServices/ClinicalManagement/Services/InpatientVitalSignService.cs` | Proyeksi deret episode mengisi dua properti di atas |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif pada create, patch SOAP, respons catatan, lini masa, dan deret tanda vital. Penolakan 400 baru hanya bila field baru dipakai salah, atau bila jalur perawat mengubah baris ukuran dokter |
| Database | Kolom `TrxDoctorConsultation.SourceVitalSignId` (`uuid NULL`) + index + FK `RESTRICT`. **Diterapkan ke `QuilvianNewDevHamzah` saja**, lewat skrip SQL idempoten (bagian 5). Tidak ada backfill. Baris lama bernilai `NULL`. DB tim tidak disentuh |
| Keamanan/Auth | Tidak ada `[AccessAction]`/`[AccessPermission]` baru atau berubah, dan tidak ada hardcode peran. Penjaga baru bersifat **kepemilikan data**: baris milik catatan dokter hanya diubah lewat catatan itu |

**Backend Governance Preflight.** Area `HealthServices`; modul pemilik `ClinicalManagement` (prefix registry `Cli`, status `ACTIVE / LEGACY`; tabel legacy `TrxDoctorConsultation`, `TrxPatientVitalSign` belum dimigrasi ke prefix itu dan tidak di-rename pada task ini). Sub-modul dokter-rawat-inap tidak memiliki tabel (`RWI-DEC-081`). Keberlakuan `TOUCHED LEGACY` pada controller dan entity `Trx*` yang sudah ada, dan `NEW CODE` pada `DoctorConsultationVitalSignService` dan `PatientVitalSignCalculation`. QBE yang berlaku:
- `QBE-SVC-001`: logika di service modul, bukan di controller;
- `QBE-CODE-003` dan `QBE-CODE-006`: nomor baris lewat `NumberSeriesAllocator`, bukan `Count + 1` seperti jalur perawat lama. Prefix `VTD` dipisah dari `VTS-` milik perawat, supaya indeks unik `VitalSignRecordNumber` tidak bertabrakan;
- `QBE-DB-*`: migration tangan tanpa Designer, sesuai kebijakan repo.

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/clinical-management/doctor-consultations` | Membuat catatan; menerima `sourceVitalSignId` **atau** `isDoctorMeasuredVitalSign` | `DoctorConsultation : Create` |
| `PATCH` | `/api/v1/health-services/clinical-management/doctor-consultations/{id}/soap` | Menyimpan SOAP; mengganti sumber tanda vital dan mencatat ukuran dokter | `DoctorConsultation : WriteSoap` |
| `PATCH` | `/api/v1/health-services/clinical-management/doctor-consultations/{id}/cancel` | Membatalkan catatan; baris ukuran dokternya ikut dibatalkan | `DoctorConsultation : Cancel` |
| `GET` | `/api/v1/health-services/clinical-management/doctor-consultations/episodes/{episodeId}/soap-timeline` | Lini masa; `sourceVitalSignId`, `vitalSignObservedAt`, `vitalSignObservedByName`, `isDoctorMeasuredVitalSign` | `DoctorConsultation : Read` |

#### Health Services / Clinical Management / Patient Vital Sign

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/clinical-management/patient-vital-signs/episodes/{episodeId}` | Deret episode; baris ukuran dokter ikut tampil dengan `vitalSignSource = 2` (`DoctorConsultation`) dan `consultationId` | `PatientVitalSign : Read` |
| `PUT` | `/api/v1/health-services/clinical-management/patient-vital-signs/{id}` | Menolak baris ukuran dokter (400 `DOCTOR_VITAL_SIGN_READ_ONLY`) | `PatientVitalSign : Update` |
| `PATCH` | `/api/v1/health-services/clinical-management/patient-vital-signs/{id}/cancel` | Menolak baris ukuran dokter (400 `DOCTOR_VITAL_SIGN_READ_ONLY`) | `PatientVitalSign : Cancel` |

Contoh body patch SOAP dengan ukuran dokter:

```json
{ "subjective": "Sesak berkurang", "isDoctorMeasuredVitalSign": true, "sourceVitalSignId": null,
  "bloodPressureSystolic": 130, "bloodPressureDiastolic": 85, "temperature": 36.7, "oxygenSaturation": 97 }
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ./QuilvianSystemBackend.csproj --no-incremental -c Debug -m:1 -p:UseSharedCompilation=false -p:RunAnalyzers=false` | Exit 0, **0 error**, 230 warning (garis dasar 222), **0 warning di 12 berkas task ini**, 1 m 45 d | `PASS` | Keluaran build 30-09-2026; delapan warning tambahan milik berkas agen lain |
| DDL runtime = migration | `dotnet ef dbcontext script` dibandingkan dengan `dotnet ef migrations script 20260928093849_AddMasterNursingDiagnosisSdki 20260930110000_AddDoctorConsultationSourceVitalSign --idempotent` | `PASS` | Kolom, index, dan FK identik |
| Penerapan migration | Skrip idempoten dijalankan lewat `dotnet fsi` + Npgsql yang **menolak berjalan** pada database selain `QuilvianNewDevHamzah` | `PASS` | Katalog: `SourceVitalSignId uuid nullable=YES`; `FOREIGN KEY ("SourceVitalSignId") REFERENCES "TrxPatientVitalSign"("Id") ON DELETE RESTRICT`; `__EFMigrationsHistory` memuat `20260930110000_AddDoctorConsultationSourceVitalSign (9.0.18)`. Dicek ulang baca-saja 30-09-2026 |
| `dotnet ef database update` | — | `EXISTING / ENVIRONMENT ISSUE` | EF 9 menolak dengan `PendingModelChangesWarning` karena `MstDailyNursingAction` (pekerjaan agen lain) sudah masuk model tanpa migration. Karena itu skrip idempoten dipakai, dan pekerjaan agen lain tidak disentuh |
| Tidak ada data yang tertaut sebelum dipakai | 0 catatan bertautan; 0 baris ukuran dokter rawat inap | `PASS` | Kueri baca-saja 30-09-2026 |
| Skenario runtime kriteria 1–5 | — | `NOT RUN` | Backend tidak dijalankan pada sesi ini |

Uji manual: `NOT FEASIBLE` — backend dan akun dokter tidak dijalankan pada sesi ini.

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

**Tidak dijalankan:** pemanggilan HTTP create/patch/cancel dengan data episode nyata. Alasannya: runtime dijalankan pemilik (`soap.md` DoD butir 4).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (`soap.md` 7.6) | Status | Bukti |
| --- | --- | --- |
| 1. Mengirim ID tanda vital episode lain menghasilkan 400 | Terpenuhi di tingkat source; runtime `NOT RUN` | `DoctorConsultationVitalSignService.ResolveAsync` (`PenolakanSumberPasienLain`), dipanggil create dan patch sebelum mutasi |
| 2. Koreksi nilai oleh perawat setelah SOAP diselesaikan tidak mengubah snapshot catatan dokter | Terpenuhi di tingkat source | Nilai disalin ke kolom catatan (`CopySnapshot`), bukan dibaca ulang dari baris perawat |
| 3. Menyimpan draf dua kali dengan TTV manual berbeda menghasilkan satu baris berisi nilai terakhir | Terpenuhi di tingkat source; runtime `NOT RUN` | `RecordDoctorMeasuredAsync` mencari baris milik catatan (`FindOwnRowAsync`) lalu memperbaruinya |
| 4. Baris buatan dokter muncul di `GET /patient-vital-signs/episodes/{episodeId}` dengan sumber `DoctorConsultation` | Terpenuhi di tingkat source; runtime `NOT RUN` | Baris ber-`InpEpisodeId`, `VitalSignSource = DoctorConsultation`, status `Recorded`; proyeksi mengirim `vitalSignSource` dan `consultationId` |
| 5. Membatalkan catatan ikut membatalkan baris tanda vital buatan dokter | Terpenuhi di tingkat source; runtime `NOT RUN` | `CancelOwnRowsAsync` di jalur `PATCH cancel` sebelum `SaveChanges` |
| 6. `dotnet build` 0 error | Terpenuhi | Bagian 5 |
| DoD 3 — migration hanya ke `QuilvianNewDevHamzah` | Terpenuhi | Bagian 5 |
| DoD 7 — laporan, roadmap, status `soap.md` | Terpenuhi | Laporan ini, `backend-roadmap-v2.md`, `requirement-traceability-v2.md` bagian 17, `soap.md` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | EF 9 `PendingModelChangesWarning` karena model agen lain tanpa migration (`MstDailyNursingAction`). Bukan dari task ini |
| Masalah yang diketahui | Layar perawat belum memberi label "Dokter" dan belum menyembunyikan tombol Ubah/Batal pada baris ukuran dokter. Backend sudah menolak dengan pesan yang jelas; penyesuaian layar perawat perlu task frontend keperawatan tersendiri |
| Risiko tersisa | (1) **Migration wajib ikut ke setiap database tempat backend ini dijalankan.** Entity memetakan `SourceVitalSignId`, sehingga tanpa migration `20260930110000_AddDoctorConsultationSourceVitalSign` setiap kueri `TrxDoctorConsultation` gagal (kolom tidak ada). Saat ini baru `QuilvianNewDevHamzah` yang menerimanya; DB tim tidak disentuh sesuai aturan. (2) Dua `SaveChanges` dalam satu transaksi di jalur create. Bila gagal di tengah, seluruh transaksi di-rollback dan nomor `VTD` yang sudah diterbitkan hangus (`INV-PLT-002`, perilaku allocator yang disengaja) |
| Perubahan sampingan | `NONE`. `Program.cs` juga memuat pendaftaran `DailyNursingActionService` dan seeder `MstDailyNursingActionSeeder` milik agen lain. Keduanya tidak disentuh, tetapi ikut terlihat pada `git diff Program.cs` |
| Interupsi | Sesi terpotong ringkasan konteks; dilanjutkan dari kondisi terverifikasi tanpa penyuntingan ganda |
| Status Git | Berkas task SOAP backend: `M` `DiagnosisRecommendationResolverController.cs`, `DoctorConsultationController.cs`, `PatientDiagnosisController.cs`, `PatientVitalSignController.cs`, `DiagnosisRecommendationResolverDtos.cs`, `DoctorConsultationDtos.cs`, `PatientVitalSignDtos.cs`, `TrxDoctorConsultation.cs`, `InpatientVitalSignService.cs`, `ConsultationValidationService.cs`, `ApplicationDbContextModelSnapshot.cs`, `Program.cs` (bersama agen lain), `TrxDoctorConsultationConfiguration.cs`; `??` `DoctorConsultationVitalSignService.cs`, `PatientVitalSignCalculation.cs`, `Migrations/20260930110000_AddDoctorConsultationSourceVitalSign.cs`, `docs/.../rencana-kerja/soap/`. Berkas `NursingIntervention*`, `PrescribingDrug*`, `DailyNursingAction*`, dan `Repositories/ApplicationDbContext.cs` milik pekerjaan lain dan tidak disentuh |
| Langkah berikutnya | Pemilik menjalankan backend dan menguji kriteria 1, 3, 4, 5 lewat Form SOAP (`FE-RWI-140`). Buka task frontend keperawatan untuk label "Dokter" dan penyembunyian aksi pada baris ukuran dokter |
