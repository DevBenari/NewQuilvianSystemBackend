# PAY-BE-FIX-029 — Jadwal Angsuran Potongan: sumber Finance AR dan pensiun endpoint simulasi (backend)

Tanggal: 24 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `261bb431` (working tree belum di-commit). Frontend: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `b05275ecd`.

`dotnet build`: **NOT RUN** (dilarang pada sesi ini). `MIGRATION`: tidak ada, tanpa perubahan skema. `AUTOMATED TEST`: NOT APPLICABLE (backend tanpa project test); kontrak BE diperiksa lewat tes statis di sisi FE (`payroll-deduction-schedules-finance.test.mjs`, 11 lulus). Backend Governance Preflight: Area Corporate, modul HumanResource/PayrollManagement (`Prl`/jadwal potongan legacy `Trx`), `TOUCHED LEGACY`; tidak ada entity baru.

## 1. Akar masalah (dari source)
Halaman memanggil `GET hr-deductions/schedules` yang hanya membaca `TrxPayrollDeductionSchedule`. Tabel itu terisi hanya oleh `POST schedules/sync-finance` (manual) atau eksekusi tertentu; perjanjian angsuran di Finance AR (`FinReceivableInstallmentPlan`/`FinReceivableInstallment`) tidak pernah dibaca saat GET. Akibatnya: Jadwal 0, Rp0, "Tidak ada jadwal angsuran" walau Finance sudah punya angsuran. Bawaan filter `Active` juga memakai status model lama.

## 2. Pemetaan Finance AR vs legacy
| Aspek | Finance AR | HRD legacy |
| --- | --- | --- |
| Entity | `FinReceivableInstallment` + `Plan` + `Receivable` | `TrxPayrollDeductionSchedule` |
| Granularitas | satu baris per angsuran/periode | satu baris per perjanjian |
| Identitas pegawai | `BenefitOwnerId` (MstEmployee) dan `BenefitOwnerWorkforceProfileId` (canonical, termasuk dokter) | `EmployeeId` |
| Status | DIJADWALKAN, TERBAYAR_SEBAGIAN, TERBAYAR, TERTUNGGAK, DIBATALKAN; perjanjian MENUNGGU/DISETUJUI/DITOLAK/SELESAI/DIBATALKAN | Active/Paused/Completed/ClearedAtSeparation |
| Prioritas | tidak ada | `PriorityLevel` (sync lama mengarang 2) |
| Kunci hubung | `PlanNumber` | `ExternalReferenceId` = `PlanNumber` (sync lama) |

## 3. Keputusan sumber data
Finance AR = sumber utama. Legacy hanya ditampilkan bila **tidak** menunjuk `PlanNumber` mana pun; salinan disembunyikan dan dihitung (`hiddenLegacyDuplicateCount`). Jadwal legacy tidak ditampilkan bila filter periode atau status Finance spesifik aktif (tidak ada padanan). `PriorityLevel` tidak dikarang untuk data Finance. Kriteria yang ditampilkan: `!IsDelete`, `DebtorType = EMPLOYEE_BENEFIT`, owner terisi, perjanjian DISETUJUI/SELESAI/DIBATALKAN (MENUNGGU/DITOLAK bukan jadwal).

## 4. Perubahan
| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/PayrollManagement/DTOs/DeductionFinanceScheduleDtos.cs` (baru) | Filter, baris, ringkasan, baris legacy, respons |
| `Areas/Corporate/PayrollManagement/Services/FinanceInstallmentScheduleService.cs` (baru) | `IFinanceInstallmentScheduleService.GetAsync`: baca-saja (`AsNoTracking`), identitas lewat `FinanceBenefitOwnerDirectory`, ringkasan dari himpunan yang sama dengan tabel, saldo piutang dijumlahkan sekali per `ReceivableId`, DIBATALKAN dikecualikan dari total, keterkaitan Payroll dari `PrlPayrollVariableInputs` (`SourceType = FinanceReceivableInstallment`), halaman maks 100, legacy maks 200 |
| `.../Controllers/PayrollDeductionScheduleController.cs` | `GET schedules/finance` (izin `PayrollDeductionSchedule`:`Read`; `ArgumentException` → 400); aksi `engine/simulate` dihapus; deskripsi controller tanpa kata simulasi |
| `.../Services/IPayrollDeductionEngineService.cs` | `SimulateEmployeeDeductionsAsync` dihapus dari kontrak publik |
| `.../Services/PayrollDeductionEngineService.cs` | Metode menjadi `private CalculateEmployeeDeductionsAsync`; `ExecutePayrollDeductionsAsync` memanggilnya |
| `Program.cs` | DI `IFinanceInstallmentScheduleService` |

## 5. Penghapusan simulasi dan perlindungan proses potongan
Audit consumer: satu-satunya pemanggil `GET engine/simulate` di source adalah `restitution-service.simulateDeductions` (FE, sudah dihapus pada PAY-FE-FIX-029). Dokumen lama BE-HRD-164, FE-HRD-R40, dan `01-existing-capability-map.md` masih menyebut endpoint itu (riwayat, tidak diubah). Logika perhitungan (batas potongan, prioritas, parsial, carry-over, audit) **tidak diubah**; hanya aksesibilitasnya (publik → privat). Tidak ada engine legacy yang dihidupkan sebagai sumber potongan otomatis baru. `GET schedules/finance` tidak menulis apa pun (tes statis menolak `SaveChanges`/`ExecuteUpdate`/`Add` pada DbContext).

## 6. Status
| Butir | Status |
| --- | --- |
| Source BE | IMPLEMENTED, VERIFIED STATIC (keseimbangan kurung dan referensi diperiksa) |
| `dotnet build` | NOT RUN |
| Isi data nyata (apakah perjanjian berstatus DISETUJUI, identitas pemilik cocok) | NEEDS RUNTIME UAT |
| Dokter tetap tanpa MstEmployee | Dipetakan lewat `WorkforceProfileId`; NEEDS RUNTIME UAT |
| Prioritas potongan untuk Finance | BLOCKED: Finance tidak menyimpan; perlu keputusan pemilik |
| Dashboard Potongan & Klaim | Tidak diubah (masih `getSchedules` legacy); NEEDS CONFIRMATION apakah dipindah ke Finance |

## 7. Skenario (20) — statis
1-4 (angsuran Finance tampil tanpa sinkronisasi, tanpa Payroll Run, dokter, status perjanjian): IMPLEMENTED, NEEDS RUNTIME UAT. 5-7 (legacy tetap tampil, tanpa duplikasi, hidden count): VERIFIED STATIC. 8-10 (ringkasan sama dengan tabel, saldo sekali per piutang, tanpa label "/ Bulan"): VERIFIED STATIC. 11-13 (filter periode/status/pegawai; status tak dikenal dan periode tak valid → 400): IMPLEMENTED. 14 (GET tidak menulis): VERIFIED STATIC. 15-16 (simulate dihapus; Execute utuh): VERIFIED STATIC. 17 (izin Read): VERIFIED STATIC. 18 (dashboard tidak regresi): VERIFIED STATIC (kontrak lama tak disentuh). 19-20 (performa data besar, EF translate): NOT RUN.

## 8. Risiko dan keputusan pemilik
- `dotnet build` belum dijalankan; ekspresi EF (join, `Any` pada `PlanNumber`) perlu build dan UAT.
- Filter pegawai di FE memakai resource `employees`; dokter tanpa MstEmployee hanya muncul tanpa filter pegawai.
- Dashboard masih membaca tabel legacy: keputusan pemilik.
- Prioritas potongan Finance: keputusan pemilik.
