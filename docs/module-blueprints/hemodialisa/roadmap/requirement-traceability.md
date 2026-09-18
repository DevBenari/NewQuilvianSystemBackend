# Requirement Traceability — Modul Hemodialisa

| Field | Value |
|---|---|
| Traceability ID | `HMD-TRACE-001` |
| Revision | `1` |
| Status | `approved` |
| Blueprint ID | `HMD-BP-001` revision 1, status `approved` |
| Backend SHA baseline | `190c91a0` — branch `MHamzah` |
| Frontend SHA baseline | `a38683142` — branch `HamzahV2` |
| Kontrak masukan | `HMD-CONTRACT-v1` (`api-contract.md`, `state-transition-matrix.md`, `validation-matrix.md`, `integration-contract.md`, `permission-audit-matrix.md`) — status `approved` |
| Owner | Muhammad Hamzah (`HMD-DEC-006`) |
| `approved_by` / `approved_at` | Muhammad Hamzah / 2026-09-18T15:40:07+07:00 |
| Tanggal persetujuan | 18 September 2026 |

> [!NOTE]
> **Tujuan Dokumen**
> Dokumen ini adalah jembatan akuntabilitas mutlak antara kebutuhan bisnis, keputusan arsitektur, spesifikasi teknis, task delivery pengembang, dan bukti pengujian otomatis.
> 
> Prinsip panduan rekayasa Quilvian:
> - Bila sebuah keputusan atau kebutuhan tidak memiliki task pelaksana, ia **belum akan pernah terwujud**.
> - Bila sebuah task tidak memiliki asal-usul requirement/decision yang sah, ia lahir dari **selera subjektif pengembang**, bukan kebutuhan rumah sakit.
> - Bila sebuah task tidak memiliki uji pembuktian, klaim penyelesaiannya **tidak bernilai bukti**.

---

## 1. Pemetaan dari Keputusan Bisnis (`HMD-DEC-*`) ke Task dan Uji

| Decision ID | Isi Keputusan Ringkas | Task Backend | Task Frontend | Bukti Pengujian / Acceptance | Status Kesiapan |
|---|---|---|---|---|:---:|
| `HMD-DEC-001` | Scope Phase 1 saja: 25 Feature Must Have + 1 Permintaan Masuk | Seluruh Task Backend (`BE-HMD-01..19`) | Seluruh Task Frontend (`FE-HMD-01..19`) | Seluruh skenario UAT (`UAT-01` s/d `UAT-22`) | Terpetakan Penuh |
| `HMD-DEC-002` | Phase 2 dan Phase 3 di-*grill* terpisah, tidak dikerjakan di MVP | — (Sengaja tidak dibuat) | — (Sengaja tidak dibuat) | Dokumen `04-prd-to-mvp.md` Bagian 8 | Sesuai Batasan |
| `HMD-DEC-004` | Bentuk arsitektur `SINGLE` (satu bounded context utuh) | `BE-HMD-01` | `FE-HMD-01` | Pemeriksaan struktur folder repositori | Terpetakan Penuh |
| `HMD-DEC-006` | Kepemilikan modul tunggal pada Muhammad Hamzah | — (Tata kelola proyek) | — (Tata kelola proyek) | Manifest `HMD-BP-001` | Terpetakan Penuh |
| `HMD-DEC-007` | Registry `HealthServices / HemodialysisManagement / Hemodialysis / Hmd / ACTIVE` disetujui | `BE-HMD-01` | `FE-HMD-01` | Pemeriksaan QBE preflight scanner backend & frontend | **Blocker MVP-0** (5 tindakan registrasi) |
| `HMD-DEC-008` | Entitas Permintaan HD Masuk (`HmdOrder`) menyalin pola `LabOrder`/`RadOrder` | `BE-HMD-07` | `FE-HMD-07`, `FE-HMD-08` | `UAT-01`, `UAT-02`, `OrderWorkflowTests` | Terpetakan Penuh |
| `HMD-DEC-009` | Pemisahan peran dokter: `DoctorId` (DPJP Sesi) dan `InstructingDoctorId` (Pembuat Resep) | `BE-HMD-09`, `BE-HMD-17`, `BE-HMD-18` | `FE-HMD-11`, `FE-HMD-17` | `BillingHandoffAndResilienceTests`, Uji integrasi 2 dokter | Terpetakan Penuh |
| `HMD-DEC-010` | Kualifikasi klinis open saat dev; penunjukan badan klinis digeser ke syarat go-live | `BE-HMD-05` | `FE-HMD-04` | Audit gate `HMD-GATE-001` | Terpetakan Penuh (Go-Live Gate) |
| `HMD-DEC-011` | Pemegang akun klinis sementara: Muhammad Hamzah (acting) | `BE-HMD-05` | `FE-HMD-04` | Audit log pengubahan checklist overridable | Terpetakan Penuh |
| `HMD-DEC-012` | Sesi `Stopped` menerbitkan tindakan dengan `IsBillable = false` beserta alasan | `BE-HMD-16`, `BE-HMD-18` | `FE-HMD-16`, `FE-HMD-18` | `UAT-20`, `BillingHandoffTests` non-billable | Terpetakan Penuh |
| `HMD-DEC-013` | Verifikasi kompetensi staf 3 status; penegakan via sakelar `HmdSetting` | `BE-HMD-03`, `BE-HMD-11` | `FE-HMD-05`, `FE-HMD-13` | `WorklistAndStaffAssignmentTests` | Terpetakan Penuh |

---

## 2. Pemetaan Kebutuhan Fungsional (`FR-HMD-*`) dan Non-Fungsional (`NFR-*`)

