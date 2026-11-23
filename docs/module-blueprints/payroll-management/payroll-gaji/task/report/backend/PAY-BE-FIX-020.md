# Laporan Audit Ulang Backend — `PAY-BE-FIX-020`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-020` (ad-hoc lanjutan seri `PAY-BE-FIX`); melanjutkan audit dan desain `PAY-BE-FIX-012` |
| Judul | Penyatuan penetapan seluruh jenis tunjangan pegawai (Master → Kebijakan per Grade Gaji → Penetapan pegawai terpadu), pelepasan alur Transportasi khusus |
| Klasifikasi | **`EPIC`** (dua repository, tabel baru, jalur uang). Aturan `STOP → DECOMPOSE` |
| Task mode | `CROSS-REPO`, **analisis saja** |
| Target tulis | **Tidak ada source backend atau frontend yang diubah.** Hanya dokumen ini |
| Branch / HEAD | BE `Yasmina` `ab340bc7de4e9a041ca3a8e0289dc910f417ec82`; FE `yasmina` `ab5843be747a32b04c90ac1a63236835234149d6` (keduanya cocok dengan remote `origin` DevBenari). Working tree keduanya **tidak bersih** (pekerjaan `PAY-BE/FE-FIX-015` sampai `019`, belum di-commit) |
| Tanggal | 23 November 2026 |
| Status | ⛔ **Berhenti pada kondisi-henti yang ditetapkan tugas**: branch target belum dikonfirmasi pengguna, dan penyimpanan relasi kebijakan umum membutuhkan tabel/kolom baru (`BLOCKED_BY_SCHEMA`) |
| Tindak lanjut | **23 Nov 2026**: pemilik mengonfirmasi branch `Yasmina`/`yasmina` dan memilih F2+F3; dikerjakan di `PAY-BE-FIX-021` (source selesai, DDL untuk pemilik). Permintaan Transportasi "semua metode" tidak dapat dipenuhi penuh sebelum F5/F6 (lihat `PAY-BE-FIX-021` bagian 2) |

## 1. Diagnosis

Seluruh temuan `PAY-BE-FIX-012` (20 Nov) **diverifikasi ulang terhadap source saat ini dan masih berlaku**:

| Fakta | Bukti (source saat ini) |
| --- | --- |
| Pilihan jenis dibatasi aturan kelayakan | `WorkforceAllowanceEligibilityRules.cs`: kategori `Shift, OnCall, Hazard, Transport, Variable` ditolak; `PolicyBased` dan `ManualInput` tidak boleh ditetapkan langsung; jenis yang punya kebijakan aktif Shift/On-call/Risiko/Transport dikeluarkan (baris 30, 66–73, 96–103, 129–142) |
| Tidak ada kebijakan umum maupun tarif per grade | Tidak ada entity `PrlAllowancePolicy` / `PrlAllowancePolicyGradeRate` pada `Areas/` dan `Repositories/` |
| Penetapan individual tak punya relasi kebijakan | `PrlWorkforceAllowance`: hanya `CalculationMethod`, `Amount`, `Percentage`, `EffectiveStartDate`, `AllowanceStatus`; tanpa `AllowancePolicyId`/`SalaryGradeId` |
| Mesin hitung melewati Transport pada penetapan pegawai | `PayrollCalculationService.cs:1239–1241` (peringatan "dilewati ... agar tidak terjadi perhitungan ganda"); Transport dihitung terpisah dari `PrlTransportAllowances` (baris 371, 661, 873, 1370+) dan hanya metode `FixedMonthly` (baris 1388) |
| FE masih memuat jalur khusus | `payroll-workforce-allowance-section.jsx` (tombol `manageTransportLabel`, `canManageTransport`), `...form-modal.jsx` (`openTransport`, `typesNoEligible*`, `openMasterLabel`, input nominal/persentase), route `src/app/payroll/workforce-compensations/[id]/transport-allowance/page.jsx` |

**Konsekuensi yang menahan tugas:** menghapus "Kelola Transportasi" atau route lama sekarang akan menghilangkan satu-satunya jalur yang bisa menetapkan dan membayar transport (kondisi henti "penghapusan jalur akan menghilangkan pembayaran existing"). Membuka Transportasi di form umum tanpa mengubah mesin hitung membuatnya **tidak terbayar** (dilewati), dan tanpa melewatkan jalur lama akan **terbayar ganda**.

## 2. Yang berubah sejak `PAY-BE-FIX-012` (prasyarat yang kini tersedia)

