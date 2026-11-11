# Human Resource — Pekerjaan Backend yang Belum Selesai

| Field | Nilai |
|---|---|
| Jenis dokumen | Daftar kerja hasil audit — **bukan** pengganti roadmap. Status resmi tetap di [backend-roadmap.md](backend-roadmap.md) |
| Dasar | Audit implementasi roadmap backend HR terhadap source, 10 November 2026 |
| Commit yang diaudit | `edfd843bac1c2860da7ae9a5ddcb68a75cf42666` (branch `MHamzah`), ditambah perubahan `BE-HRD-316` yang belum di-commit |
| Cakupan | `BE-HRD-001` s.d. `BE-HRD-333`, source di `Areas/Corporate/HumanResource`, `Areas/SelfServices/HumanResource`, dan `Shared/HumanResource` |
| Pembaca | Pemilik modul HR dan developer backend HR |

---

## 1. Ringkasan

Roadmap menandai **333 dari 333 task** selesai (✅), kecuali `BE-HRD-316` yang diturunkan ke 🟡 pada
10 November 2026. Audit menemukan sebagian task **belum benar-benar bekerja**. Source-nya ada dan
build-nya lulus, tetapi isinya kosong, memakai angka karangan, menyimpan data hanya di memori, atau
hanya menulis log.

| Kelompok | Isi | Jumlah | Butuh keputusan pemilik? |
|---|---|---:|---|
| A | Kode kosong | 4 task | Ya, 3 dari 4 |
| B | Data karangan atau hard-code | 6 task | 2 task ya, 4 task tidak |
| C | Data penting hanya di memori server | 6 task | Tidak |
| D | Worker yang hanya menulis log | 12 task (17 worker) | Ya — sebagian besar menunggu notifikasi |
| E | Nomor dokumen dari angka acak | 14 service | Tidak |
| F | Logika dimatikan atau tidak tersambung | 2 task | 1 ya, 1 tidak |
| G | Sisa remediasi keamanan | 6 temuan | Ya — satu keputusan identitas |
| H | Ketidaksesuaian dokumen roadmap | 5 butir | Tidak |

**Yang sudah dikerjakan pada audit ini** (tidak dimasukkan ke daftar di bawah):

- Remediasi otorisasi 74 controller dan 440 endpoint — lihat
  [evidence/03-hr-authorization-remediation.md](../evidence/03-hr-authorization-remediation.md).
- `BE-HRD-316` 🟡 — keputusan kotak masuk V2 kini diteruskan ke modul asal. Sisanya ada di kelompok A.

### 1.1 Cara membaca kolom "Butuh keputusan?"

- **Tidak** — developer bisa langsung mengerjakan memakai pola yang sudah ada di source.
- **Ya** — ada pilihan bisnis atau infrastruktur yang harus diputuskan pemilik modul lebih dulu.
  Pertanyaannya ditulis di kolom itu.

### 1.2 Cara memperbarui dokumen ini

Bila sebuah butir sudah diperbaiki, isi kolom **Status perbaikan** dengan tanggal dan tautan
laporan task, lalu tandai status task-nya di roadmap seperti biasa. Jangan menghapus barisnya.

---

## 2. Kelompok A — Kode kosong

Method atau worker-nya ada, tetapi tidak melakukan apa pun.

**Contoh di rumah sakit.** Perawat mengajukan cuti, atasannya punya tiket menunggu, tetapi tidak
ada satu pun pemberitahuan yang terkirim. Tidak lewat aplikasi, email, maupun WhatsApp. Atasan baru
tahu bila kebetulan membuka kotak masuknya.

| Task | Masalah | Bukti | Butuh keputusan? | Status perbaikan |
|---|---|---|---|---|
| `BE-HRD-314` | Gateway notifikasi hanya berisi `await Task.CompletedTask`. Aplikasi belum punya tabel notifikasi, kanal real-time notifikasi, maupun pengirim email/WhatsApp. Ringkasan harian ke approver dihitung tetapi tidak dikirim ke mana pun | `WorkflowManagement/Services/UnifiedNotificationGatewayService.cs:26-31` | **Ya.** Kanal mana dulu: lonceng di aplikasi, email, atau WhatsApp? Notifikasi milik modul HR atau platform bersama? | 🟡 11 November 2026 — kanal pertama: lonceng dalam aplikasi (`HrdNotification` + SignalR), milik modul SDM; email/WhatsApp tidak tersedia; migration sudah dijalankan 11 November 2026 ([laporan](../task/report/backend/BE-HRD-314.md)) |
| `BE-HRD-313` | `BulkApprovalProcessingWorker` hanya loop `Task.Delay(30 detik)` | `WorkflowManagement/Workers/WorkflowV2Workers.cs:188` | **Ya.** Persetujuan massal sudah diproses langsung di endpoint. Worker ini dihapus, atau diisi antrean? | 🟡 11 November 2026 — worker dihapus; pelaporan per tiket lewat pusat pemberitahuan ([laporan](../task/report/backend/BE-HRD-313.md)) |
| `BE-HRD-333` | `TravelCurrencyRateSyncWorker` (`TRV-W4`) hanya menulis log; kurs Bank Indonesia tidak pernah diambil | `BusinessTravelManagement/Workers/BusinessTravelWorkers.cs:166` | **Ya.** Sumber kurs: API BI, input manual Keuangan, atau master kurs yang sudah ada? | 🟡 11 November 2026 — kurs dari master Keuangan `MstExchangeRate` diterapkan `TRV-W4`; 5 endpoint hak akses masih kurang ([laporan](../task/report/backend/BE-HRD-333.md)) |
| `BE-HRD-316` 🟡 | `INT-HR-94` belum ada: pengingat menunjuk Plh saat cuti pejabat struktural disetujui | [laporan BE-HRD-316](../task/report/backend/BE-HRD-316.md) bagian 6 | **Ya.** Data jabatan belum punya penanda "pejabat struktural"; pengingatnya butuh `BE-HRD-314` | 🟡 11 November 2026 — `INT-HR-94` ada: pejabat = manajer aktif dengan bawahan; pengingat Plh lewat pusat pemberitahuan ([laporan](../task/report/backend/BE-HRD-316.md)) |