| ID Requirement | Deskripsi Kebutuhan | Task Backend | Task Frontend | Skenario UAT / Uji Terkait |
|---|---|---|---|---|
| `FR-HMD-001` | Permintaan wajib menyertakan kunjungan sah | `BE-HMD-07` | `FE-HMD-07` | `UAT-01`, validasi 400 Bad Request |
| `FR-HMD-002` | Permintaan diterima tidak otomatis menjadi sesi | `BE-HMD-07` | `FE-HMD-08` | `UAT-01`, verifikasi tabel sesi kosong |
| `FR-HMD-003` | Menahan (Hold) vs Menolak (Reject) dipisah wewenangnya | `BE-HMD-07` | `FE-HMD-08` | `UAT-02`, uji penolakan non-dokter 403 |
| `FR-HMD-004` | Penolakan permintaan bersifat akhir (terminal state) | `BE-HMD-07` | `FE-HMD-08` | Uji transisi state rejected -> accept (422) |
| `FR-HMD-010` | Satu pasien satu episode HD aktif | `BE-HMD-08` | `FE-HMD-09` | `UAT-03`, `UAT-04` (tolak 409) |
| `FR-HMD-011` | Episode tidak dapat ditutup bila ada sesi menggantung | `BE-HMD-08` | `FE-HMD-10` | Uji penutupan episode tertahan 422 |
| `FR-HMD-012` | Kelayakan klinis lahir dari keputusan manusia | `BE-HMD-08` | `FE-HMD-10` | Form penilaian kelayakan dokter |
| `FR-HMD-013` | Status serologi dirujuk, bukan disalin nilainya | `BE-HMD-08` | `FE-HMD-10` | Uji integritas data serologi rujukan |
| `FR-HMD-014` | Isolasi diputuskan tim PPI, tidak otomatis | `BE-HMD-08` | `FE-HMD-10` | Uji alokasi mesin berdasarkan keputusan PPI |
| `FR-HMD-020` | Resep HD aktif tidak dapat disunting di tempat | `BE-HMD-09` | `FE-HMD-11` | `UAT-05`, `UAT-06` (tolak 423) |
| `FR-HMD-021` | Satu episode satu resep aktif (transaksional) | `BE-HMD-09` | `FE-HMD-11` | `UAT-05`, uji aktivasi otomatis supersede resep lama |
| `FR-HMD-022` | Deviasi dosis/UF dicatat pada sesi, bukan resep | `BE-HMD-14` | `FE-HMD-15` | Uji deviasi ultrafiltrasi intra-HD |
| `FR-HMD-030` | Triple collision check (pasien, mesin, station) | `BE-HMD-10` | `FE-HMD-13` | `UAT-07`, `UAT-08`, uji konkurensi 409 |
| `FR-HMD-031` | Mesin tidak siap/rusak tidak dapat dipakai | `BE-HMD-04`, `BE-HMD-10` | `FE-HMD-03`, `FE-HMD-13` | Uji filter mesin ready & uji blokir penjadwalan |
| `FR-HMD-032` | Kebutuhan isolasi menentukan mesin/station sah | `BE-HMD-10` | `FE-HMD-13` | Uji tolak alokasi mesin umum untuk pasien isolasi |
| `FR-HMD-033` | Perubahan status mesin meninggalkan jejak riwayat | `BE-HMD-04` | `FE-HMD-03` | Uji persistensi `HmdMachineStatusHistory` |
| `FR-HMD-034` | Penugasan staf memiliki 3 status kompetensi | `BE-HMD-11` | `FE-HMD-13` | Uji status `NotVerifiable` pada penugasan perawat |
| `FR-HMD-040` | Unit siap hanya bila seluruh butir wajib terpenuhi | `BE-HMD-06` | `FE-HMD-06` | `UAT-09`, uji tolak kesiapan unit parsial |
| `FR-HMD-041` | Hasil uji air memiliki masa berlaku dinamis (setting) | `BE-HMD-05`, `BE-HMD-06` | `FE-HMD-05`, `FE-HMD-06` | `UAT-10`, uji air kedaluwarsa 422 |
| `FR-HMD-042` | Unit tidak siap tidak mematikan sesi yang berjalan | `BE-HMD-06` | `FE-HMD-06` | Uji isolasi status unit terhadap sesi InProgress |
| `FR-HMD-050` | 12 butir checklist persiapan diperiksa lengkap | `BE-HMD-12` | `FE-HMD-14` | `UAT-11`, uji checklist lengkap |
| `FR-HMD-051` | Checklist hanya boleh dilewati bila IsOverridable | `BE-HMD-05`, `BE-HMD-12` | `FE-HMD-04`, `FE-HMD-14` | `UAT-12`, uji penolakan override non-overridable |
| `FR-HMD-052` | Sesi tidak dapat siap tanpa DPJP | `BE-HMD-13` | `FE-HMD-14` | Uji tolak ready tanpa dokter penanggung jawab |
| `FR-HMD-053` | Pemeriksaan ulang status mesin tepat saat mulai sesi | `BE-HMD-13` | `FE-HMD-15` | Uji pembatalan mulai sesi saat mesin mendadak rusak |
| `FR-HMD-054` | Mulai sesi idempoten terhadap klik ganda | `BE-HMD-13` | `FE-HMD-15` | `UAT-13`, uji transmisi dobel idempotency key |
| `FR-HMD-055` | Waktu mulai authoritative dari waktu server | `BE-HMD-13` | `FE-HMD-15` | Uji manipulasi jam klien vs stempel waktu server |
| `FR-HMD-060` | Pemantauan berkala kronologis tanpa saling menimpa | `BE-HMD-14` | `FE-HMD-15` | `UAT-14`, uji riwayat multi-observasi terurut |
| `FR-HMD-061` | Pemakaian obat diteruskan ke Farmasi | `BE-HMD-15` | `FE-HMD-15` | Uji integrasi penerusan pemakaian obat ke Farmasi |
| `FR-HMD-062` | Kegagalan Farmasi tidak menghapus catatan klinis | `BE-HMD-15` | `FE-HMD-15` | Uji ketahanan catatan obat saat Farmasi error |
| `FR-HMD-063` | Diagnosis komplikasi tidak disimpulkan otomatis | `BE-HMD-14` | `FE-HMD-15` | Uji form pencatatan intervensi komplikasi manual |
| `FR-HMD-070` | Selesai fisik berbeda dengan catatan selesai | `BE-HMD-16` | `FE-HMD-16` | Transisi status Completed -> AwaitingFinalization |
| `FR-HMD-071` | Dua pelaku dokumentasi disimpan terpisah | `BE-HMD-16`, `BE-HMD-17` | `FE-HMD-16`, `FE-HMD-17` | `UAT-16`, verifikasi kolom Documented vs Signed |
| `FR-HMD-072` | Hanya DPJP sesi yang berhak mengesahkan | `BE-HMD-17` | `FE-HMD-17` | `UAT-17`, uji penolakan dokter non-DPJP 403 |
| `FR-HMD-073` | Pengesahan & pendaftaran rekam medis 1 transaksi | `BE-HMD-02`, `BE-HMD-17` | `FE-HMD-17` | Uji rollback atomik pendaftaran rekam medis |
| `FR-HMD-074` | Catatan disahkan terkunci permanen (Temuan Kritis 1) | `BE-HMD-02`, `BE-HMD-17` | `FE-HMD-17` | `UAT-18`, uji penolakan edit dokumen final (423) |
| `FR-HMD-080` | Sesi selesai menerbitkan tepat 1 tagihan | `BE-HMD-18` | `FE-HMD-18` | `UAT-19`, verifikasi baris tagihan kasir |
| `FR-HMD-081` | Sesi stopped tidak menerbitkan tagihan otomatis | `BE-HMD-16`, `BE-HMD-18` | `FE-HMD-16`, `FE-HMD-18` | `UAT-20`, verifikasi `IsBillable = false` |
| `FR-HMD-082` | Kegagalan billing tidak membuka catatan medis | `BE-HMD-18` | `FE-HMD-18` | `UAT-21`, uji isolasi transaksi medis vs billing |
| `FR-HMD-083` | Dua kolom dokter terisi sesuai perannya | `BE-HMD-18` | `FE-HMD-18` | Uji verifikasi DPJP vs Instructing Doctor |
| `NFR-001` | Transaksi atomik dan pembatalan utuh | `BE-HMD-01`, `13`, `17` | — | Uji rollback EF Core transaksional |
| `NFR-002` | Pencegahan tabrakan konkuren | `BE-HMD-10` | `FE-HMD-13` | Concurrency test alokasi sumber daya |
| `NFR-003` | Idempotensi transaksi kritis | `BE-HMD-13`, `BE-HMD-18` | `FE-HMD-15`, `FE-HMD-18` | Uji transmisi payload ganda |
| `NFR-004` | Waktu authoritative server | `BE-HMD-06`, `13`, `14`, `16`, `17` | — | Uji DateTimeOffset.UtcNow server |
| `NFR-005` | Rekaman jejak audit 10 peristiwa kritis | `BE-HMD-19` | — | Audit trail log verification |
| `NFR-006` | Perlindungan privasi serologi di layar bersama | `BE-HMD-11`, `BE-HMD-19` | `FE-HMD-10`, `FE-HMD-12`, `FE-HMD-19` | Audit penanda isolasi tanpa teks diagnosis |
| `NFR-007` | Prinsip gagal tertutup (Fail-Closed) | `BE-HMD-07`, `BE-HMD-12` | `FE-HMD-14` | `UAT-22`, uji proteksi encounter/pasien tidak sah |
| `NFR-008` | Catatan final tetap final (resilience) | `BE-HMD-17`, `BE-HMD-18` | `FE-HMD-17` | Uji retry billing tanpa mutasi dokumen final |
| `NFR-009` | Empat kondisi visual layar (Loading/Empty/Error/Data) | — | `FE-HMD-01` s/d `FE-HMD-19` | Verifikasi komponen `ClinicalStateBoundary` |
| `NFR-010` | Pencegahan data basi saat ganti pasien | — | `FE-HMD-10`, `FE-HMD-14` | Uji pembersihan state instan saat ganti ID |
| `NFR-011` | Kebijakan operasional berupa data, bukan kode | `BE-HMD-03`, `BE-HMD-05` | `FE-HMD-04`, `FE-HMD-05` | Uji modifikasi setting & checklist overridable dinamis |