| Hal | Status |
| --- | --- |
| Kebijakan Transport punya `SalaryGradeId`/`SalaryStructureId` (PAY-BE-FIX-014) | IMPLEMENTED (DDL diterapkan pemilik) |
| Kebijakan Shift/On-call/Risiko punya `SalaryGradeId`/`SalaryStructureId`/`EmployeeGradeId` + helper validasi `AllowancePolicyScopeValidator` (PAY-BE-FIX-018/019) | IMPLEMENTED di source; migration `AddSalaryGradeAndStructureToShiftOnCallHazardAllowancePolicy` menunggu diterapkan |
| Aturan cakupan grade/struktur/masa berlaku satu definisi (`TransportAllowancePolicyScopeRules`) | IMPLEMENTED, dapat dipakai kebijakan umum |
| Kebijakan umum + tarif per grade, tautan penetapan→kebijakan, batch idempotent | **Belum ada** (`BLOCKED_BY_SCHEMA`) |

Pemetaan: kebijakan khusus tetap memakai `SalaryGradeId` (bukan `EmployeeGradeId`); kebijakan transport lama yang hanya punya `EmployeeGradeId` ditandai "tanpa cakupan Grade Gaji" dan harus dipetakan eksplisit (tabel pemetaan oleh HRD) sebelum dipindah, tidak ditebak.

## 3. Rancangan (tidak diubah dari `PAY-BE-FIX-012` bagian 3, 4, 5, 8, 13)

Master `MstAllowanceType` → `PrlAllowancePolicy` → `PrlAllowancePolicyGradeRate` (per `SalaryGradeId`) → `PrlWorkforceAllowance` (+ `AllowancePolicyId`, `AllowancePolicyGradeRateId`, `AssignmentSource`) → kalkulasi. Detail kolom, constraint (tidak boleh dua tarif aktif beririsan untuk kebijakan+grade; penetapan unik per pegawai+jenis+periode+sumber), idempotency, dan batch bulk ada di `PAY-BE-FIX-012` §3.2.

**Pemisahan konsep eligibility** (permintaan baru, belum ada di 012): `Selectable` (tampil di dropdown), `Assignable` (boleh menghasilkan penetapan), `Payable` (dihitung payroll). Diturunkan dari kategori master + kebijakan aktif, **bukan satu boolean**:

| Jenis | Selectable | Assignable | Payable otomatis |
| --- | --- | --- | --- |
| Rutin (Makan, Jabatan, Komunikasi, Transport `FixedMonthly`) | Ya | Ya, via kebijakan | Ya, per periode kebijakan |
| Aktivitas (Shift, On-call, Risiko, Transport `PerAttendance`/`PerDay`/`Reimbursement`) | Ya | Ya sebagai *eligibility* | **Tidak**: menunggu aktivitas aktual; consumer Payroll Run Shift/On-call/Risiko **belum ada** |
| Insidental (Bonus, Insentif, Honor tugas tambahan) | Ya | Tidak sebagai pembayaran bulanan | Tidak; tetap Payroll Run → Penghasilan Variabel |
| Lembur | Tidak | Tidak | Hanya dari data lembur terverifikasi |

## 4. Kontrak (semuanya `PROPOSED`; GET tidak mengubah data)

| Metode | Rute (usulan) | Fungsi |
| --- | --- | --- |
| GET | `.../workforce-profiles/{id}/allowance-assignments/eligible-types` | Jenis aktif beserta `selectable/assignable/payable` dan kategori; sumber yang sama dipakai FE dan BE |
| GET | `.../workforce-profiles/{id}/allowance-assignments/policies?allowanceTypeId=` | Kebijakan aktif untuk jenis itu yang cocok dengan Grade Gaji efektif pegawai (`WfpSalaryAssignment` primer aktif), masa berlaku, dan scope legal entity/site |
| GET | `.../allowance-assignments/preview?allowanceTypeId=&policyId=` | Hasil resolusi: grade yang dipakai, tarif, metode, batas, masa berlaku **dari backend**; hanya untuk role `ViewAmount` |
| POST | `.../allowance-assignments` (diperluas) | Hanya `allowanceTypeId`, `allowancePolicyId` (+ kunci idempotency); backend menghitung ulang seluruh nilai, menolak duplikat/irisan periode, mencatat audit tanpa nominal |

Izin tetap: `WorkforceAllowanceAssignment` Read/Create/Update/ViewAmount (+ approval bila `RequiresApproval`).

## 5. Rencana bertahap (dekomposisi EPIC)