---

## 3. Kelompok B — Data karangan atau hard-code

Layar menampilkan angka atau orang yang tidak pernah ada di database, seolah-olah data asli.

**Contoh di rumah sakit.** Radiografer menerima dosis radiasi 6 mSv dalam setahun — di atas ambang
waspada. Radar surveilans tetap menampilkan **4,8 mSv** untuk semua pekerja, jadi tidak ada alarm.

| Task | Masalah | Bukti | Butuh keputusan? | Status perbaikan |
|---|---|---|---|---|
| `BE-HRD-298` | Dosis kumulatif TLD selalu `yearlyDose = 4.8m` dan `quarterlyDose = 1.2m` | `OccupationalHealthManagement/Services/HazardSurveillanceService.cs:89-91` | Tidak — hitung dari catatan dosis yang disimpan endpoint `surveillance/radiation-dose` | 🟡 11 November 2026 — alarm dosis ke pegawai dan pemegang `OccupationalMedicalExamination : RecordHazardSurveillance` ([laporan](../task/report/backend/BE-HRD-298.md)) |
| `BE-HRD-240` | Bila database kosong, radar sertifikat menampilkan 4 staf fiktif ("Ns. Siti Rahma S.Kep", "dr. Ahmad Fauzi Sp.A", "dr. Robert Sp.BS") ber-GUID tetap. Broadcast melapor terkirim ke **minimal 5 orang** tanpa mengirim apa pun. Nama pelatihan di-hard-code | `LearningAndDevelopment/Services/TrainingExpiryRadarService.cs:66`, `:79`, `:99`, `:107`, `:234-300` | Tidak — hapus data fiktif dan laporkan jumlah sebenarnya | 🟡 10 November 2026 — data fiktif dihapus, siaran melaporkan 0 pengiriman; pengiriman menunggu `BE-HRD-314` ([laporan](../task/report/backend/BE-HRD-240.md)) |
| `BE-HRD-233` | Bila dokter belum punya riwayat OPPE, skor 3 tahun diisi 82,5 / 85,0 / 88,5 | `PerformanceManagement/Services/RecredentialingQualityService.cs:57-64` | Tidak — kembalikan "data belum cukup" | ✅ 10 November 2026 — data belum cukup dilaporkan apa adanya ([laporan](../task/report/backend/BE-HRD-233.md)) |
| `BE-HRD-239` | Bila tabel aturan kosong, 5 aturan pelatihan wajib bawaan ber-GUID tetap tetap tampil (minor) | `LearningAndDevelopment/Services/MandatoryTrainingRuleService.cs:21` | Tidak — pindahkan ke seeder data awal | ✅ 10 November 2026 — dipindah ke `MandatoryTrainingRuleSeeder` ([laporan](../task/report/backend/BE-HRD-239.md)) |
| `BE-HRD-225` | Metrik OPPE dari SIMRS berupa konstanta: volume 120, kelengkapan EMR 92,5, insiden KMKP 0, lainnya 88 | `PerformanceManagement/Services/OppeDataAggregationService.cs:261-267` | **Ya.** Dari tabel mana di SIMRS (klinis/rekam medis) angka ini diambil? | 🟡 11 November 2026 — 4 tipe indikator ditarik dari tabel pelayanan aplikasi; KMKP dan clinical pathway tidak punya tabel sumber ([laporan](../task/report/backend/BE-HRD-225.md)) |
| `BE-HRD-235` | Pengiriman wewenang klinis ke SIMRS tidak memanggil SIMRS: status `200` dikarang dan latensi diambil acak 25–65 ms | `PerformanceManagement/Services/SimrsClinicalPrivilegeDispatcher.cs:77` | **Ya.** SIMRS ada di aplikasi yang sama — cukup tulis ke tabel wewenang klinis, atau lewat API terpisah? | 🟡 11 November 2026 — dispatcher menulis ke `WfpClinicalPrivilege` (tabel bersama yang dibaca modul klinis); migration sudah dijalankan 11 November 2026 ([laporan](../task/report/backend/BE-HRD-235.md)) |

---

## 4. Kelompok C — Data penting hanya di memori server

Data disimpan di variabel `static`, bukan di database. Semua hilang setiap kali aplikasi restart
atau di-deploy ulang, dan tidak terbagi bila aplikasi berjalan lebih dari satu instance.

**Contoh di rumah sakit.** Komite medik menurunkan tier rekredensialing seorang dokter dan
mencatat alasannya. Server di-restart malam itu untuk pembaruan. Besok paginya catatan penyesuaian
tier itu hilang — padahal itu jejak audit akreditasi.

