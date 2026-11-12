# State Transition Matrix — Sub-modul Payroll Gaji

| Field | Nilai |
|---|---|
| Blueprint | `PRL-BP-001` / `payroll-gaji`, revision `1` |
| Status | `approved` |
| `last_changed_in` | `PGJ-C1` |
| Owner | Yasmin; `approved_by`: Yasmin; `approved_at`: 2026-11-12 |
| `input_revision` | `02-backend-architecture.md` r1 |
| Traceability | PRL-DEC-07, 10, 31, 38, 43; `EPIC PGJ-04`; FR-PGJ-030 s.d. FR-PGJ-037 |
| Dampak kompatibilitas | Status baru hanya untuk `SourceType = PayrollManualInput`; status sumber otomatis tidak berubah |

## 1. Input variabel manual (`PrlPayrollVariableInput`, sumber `PayrollManualInput`)

Syarat umum seluruh transisi oleh petugas: run induk berstatus `Draft`, `CollectingInput`, atau
`Calculated` (run menerima input). Hak akses berupa pasangan Resource dan Action pada
`contracts/api-contract.md`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Buat atau impor | `Draft` | `PayrollVariableInput : Create` / `Import` | Pegawai ada di run; komponen aktif bertipe `Earning`; nominal > 0 | Ditolak, kode 400 |
| `Draft` | Ubah isi | `Draft` | `PayrollVariableInput : Update` | Token versi cocok | 409 bila versi berubah |
| `Draft` | Ajukan | `Submitted` | `PayrollVariableInput : Submit` | Isi lengkap | 400 |
| `Submitted` | Setujui | `Approved` | `PayrollVariableInput : Approve` | Pengaju boleh sama dengan penyetuju | 400 bila run tidak menerima input |
| `Submitted` | Tolak | `Rejected` | `PayrollVariableInput : Reject` | Alasan wajib | 400 |
| `Draft`, `Submitted`, `Approved` | Batalkan | `Cancelled` | `PayrollVariableInput : Cancel` | Alasan wajib | 400 |
| `Approved` | Run disahkan | `Applied` | Sistem, saat aksi pengesahan run | — | — |
| `Draft`, `Submitted`, `Approved` | Run dibatalkan | `Cancelled` | Sistem, saat aksi pembatalan run | Alasan sistem "Run dibatalkan" | — |

**Transisi tidak sah** (ditolak kode 409 dengan pesan pada `validation-matrix.md` aturan `VI-09`):

| Dari | Tindakan yang ditolak | Sebab |
|---|---|---|
| `Rejected` | Ubah, ajukan, setujui | Input ditolak bersifat final; buat input baru |
| `Cancelled` | Apa pun | Final |
| `Applied` | Apa pun | Sudah dibayar lewat run yang disahkan; koreksi lewat restitusi atau reversal run |
| `Approved` | Ubah isi | Batalkan dulu, lalu buat input baru |
| `Draft` | Setujui langsung | Harus diajukan dulu |
| Status apa pun | Aksi oleh petugas saat run `Submitted` atau lebih jauh | Run sudah tidak menerima input |

**Contoh:** petugas mengimpor 120 insentif Mei (120 baris `Draft`), mengajukan semuanya,
menyetujui 118, dan menolak 2 karena nominal salah. Run Mei dihitung ulang; 118 insentif masuk
gaji. Saat run disahkan, 118 baris menjadi `Applied`. Dua baris `Rejected` tetap tersimpan
sebagai jejak.

**Status idempoten:** aksi ke status yang sudah berlaku (misalnya menyetujui input `Approved`)
dikembalikan apa adanya tanpa perubahan.

## 2. Baris tunjangan struktur (`MstSalaryStructureAllowance`)

Master tanpa lifecycle bertingkat.

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Tambah | Aktif | `SalaryStructureAllowance : Create` | Aturan `SA-01` s.d. `SA-06` | 400 atau 409 |
| Aktif | Ubah | Aktif / nonaktif | `SalaryStructureAllowance : Update` | Token versi cocok | 409 |
| Aktif / nonaktif | Hapus (tandai `IsDelete`) | Terhapus | `SalaryStructureAllowance : Delete` | — | — |
| Terhapus | Apa pun | — | — | Tidak tersedia | 404 |

Run yang sudah dihitung tidak berubah ketika baris diubah atau dihapus; nilai baru berlaku pada
perhitungan berikutnya. Run yang sudah disahkan menyimpan snapshot di baris komponen.

## 3. Payroll run — perubahan pada transisi yang sudah ada

| Dari | Tindakan | Ke | Syarat tambahan dari desain ini | Bila dilanggar |
|---|---|---|---|---|
| `Calculated` | Ajukan | `Submitted` | Tidak ada input manual berstatus `Submitted`; tidak ada input manual yang `UpdateDateTime`-nya setelah `CalculatedAt` | 409, aturan `VI-10` dan `VI-11` |
| `Submitted` | Sahkan | `Approved` | Input manual `Approved` → `Applied` di transaksi yang sama | — |
| Status yang boleh dibatalkan | Batalkan | `Cancelled` | Input manual yang belum `Applied` → `Cancelled` di transaksi yang sama | — |

Transisi run lainnya tidak berubah.

## 4. Simulasi

Simulasi tidak punya status tersimpan. Run sementara berjenis `Simulation` hanya ada di dalam
transaksi yang selalu dibatalkan.
