# Laporan Perubahan Backend — `BE-RJP-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJP-002` |
| Judul | Klasifikasi antrean dengan resolver (memenuhi `BE-CRM-022`) |
| Slice | `MVP-0`, `EPIC RP-03` — satu aturan member di antrean |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/priority-reservation-backend-roadmap.md` bagian `BE-RJP-002` |
| Trace | `FR-RP-020` s.d. `022`; `RJ-PRI-DEC-006`, `007`; `CRM-DEC-032`; `crm-membership` `FR-CRM-042`, `AT-36`; `integration-contract.md` `INT-RP-01`; `02-backend-architecture.md` RP.4.2 |
| Contract version | `RJ-PRI-RESV-001@1.0.0` `approved` (Sukma Giri, 2026-10-16) |
| Dependency | `BE-CRM-010` ✅ (roadmap `crm-membership`) |
| Klasifikasi | `MEDIUM` — skor 5: berkas diperiksa 9–20 (1), berkas diubah 3 (0), logika bisnis sedang (1), kontrak API memakai yang ada (1), database perilaku query (1), keamanan 0, workflow terbatas (1) |
| Task mode | `BACKEND` (`CROSS-REPO MODE`, wewenang `RJ-PRI-DEC-031`) |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/RegistrationManagement/**`, laporan dan roadmap `docs/module-blueprints/rawat-jalan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `a6499cdd` (branch `sukmagp`). Baseline desain `551e1cbb`; selisihnya hanya Radiologi (3 migration, 2 baris `Program.cs`), tidak menyentuh berkas task ini |
| Tanggal | 2026-10-16 |
| Status | ✅ Selesai — 8/8 skenario runtime `PASS` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (Registration) |
| Pemilik / prefix | Registration, `Reg` (`ACTIVE / LEGACY`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md:23`) |
| Keberlakuan | `TOUCHED LEGACY` — service dan controller yang sudah ada. Tanpa entity baru |
| QBE yang berlaku | `QBE-SVC-001` (logika di service), `QBE-API-001` (kontrak API tidak berubah), `QBE-VAL-001`, `QBE-DTO-001` |
| Governance terbaca | `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, registry, `rules/backend/*` — semuanya terbaca |

---

## 1. Masalah yang diperbaiki

Antrean Rawat Jalan menentukan "member" dengan kuerinya sendiri. Kueri itu berbeda dari aturan member CRM yang dipakai
layar lain, dan menilai status pada **tanggal kunjungan**, bukan tanggal pendaftaran.

Contoh sebelum perubahan: Ibu B (Gold) membuat booking hari ini, 16 Okt 2026, untuk Senin 19 Okt. Gold-nya habis 17 Okt.
Antrean menilai pada 19 Okt, sehingga Ibu B mendapat antrean Reguler. Padahal `CRM-DEC-005` menyatakan kunjungan yang
didaftarkan sebelum membership habis tetap memakai tingkat saat didaftarkan.

## 2. Proses bisnis

1. Petugas atau kiosk mendaftarkan kunjungan Rawat Jalan berpoliklinik.
2. Sistem menanyakan resolver CRM: apakah pasien punya membership Aktif **hari ini** (kalender WIB)?
3. Bila ya, kebijakan tingkat (`PriorityQueue`, `PriorityLevel`, `QueueAudience`, `PublicDisplayMode`) menentukan
   antrean. Bila tidak, pasien Reguler.
4. Klasifikasi disimpan sebagai snapshot di antrean, sama seperti sebelumnya.
5. **Ubah jadwal** booking (form Perbarui di Daftar Pasien RJ) menilai ulang pada hari perubahan, karena antrean lama
   dibatalkan dan nomor baru diterbitkan.

Jalur tidak normal: membership berstatus Aktif tetapi tanggal akhirnya sudah lewat (proses malam belum berjalan) dibaca
Reguler, karena resolver bersifat fail-closed.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; registry; `rules/backend/TASK_RULES.md`,
`TASK_CLASSIFICATION.md`, `API_RULES.md`, `DATABASE_RULES.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md`;
`Areas/HealthServices/PatientManagement/Membership/Services/PatientMembershipStatusResolver.cs`;
`Areas/HealthServices/RegistrationManagement/Services/OutpatientQueueClassificationService.cs`;
`.../Controllers/PatientEncounterController.cs` (konstruktor, pembuatan kunjungan);
`.../Services/OutpatientEncounterScheduleService.cs`; `Program.cs` (DI); enum `MembershipStatus`; master
`MstMembershipTier`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientQueueClassificationService.cs` | Memakai `PatientMembershipStatusResolver.GetActiveAsync` sebagai penentu member. Parameter tanggal menjadi `classificationDateWib` (kosong = hari ini WIB). Tambah `FromReservationSnapshot` untuk `BE-RJP-008` (Reguler bila membership snapshot Dibatalkan) |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Pembuatan kunjungan menilai pada hari pendaftaran. Cadangan konstruktor (harness uji lama) kini membangun resolver. Saat runtime service tetap disuntik DI (`Program.cs:501`, `505`) |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientEncounterScheduleService.cs` | Ubah jadwal menilai pada hari perubahan, bukan tanggal kunjungan baru |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk request/response tidak berubah. **Perilaku** berubah: klasifikasi booking dinilai pada hari pendaftaran (`RJ-PRI-DEC-007`), dan flag `IsActive` membership tidak lagi dipakai (selisih yang sudah diterima di `CRM-DECREQ-001`) |
| Database | Tidak ada perubahan schema atau migration. `has-pending-model-changes` bersih |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada endpoint atau izin yang berubah |

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/admin`, `/`, `/kiosk` | Daftar kunjungan. Klasifikasi antrean kini memakai resolver CRM pada hari pendaftaran | `PatientEncounter : Create` / policy kiosk (tidak berubah) |

Ubah jadwal (`PATCH .../outpatient-encounters/{id}/registration-data`) ikut menilai ulang pada hari perubahan. Hak akses
tidak berubah.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:UseSharedCompilation=false -o <scratchpad>/be-build` | Build succeeded, 0 error, 286 warning (lama) | `PASS` | Keluaran build |
| `dotnet ef migrations has-pending-model-changes --configuration Release` | "No changes have been made to the model since the last migration." exit 0 | `PASS` | Keluaran perintah |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <3 berkas>` | VIOLATION 0, REVIEW 0, INFO 0, Final result PASS | `PASS` | Keluaran QBE |
| R1 — Gold aktif, daftar hari ini | Prioritas, `QueueAudience` Member, snapshot membership benar — identik aturan lama | `PASS` | `RegQueue` `(True, 2, 4d531597…, 1)` |
| R2 (`R-RP-07`) — booking 19 Okt dibuat 16 Okt, Gold habis 17 Okt | Prioritas | `PASS` | `(True, 2, a57212b7…, 1)` |
| R3 (`R-RP-06`) — Gold Kedaluwarsa kemarin | Reguler, tanpa snapshot membership | `PASS` | `(False, 1, None, 18)` |
| R4 — status Aktif tetapi tanggal akhir kemarin | Reguler (fail-closed) | `PASS` | `(False, 1, None, 19)` |
| R5 — tanpa membership | Reguler | `PASS` | `(False, 1, None, 20)` |
| R6 — `queueClassification` pada respons | Sesuai antrean | `PASS` | `isPriorityQueue` `true`/`false` |
| R7 — ubah jadwal saat Gold masih aktif | Tetap prioritas di tanggal baru 26 Okt | `PASS` | `(True, '2026-10-26', 1)` |
| R8 — ubah jadwal sesudah Gold Kedaluwarsa | Dinilai ulang hari ini → Reguler di 2 Nov | `PASS` | `(False, '2026-11-02', 2)` |
| Log server uji 7185 | 0 baris `ERR`/`FTL` | `PASS` | `be7185.log` |

Uji manual: `NOT APPLICABLE` — tidak ada perubahan layar.

**Tidak dijalankan:** `dotnet test` — `NOT RUN`, tidak ada project test (pola Bank Darah).

**Keadaan DB sesudah run (`QuilvianNewDevSukma`, data samaran sufiks `4896`):** 5 pasien `RJP2 UJI * 4896`; 4 baris
`MstPatientMembership` Gold (`MemberNumber` `UJI-RJP2-4896-*`) disisipkan lewat SQL; membership B diubah menjadi
Kedaluwarsa untuk R8; 5 kunjungan beserta antreannya; antrean B dipindah dua kali (antrean lama dibatalkan oleh alur ubah
jadwal). Tidak ada data master yang diubah.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `R-RP-06` | Terpenuhi | R3 |
| `R-RP-07` | Terpenuhi | R2 |
| `UAT-RP-05` (booking tetap prioritas sesudah membership habis) | Terpenuhi (sisi API) | R2 |
| `UAT-RP-06` (Kedaluwarsa kemarin → Reguler) | Terpenuhi (sisi API) | R3 |
| Pasien yang memenuhi aturan CRM mendapat klasifikasi identik | Terpenuhi | R1 |
| Baris `BE-CRM-022` di roadmap `crm-membership` ditandai dipenuhi | Terpenuhi | Roadmap `crm-membership` diperbarui |
| Laporan task tertulis | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | IDE menampilkan peringatan SonarLint lama pada `PatientEncounterController.cs` (kompleksitas, parameter). Tidak berasal dari diff ini |
| Masalah yang diketahui | Nomor prioritas masih memakai daftar `1, 3, 5, 10, 15` sampai `BE-RJP-001` |
| Risiko tersisa | Booking mingguan yang sudah berjalan bisa berubah klasifikasi saat diubah jadwalnya. Petugas perlu diberi tahu sebelum rilis |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat bagian status Git pada ringkasan sesi; perubahan task ini: 3 berkas source di atas + dokumen blueprint |
| Langkah berikutnya | `BE-RJP-001` |
