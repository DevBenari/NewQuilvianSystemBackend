# Permission and Audit Matrix — Sub-modul Payroll Gaji

| Field | Nilai |
|---|---|
| Blueprint | `PRL-BP-001` / `payroll-gaji`, revision `1` |
| Status | `approved` |
| `last_changed_in` | `PGJ-C1` |
| Owner | Yasmin; security owner: Security Owner; `approved_by`: Yasmin; `approved_at`: 2026-11-12 |
| `input_revision` | `contracts/api-contract.md` r1, `data/data-dictionary.md` r1 |
| Traceability | PRL-DEC-10, 16, 40, 43; DEC-PRL-013 |

Pemetaan endpoint ke hak akses **tidak** ditulis ulang di sini. Sumbernya kolom **Hak akses**
pada `contracts/api-contract.md`. Atribut diturunkan menjadi
`[AccessPermission("<Resource>", "<Action>")]`, dan pencatatan log mengikuti konvensi: `GET`
tidak dicatat, selain `GET` dicatat.

## 1. Cara kerja hak akses di repository ini

| Bagian | Berkas source @ `89b9fd69` | Peran |
|---|---|---|
| Atribut kelas | `Attributes/AccessControllerAttribute.cs` | Mendaftarkan `ControllerName` sebagai Resource yang tampil di layar Akses Role |
| Atribut aksi | `Attributes/AccessActionAttribute.cs` | Mendaftarkan Action agar dapat dicentang admin |
| Atribut penegak | `Attributes/AccessPermissionAttribute.cs`, `Filters/AccessPermissionFilter.cs` | Menolak permintaan tanpa hak dengan kode 403 |
| Jenis akses | `Constants/AccessTypes.cs` | Hanya `Read`, `Create`, `Update`, `Delete` |
| Pendaftaran menu akses | `Seeders/AccessMenuSeeder.cs` | Menyusun daftar akses yang dapat diatur admin |

Hak akses diberikan admin lewat Pengaturan → Manajemen Role → Akses Role. Kode **MUST NOT**
memeriksa nama peran, departemen, atau jabatan.

Argumen pertama `[AccessPermission]` **MUST** sama persis dengan `ControllerName`, dan argumen
kedua sama persis dengan argumen pertama `[AccessAction]` pada method yang sama. Bila menyimpang,
hasilnya 403 permanen yang tidak bisa diperbaiki dari layar Akses Role.

## 2. Resource dan action baru

| Resource (`ControllerName`) | Action | Jenis akses (`AccessTypes`) | Catatan |
|---|---|---|---|
| `SalaryStructureAllowance` | `Read` | `Read` | — |
| `SalaryStructureAllowance` | `Create` | `Create` | — |
| `SalaryStructureAllowance` | `Update` | `Update` | — |
| `SalaryStructureAllowance` | `Delete` | `Delete` | — |
| `PayrollVariableInput` | `Read` | `Read` | — |
| `PayrollVariableInput` | `Create` | `Create` | — |
| `PayrollVariableInput` | `Import` | `Create` | — |
| `PayrollVariableInput` | `Update` | `Update` | Hanya isi input `Draft` |
| `PayrollVariableInput` | `Submit` | `Update` | Dipakai juga oleh `bulk-submit` |
| `PayrollVariableInput` | `Approve` | `Update` | Dipakai juga oleh `bulk-approve` |
| `PayrollVariableInput` | `Reject` | `Update` | — |
| `PayrollVariableInput` | `Cancel` | `Update` | — |
| `PayrollRun` | `Simulate` | `Read` | Action baru pada Resource yang sudah ada; tidak mengubah data |

Resource dan action yang dipakai ulang tanpa perubahan: `PayrollRun : Read`,
`PayrollReport : ViewAmount`, `TransportAllowancePolicy : *`, `WorkforceTransportAllowance : *`,
`OvertimeRate : Read`, `SalaryStructure : Create/Update`, `SalaryGrade : Update`.

## 3. Peta peran ke hak akses (usulan)

Usulan awal untuk admin. Admin yang menetapkan, bukan kode.

| Peran rumah sakit | Hak yang diusulkan |
|---|---|
| Staf Payroll | `SalaryStructureAllowance : Read/Create/Update/Delete`, `PayrollVariableInput : Read/Create/Import/Update/Submit/Approve/Reject/Cancel`, `PayrollRun : Simulate` beserta `PayrollRun : ReadPeriods` (opsi periode pada layar simulasi), `TransportAllowancePolicy : *`, `WorkforceTransportAllowance : *`, `OvertimeRate : Read` |
| Kepala Payroll | Seluruh hak Staf Payroll, ditambah `PayrollReport : ViewAmount` |
| Finance | `PayrollReport : ViewAmount` |
| Staf HRD | `SalaryStructureAllowance : Read`, `PayrollRun : Read` |

Persetujuan input satu langkah (PRL-DEC-10, 43): pemegang `PayrollVariableInput : Approve` boleh
menyetujui input yang ia ajukan sendiri.

## 4. Kewenangan yang tidak dapat dijaga mesin hak akses

| Penjaga di aturan bisnis | Apa yang dijaganya | Apa yang **tidak** dijaganya | Risiko |
|---|---|---|---|
| Status run menerima input | Input tidak berubah setelah run diajukan | Siapa yang mengajukan run | Rendah |
| Penolakan pengajuan run bila input berubah setelah perhitungan | Hasil yang diajukan selalu sesuai input terbaru | — | Rendah |
| Pengaju boleh = penyetuju input | Sengaja diizinkan PRL-DEC-10 | Pemisahan tugas pada insentif dan bonus | **Diterima pemilik**: satu orang dapat membuat dan menyetujui insentif. Jejak pengaju dan penyetuju tetap tersimpan |
| `RequireDifferentApprover` pada run (bawaan `false`) | Bila dinyalakan, pengaju run tidak boleh mengesahkan | Input variabel | Tidak diubah desain ini |

Pejabat yang boleh menyetujui pengecualian gaji (`ApproveException`) belum dirancang di sini
karena menunggu DEC-PRL-002 dan DEC-PRL-013.

## 5. Audit

| Lapisan | Yang dicatat |
|---|---|
| Kolom audit `IdentityModel` | Pembuat, pengubah, penghapus, waktu |
| Kolom keputusan input | `SubmittedAt/By`, `ApprovedAt/By`, `RejectedAt/By`, `DecisionReason`, serta `CancelDateTime/CancelBy` bawaan |
| Custom logger | Seluruh aksi non-`GET`: `EntityId`, controller, action, status — **tanpa nominal** |
| Snapshot run | Baris komponen hasil hitung menyimpan nama, kode, sumber, dan nominal saat dihitung |

Kejadian yang wajib meninggalkan jejak tahan lama: persetujuan, penolakan, dan pembatalan input;
perubahan dan penghapusan baris tunjangan struktur; pengesahan run.

## 6. Kolom sensitif dan masa simpan

Kolom sensitif: `MstSalaryStructureAllowance.Amount`, `MstSalaryStructureAllowance.Percentage`,
`PrlPayrollVariableInput.Amount`, `PrlPayrollVariableInput.Rate`, dan seluruh nominal pada
respons laporan. Payload log **MUST NOT** memuatnya.

Masa simpan mengikuti data penggajian yang sudah ada. Tidak ada aturan masa simpan baru;
penghapusan hanya penandaan.