| Task | Data yang hilang | Bukti | Butuh keputusan? | Status perbaikan |
|---|---|---|---|---|
| `BE-HRD-234` | Penyesuaian tier oleh komite (jejak audit) | `PerformanceManagement/Services/RecredentialingQualityService.cs:26` | Tidak | 🟡 10 November 2026 — tabel `HrdRecredentialingTierOverride`; migration sudah dijalankan 11 November 2026 ([laporan](../task/report/backend/BE-HRD-234.md)) |
| `BE-HRD-235` | Putusan rekredensialing dan log pengiriman ke SIMRS | `RecredentialingQualityService.cs:27`, `SimrsClinicalPrivilegeDispatcher.cs:17` | Tidak | 🟡 10 November 2026 — tabel `HrdRecredentialingVerdict` dan `HrdSimrsPrivilegeSyncLog`; migration sudah dijalankan 11 November 2026 ([laporan](../task/report/backend/BE-HRD-235.md)) |
| `BE-HRD-229` | Feed pemicu FPPE yang diterima | `PerformanceManagement/Services/FppeTriggerDetectionService.cs:15` | Tidak | 🟡 10 November 2026 — tabel `HrdFppeTriggerEvent`; migration sudah dijalankan 11 November 2026 ([laporan](../task/report/backend/BE-HRD-229.md)) |
| `BE-HRD-241` | Status buddy system untuk staf yang sertifikatnya kedaluwarsa | `LearningAndDevelopment/Services/TrainingExpiryRadarService.cs:24` | Tidak | 🟡 10 November 2026 — tabel `HrdShiftBuddySafeguard`; migration sudah dijalankan 11 November 2026 ([laporan](../task/report/backend/BE-HRD-241.md)) |
| `BE-HRD-159` | Penguncian PIN slip gaji dan sesi gaji sensitif 15 menit. Penyerang yang terkunci karena salah PIN 3 kali bisa langsung mencoba lagi setelah restart | `PayrollManagement/Services/PayrollPayslipDistributionService.cs:27`, `:30` | Tidak | 🟡 10 November 2026 — tabel `HrdPayslipPinLockout`/`HrdPayslipPinSession`; migration sudah dijalankan 11 November 2026 ([laporan](../task/report/backend/BE-HRD-159.md)) |
| `BE-HRD-204` | Penghitung salah-PIN untuk membuka nominal surat (anti brute-force, maksimal 3 kali per 30 menit) | `HrServiceManagement/Services/PinRateLimiterService.cs:29` | Tidak | 🟡 10 November 2026 — tabel `HrdPinAttemptRecord`, limiter Scoped; migration sudah dijalankan 11 November 2026 ([laporan](../task/report/backend/BE-HRD-204.md)) |

Perbaikan kelompok ini menambah tabel baru. File migration disiapkan bersama perubahannya, dan
pemilik yang menjalankan `dotnet ef database update`.

---

## 5. Kelompok D — Worker yang hanya menulis log

Worker berjalan sesuai jadwal dan menemukan kasus yang perlu ditindaklanjuti, tetapi hanya
menulis `LogWarning`. Tidak ada data yang berubah dan tidak ada orang yang diberi tahu.

**Contoh di rumah sakit.** Seorang perawat diskors. `ItAccessRevocationSyncWorker` seharusnya
mencabut akses SIMRS-nya. Worker itu hanya menulis log, sehingga **perawat yang diskors tetap bisa
masuk SIMRS dan membuka rekam medis pasien**.

