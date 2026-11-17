# Nilai Aset — Matriks Hak Akses dan Audit

| Field | Value |
|---|---|
| `last_changed_in` | `AST-NLI-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |

Endpoint → hak akses: kolom `Hak akses` pada [`api-contract.md`](api-contract.md). Logger: selain `GET`
dicatat; pengecualian bernama: `GET /reports/valuation/export` tidak dicatat. Cara kerja hak akses sama
dengan `../../register/contracts/permission-audit-matrix.md` bagian 1.

## 1. Peta peran

| Resource : Action | Petugas Aset | Kepala Bagian Aset | Kepala Akuntansi | Direktur | Auditor |
|---|:---:|:---:|:---:|:---:|:---:|
| `AssetValuationParameter : Read` | ✓ | ✓ | ✓ | ✓ | |
| `AssetValuationParameter : Update` | | | ✓ | | |
| `AssetDepreciationPolicy : Read` / `Update` | Read | Read | ✓ | Read | |
| `AssetSubledgerSetup : Read` / `Update` | Read | Read | ✓ | | |
| `AssetBook : Read` | ✓ | ✓ | ✓ | ✓ | |
| `AssetDepreciationRun : Read` | ✓ | ✓ | ✓ | ✓ | |
| `AssetDepreciationRun : Process` | ✓ | | ✓ | | |
| `AssetValueChange : Read` | ✓ | ✓ | ✓ | ✓ | |
| `AssetValueChange : Create` | ✓ | | | | |
| `AssetValueChange : Approve` | | | ✓ | | |
| `AssetAccountingOutbox : Read` | ✓ | | ✓ | | |
| `AssetAccountingOutbox : Retry` | | | ✓ | | |
| `AssetReconciliation : Read` / `Send` | Read | | ✓ | | |

Auditor tidak diberi hak apa pun di sub-modul ini karena seluruh isinya rupiah (`AST-DEC-025`). Bila
auditor perlu memeriksa nilai, itu keputusan pemilik yang baru.

## 2. Kewenangan yang tidak dapat dijaga mesin hak akses

| Aturan | Penjaga | Yang tidak dijaga | Risiko |
|---|---|---|---|
| Penyetuju ≠ pengaju | `AssetValueChangeService` | Satu orang dua akun | Risiko organisasi |
| Proses penyusutan berurutan dan sekali | Unique index `(LegalEntityId, Period)` + pemeriksaan service | — | — |
| Kejadian tidak dobel | Unique `(SourceTransactionId, SourceVersion)` + idempotensi sisi Akuntansi | Template jurnal berulang yang masih aktif di Akuntansi | Penyusutan tercatat dua kali; dijaga `DEP-AST-03` sebelum penyalaan |

## 3. Audit

Parameter dan kebijakan tidak pernah ditimpa — setiap perubahan baris baru dengan penetap dan waktu;
setiap perubahan nilai menjadi entri `AstAssetValueEntry` tak terubah; proses penyusutan menyimpan pemroses,
waktu, jumlah aset, dan total; kotak keluar menyimpan setiap percobaan dan galat (tanpa kredensial); linimasa
aset `NilaiBerubah` untuk setiap perubahan nilai yang disetujui.

## 4. Kolom sensitif

Seluruh kolom rupiah pada `AstValuationParameter`, `AstAssetBook`, `AstAssetValueEntry`, `AstDepreciationRun`,
`AstDepreciationRunLine`, `AstValueChangeRequest`, dan `ComponentsJson`/`SubledgerJson` pada kotak keluar.
Tidak masuk logger; `LastError` kotak keluar disaring agar tidak memuat kredensial maupun isi payload.
