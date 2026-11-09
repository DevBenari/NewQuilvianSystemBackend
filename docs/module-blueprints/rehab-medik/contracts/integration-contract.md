# Rehab Medik — Integration Contract

| Field | Nilai |
|---|---|
| Blueprint | `RHB-BP-001` r1 |
| `last_changed_in` | `v1` — status `draft` |
| Owner | Sukma Giri Pratama — belum disetujui |
| Traceability | RHB-DEC-007, 013, 016, 018, 019, 021; capability map C-02..C-16 |

Rilis 1 **tidak** memanggil sistem luar rumah sakit. SATUSEHAT (FEAT-030/031) dan rekonsiliasi
klaim (FEAT-033) adalah Phase 3. Semua integrasi di bawah bersifat **internal antarmodul**,
sinkron, di dalam satu proses aplikasi, kecuali pemancaran fakta billing yang dapat diulang.

| ID | Arah | Modul lain | Titik sentuh | Cara | Gagal / timeout | Persetujuan |
|---|---|---|---|---|---|---|
| INT-RHB-01 | Baca | Registration | `RegPatientEncounter` (milik pasien, terbuka) | Query `AsNoTracking` di `RhbServiceSupport`, pola `HmdServiceSupport.CheckEncounterAsync` | Permintaan ditolak 422 `RHB-VAL-051` | Tidak perlu (baca) |
| INT-RHB-02 | Baca | InPatient | `InpEpisode.PhysicallyLeftAt` | Query | Ditolak 422 `RHB-VAL-052` | Tidak perlu |
| INT-RHB-03 | Baca | PatientManagement, HR, MasterData | `MstPatient`, `MstDoctor`, `MstProfession`, `MstProcedure` | Query di `RhbLookupService` | Pilihan kosong + pesan | Tidak perlu |
| INT-RHB-04 | Baca | ClinicalManagement | Surat rujukan internal | Query baca | Pilihan kosong | Tidak perlu |
| INT-RHB-05 | Tulis (status keutuhan) | MedicalRecord | `ClinicalDocumentIntegrityService.RegisterAsync / EnsureMutableAsync / SignAsync / MarkCancelledAsync` | Pemanggilan service di transaksi pemanggil | Transaksi rehab dibatalkan seluruhnya | **RHB-DEC-019** (bukti OQ-012) |
| INT-RHB-06 | Dibaca oleh | MedicalRecord | `RhbMedicalRecordDocumentSource : IMedicalRecordDocumentSource` | RM memanggil, rehab menjawab dokumen final saja | RM membatasi waktu, sehingga rehab menghormati `CancellationToken` | RHB-DEC-019 |
| INT-RHB-07 | Baca | ClinicalManagement | `ClinicalInstrumentService.ResolveForPatientAsync(kind, patientId)` dan validasi jawaban/skor | Pemanggilan service | Instrumen tidak tampil, hanya pengukuran terstruktur | RHB-DEC-018 |
| INT-RHB-08 | Tulis | ClinicalManagement | `TrxPatientProcedure` per intervensi sesi final. `IdempotencyKey = "RHB-SESINT-{interventionId:N}"`, `ProcedureSource = RehabilitationSession`, `DoctorId` = `InstructingDoctorId` = `RhbEpisode.ResponsibleDoctorId`, `EncounterId` = encounter sesi | Di transaksi tanda tangan sesi | Tanda tangan dibatalkan seluruhnya | **RHB-OQ-015** |
| INT-RHB-09 | Tulis (fakta) | Billing lewat ClinicalManagement | `ClinicalMilestoneFactProducer.EmitChargeEligibilityAsync` dengan `SourceContext = BillingSourceContract.ProcedureSourceContext`, `EffectType = ProcedureChargeEffectType`, `CorrelationId = sessionId` | Setelah commit tanda tangan. Hasil `Emitted`/`Replayed` → `Succeeded`, `OutcomeUnknown` → `Pending`, galat → `Failed`. | Dapat diulang `POST /{id}/billing-handoff/retry`. Idempoten karena kunci sumber sama. | **RHB-OQ-015** |
| INT-RHB-10 | Baca | Platform NumberSeries | `NumberSeriesAllocator.AllocateAsync` (`RHB_REQUEST`, `RHB_EPISODE`, `RHB_SESSION`) | Di transaksi pembuatan | Transaksi dibatalkan | Tidak perlu |

**Contoh INT-RHB-09.** Sesi `RHB-SES-2026-000101` ditandatangani pukul 10.40 dengan dua
intervensi (TENS, latihan penguatan). Dalam transaksi tanda tangan terbentuk dua
`TrxPatientProcedure`. Setelah commit, fakta charge pertama terkirim, tetapi yang kedua timeout,
sehingga status sesi `Pending`. Petugas billing menekan "Ulang". Fakta pertama dijawab `Replayed`
(tidak tertagih dua kali) dan yang kedua `Emitted`, sehingga status menjadi `Succeeded`.

**Koreksi tagihan.** Bila sesi final ternyata salah, koreksinya lewat addendum klinis. Pembatalan
item tagihan mengikuti jalur *void* sumber milik Billing (`ValidateVoid`) dan **tidak** menghapus
catatan sesi. Detail void lintas modul untuk sumber rehab ditinjau bersama pemilik Billing
(OQ-015).
