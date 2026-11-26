# PAY-BE-FIX-027 — Tambah Tunjangan Pegawai: semua jenis dapat dipilih, resolusi kebijakan multi-sumber (backend)

Tanggal: 24 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `261bb431` (working tree memuat PAY-BE-FIX-025/026 yang belum di-commit). Lanjutan PAY-BE-FIX-021/022. Pasangan: PAY-FE-FIX-027.

`AUTOMATED TEST: NOT APPLICABLE` (backend). Tes teks/perilaku di sisi frontend yang membaca source backend: 12 tes baru + 53 tes lama lulus. `dotnet build`, migration, database, runtime: NOT RUN (status **IMPLEMENTED di source, belum dikompilasi**).

## 1. Akar masalah
Dropdown pertama dikunci frontend dengan `disabled: !assignable`, padahal backend sudah mengirim `Selectable=true`. `Assignable` memang bernilai false untuk Lembur, Transport (tanpa kebijakan umum), Siaga, Risiko, dan Shift karena alur lama hanya mengenal tunjangan rutin dari `PrlAllowancePolicy`. Kebijakan Transport Bulanan Karyawan (`PrlTransportAllowancePolicy`) tidak pernah masuk daftar yang dapat disimpan karena kunci asing penetapan umum menunjuk `PrlAllowancePolicy`.

## 2. Matriks jenis tunjangan, sumber kebijakan, dan jalur
| Jenis | Pengenal | Sumber kebijakan | AssignmentMode | Jalur simpan | Status |
| --- | --- | --- | --- | --- | --- |
| Tunjangan rutin (Makan, Jabatan, ...) | kategori bukan Transport/Shift/OnCall/Hazard/Variable, komponen bukan lembur | `PrlAllowancePolicy` + tarif per Grade Gaji | Policy | `POST from-policy` (General) -> `PrlWorkforceAllowance` | IMPLEMENTED (sudah ada), CanSave bila ada kebijakan |
| Transport | kategori Transport | `PrlAllowancePolicy` (General) **dan** `PrlTransportAllowancePolicy` (TransportLegacy) | TransportPolicy | General -> `PrlWorkforceAllowance`; TransportLegacy FixedMonthly -> `PrlTransportAllowance` | IMPLEMENTED; metode lain = ditinjau saja |
| Shift | kategori Shift | `MstShiftAllowancePolicy` | ActivityPolicy | tidak ada | **BLOCKED** (consumer aktivitas payroll belum ada) |
| Siaga (On Call) | kategori OnCall | `MstOnCallAllowancePolicy` | ActivityPolicy | tidak ada | **BLOCKED** |
| Risiko | kategori Hazard | `MstHazardAllowancePolicy` | ActivityPolicy | tidak ada | **BLOCKED** |
| Lembur | komponen payroll `IsOvertimeBased` atau metode `Overtime` | tidak ada | OvertimeHandoff | tidak ada; lewat HRD Lembur + handoff | **BLOCKED** dari popup; jalur sah sudah ada di HRD Lembur |
| Bonus / Insentif / Honor / manual | kind Variable/Manual | tidak ada | VariableInput | tidak ada | **BLOCKED** dari popup (butuh Payroll Run) |

## 3. Matriks metode dan field popup
| Metode | Field yang tampil | Sumber |
| --- | --- | --- |
| FixedAmount / PercentageOfBaseSalary (umum) | nominal atau persen, batas, pajak, prorata, masa berlaku (hanya baca) | `PreviewAsync` General |
| FixedMonthly (transport lama) | nominal bulanan (maks. batas bulanan), prorata, masa berlaku | `PreviewLegacyTransportAsync` |
| PerAttendance / PerDay / Reimbursement | rincian kebijakan; **tidak dapat disimpan** ("belum dihitung mesin payroll") | `EvaluateLegacyTransportAsync` |
| Shift / Siaga / Risiko | tarif, persen, batas per periode, syarat (kehadiran cocok, verifikasi atasan, clearance K3, dst.) hanya baca | `PreviewActivityAsync`, `CanSave=false` |
| Lembur / manual | daftar data yang dibutuhkan + penjelasan jalur; tanpa isian | `RequiredInputs`, `Reason` |
Tidak ada isian nominal, metode, atau tanggal bebas pada popup mana pun.

## 4. Perubahan backend
- `DTOs/PayrollAllowancePolicyAssignmentDtos.cs` (additive): `WorkforceAllowanceAssignmentModes`; sumber `ShiftActivity/OnCallActivity/HazardActivity`; tipe jenis + `AssignmentMode, RequiresPolicy, RequiredInputs, RequiresVerification, CanSave, Hint`; opsi kebijakan + `Selectable`; pratinjau + `PolicySource, CanSave, BlockedReason, RequiresVerification, Requirements`; request + `PolicySource` (kosong = General).
- `Services/PayrollWorkforceAllowanceService.Sources.cs` (baru): `ScopeMatches` (satu aturan cakupan: Grade Gaji, Struktur Gaji, legal entity, fasilitas, unit, kategori dan tipe kepegawaian dari Struktur Gaji efektif), `BuildProfile`, `LoadActivityPoliciesAsync`, `PreviewActivityAsync`, `EvaluateLegacyTransportAsync`, `ResolveLegacyTransportAsync`, `PreviewLegacyTransportAsync`, `CreateLegacyTransportAsync`, `NormalizePolicySource`.
- `Services/PayrollWorkforceAllowanceService.Policy.cs`: `EffectiveGrade` + struktur/unit/kategori/tipe; `GetEligibleTypesAsync`, `GetPoliciesAsync` ditulis ulang untuk multi-sumber; `PreviewAsync(..., policySource, ...)`; `CreateFromPolicyAsync` menjadi pengarah per sumber; jalur umum menolak komponen lembur.
- `Controllers/PayrollWorkforceAllowancesController.cs`: query `policySource` pada pratinjau. Route, izin, `[AccessAction]` tidak berubah.