| Task | Worker | Bukti | Status perbaikan |
|---|---|---|---|
| `BE-HRD-285` ⚠️ | `ItAccessRevocationSyncWorker` — pencabutan akses SIMRS pegawai diskors (`EMP-W6`) | `EmployeeRelationManagement/Workers/EmployeeRelationWorkers.cs:249` | 🟡 11 November 2026 — akun dikunci/dipulihkan otomatis, jejak `HrdSuspensionAccessAction`; migration sudah dijalankan 11 November 2026 ([laporan](../task/report/backend/BE-HRD-285.md)) |
| `BE-HRD-278` | `SuspensionPeriodMonitoringWorker` — pemantau masa skorsing | `EmployeeRelationWorkers.cs:63` | 🟡 11 November 2026 — eskalasi hari ke-150/180 ke pemegang `EmployeeDisciplinary : ManageSuspension` ([laporan](../task/report/backend/BE-HRD-278.md)) |
| `BE-HRD-279` | `GrievanceSlaEscalationWorker` — eskalasi SLA mediasi keluhan 14 hari | `EmployeeRelationWorkers.cs:112` | 🟡 11 November 2026 — eskalasi ke pemegang `EmployeeGrievance : Mediate` ([laporan](../task/report/backend/BE-HRD-279.md)) |
| `BE-HRD-282` | `PkwtContractExpiryRadarWorker` — radar kontrak PKWT habis | `EmployeeRelationWorkers.cs:158` | 🟡 11 November 2026 — radar H-60/H-30 ke pegawai dan pemegang `EmployeeSeparation : Initiate` ([laporan](../task/report/backend/BE-HRD-282.md)) |
| `BE-HRD-284` | `ExitClearanceChecklistReminderWorker` — pengingat checklist exit clearance | `EmployeeRelationWorkers.cs:204` | 🟡 11 November 2026 — pengingat harian per proses ke pemegang `MultiDeptExitClearance : SignoffChecklist` ([laporan](../task/report/backend/BE-HRD-284.md)) |
| `BE-HRD-268` | `PerformanceCalibrationDeadlineWorker`, `PerformancePipMilestoneMonitoringWorker`, `ClinicalCareerLadderExpiryWorker`, `PerformanceDisputeSlaEnforcementWorker` | `PerformanceManagement/Workers/PerformanceManagementWorkers.cs:129`, `:176`, `:226`, `:269` | 🟡 11 November 2026 — keempat worker mengirim pemberitahuan; `PRF-W5` kini menghitung masa 3 tahun ([laporan](../task/report/backend/BE-HRD-268.md)) |
| `BE-HRD-236` | `FppeProctoringMonitoringWorker` — tenggat proctoring FPPE | `PerformanceManagement/Workers/FppeProctoringMonitoringWorker.cs:17` | 🟡 11 November 2026 — peringatan H-14/H-7/lewat ke proctor, dokter, komite ([laporan](../task/report/backend/BE-HRD-236.md)) |
| `BE-HRD-296` | `SerialSurveillanceFollowUpWorker` — tindak lanjut serologi pajanan | `OccupationalHealthManagement/Workers/OccupationalHealthWorkers.cs:16` | 🟡 11 November 2026 — jadwal serologi ke pegawai dan pemegang `NeedleStickEmergency : ManageSerialSurveillance` ([laporan](../task/report/backend/BE-HRD-296.md)) |
| `BE-HRD-297` | `VaccineBoosterReminderWorker` — pengingat booster vaksin | `OccupationalHealthWorkers.cs:121` | 🟡 11 November 2026 — pengingat booster ke pegawai dan pemegang `StaffVaccination : RecordVaccination` ([laporan](../task/report/backend/BE-HRD-297.md)) |
| `BE-HRD-298` | `RadiationDoseThresholdAlertWorker` — alarm ambang dosis radiasi | `OccupationalHealthWorkers.cs:222` | Belum |
| `BE-HRD-301` | `BPJSOccupationalInjuryFollowUpWorker` — tindak lanjut klaim BPJS JKK | `OccupationalHealthWorkers.cs:269` | 🟡 11 November 2026 — peringatan H-12 jam dan pelanggaran ke pemegang `WorkRestrictionAndInjury : PrepareBpjsClaimStage1` ([laporan](../task/report/backend/BE-HRD-301.md)) |
| `BE-HRD-328` | `TravelAdvanceSettlementReminderWorker` — pengingat settlement uang muka dinas | `BusinessTravelManagement/Workers/BusinessTravelWorkers.cs:17` | 🟡 11 November 2026 — pengingat H+3/H+5 ke pegawai ([laporan](../task/report/backend/BE-HRD-328.md)) |

**Keputusan yang dibutuhkan:**

1. Sebagian besar worker ini seharusnya **memberi tahu seseorang**, jadi menunggu kanal notifikasi
   dari `BE-HRD-314`.
2. `BE-HRD-285` dan `BE-HRD-278` juga butuh aturan: **apakah sistem boleh mencabut akses SIMRS
   dan memulihkannya kembali secara otomatis** saat skorsing dimulai dan berakhir, atau cukup
   memberi tahu admin IT?

---

## 6. Kelompok E — Nomor dokumen dari angka acak

Nomor dokumen resmi dibuat dari tanggal ditambah angka acak 3–4 digit. Dua dokumen bisa mendapat
nomor yang sama. Urutannya juga tidak berurutan, padahal surat resmi seperti SP dan skorsing
menuntut penomoran berurutan. Ini juga melanggar `QBE-CODE-003`.

**Contoh di rumah sakit.** Dua Surat Peringatan diterbitkan pada bulan yang sama. Keduanya bisa
bernomor `SP1/HRD/11/2026/482`. Yang satu menimpa arsip yang lain saat dicari berdasarkan nomor.

| Service | Dokumen | Task terkait | Bukti | Status perbaikan |
|---|---|---|---|---|
| `DisciplinaryIncidentService` | Laporan insiden disiplin | `BE-HRD-273` | `EmployeeRelationManagement/Services/DisciplinaryIncidentService.cs:41` | ✅ 10 November 2026 ([laporan](../task/report/backend/BE-HRD-273.md)) |
| `DisciplinaryInvestigationService` | BAP investigasi | `BE-HRD-274` | `DisciplinaryInvestigationService.cs:33` | ✅ 10 November 2026 ([laporan](../task/report/backend/BE-HRD-274.md)) |
| `DisciplinarySanctionService` | Surat Peringatan | `BE-HRD-275` | `DisciplinarySanctionService.cs:73` | ✅ 10 November 2026 ([laporan](../task/report/backend/BE-HRD-275.md)) |
| `DisciplinarySuspensionService` | Surat skorsing | `BE-HRD-277` | `DisciplinarySuspensionService.cs:27` | ✅ 10 November 2026 ([laporan](../task/report/backend/BE-HRD-277.md)) |
| `EmployeeGrievanceService` | Tiket keluhan | `BE-HRD-279` | `EmployeeGrievanceService.cs:23` | ✅ 10 November 2026 ([laporan](../task/report/backend/BE-HRD-279.md)) |
| `WhistleblowerCaseService` | Tiket whistleblower | `BE-HRD-280` | `WhistleblowerCaseService.cs:29` | ✅ 10 November 2026 ([laporan](../task/report/backend/BE-HRD-280.md)) |
| `BipartiteForumService` | Register forum bipartit | `BE-HRD-281` | `BipartiteForumService.cs:27` | ✅ 10 November 2026 ([laporan](../task/report/backend/BE-HRD-281.md)) |
| `EmployeeSeparationService` | Nomor terminasi | `BE-HRD-282` | `EmployeeSeparationService.cs:32` | ✅ 10 November 2026 ([laporan](../task/report/backend/BE-HRD-282.md)) |
| `BenefitClaimService` | Klaim COB | `BE-HRD-188` | `BenefitManagement/Services/BenefitClaimService.cs:241` | ✅ 10 November 2026 ([laporan](../task/report/backend/BE-HRD-188.md)) |
| `BenefitEnrollmentService` | Kepesertaan benefit | `BE-HRD-181` s.d. `BE-HRD-185` | `BenefitEnrollmentService.cs:420` | ✅ 10 November 2026 — memakai deret resmi `BEN-` ([laporan](../task/report/backend/BE-HRD-181.md)) |
| `BenefitInHouseService` | Referensi koreksi plafon | `BE-HRD-186`, `BE-HRD-187` | `BenefitInHouseService.cs:319` | ✅ 10 November 2026 ([laporan](../task/report/backend/BE-HRD-186.md)) |
| `ProctoringWorkflowService` | Penugasan FPPE | `BE-HRD-230` s.d. `BE-HRD-232` | `PerformanceManagement/Services/ProctoringWorkflowService.cs:95` | ✅ 10 November 2026 — index unik menunggu migration ([laporan](../task/report/backend/BE-HRD-230.md)) |
| `WorkflowInstanceCreationService` | Nomor permohonan workflow | `BE-HRD-304` | `WorkflowManagement/Services/WorkflowInstanceCreationService.cs:67` | ✅ 10 November 2026 ([laporan](../task/report/backend/BE-HRD-304.md)) |
| `ApprovalDelegationV2Service` | Nomor delegasi Plh | `BE-HRD-307` | `WorkflowManagement/Services/ApprovalDelegationV2Service.cs:54` | ✅ 10 November 2026 ([laporan](../task/report/backend/BE-HRD-307.md)) |

