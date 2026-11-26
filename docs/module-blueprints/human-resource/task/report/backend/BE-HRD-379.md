# Laporan Perubahan Backend — `BE-HRD-379`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-379` |
| Judul | Jejak audit 12 event `HRD_TRV_*` bernama matriks |
| Slice | `S-D5` amandemen / `MVP-70` · `EPIC HRD-122` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 27, kartu `BE-HRD-379` |
| Trace | `FR-TRV-33`; `HRD-DEC-228`; `HRD-TF-076`, `HRD-TF-078`; Permission 32.2; AT `AT-HRD-TRV-36` |
| Contract version | `v28` — approved 19 November 2026; tidak diubah |
| Dependency | `BE-HRD-374` ✅ (event 1, 2, 11, 12 terisi nyata sesudah `BE-HRD-375`/`376` ✅) |
| Klasifikasi | `LOW–MEDIUM` — satu berkas source, hanya baca; tanpa API/schema baru |
| Task mode | `BACKEND` (instruksi pengguna 19 November 2026: "lanjut"; regresi penuh ditunda ke `BE-HRD-380` atas persetujuan pengguna) |
| Target tulis | `Services/TravelAuditTrailService.cs`; uji di `TravelWorkflowLifecycleTests.cs` (2 baru) |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `43f66daf` (branch `yoga`); `BE-HRD-376` s.d. `378` belum di-commit |
| Tanggal | 19 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terbukti: harness 2 uji baru (kontrol negatif 6 mutasi tertangkap), seluruh uji perjalanan dinas 78/78, QBE Strict tanpa temuan, `GET /hrd/travel/audit-logs` devYoga untuk empat SPPD uji. Regresi penuh dijalankan di `BE-HRD-380` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `BusinessTravelManagement` |
| Pemilik / prefix registry | Tanpa entity baru; membaca `TrxPayrollDeductionSchedule` milik Payroll (`Prl`) hanya lewat `ExternalReferenceId` `TRV-ADV-{id}` yang ditulis worker modul ini |
| Keberlakuan | `TOUCHED LEGACY` (`TravelAuditTrailService`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-AUD-001` (jejak diturunkan dari data domain, terpisah dari log aplikasi), `QBE-ENUM-001` (kode event sebagai konstanta publik layanan), `QBE-PAGE-001` (paging yang ada dipertahankan) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-API-*` (endpoint tidak berubah), `QBE-DB-*` |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/*` suite Skill |

---

## 1. Kondisi as-is terhadap kontrak

| No | Event matriks | As-is (`HRD-TF-076`) | Sesudah task ini — sumber data |
| --- | --- | --- | --- |
| 1 | `HRD_TRV_REQUEST_SUBMITTED` | Tidak ada (yang ada `REQUEST_CREATED` dari `CreateDateTime`) | `SubmittedAt` / `SubmittedByUserId` |
| 2 | `HRD_TRV_REQUEST_APPROVED` | `REQUEST_APPROVAL` dari `TrxBusinessTravelApproval` yang tidak pernah ditulis | `ApprovedAt` / `ApprovedByUserId` |
| 3 | `HRD_TRV_ADVANCE_REQUESTED` | Bernama `ADVANCE_SUBMITTED` | Sama, nama diganti |
| 4–5, 7–8 | `ADVANCE_PAID`, `SETTLEMENT_SUBMITTED`, `SETTLEMENT_VERIFIED`, `SETTLEMENT_CLOSED` | Sudah benar | Tidak berubah |
| 6 | `HRD_TRV_MISSING_RECEIPT_DECLARED` | Tidak ada | `TrxTravelExpenseItem.IsMissingReceiptDeclared` (lewat klaim SPPD); waktu `UpdateDateTime ?? CreateDateTime`; pelaku `MissingReceiptApprovedBy`; alasan dipotong 200 karakter |
| 9 | `HRD_TRV_SPPD_FROZEN_OVERDUE` | Bernama `SETTLEMENT_DEFAULTED` | `TrxTravelSettlement.DefaultedAt` (worker TRV-W2) |
| 10 | `HRD_TRV_PAYROLL_DEDUCTION_TRIGGERED` | Tidak ada | `TrxPayrollDeductionSchedule` dengan `ExternalReferenceId = "TRV-ADV-{advanceId:N}"`; waktu `CreateDateTime` |
| 11 | `HRD_TRV_ATTENDANCE_SYNCED` | Tidak ada | `TrxTravelAttendanceLink.SyncedAt` per tanggal; pelaku `CreateBy` (penyetuju, `BE-HRD-376`) |
| 12 | `HRD_TRV_ROSTER_LOCK_APPLIED` | Tidak ada | SPPD `HasScheduleConflict` dengan status disetujui atau lanjutan; waktu = `SyncedAt` paling awal; pelaku `ApprovedByUserId` |
| — | Tambahan | `REQUEST_CREATED`, tiga event uang muka | Tetap; **baru** `HRD_TRV_REQUEST_REJECTED` dari `RejectedAt`/`RejectedByUserId` |

`TrxBusinessTravelApproval` tidak lagi dibaca (tidak pernah ditulis; riwayat per tahap ada di Workflow V2 — desain 36.8).

---

## 2. Perubahan yang dikerjakan

| Berkas | Isi |
| --- | --- |
| `Services/TravelAuditTrailService.cs` | 17 konstanta kode event publik; penyusun jejak menurut tabel di atas; pesan contoh matriks ("SPPD diajukan ke Surabaya, berangkat 2026-12-01.", "Gembok roster dipasang … s.d. …", "Hak SPPD dibekukan: settlement menunggak lebih dari 14 hari."); tanpa nomor rekening, gaji, atau alasan medis. Filter tanggal, urutan, dan paging tidak berubah |

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat | `PASS` | Keluaran build |
| Uji jejak: satu SPPD diajukan → disetujui (Workflow V2 nyata) → uang muka dibuat dan dicairkan → klaim dengan deklarasi tanpa nota (alasan > 300 karakter) → settlement diajukan, menunggak, diverifikasi, ditutup → jadwal potongan `TRV-ADV-…` | Ke-12 kode matriks muncul; `REQUEST_APPROVAL`, `ADVANCE_SUBMITTED`, `SETTLEMENT_DEFAULTED` tidak muncul; 3 event presensi oleh Bima; disetujui/gembok oleh Bima; pesan sesuai; alasan tanpa nota tepat 200 karakter | `PASS` (AT-36) | Keluaran `dotnet test` |
| Uji jejak: ditolak; gembok dilepas sesudah batal | `REQUEST_REJECTED` muncul tanpa event persetujuan/presensi/gembok; gembok tercatat sesudah disetujui dan hilang sesudah SPPD dibatalkan | `PASS` | idem |
| Catatan proses | Putaran mutasi pertama tidak bermakna: berkas uji gagal dikompilasi (kurang `using` model Payroll). Diperbaiki, kompilasi dipastikan, lalu mutasi diulang | Catatan | — |
| Kontrol negatif (6 mutasi; berkas dipulihkan, `cmp` identik) | A1 event disetujui, A2 nama `ADVANCE_REQUESTED`, A3 pemotongan alasan, A4 penyaring gembok, A5 event potongan payroll, A6 pelaku presensi — semuanya tertangkap | `PASS` | Keluaran per putaran |
| Uji perjalanan dinas lengkap (`TravelWorkflowLifecycleTests`, `TravelSubmitTests`, `TravelContractV25Tests`, `TravelWorkflowDefinitionSeederTests`) | 78/78 lulus | `PASS` | idem |
| QBE `Strict` | 0 temuan | `PASS` | Keluaran checker |
| devYoga `GET /hrd/travel/audit-logs?travelRequestId=…` (superadmin, baca-saja) | `0003`: `REQUEST_CREATED`, `SUBMITTED`, `APPROVED`, 3× `ATTENDANCE_SYNCED`, `ROSTER_LOCK_APPLIED`, `ADVANCE_REQUESTED`. `0004`: `CREATED`, `SUBMITTED`, `REJECTED`. `0007` (disetujui lalu dibatalkan): presensi tercatat, tanpa `ROSTER_LOCK_APPLIED`. `0010`: termasuk `SETTLEMENT_SUBMITTED` dan gembok. Seluruh `200` | `PASS` | `a379.out` |
| Event 4, 6–10 di devYoga | Tidak terbentuk: belum ada pencairan kasir, verifikasi/penutupan settlement, tunggakan, potongan payroll, atau deklarasi tanpa nota di devYoga | `NOT RUN` (harness saja) | — |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Satu SPPD sampai settlement ditutup (dengan deklarasi tanpa nota, tunggakan > 14 dan > 30 hari) memuat ke-12 kode matriks; nama lama hilang | Terpenuhi (harness; sebagian event di devYoga) | AT-36 + A1/A2/A5 |
| 2. Pesan tanpa rekening, gaji, atau alasan medis; alasan tanpa nota dipotong 200 karakter | Terpenuhi | Uji + A3 |
| Migration | Tidak ada | — |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Perilaku yang perlu diketahui | `HRD_TRV_ATTENDANCE_SYNCED` tetap tampil untuk presensi yang kemudian dibatalkan (peristiwa sinkron memang terjadi); `HRD_TRV_ROSTER_LOCK_APPLIED` hanya tampil selama gembok masih terpasang (penanda `HasScheduleConflict`), sesuai matriks 32.2 |
| Risiko | Jejak dihitung di memori dari seluruh SPPD bila `travelRequestId` kosong (perilaku lama; paging sesudah perhitungan) — dapat berat bila data SPPD banyak |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy |
| Task berikutnya | `BE-HRD-380` — verifikasi amandemen `v28` (uji kontrak 25 endpoint, registri, build, QBE, regresi penuh) dan penutupan `BE-HRD-333` |