---

## 3. Pemetaan Kemampuan Teridentifikasi (`CAP-*`) ke Perubahan Sistem

| ID Kemampuan | Deskripsi Kemampuan Asal | Status Audit | Task Terkait | Status Pasca-Task |
|---|---|---|---|---|
| `CAP-01` | Verifikasi identitas pasien | `READY TO REUSE` | `BE-HMD-01`, `FE-HMD-09`, `FE-HMD-14` | Tetap reuse, dirujuk via `PatientId` |
| `CAP-02` | Konteks kunjungan pasien | `READY TO REUSE` | `BE-HMD-01`, `BE-HMD-07`, `FE-HMD-07` | Tetap reuse, dirujuk via `EncounterId` |
| `CAP-03` | Konteks rawat inap opsional | `READY TO REUSE` | `BE-HMD-01`, `BE-HMD-07`, `FE-HMD-07` | Tetap reuse, dirujuk via `InpEpisodeId` |
| `CAP-04` | Persetujuan tindakan sah | `READY TO REUSE` | `BE-HMD-12`, `FE-HMD-14` | Tetap reuse, divalidasi dari `TrxPatientConsent` |
| `CAP-05` | Tanda vital pra & pasca HD | `READY TO REUSE` | `BE-HMD-12`, `BE-HMD-16`, `FE-HMD-14`, `16` | Tetap reuse via `TrxPatientVitalSign` |
| `CAP-06` | Penilaian generik pasien | `READY TO REUSE` | `BE-HMD-16`, `FE-HMD-16` | Reuse via `TrxPatientAssessment` + extension |
| `CAP-07` | Finalisasi & penguncian catatan | `EXTEND` | `BE-HMD-02`, `BE-HMD-17`, `FE-HMD-17` | Menjadi terintegrasi di 2 tempat Rekam Medis |
| `CAP-08` | Koreksi via addendum catatan | `EXTEND` | `BE-HMD-02`, `BE-HMD-17`, `FE-HMD-17` | Menjadi aktif via `ClinicalNoteAddendum` |
| `CAP-09` | Jejak audit akses rekam medis | `READY TO REUSE` | `BE-HMD-19` | Tetap reuse via `MrcAccessLog` |
| `CAP-10` | Tindakan yang dapat ditagih | `READY TO REUSE` | `BE-HMD-13`, `BE-HMD-18` | Tetap reuse via `TrxPatientProcedure` |
| `CAP-11` | Jalur serah terima ke Billing | `READY TO REUSE` | `BE-HMD-18`, `FE-HMD-18` | Tetap reuse via `BillingSourceContract.Procedure` |
| `CAP-12` | Master tindakan hemodialisis | `READY TO REUSE` | `BE-HMD-03` | Data terdaftar di `MstProcedure` |
| `CAP-13` | Registry mesin HD & kelaikan | `MISSING` | `BE-HMD-04`, `FE-HMD-03` | Menjadi ADA (`HmdMachine`, `HmdMachineStatusHistory`) |
| `CAP-14` | Master station/kursi HD | `MISSING` | `BE-HMD-04`, `FE-HMD-04` | Menjadi ADA (`HmdStation`) |
| `CAP-15` | Kesiapan unit & pengolahan air | `MISSING` | `BE-HMD-06`, `FE-HMD-06` | Menjadi ADA (`HmdUnitReadiness`, `HmdUnitReadinessDetail`) |
| `CAP-16` | Penetapan isolasi infeksius | `MISSING` | `BE-HMD-08`, `FE-HMD-10` | Menjadi ADA (`HmdIsolationDecision`) |
| `CAP-17` | Ruang dan unit layanan | `READY TO REUSE` | `BE-HMD-01`, `BE-HMD-03` | Tetap reuse via `MstServiceUnit`, `MstRoom` |
| `CAP-18` | Enum `ServiceUnitType.Hemodialysis` | `EXTEND` | `BE-HMD-02` | Menjadi ADA (`Hemodialysis = 10`) |
| `CAP-19` | Identitas dokter & petugas | `READY TO REUSE` | `BE-HMD-01`, `BE-HMD-11` | Tetap reuse via `MstDoctor`, `MstWorkforceProfile` |
| `CAP-20` | Kredensialing & kompetensi staf | `REUSE WITH ADAPTER` | `BE-HMD-11`, `FE-HMD-13` | Adapter 3 status selesai; pembacaan otomatis via HR ditunda |
| `CAP-21` | Jadwal dinas petugas shift | `REUSE WITH ADAPTER` | `BE-HMD-11` | Digunakan sebagai referensi validasi shift staf |
| `CAP-22` | Baca hasil lab serologi | `REUSE WITH ADAPTER` | `BE-HMD-08`, `FE-HMD-10` | Dicatat manual via rujukan; integrasi otomatis ditunda |
| `CAP-23` | Pemakaian obat ke Farmasi | `READY TO REUSE` | `BE-HMD-15`, `FE-HMD-15` | Terhubung ke `PhmDrugUsage` dengan pending sync resilience |
| `CAP-24` | Kesiapan obat & BMHP shift | `REUSE WITH ADAPTER` | `BE-HMD-06`, `FE-HMD-06` | Diverifikasi via checklist kesiapan unit shift |
| `CAP-25` | Episode / Program HD aktif | `MISSING` | `BE-HMD-08`, `FE-HMD-09`, `FE-HMD-10` | Menjadi ADA (`HmdEpisode`) |
| `CAP-26` | Penilaian kelayakan klinis | `MISSING` | `BE-HMD-08`, `FE-HMD-10` | Menjadi ADA (`HmdEligibilityAssessment`) |
| `CAP-27` | Status akses vaskular pasien | `MISSING` | `BE-HMD-08`, `FE-HMD-10` | Menjadi ADA (`HmdVascularAccess`) |
| `CAP-28` | Resep HD & riwayat revisi | `MISSING` | `BE-HMD-09`, `FE-HMD-11` | Menjadi ADA (`HmdPrescription` immutable lifecycle) |
| `CAP-29` | Penjadwalan sesi & collision | `MISSING` | `BE-HMD-10`, `FE-HMD-12`, `FE-HMD-13` | Menjadi ADA (`HmdScheduleService`, anti-collision) |
| `CAP-30` | Checklist persiapan Pra-HD | `MISSING` | `BE-HMD-12`, `FE-HMD-14` | Menjadi ADA (`HmdSessionChecklist`, overridable gate) |
| `CAP-31` | Sesi hemodialisa eksekusi | `MISSING` | `BE-HMD-13`, `FE-HMD-15` | Menjadi ADA (`HmdSession`, idempotency start) |
| `CAP-32` | Pemantauan & parameter mesin | `MISSING` | `BE-HMD-14`, `FE-HMD-15` | Menjadi ADA (`HmdSessionObservation` kronologis) |
| `CAP-33` | Catatan obat intra-HD | `MISSING` | `BE-HMD-15`, `FE-HMD-15` | Menjadi ADA (`HmdSessionMedication`) |
| `CAP-34` | Pencatatan komplikasi HD | `MISSING` | `BE-HMD-14`, `FE-HMD-15` | Menjadi ADA (`HmdSessionComplication`) |
| `CAP-35` | Disposisi pasca-HD | `MISSING` | `BE-HMD-16`, `FE-HMD-16` | Menjadi ADA (`HmdSessionAssessment` discharge destination) |
| `CAP-36` | Permintaan HD masuk | `MISSING` | `BE-HMD-07`, `FE-HMD-07`, `FE-HMD-08` | Menjadi ADA (`HmdOrder`, order worklist) |
| `CAP-37` | 9 Base components klinis | `READY TO REUSE` | `FE-HMD-01` s/d `FE-HMD-19` | Dipakai ulang 100% tanpa membuat duplikat |
| `CAP-38` | Pola arsitektur frontend | `READY TO REUSE` | `FE-HMD-01`, `FE-HMD-02` | Menyalin pola terbukti modul Radiologi |
| `CAP-39` | Butir menu Hemodialisa | `MISSING` | `FE-HMD-01` | Menjadi ADA di sidebar left-menu |
| `CAP-40` | Titik sentuh kartu Rawat Inap | `MISSING` | `FE-HMD-07` | Menjadi AKTIF pada dokter dan perawat rawat inap |