## 5. Alur proses
Pilih jenis -> backend memberi jalur -> (berkebijakan) pilih kebijakan yang cocok cakupan -> pratinjau hasil resolusi server -> Simpan hanya bila `CanSave` -> tulis ke tabel penetapan yang kunci asingnya benar -> Payroll Run membaca: `PrlWorkforceAllowance` Approved (Transport hanya `AssignmentSource=Policy`) atau `PrlTransportAllowance` Active FixedMonthly (via `BuildTransportAllowanceLines`). Jenis berbasis aktivitas/lembur/variabel tidak menghasilkan transaksi apa pun dari popup; lembur tetap lewat realisasi terverifikasi dan handoff HRD, penghasilan variabel lewat Payroll Run, Penghasilan Variabel dengan persetujuannya.

## 6. Pengamanan
- Identitas kebijakan tidak pernah dipertukarkan: setiap pilihan membawa `Source`; simpan transport lama menulis `TransportAllowancePolicyId = policy.Id` (bukan `AllowancePolicyId`). Sumber tak dikenal -> 400.
- Cakupan dicek ulang di pratinjau dan simpan (bukan hanya daftar); kebijakan grade/struktur lain -> 400.
- Nominal selalu dari kebijakan di server (nominal bulanan dibatasi batas bulanan); klien tidak mengirim nominal/tanggal. `ViewAmount` tetap menyembunyikan nominal pratinjau.
- Persetujuan: jalur transport lama tidak punya alur persetujuan, sehingga jenis yang `RequiresApproval` ditolak dan diarahkan ke Kebijakan umum (yang punya alur Draft/Submit/Approve).
- Pembayaran ganda: pagar dua arah (`HasUnifiedTransportAsync` pada simpan lama; `HasLegacyMonthlyTransportAsync` pada simpan umum) + indeks unik satu baris transport lama per pegawai; mesin hitung tetap menggagalkan pegawai bila dua sumber beririsan.
- Tidak ada perubahan skema, migration, data legacy, payroll historis. Audit tanpa nominal (`WorkforceAllowance.CreateFromLegacyTransportPolicy`).

## 7. Kasus Transport Bulanan Karyawan Grade 3 (TAP-RSMMC-00001)
Ditemukan bila Grade Gaji efektif pegawai = Grade 3 dan Struktur Gaji pegawai cocok (struktur kosong pada kebijakan = semua struktur pada grade itu). FixedMonthly Rp250.000, ikut payroll -> pratinjau Rp250.000 dan dapat disimpan **bila** pegawai belum punya baris transport lama (indeks unik), jenis tidak mewajibkan persetujuan, dan tidak beririsan dengan penetapan umum. Bila tidak, popup menampilkan penyebab konkret dan tidak membuat penetapan.

## 8. Risiko, blocker, keputusan owner
1. **BLOCKED** Shift, Siaga, Risiko: tidak ada consumer payroll yang membayar dari aktivitas verified dan tidak ada penyimpanan aktivitas yang divalidasi. Opsi: bangun penghubung roster/absensi/penugasan -> `PrlPayrollVariableInput` atau tabel aktivitas (butuh skema dan keputusan), atau tetap lewat input variabel manual.
2. **BLOCKED** Lembur: dibayar lewat HRD Lembur + handoff; tidak diduplikasi di popup. Opsi: daftar realisasi lembur siap-handoff milik pegawai pada popup (butuh endpoint baca).
3. **BLOCKED** Variabel/manual: `CreatePayrollVariableInputRequest` butuh `PayrollRunEmployeeId` (konteks Payroll Run), hanya `InputType` Incentive/Bonus/AdditionalDutyHonorarium. Opsi: endpoint pembuka yang memilih Payroll Run lalu memetakan pegawai ke baris run (butuh keputusan alur persetujuan).
4. **NEEDS CONFIRMATION** Transport lama: satu baris per pegawai (indeks unik); pegawai yang sudah punya baris (walau nonaktif) tidak dapat memakai jalur ini. Pemindahan kebijakan ke Kebijakan umum (skrip PAY-BE-FIX-022) tetap jalur yang disarankan untuk persetujuan dan banyak penetapan.
5. **NEEDS CONFIRMATION** PerAttendance/PerDay/Reimbursement: mesin belum menghitung; tidak ada penetapan otomatis.
6. Cakupan unit/kategori/tipe kepegawaian diambil dari Struktur Gaji efektif (profil tidak menyimpannya); kebijakan Shift/Siaga/Risiko tanpa Grade Gaji tetap ditampilkan (ditinjau saja).
7. `dotnet build` belum dijalankan; kompilasi C# belum terbukti. Performa pemuatan daftar jenis (beberapa query kecil) belum diukur.