**Tambahan keamanan:** PIN 4 digit untuk membuka nominal surat finansial dibuat dengan
`Random.Shared`, bukan pembangkit acak kriptografis (`EmployeeDocumentIssuanceService.cs:316`,
`BE-HRD-204`). **Diperbaiki 10 November 2026**: kini `RandomNumberGenerator.GetInt32`.

**Butuh keputusan? Tidak.** Proyek sudah punya `Areas/Corporate/HumanResource/LifecycleManagement/Services/HrdNumberSeriesService.cs`.
Format awalan nomor yang sekarang dipakai tetap dipertahankan; yang diganti hanya bagian acaknya
menjadi nomor urut, ditambah unique index sesuai `QBE-CODE-004`.

---

## 7. Kelompok F — Logika dimatikan atau tidak tersambung

| Task | Masalah | Bukti | Butuh keputusan? | Status perbaikan |
|---|---|---|---|---|
| `BE-HRD-032` | Pemisahan pengaju dan penyetuju pendaftaran benefit (`FIN-VAL-271`) dimatikan dengan catatan "SEMENTARA". Petugas bisa menyetujui pengajuannya sendiri | `BenefitManagement/Services/EmployeeBenefitEnrollmentService.cs:408`, `:651` | **Ya.** Boleh dinyalakan kembali sekarang? | ✅ Tidak perlu perubahan — `FIN-DEC-233` (3 November 2026) menyatakan `FIN-VAL-271` superseded; pengaju boleh menyetujui sendiri dengan audit dua aktor terpisah. Catatan "SEMENTARA" sudah tidak ada di source |
| `BE-HRD-142` | `CompensatoryTimeOffExpirationService` (kedaluwarsa jatah libur pengganti lembur 90 hari) hanya dipanggil dari controller. Scheduler masih memakai `OvertimeCompensatoryExpiryService` versi lama | `OvertimeManagement/Services/OvertimeSchedulerService.cs:18`, `:27`; `Program.cs:1295`, `:1315` | Tidak — samakan dengan service baru, atau tetapkan salah satu sebagai satu-satunya | ✅ 10 November 2026 — scheduler menjalankan kedua mesin ([laporan](../task/report/backend/BE-HRD-142.md)) |

---

## 8. Kelompok G — Sisa remediasi keamanan

Keenam temuan ini menyangkut **kepemilikan data**: pengguna bisa membaca data milik orang lain
dengan mengganti ID di alamat API. Rinciannya ada di
[evidence/03-hr-authorization-remediation.md](../evidence/03-hr-authorization-remediation.md)
bagian 8.

| No | Lokasi | Masalah | Status perbaikan |
|---:|---|---|---|
| 1 | `EmployeeGrievanceController.GetMyTickets` | `employeeId` dari query — tiket keluhan orang lain bisa dibaca | ✅ 11 November 2026 — identitas dari token (`HrActorClaims`), parameter `employeeId` dihapus |
| 2 | `EmployeeGrievanceController.SubmitGrievance` | Pelapor diisi dari body — keluhan bisa diajukan atas nama orang lain | ✅ 11 November 2026 — pelapor dari token; body diabaikan |
| 3 | `HealthRecordsController.GetMyHealth` | `employeeId` dari query — profil kesehatan orang lain bisa dibaca | ✅ 11 November 2026 — dari klaim `employee_id`, parameter query dihapus |
| 4 | `OppeOngoingEvaluationController.GetAssignedPeerReviews` | `evaluatorId` dari route — tugas telaah sejawat dokter lain bisa dilihat | ✅ 11 November 2026 — evaluator dari klaim `workforce_profile_id`; route yang tidak cocok ditolak 403 |
| 5 | 10 controller Benefit, Perjalanan Dinas, ESS, Workflow V2 | Membaca klaim `"EmployeeId"`/`"WorkforceProfileId"` yang tidak ada di token; selalu jatuh ke ID akun. Juga menghalangi pemeriksaan "apakah penyetuju memang approver tahapan" di `WorkflowApprovalActionService` | 🟡 11 November 2026 — klaim `workforce_profile_id`/`employee_id` lewat `HrActorClaims`; data lama dipetakan ulang oleh migration `20261111020000` (belum dijalankan) |
| 6 | `HealthRecordsController.IsDoctorRole` | Peran dokter K3 ditulis langsung di kode (`IsInRole("DokterK3RS")`) | ✅ 11 November 2026 — diganti pemeriksaan hak akses `OccupationalHealthRecord : ViewClinicalDetail` via `AccessPermissionService` |