---

## 4. Pemetaan Skenario UAT (`UAT-01` s/d `UAT-22`)

| Skenario UAT | Sasaran Uji | Task Backend Pembuktian | Task Frontend Pembuktian | Indikator Keberhasilan |
|---|---|---|---|---|
| `UAT-01` | Permintaan cito dari bangsal berhasil | `BE-HMD-07` | `FE-HMD-07`, `FE-HMD-08` | Order masuk bertanda cito di unit HD |
| `UAT-02` | Koordinator mencoba menolak order | `BE-HMD-07` | `FE-HMD-08` | Tombol tolak tidak muncul bagi koordinator |
| `UAT-03` | Program HD dibuka dan diaktifkan | `BE-HMD-08` | `FE-HMD-09`, `FE-HMD-10` | Episode aktif muncul di daftar pasien HD |
| `UAT-04` | Pembuatan episode kedua ditolak | `BE-HMD-08` | `FE-HMD-09` | Ditolak 409 Conflict, pesan edukatif tampil |
| `UAT-05` | Resep HD diganti, riwayat utuh | `BE-HMD-09` | `FE-HMD-11` | Resep baru aktif, resep lama tersupersede |
| `UAT-06` | Resep aktif dicoba disunting in-place | `BE-HMD-09` | `FE-HMD-11` | Ditolak 423 Locked, anjuran buat resep baru |
| `UAT-07` | Dua koordinator berebut mesin sama | `BE-HMD-10` | `FE-HMD-13` | 1 request sukses, 1 request ditolak 409 |
| `UAT-08` | Penjadwalan normal berhasil | `BE-HMD-10`, `BE-HMD-11` | `FE-HMD-12`, `FE-HMD-13` | Sesi muncul di worklist harian unit |
| `UAT-09` | Unit shift dinyatakan siap | `BE-HMD-06` | `FE-HMD-06` | Banner hijau kesiapan aktif di worklist |
| `UAT-10` | Hasil uji air kedaluwarsa | `BE-HMD-06` | `FE-HMD-06` | Ditolak 422, tombol siap terkunci nonaktif |
| `UAT-11` | Sesi dimulai persiapan lengkap | `BE-HMD-12`, `BE-HMD-13` | `FE-HMD-14`, `FE-HMD-15` | Sesi berstatus InProgress, tindakan dibuat |
| `UAT-12` | Butir persiapan non-overridable dilewati | `BE-HMD-12` | `FE-HMD-14` | Override ditolak 422 oleh kebijakan |
| `UAT-13` | Tombol Mulai ditekan dua kali | `BE-HMD-13` | `FE-HMD-15` | Tepat 1 sesi berjalan, tindakan tidak ganda |
| `UAT-14` | Pemantauan berkala tercatat lengkap | `BE-HMD-14` | `FE-HMD-15` | 5 pengamatan tampil urut di garis waktu |
| `UAT-15` | Pemberian obat tanpa dosis ditolak | `BE-HMD-15` | `FE-HMD-15` | Ditolak 400 Bad Request di klien dan server |
| `UAT-16` | Catatan disahkan dan terkunci | `BE-HMD-17` | `FE-HMD-16`, `FE-HMD-17` | Status Finalized, Documented vs Signed beda |
| `UAT-17` | Perawat mencoba mengesahkan sesi | `BE-HMD-17` | `FE-HMD-17` | Tombol Sahkan tidak muncul bagi perawat |
| `UAT-18` | Catatan final dicoba disunting langsung | `BE-HMD-02`, `BE-HMD-17` | `FE-HMD-17` | Ditolak 423, koreksi diarahkan ke addendum |
| `UAT-19` | Sesi selesai menerbitkan tagihan | `BE-HMD-18` | `FE-HMD-18` | Tepat 1 baris tagihan muncul di billing |
| `UAT-20` | Sesi dihentikan tidak menagih otomatis | `BE-HMD-16`, `BE-HMD-18` | `FE-HMD-16`, `FE-HMD-18` | `IsBillable = false`, tidak ada tagihan HD |
| `UAT-21` | Penagihan gagal tidak buka catatan | `BE-HMD-18` | `FE-HMD-18` | Sesi tetap Finalized, retry tagihan sukses |
| `UAT-22` | Konteks pasien gagal diverifikasi | `BE-HMD-07`, `BE-HMD-12` | `FE-HMD-14`, `FE-HMD-19` | Fail-closed, UI kunci seluruh tombol input |