| Fase | Isi | Prasyarat |
| --- | --- | --- |
| F2 | Entity/konfigurasi `PrlAllowancePolicy`, `PrlAllowancePolicyGradeRate`, kolom tautan penetapan + DDL untuk pemilik; layar kelola kebijakan umum | Keputusan D2–D8, konfirmasi branch |
| F3 | Backend: eligible-types, policies, preview, POST berbasis kebijakan (tanpa mengubah mesin hitung) | F2 diterapkan |
| F4 | Frontend: nested dropdown, nominal read-only, hapus panel besar; **tombol/route Transportasi tetap** | F3 |
| F5 | Skrip pemindahan `PrlTransportAllowance` (hanya `FixedMonthly`) → penetapan umum + pemeriksaan paritas | F3, pemetaan grade HRD |
| F6 | Mesin hitung: sumber baru dibaca, `PrlTransportAllowance` baru tidak boleh dibuat, bukti tidak ada ganda | F5 |
| F7 | Hapus tombol dan route Transportasi; histori tetap dapat dibaca | F6 terbukti |

## 6. Hasil static review (35 skenario)

Tidak ada source yang diubah, sehingga **tidak ada skenario baru yang dapat berstatus Static Review Passed**.

| Skenario | Status |
| --- | --- |
| 1–3 (hapus tombol/route/CTA Transportasi) | `Not Verified`, sengaja ditahan sampai F6 (kondisi henti) |
| 4–14, 17–20, 31–32 (dropdown jenis/kebijakan, preview, POST berbasis kebijakan, anti-duplikat) | `BLOCKED_BY_SCHEMA` |
| 15–16 (Batal tidak melakukan POST; modal dapat ditutup) | Perilaku saat ini tetap (perbaikan `PAY-FE-FIX-012`); `Not Verified` (runtime) |
| 21–24 (anti-ganda struktur/transport, grade berganti, payroll final utuh) | `Not Verified`; penjaga existing: mesin melewati Transport pada penetapan pegawai (`PayrollCalculationService.cs:1239`), snapshot final tidak disentuh |
| 25–28 (bonus/insentif/lembur/aktivitas) | Aturan tertulis di §3; consumer Shift/On-call/Risiko `belum ada` → tidak dibayar otomatis |
| 29–30 (RBAC, `ViewAmount`) | Izin existing tidak diubah; `Not Verified` (data role sungguhan) |
| 33–35 (import FE, dependensi BE, tanpa migration/schema) | Tidak ada perubahan; migration tidak dibuat/dijalankan pada tugas ini |

## 7. Pemisahan status

| Kategori | Isi |
| --- | --- |
| IMPLEMENTED | Tidak ada pada tugas ini. (Prasyarat di §2 dikerjakan pada tugas sebelumnya.) |
| PROPOSED | Kontrak §4, entity §3, fase §5, klasifikasi Selectable/Assignable/Payable |
| NOT VERIFIED | Seluruh perilaku runtime, build, tampilan, RBAC data nyata |
| BLOCKED_BY_SCHEMA | `PrlAllowancePolicy`, `PrlAllowancePolicyGradeRate`, tiga kolom tautan pada `PrlWorkforceAllowance`, tabel batch |
| NEEDS CONFIRMATION | Branch target; D2–D8 (bagian berikut); pemetaan `EmployeeGradeId`→`SalaryGradeId` untuk kebijakan transport lama |

## 8. Keputusan dan konfirmasi yang dibutuhkan

| No | Hal | Rekomendasi |
| --- | --- | --- |
| B | Branch target BE `Yasmina` / FE `yasmina` pada HEAD di atas (dua nama dalam prompt, `Yasminda`/`yasmin`, 404) | Konfirmasi memakai `Yasmina`/`yasmina` |
| D2 | Prompt ini menyebut Transportasi masuk alur umum; keputusan 16 Nov membatasi: hanya `FixedMonthly` yang rutin, `PerAttendance`/`PerDay`/`Reimbursement` tetap jalur aktivitas | Pertahankan D2 |
| D3 | Prioritas sumber bila satu jenis muncul dari beberapa sumber | Struktur > penetapan pegawai > transport lama |
| D5 | Risiko: rutin karena penempatan atau berbasis paparan | Pemilik kebijakan RS |
| D6–D8 | Kebijakan tidak menimpa `RequiresApproval`; cara pindah data transport; consumer Shift/On-call/Risiko fase terpisah | Sesuai `PAY-BE-FIX-012` §14 |

## 9. Pernyataan

`dotnet build`, `npm run build`, migration, dan uji runtime **tidak dijalankan**. Tidak ada source, schema, `ModelSnapshot`, atau data yang diubah. Tidak ada commit, push, atau deploy.