**Satu keputusan membuka nomor 1–5:** data "milik saya" diturunkan dari klaim token
**`employee_id`** atau **`workforce_profile_id`**? Untuk nomor 6: cukup memakai hak akses
`OccupationalHealthRecord : ViewClinicalDetail` yang sudah ada?

---

## 9. Kelompok H — Ketidaksesuaian dokumen roadmap

| No | Masalah | Lokasi di `backend-roadmap.md` | Status perbaikan |
|---:|---|---|---|
| 1 | Register status: kolom status `BE-HRD-079` dan `BE-HRD-104` s.d. `BE-HRD-130` kosong; `BE-HRD-146` s.d. `BE-HRD-175` kolom status dan ringkasan tertukar; `BE-HRD-176` s.d. `BE-HRD-333` (158 task) tidak tercantum | Tabel register, baris 700 s.d. 874 | ✅ 10 November 2026 — register diisi, kolom dirapikan, 158 baris ditambahkan |
| 2 | Tabel "Gerbang yang masih terbuka" masih mendaftar 5 gerbang, padahal task yang tertahan sudah ✅. `HRD-PLAN-GAP-01` sudah ditandai ✅ di baris 391 dan 601; `INT-HR-31` sudah dipakai `RosterValidationService`. `HRD-PLAN-GAP-02` dan `HRD-DEC-032` perlu dicek apakah benar sudah tertutup | Baris 674 s.d. 682 | ✅ 10 November 2026 — kelima gerbang ditandai tertutup dengan bukti task; `HRD-PLAN-GAP-02` dan `HRD-DEC-032` terbukti tertutup lewat laporan `BE-HRD-049`, `BE-HRD-050`, `BE-HRD-085` |
| 3 | `BE-HRD-317` bertanda ✅ padahal dependensinya `BE-HRD-316` 🟡 | Kartu `BE-HRD-317` dan grafik B79 | ✅ 10 November 2026 — diturunkan ke 🟡 pada roadmap dan laporan |
| 4 | Laporan `BE-HRD-237` meluluskan uji `AT-HRD-OPP-03` (agregasi SIMRS otomatis), padahal kodenya hard-code (lihat `BE-HRD-225`) | `task/report/backend/BE-HRD-237.md` | ✅ 10 November 2026 — klaim `AT-HRD-OPP-03` dicabut pada laporan dan traceability; task diturunkan ke 🟡 |
| 5 | Laporan `BE-HRD-333` mengklaim 24 hak akses Perjalanan Dinas terkunci. Sesudah remediasi 10 November 2026 baru ada 19 aksi; 5 sisanya belum punya endpoint | `task/report/backend/BE-HRD-333.md` | ✅ 10 November 2026 — laporan diberi koreksi 19 aksi/5 belum; task diturunkan ke 🟡 |

---

## 10. Urutan kerja yang disarankan

### Gelombang 1 — bisa langsung dikerjakan, tanpa keputusan baru

| Urutan | Kelompok | Task | Alasan urutan |
|---:|---|---|---|
| 1 | E | 14 service | Paling banyak, polanya seragam, dan memakai `HrdNumberSeriesService` yang sudah ada |
| 2 | C | `BE-HRD-159`, `204`, `229`, `234`, `235`, `241` | Dua di antaranya pengaman PIN gaji dan surat; sisanya jejak audit akreditasi |
| 3 | B | `BE-HRD-298`, `240`, `233`, `239` | Menghapus data karangan dari layar |
| 4 | F | `BE-HRD-142` | Satu perubahan kecil pada scheduler |
| 5 | H | 5 butir | Merapikan roadmap sesudah gelombang 1 selesai |

### Gelombang 2 — sesudah pemilik memutuskan

| Keputusan | Membuka |
|---|---|
| Kanal notifikasi dan pemilik fitur notifikasi | `BE-HRD-314`, lalu seluruh kelompok D dan `INT-HR-94` |
| Aturan cabut/pulihkan akses SIMRS otomatis | `BE-HRD-285`, `BE-HRD-278` |
| Sumber data SIMRS untuk metrik dan wewenang klinis | `BE-HRD-225`, `BE-HRD-235` |
| Sumber kurs | `BE-HRD-333` |
| Nasib worker persetujuan massal | `BE-HRD-313` |
| Menyalakan kembali maker-checker benefit | `BE-HRD-032` |
| Klaim identitas pegawai (`employee_id` atau `workforce_profile_id`) | Kelompok G nomor 1–5 |

---

## 11. Yang tidak termasuk daftar ini

- **Tidak adanya project test otomatis bukan kekurangan.** `rules/backend/TEST_POLICY.md` melarang
  membuatnya kecuali pemilik memintanya. Yang tetap dicatat adalah laporan yang mengklaim uji
  `AT-HRD-*` lulus tanpa bukti (kelompok H).