---

## 5. Analisis Kesenjangan Cakupan (Coverage Gap Analysis) dan Gerbang Go-Live

### 5.1 Kesenjangan Cakupan yang Sengaja Ditunda (Post-MVP)

Kebutuhan berikut sengaja **tidak** memiliki task implementasi pada Phase 1 sesuai keputusan `HMD-DEC-001` dan `HMD-DEC-002`. Ketiadaannya tidak menghalangi pelayanan satu sesi cuci darah yang aman:

1. **Perhitungan Adekuasi Kt/V & URR Otomatis**: Memerlukan pembacaan laboratorium ureum pre/post terotomatisasi (`CAP-22`). Pengganti saat MVP: Dokter menghitung adekuasi secara manual dari hasil laboratorium biasa.
2. **Pembacaan Otomatis Sensor Mesin HD (IoT)**: Parameter QB, QD, TMP diinput manual oleh perawat dari layar mesin ke form pemantauan berkala (`BE-HMD-14`).
3. **Integrasi Eksternal SATUSEHAT Kemenkes**: Belum dibangun pada Phase 1 untuk menjaga kestabilan pelayanan internal RS. Data hemodialisis telah terstruktur sehingga siap dipetakan saat modul integrasi platform tersedia.
4. **Verifikasi Online SEP / Klaim BPJS Kesehatan Langsung**: Penjamin diverifikasi manual melalui data registrasi pasien yang sudah ada di sistem.

### 5.2 Gerbang Go-Live yang Wajib Disahkan Sebelum Pelayanan Pasien Nyata

Penyelesaian task engineering (`BE-HMD-*` dan `FE-HMD-*`) **belum** mengizinkan sistem dipakai untuk melayani pasien sungguhan. Enam gerbang go-live berikut wajib dipenuhi secara administratif:

| ID Gerbang | Deskripsi Persyaratan Tata Kelola | Dampak Bila Belum Terpenuhi | Status Saat Ini |
|---|---|---|:---:|
| `HMD-GATE-001` | Penunjukan resmi Komite Tata Kelola Klinis Modul HD | Tidak ada penanggung jawab hukum atas keselamatan klinis unit | Belum Ditunjuk |
| `HMD-GATE-002` | Pengesahan daftar butir persiapan yang boleh di-override dokter | Seluruh butir checklist terkunci tidak boleh dilewati (`IsOverridable = false`) | Terkunci Paling Ketat |
| `HMD-GATE-003` | Pengesahan wewenang peran yang berhak mengesahkan catatan sesi | Berjalan dengan aturan baku: DPJP yang tercatat pada sesi | Baku Terdefinisi |
| `HMD-GATE-004` | Pengesahan wewenang peran yang berhak menolak permintaan | Berjalan dengan aturan baku: Dokter dialisis dengan alasan klinis | Baku Terdefinisi |
| `HMD-GATE-005` | Pengukuhan definitif pemegang akun tata kelola klinis | Pemegang akun sementara (Muhammad Hamzah) bertindak sebagai acting | Acting Ditunjuk |
| `HMD-GATE-006` | Penetapan kebijakan dokter kedua bila unit hanya memiliki 1 DPJP | Berjalan dengan aturan standar: Dokumentasi oleh Perawat, Pengesahan oleh DPJP | Siap Beroperasi |

---