- Migration yang belum diterapkan ke database dan pengujian runtime dengan akun sungguhan belum
  diperiksa pada audit ini.
- `BE-HRD-011`, `BE-HRD-016`, `BE-HRD-020`, dan `BE-HRD-021` tidak punya kartu sendiri karena
  direalisasikan lewat task slice lain. Itu bukan pekerjaan yang tertinggal.


---

## 12. Hasil Gelombang 1 (10 November 2026)

| Urutan | Kelompok | Hasil |
|---:|---|---|
| 1 | E — 14 service | ✅ Seluruh angka acak diganti nomor urut dari `NumberSeriesAllocator` (provider bersama Platform, `QBE-CODE-003`/`006`). `HrdNumberSeriesService` **tidak** dipakai karena masih memakai Count+1. Awalan dan periode dipertahankan; placeholder kepesertaan benefit memakai deret resmi `BEN-`. Index unik `EngagementNumber` ditambahkan (migration). PIN 4 digit memakai `RandomNumberGenerator` |
| 2 | C — 6 task | 🟡 Delapan tabel `Hrd*` baru menggantikan penyimpanan in-memory; kode selesai dan build lulus, **migration `20261110050000_AddHrdDurableAuditAndPinLockouts` belum dijalankan** (wewenang pemilik). Dispatcher SIMRS mencatat status jujur `PENDING_INTEGRATION` sampai jalur integrasi diputuskan |
| 3 | B — 4 task | ✅ `BE-HRD-233`, `BE-HRD-239`; 🟡 `BE-HRD-298` (kolom dosis menunggu migration), `BE-HRD-240` (pengiriman menunggu `BE-HRD-314`) |
| 4 | F — `BE-HRD-142` | ✅ Scheduler menjalankan kedua mesin penghangusan CTO |
| 5 | H — 5 butir | ✅ Register, gerbang, dan tiga laporan dikoreksi; `BE-HRD-237`, `BE-HRD-317`, `BE-HRD-333` diturunkan ke 🟡 |

**Verifikasi bersama:** `dotnet build QuilvianSystemBackend.csproj` **0 error**, 606 warning (garis dasar sebelum sesi 578; selisih berasal dari merge cabang lain yang masuk HEAD pada sesi yang sama, bukan dari berkas task ini); `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 58 berkas yang disentuh: **0 VIOLATION, 0 REVIEW, PASS**; uji runtime `NOT RUN`; migration SQL idempoten `Migrations/20261110050000_AddHrdDurableAuditAndPinLockouts.cs` disiapkan; **belum dijalankan** ke database mana pun (wewenang pemilik, `dotnet ef database update`).

**Peringatan untuk pemilik.** Selama sesi ini pemilik melakukan commit dan merge; commit `c44b48b1` ikut membawa migration hasil scaffold otomatis `20261110044224_AddHrdDurableAuditAndPinLockouts` (14.620 baris, 79 `CreateTable`, 26 `DropTable`, 24 `DropColumn`) beserta snapshot model yang diregenerasi. Migration itu **tidak boleh dijalankan**: isinya drift lama antara snapshot dan migration SQL manual slice sebelumnya, bukan perubahan task ini. Pada working tree kedua berkasnya sudah dihapus dan diganti migration SQL manual `20261110050000`; penghapusan itu perlu di-commit pemilik. Snapshot model di HEAD dibiarkan karena sudah bercampur dengan merge cabang lain.

**Temuan baru di luar daftar (tidak dikerjakan):** `MandatoryTrainingRuleService.CreateOrUpdateRuleAsync` membuat aturan tanpa `TrainingCatalogId` yang wajib (akan gagal FK); `HazardSurveillanceService` masih membentuk `SurveillanceNumber` dari potongan GUID; `TrxWorkflowInstance.WorkflowNumber` belum punya index unik; PIN cadangan slip gaji `123456`/`000000`/`112233`/`654321` masih ada di kode.

---

## 13. Hasil Gelombang 2 (11 November 2026)

Pemilik meminta Gelombang 2 dikerjakan seluruhnya tanpa sesi keputusan terpisah. Ketujuh keputusan diambil dari bukti source dan prinsip produk (sederhana, profesional, beban pengguna rendah), lalu dicatat sebagai **asumsi eksplisit** pada laporan tiap task. Pemilik dapat membalikkan setiap keputusan; kodenya terpisah per keputusan.

| Keputusan | Yang diterapkan | Alasan dari bukti |
|---|---|---|
| Kanal notifikasi dan pemiliknya | Lonceng dalam aplikasi, milik modul SDM: tabel `HrdNotification`, endpoint `api/v1/self-service/hr/notifications`, hub SignalR `/hubs/notifications`. Penerima "petugas" dihitung dari hak akses di layar Akses Role, bukan nama peran di kode | Platform tidak punya tabel notifikasi, pengirim email, maupun WhatsApp; yang ada hanya SignalR. Kanal lain dicatat sebagai `RequestedChannel` |
| Cabut/pulihkan akses SIMRS | Otomatis: akun pegawai diskors dinonaktifkan dan dikunci sampai akhir skorsing, dipulihkan saat berakhir (hanya akun yang dikunci sistem), jejak `HrdSuspensionAccessAction` | SIMRS ada di aplikasi ini; pemeriksaan hak akses per request hanya menerima akun aktif, jadi penonaktifan langsung memutus akses |
| Sumber data SIMRS untuk metrik OPPE | Tabel pelayanan aplikasi: kunjungan, visite, resume pulang, resep, surveilans IDO. KMKP dan clinical pathway dilaporkan tidak tersedia | Tabel-tabel itu ada dan punya kolom dokter; tabel insiden pasien dan CP tidak ada |
| Jalur integrasi wewenang klinis | Tulis langsung ke `WfpClinicalPrivilege` | Resolver laboratorium dan kamar operasi sudah membaca tabel itu; tidak ada API SIMRS terpisah |
| Sumber kurs | Master kurs Keuangan `MstExchangeRate` | Master kurs sudah ada; API BI belum |
| Worker persetujuan massal | Dihapus | Endpoint sudah memproses dan melaporkan per tiket |
| Maker-checker benefit | Tidak diubah | `FIN-DEC-233` (3 November 2026) sudah memutuskan pengaju boleh menyetujui sendiri |
| Klaim identitas pegawai | `workforce_profile_id` untuk tabel berkolom `WorkforceProfileId` (perjalanan dinas, benefit, ESS, keluhan); `employee_id` untuk tabel yang merujuk `MstEmployee` (workflow V2, kesehatan kerja); cadangan ID akun; helper `Shared/HumanResource/Helpers/HrActorClaims.cs`; data lama dipetakan ulang di migration | Token sudah memuat kedua klaim; tiap modul memakai kolom berbeda |

**Verifikasi bersama:** `dotnet build QuilvianSystemBackend.csproj` **0 error**, 602 warning (garis dasar 606; tidak ada warning baru pada berkas yang disentuh); `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 43 berkas: **0 VIOLATION, 0 REVIEW, PASS**; uji runtime `NOT RUN`; migration `Migrations/20261111020000_AddHrdNotificationAndSuspensionAccess.cs` (tabel `HrdNotification`, `HrdSuspensionAccessAction`, pemetaan ulang kolom identitas) disiapkan, **belum dijalankan**.

**Delta kontrak API:** `GET /my-tickets` keluhan tidak lagi menerima `employeeId`; `GET my-health` tidak lagi menerima `employeeId`; `GET peer-reviews/assigned/{evaluatorId}` menolak 403 bila `evaluatorId` bukan pengguna login; endpoint baru pusat notifikasi (4 endpoint, controller `HrdNotification`); worker `WF-W5` dihapus.

**Temuan baru di luar daftar (tidak dikerjakan):** `HealthRecordService.GetClinicalDetailAsync` masih mengisi golongan darah "O", rhesus "+", unit "Unit Kerja Terdaftar", dan nama dokter "Dokter Hiperkes RS" sebagai nilai tetap (data karangan); `BusinessTravelAdvancesController` masih menentukan petugas Finance dari nama peran; `WorkflowApprovalActionService` belum memeriksa bahwa penyetuju memang approver tahapan (kini dapat dikerjakan karena klaim `employee_id` sudah benar); `TravelSettlementOverdueWorker` (`TRV-W2`) potongan payroll H+30 masih hanya log.


---

## 14. Eksekusi migration (11 November 2026)

Atas instruksi pemilik, seluruh migration tertunda dijalankan ke database pengembangan `QuilvianNewDevHamzah` dengan `dotnet ef database update`. Pernyataan "migration belum dijalankan" pada bagian 12 dan 13 sudah tidak berlaku. Status task tetap 🟡 karena uji penerimaan dengan akun sungguhan belum dijalankan.

| Hal | Hasil |
|---|---|
| Migration diterapkan | 23: 21 yang tertunda (termasuk milik modul Optical, Rehabilitation, dan Fin Purchasing) ditambah 2 migration baru di bawah |
| Migration tertunda sesudahnya | 0 |
| Perbandingan model dan database | 996 tabel model ada semua; 0 kolom model yang hilang |

**Perbaikan yang dibutuhkan agar berhasil:**

| Berkas | Perubahan | Alasan |
|---|---|---|
| `Migrations/20261109160000_AddPerformanceManagementCoreEntities.cs` | Seed `ON CONFLICT ("MetricCode")` diberi `WHERE "IsDelete" = false` | Index unik `MetricCode` bersyarat, sehingga PostgreSQL menolak dengan galat `42P10` |
| `Migrations/20261110040000_AddMissingHrdTablesFromModel.cs` (baru) | 15 tabel HR (OPPE, FPPE, SPK/RKK, dan lainnya) | Ada di model tetapi tidak pernah dibuat migration mana pun |
| `Migrations/20261111030000_AlignHrdColumnsWithModel.cs` (baru) | 185 kolom pada 33 tabel HR, 17 index, 15 foreign key | Ada di model tetapi tidak ada di database, sehingga pembacaan entity akan gagal. Seluruh tabel itu kosong saat dijalankan |

**Temuan yang tidak diubah:**

- 41 kolom wajib tanpa nilai bawaan ada di database tetapi tidak dikenal model, pada `HrdProbation`, `HrdProbationAssessment`, `HrdContractCorrection`, `HrdContractDecision`, `HrdClinicalPreceptorship`, `HrdClinicalPreceptorshipPreceptor`, `HrdInitialContractBatchRow`, `HrdOnboardingAccessRequest`, `MstOnboardingTemplate`, dan `MstOnboardingTemplateTask`. Penyimpanan baris baru ke tabel itu akan gagal sampai kolom lama dilonggarkan atau dihapus. Ini keputusan desain modul siklus kerja.
- 6 foreign key pada kolom wajib baru sengaja tidak dibuat, karena nilai bawaan kolom tidak merujuk baris mana pun.
- Perbandingan hanya memeriksa keberadaan tabel dan kolom, bukan tipe data.