## 6. Tabel Master Traceability Kontrak

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
|---|---|---|---|---|---|---|---|---|---|---|
| `BE-HMD-01` | Skema 22 tabel & migration siap | `FR-HMD-001`, `FR-HMD-030`, `HMD-DEC-007` | `02-backend-architecture.md` §3, §8 | `MstPatient`, `RegPatientEncounter` | 22 Entity `Hmd*`, DbContext, Migration | Blocker Registry `HMD-DEC-007`, Approval Kontrak | 22 tabel terbentuk utuh, composite unique index terpasang | `dotnet build`, Migration test Up/Down | Arsitektur QBE prefix / Backend Lead | 100% lulus build, migration valid |
| `BE-HMD-02` | Catatan HD ditegakkan di Rekam Medis | `FR-HMD-073`, `FR-HMD-074`, Temuan Kritis 1 | `02-backend-architecture.md` §7 | `ClinicalDocumentIntegrityService` | Enum ekstensi, `JenisYangDitegakkan` di Rekam Medis | `BE-HMD-01`, Koordinasi Tim RM | `DitegakkanUntuk` mengembalikan true, dokumen final tidak bisa diedit | `HemodialysisDocumentIntegrityTests` | Kritis: Dokumen tidak terkunci bila terlewat / Backend Lead | Unit test integritas lulus |
| `BE-HMD-03` | DI 10 service & seeder master siap | `FR-HMD-041`, `FR-HMD-051`, `NFR-011` | `02-backend-architecture.md` §4, §9 | DI container, `MstServiceUnit`, `MstProcedure` | Pendaftaran scoped service, konstanta permission, seed master | `BE-HMD-01`, `BE-HMD-02` | 10 service ter-resolve, 12 checklist & setting terisi | `DependencyInjectionResolutionTests`, Seed test | Startup crash / Backend Engineer | Service resolve 100%, seed idempoten |
| `BE-HMD-04` | API mesin, history status & station | `FR-HMD-031`, `FR-HMD-033`, `CAP-13` | API Contract Grup Machine & Station | Pola `MstBed`, audit context | CRUD Mesin, CRUD Station, riwayat perubahan status | `BE-HMD-01`, `BE-HMD-03` | Mesin rusak masuk history, mesin blocked tidak bisa dijadwalkan | `MachineStatusLifecycleTests` | Mesin rusak terpakai / Backend Engineer | Controller & Service mesin selesai |
| `BE-HMD-05` | API setting unit & checklist overridable | `FR-HMD-051`, `NFR-011`, `HMD-ASM-001` | API Contract Grup Settings & Checklist | Auth claims Quilvian | `PATCH .../overridable`, CRUD setting unit | `BE-HMD-03` | Hanya admin klinis bisa ubah overridable, setting air terupdate | `ChecklistPolicyUpdateTests` | Pelonggaran checklist ilegal / Backend Engineer | Otorisasi setting & checklist teruji |
| `BE-HMD-06` | API kesiapan unit shift & uji air | `FR-HMD-040`, `FR-HMD-041`, `CAP-15` | API Contract Grup Unit Readiness | Server time context | Evaluasi kesiapan shift, validasi masa berlaku uji air | `BE-HMD-04`, `BE-HMD-05` | Uji air expired tolak siap (422), unit ready bila butir lengkap | `UnitReadinessEvaluationTests`, `UAT-09`, `UAT-10` | Shift tertunda / Backend Engineer | Lolos uji air kedaluwarsa |
| `BE-HMD-07` | API permintaan HD masuk & disposisi | `FR-HMD-001..004`, `HMD-DEC-008` | API Contract Grup Order | Pola `LabOrder`/`RadOrder` | Order CRUD, aksi accept, hold, reject klinis terminal | `BE-HMD-01`, `BE-HMD-03` | Validasi encounter wajib, tolak hanya oleh dokter | `HemodialysisOrderWorkflowTests`, `UAT-01`, `UAT-02` | Keterlambatan order cito / Backend Engineer | Workflow order teruji 100% |
| `BE-HMD-08` | API episode HD, kelayakan & isolasi | `FR-HMD-010..014`, `CAP-25..27` | API Contract Grup Episode | `MstPatient`, `MrcAccessLog` | Episode lifecycle, kelayakan klinis, akses vaskular, isolasi PPI | `BE-HMD-01`, `BE-HMD-03` | Maks 1 episode aktif, episode tertahan jika ada sesi aktif | `EpisodeLifecycleAndIsolationTests`, `UAT-03`, `UAT-04` | Kebocoran data serologi / Backend Engineer | Invariant 1 episode aktif terjaga |
| `BE-HMD-09` | API resep HD & siklus immutability | `FR-HMD-020..022`, `CAP-28` | API Contract Grup Prescriptions | `MstDoctor` | Resep Draft, Active, Superseded transaksional | `BE-HMD-08` | Resep aktif tolak edit (423), aktivasi otomatis supersede yang lama | `PrescriptionLifecycleTests`, `UAT-05`, `UAT-06` | Parameter resep usang / Backend Engineer | Immutability resep terbukti |
| `BE-HMD-10` | API jadwal sesi & triple collision | `FR-HMD-030..032`, `CAP-29` | API Contract Grup Schedule | Transaksi serializable EF | Penjadwalan sesi, cek tabrakan mesin/pasien/station, cek isolasi | `BE-HMD-04`, `BE-HMD-08`, `09` | Deteksi benturan waktu (409), tolak mesin umum untuk isolasi | `ScheduleCollisionTests`, `UAT-07`, `UAT-08` | Tabrakan mesin / Backend Lead | Lolos uji konkurensi tabrakan |
| `BE-HMD-11` | Penugasan staf, kompetensi & worklist | `FR-HMD-034`, `CAP-20`, `HMD-DEC-013` | API Contract Grup Schedule & Sessions | `MstWorkforceProfile` | `HmdCompetencyGateService`, query worklist teroptimasi | `BE-HMD-10` | Status kompetensi 3 nilai jujur, payload worklist tanpa bocor serologi | `WorklistAndStaffAssignmentTests` | Kebocoran data klinis / Backend Engineer | Worklist & penugasan staf teruji |
| `BE-HMD-12` | API checklist Pra-HD & override | `FR-HMD-050..052`, `CAP-30` | API Contract Grup Session Checklist | `TrxPatientConsent`, `TrxPatientVitalSign` | Simpan checklist, override butir overridable oleh dokter | `BE-HMD-03`, `BE-HMD-05`, `10` | Override ditolak bila non-overridable (422), simpan 12 butir sukses | `PreDialysisChecklistTests`, `UAT-11`, `UAT-12` | Pelolosan berbahaya / Backend Engineer | Validasi override terbukti ketat |
| `BE-HMD-13` | Mulai sesi idempoten & tindakan klinis | `FR-HMD-053..055`, `CAP-31` | API Contract Grup Session Start | `TrxPatientProcedure` | Sesi Ready, Start idempoten, re-check mesin JIT, server time | `BE-HMD-12` | Mulai sesi dobel hasilkan 1 sesi & 1 tindakan, waktu authoritative | `SessionStartIdempotencyTests`, `UAT-13` | Tindakan ganda / Backend Lead | Transaksi atomik mulai teruji |
| `BE-HMD-14` | API observasi berkala & komplikasi | `FR-HMD-060`, `FR-HMD-063`, `CAP-32` | API Contract Grup Observations | Audit logging | Observasi kronologis parameter mesin & komplikasi manual | `BE-HMD-13` | Riwayat observasi tidak menimpa, form komplikasi klinis mandiri | `IntraDialysisMonitoringTests`, `UAT-14` | Data hilang / Backend Engineer | Observasi kronologis terbukti |
| `BE-HMD-15` | API obat intra-HD & sync Farmasi | `FR-HMD-061`, `FR-HMD-062`, `CAP-23` | API Contract Grup Medications | `PhmDrugUsage` | Catat obat, sync asinkron ke Farmasi dengan status pending | `BE-HMD-13` | Catatan obat tidak terhapus meski Farmasi offline | `MedicationAdministrationTests`, `UAT-15` | Obat tertahan / Backend Engineer | Isolasi kegagalan logistik teruji |
| `BE-HMD-16` | Penilaian pasca-HD & submit perawat | `FR-HMD-070`, `FR-HMD-071`, `CAP-35` | API Contract Grup Session Closure | `TrxPatientAssessment` | Selesai/Stop sesi, penilaian pasca-HD, submit dokumentasi | `BE-HMD-14`, `BE-HMD-15` | Sesi stopped catat non-billable, submit isi DocumentedBy | `SessionClosureAndDocumentationTests` | Sesi menggantung / Backend Engineer | Alur stop & submit selesai |
| `BE-HMD-17` | Pengesahan DPJP & kunci rekam medis | `FR-HMD-071..074`, `CAP-07` | API Contract Grup Session Finalize | `ClinicalDocumentIntegrityService` | Finalize oleh DPJP, lock mutlak, addendum support | `BE-HMD-02`, `BE-HMD-16` | Non-DPJP ditolak 403, dokumen final tolak edit 423 | `SessionFinalizationAndLockingTests`, `UAT-16..18` | Catatan bocor/diubah / Backend Lead | Penguncian dokumen terbukti 100% |
| `BE-HMD-18` | Serah terima billing & retry handoff | `FR-HMD-080..083`, `CAP-11` | API Contract Grup Billing Handoff | `BillingSourceContract.Procedure` | Handoff post-finalize, pemetaan DPJP & resep, endpoint retry | `BE-HMD-17` | Billing gagal tidak buka catatan, sesi stopped tidak terbit tagihan | `BillingHandoffAndResilienceTests`, `UAT-19..21` | Pendapatan hilang / Backend Engineer | Handoff billing & retry teruji |
| `BE-HMD-19` | Uji otorisasi endpoint & jejak audit | `NFR-005`, `NFR-006`, Permission Matrix | Seluruh Kontrak | Test harness ASP.NET Core | Automated reflection permission test, 10 event audit trail | `BE-HMD-01..18` | 100% endpoint berizin presisi, 10 event audit tersimpan | `HemodialysisPermissionAndContractTests` | Celah keamanan / Lead QA & Backend | 100% endpoint lolos reflection test |
| `FE-HMD-01` | Navigasi sidebar & routing rute Next.js | `CAP-38`, `CAP-39`, `03-frontend-architecture.md` | `03-frontend-architecture.md` §3 | `menu-items.jsx`, resolver sidebar | Peta menu sidebar, layout rute modul, filter izin menu | Blocker Approval Kontrak | Menu tampil sesuai role, rute modul dapat diakses | `SidebarNavigationTests` | Menu hilang / Frontend Engineer | Navigasi sidebar terhubung |
| `FE-HMD-02` | Redux slice, Axios service & hooks | `03-frontend-architecture.md` §7 | Seluruh Kontrak API | Axios instance, Redux store | Data layer frontend, 7 service, Redux slices, custom hooks | `FE-HMD-01` | Interceptor tangani 423 locked, hook transisi status valid | `ApiClientAndReduxSliceTests` | State kotor / Frontend Engineer | Unit test state & service >80% |
| `FE-HMD-03` | Layar master mesin & riwayat status | `FR-HMD-031`, `FR-HMD-033`, `FE-HMD-08` | API Contract Grup Machine | `ClinicalStateBoundary` | Halaman inventaris mesin, dialog ubah status, panel riwayat | `FE-HMD-02`, `BE-HMD-04` | Ubah status tampilkan chip dan catat riwayat | `MachineMasterViewTests` | Mesin salah blokir / Frontend Engineer | Layar master mesin selesai |
| `FE-HMD-04` | Layar master station & checklist | `FR-HMD-051`, `FE-HMD-09`, `FE-HMD-10` | API Contract Grup Station & Checklist | `ClinicalStateBoundary` | Halaman station, halaman checklist items overridable | `FE-HMD-02`, `BE-HMD-04`, `05` | Toggle overridable minta konfirmasi memo klinis | `ChecklistMasterViewTests` | Checklist tertukar / Frontend Engineer | Layar master station & checklist siap |
| `FE-HMD-05` | Layar pengaturan kebijakan unit HD | `FR-HMD-041`, `FE-HMD-11`, `NFR-011` | API Contract Grup Settings | `ClinicalStateBoundary` | Form setting rasio perawat, masa air, toleransi mulai | `FE-HMD-02`, `BE-HMD-05` | Ubah setting air langsung tersimpan dan berefek | Form validation tests | Salah setting / Frontend Engineer | Layar setting unit selesai |
| `FE-HMD-06` | Lembar kesiapan unit shift & air | `FR-HMD-040..042`, `FE-HMD-05` | API Contract Grup Unit Readiness | `ClinicalStateBoundary`, `ClinicalSafetyAlert` | Shift selector, kartu kelaikan air, checklist kesiapan | `FE-HMD-02`, `BE-HMD-06` | Air expired munculkan kartu merah & kunci tombol siap | `UnitReadinessViewTests`, `UAT-09`, `UAT-10` | Shift tanpa deklarasi / Frontend Engineer | Lembar kesiapan shift aktif |
| `FE-HMD-07` | Form order HD terintegrasi rawat inap | `FR-HMD-001`, `FE-HMD-12`, `CAP-40` | API Contract Grup Order | Integrasi Rawat Inap | Pengaktifan kartu HD di rawat inap dokter & perawat, modal order | `FE-HMD-02`, `BE-HMD-07` | Order cito berhasil dikirim dengan konteks encounter terkunci | `InpatientTouchpointTests`, `UAT-01` | Order tanpa encounter / Frontend Engineer | Kartu penunjang rawat inap aktif |
| `FE-HMD-08` | Layar daftar order HD masuk | `FR-HMD-001..004`, `FE-HMD-02` | API Contract Grup Order | `ClinicalStateBoundary` | Antrean order masuk, badge cito, aksi accept, hold, reject | `FE-HMD-02`, `BE-HMD-07` | Tombol tolak tersembunyi untuk koordinator, cito di atas | `OrderWorklistViewTests`, `UAT-01`, `UAT-02` | Miss komunikasi order / Frontend Engineer | Daftar order masuk berfungsi |
| `FE-HMD-09` | Layar daftar pasien HD aktif | `FR-HMD-010`, `FE-HMD-03` | API Contract Grup Episode | `ClinicalStateBoundary` | Tabel pasien HD aktif, pencarian No RM, tombol buka berkas | `FE-HMD-02`, `BE-HMD-08` | Buka berkas arahkan ke episode workspace, badge isolasi netral | `PatientListViewTests`, `UAT-03`, `UAT-04` | Loading lambat / Frontend Engineer | Layar daftar pasien aktif |
| `FE-HMD-10` | Episode workspace - klinis & isolasi | `FR-HMD-012..014`, `FE-HMD-06` | API Contract Grup Episode | `ClinicalWorkspaceShell`, `PatientContextHeader` | Tab kelayakan, akses vaskular, tab serologi dilindungi hak akses | `FE-HMD-02`, `BE-HMD-08` | Tab serologi tolak non-otoritas, anti data basi saat ganti ID | `EpisodeWorkspaceClinicalTests` | Kebocoran data serologi / Frontend Engineer | Workspace episode klinis selesai |
| `FE-HMD-11` | Episode workspace - resep HD | `FR-HMD-020..022`, `FE-HMD-06` | API Contract Grup Prescriptions | `ClinicalRevisionHistory` | Kartu resep aktif, form revisi draf, timeline resep superseded | `FE-HMD-10`, `BE-HMD-09` | Resep aktif tanpa tombol edit, revisi perbarui timeline | `PrescriptionHistoryViewTests`, `UAT-05`, `UAT-06` | Salah input UF / Frontend Engineer | Tab resep & revisi terpasang |
| `FE-HMD-12` | Layar jadwal & daftar kerja harian | `FR-HMD-030..032`, `FE-HMD-04` | API Contract Grup Schedule | `ClinicalStateBoundary` | Worklist harian per shift, pita status unit, tombol buka sesi | `FE-HMD-02`, `BE-HMD-10`, `11` | Pasien terjadwal tampil sesuai station, navigasi buka sesi aktif | `WorklistDailyViewTests`, `UAT-08` | Data tidak sinkron / Frontend Engineer | Worklist harian berfungsi |
| `FE-HMD-13` | Dialog penjadwalan & staf | `FR-HMD-030..032`, `FR-HMD-034` | API Contract Grup Schedule | Modal dialog, alert feedback | Dialog form jadwal, cek benturan mesin, peringatan rasio perawat | `FE-HMD-12`, `BE-HMD-10` | Umpan balik visual benturan jadwal (409), peringatan rasio | `ScheduleDialogTests`, `UAT-07` | Salah pilih mesin / Frontend Engineer | Dialog penjadwalan siap |
| `FE-HMD-14` | Sesi workspace - Pra-HD & checklist | `FR-HMD-050..052`, `FE-HMD-07` | API Contract Grup Session Checklist | `ClinicalCompletionBar`, `ClinicalValidationSummary` | Vital pra-HD, 12 checklist, bar kemajuan, tombol nyatakan siap | `FE-HMD-02`, `BE-HMD-12`, `13` | Tombol siap nonaktif sebelum 100% dan ringkasan validasi muncul | `SessionPreCheckViewTests`, `UAT-11`, `UAT-12` | Checklist terlewat / Frontend Engineer | Pra-HD workspace berfungsi |
| `FE-HMD-15` | Sesi workspace - intra-HD & timeline | `FR-HMD-054..063`, `FE-HMD-07` | API Contract Grup Session Intra | `ClinicalTimeline`, `ClinicalSafetyAlert` | Mulai idempoten, garis waktu observasi, obat farmasi, komplikasi | `FE-HMD-14`, `BE-HMD-13..15` | Tombol mulai kunci instan, observasi masuk simpul timeline | `SessionIntraDialysisViewTests`, `UAT-13..15` | Form tertutup tidak sengaja / Frontend Engineer | Intra-HD workspace terpasang |
| `FE-HMD-16` | Sesi workspace - pasca-HD & submit | `FR-HMD-070`, `FR-HMD-071`, `CAP-35` | API Contract Grup Session Closure | Dialog konfirmasi darurat | Form evaluasi akhir, disposisi pulang/rawat, submit perawat | `FE-HMD-15`, `BE-HMD-16` | Tombol stop catat non-billable, submit kunci form keperawatan | `SessionPostDialysisViewTests`, `UAT-16`, `UAT-20` | Disposisi kosong / Frontend Engineer | Pasca-HD workspace teruji |
| `FE-HMD-17` | Sesi workspace - pengesahan DPJP & addendum | `FR-HMD-071..074`, `CAP-07` | API Contract Grup Session Finalize | `ClinicalAddendumList`, validation summary | Tombol sahkan khusus DPJP, mode read-only gembok, form addendum | `FE-HMD-16`, `BE-HMD-17` | Tombol sahkan hanya untuk DPJP, catatan terkunci permanen | `SessionFinalizeAndAddendumViewTests`, `UAT-16..18` | Legalitas catatan / Frontend Lead | Pengesahan DPJP & addendum teruji |
| `FE-HMD-18` | Beranda eksekutif & retry billing | `FR-HMD-082`, `FE-HMD-01`, `CAP-11` | API Contract Grup Billing Handoff | Dashboard widgets | Kartu statistik harian, panel monitoring tagihan, tombol retry | `FE-HMD-02`, `FE-HMD-17`, `BE-HMD-18` | Tombol retry billing kirim ulang tanpa dobel tagihan | `DashboardAndBillingHandoffViewTests`, `UAT-19..21` | Tagihan tertahan / Frontend Engineer | Beranda & retry tagihan selesai |
| `FE-HMD-19` | Uji navigasi, 4 state, privasi & UAT | `NFR-006`, `NFR-009`, `NFR-010`, UAT 1..22 | Seluruh Kontrak | E2E Testing Framework | Audit keterjangkauan rute, uji 4 state UI, uji privasi, 22 UAT E2E | `FE-HMD-01..18` | 100% rute terjangkau, 4 state visual aktif, 22 skenario UAT lulus | `HemodialysisFrontendUatE2ETests` | Inkonsistensi UI / Lead QA & Frontend | 100% skenario UAT lulus |
