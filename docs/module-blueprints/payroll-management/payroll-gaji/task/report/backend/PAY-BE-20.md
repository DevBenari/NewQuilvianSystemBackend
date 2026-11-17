# Laporan Perubahan Backend — `PAY-BE-20`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-20` |
| Judul | Aturan hitung input manual dan penjaga run |
| Slice | `MVP-1` — `EPIC PGJ-04` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/backend-roadmap.md`, kartu `PAY-BE-20` |
| Trace | FR-PGJ-033, 036, 037; PRL-DEC-07, 10; `../../../02-backend-architecture.md` bagian 5.4; `../../../contracts/state-transition-matrix.md` bagian 1 dan 3; `../../../contracts/validation-matrix.md` `VI-10`, `VI-11` |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12) |
| Dependency | `PAY-BE-18` 🟡 (berkas mesin hitung yang sama), `PAY-BE-19` 🟡 (input manual dan konstanta statusnya) |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 1 (2 berkas), logika bisnis 2 (aturan hitung, penjaga, perpindahan status dalam transaksi), kontrak API 1 (dua penolakan baru pada endpoint lama), database 1 (penulisan status di transaksi yang ada), keamanan 0, UI 1 (peringatan dan kode 409 baru terlihat di layar run) |
| Task mode | `BACKEND` |
| Target tulis | `Areas/Corporate/PayrollManagement/Services/PayrollCalculationService.cs`, `Areas/Corporate/PayrollManagement/Services/PayrollRunService.cs`; dokumen laporan dan penandaan status |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6722d7ca` (branch `Yasmina`), di atas perubahan `PAY-BE-14`–`19` yang belum di-commit |
| Tanggal | 2026-11-12 |
| Status | 🟡 — source selesai; lima kriteria terbukti lewat simulasi; **`dotnet build` tidak dijalankan atas instruksi pemilik**; uji proses di DB uji menunggu pemilik |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` |
| Pemilik / prefix | Payroll — `Prl` (registry `ACTIVE`) |
| Keberlakuan | `TOUCHED LEGACY` — dua service run yang sudah ada; tidak ada entity, endpoint, atau migration baru |
| QBE yang berlaku | `QBE-SVC-001` (logika di service), `QBE-NAM-001` |
| Hak akses | Tidak berubah. Penolakan baru memakai hak yang sudah ada (`PayrollRun : Submit`, `Approve`, `Cancel`) |
| Hasil | Tidak ada pelanggaran pada diff; checker resmi `NOT RUN` (tanpa `pwsh`) |

## 1. Masalah yang diperbaiki

Sesudah `PAY-BE-19`, petugas sudah bisa membuat dan memutuskan insentif, bonus, dan honor. Tetapi
ada tiga celah:

1. Mesin hitung ikut menghitung input yang baru **diajukan**, sehingga insentif yang belum
   disetujui bisa masuk gaji.
2. Run bisa diajukan walau masih ada input yang menunggu keputusan, atau walau ada input yang
   berubah sesudah run terakhir dihitung, sehingga angka yang diajukan tidak sesuai input terbaru.
3. Status input tidak mengikuti run: saat run disahkan input tetap `Approved`, dan saat run
   dibatalkan input tetap hidup.

## 2. Proses bisnis

**Tujuan.** Hanya penghasilan variabel yang sudah disetujui yang masuk gaji, dan angka yang
diajukan selalu sesuai input terbaru (PRL-DEC-07).

**Aturan hitung** (`02-backend-architecture.md` bagian 5.4):

| Sumber input | Status yang dihitung |
| --- | --- |
| Input manual (`PayrollManualInput`: insentif, bonus, honor) | `Approved`, `Applied` saja |
| Sumber otomatis lain (jasa medis, cuti, AR, data lama tanpa sumber) | Tidak berubah: `Submitted`, `Verified`, `Approved`, `Applied`, `Processed`, `Posted` |
| Cicilan piutang | Tidak berubah (dihitung layanan potongan cicilan) |

Input manual `Submitted` dihitung jumlahnya dan muncul sebagai peringatan: "{n} penghasilan
variabel manual masih menunggu keputusan dan tidak ikut dihitung."

**Penjaga saat run diajukan** (berurutan, keduanya 409):

| Aturan | Kondisi | Pesan |
| --- | --- | --- |
| `VI-10` | Ada input manual `Submitted` di run | "Masih ada {n} penghasilan variabel yang menunggu keputusan. Setujui atau tolak dulu." |
| `VI-11` | Ada input manual yang dibuat atau diubah sesudah `CalculatedAt` | "Penghasilan variabel berubah setelah perhitungan terakhir. Hitung ulang run dulu." |

**Status input mengikuti run** (di dalam transaksi serializable run yang sudah ada):

| Aksi run | Input manual | Hasil |
| --- | --- | --- |
| Disahkan | `Approved` | `Applied` |
| Dibatalkan | `Draft`, `Submitted`, `Approved` | `Cancelled`, alasan sistem "Run dibatalkan" |
| Dibatalkan | `Rejected`, `Applied` | Tidak berubah (keduanya final) |

**Contoh berangka** (kontrak): 120 insentif Mei diajukan; 118 disetujui dan 2 ditolak. Petugas
langsung mengajukan run tanpa menghitung ulang, sehingga ditolak `VI-11` karena persetujuan terjadi
sesudah hitung terakhir. Run dihitung ulang dan 118 insentif masuk gaji. Run diajukan, lalu
disahkan, dan 118 baris menjadi `Applied`. Dua baris `Rejected` tetap tersimpan sebagai jejak.

**Contoh penjaga `VI-10`:** 3 insentif masih `Submitted` saat petugas mengajukan run. Ditolak:
"Masih ada 3 penghasilan variabel yang menunggu keputusan. Setujui atau tolak dulu."

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PayrollCalculationService.cs` (loop input variabel, `CountableInputStatuses`), `PayrollRunService.cs`
(`CalculateAsync`, `SubmitAsync`, `ReturnAsync`, `ApproveAsync`, `CancelAsync`, `InTransactionAsync`,
`EnsureReadyToDecide`), `PayrollRunsController.cs` (pemetaan eksepsi lewat
`PayrollControllerSupport.RunAsync`), `PayrollControllerSupport.cs`, `PayrollManualInputTypes.cs`
(`PayrollManualInputStatuses`), `PayrollStatusCatalog.cs`, `PayrollVariableInputService.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/PayrollManagement/Services/PayrollCalculationService.cs` | (1) himpunan `ManualCountableInputStatuses` (`Approved`, `Applied`); (2) cabang khusus input `PayrollManualInput` di loop input variabel; sumber lain tetap memakai `CountableInputStatuses`; (3) peringatan jumlah input manual yang menunggu keputusan |
| `Areas/Corporate/PayrollManagement/Services/PayrollRunService.cs` | (1) `EnsureManualInputsSettledAsync` (`VI-10`, `VI-11`) dipanggil `SubmitAsync` sebelum `EnsureReadyToDecide`; (2) `MoveManualInputsAsync` dipanggil `ApproveAsync` (`Approved` → `Applied`) dan `CancelAsync` (belum `Applied` → `Cancelled`), keduanya sebelum `SaveChangesAsync` di transaksi yang sama; (3) jumlah input yang dipindah dicatat di log audit run (tanpa nominal) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk respons tidak berubah. `POST /payroll-runs/{id}/submit` kini dapat menjawab 409 `VI-10`/`VI-11`. Hasil hitung dapat memuat peringatan baru |
| Database | Tidak ada perubahan skema. Pengesahan dan pembatalan run kini juga menulis status input manual di transaksi yang sama |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Perilaku baru pada endpoint yang sudah ada:

### `[Tags("Corporate / Payroll Management / Payroll Run")]`

Base URL: `api/v1/corporate/payroll-management/payroll-runs`

| Method | Path | Hak akses | Perubahan | Kode baru |
|---|---|---|---|---|
| `POST` | `/{id}/calculate` | `PayrollRun : Calculate` | Input manual dihitung hanya bila `Approved`/`Applied`; peringatan input menunggu keputusan | — |
| `POST` | `/{id}/submit` | `PayrollRun : Submit` | Penjaga `VI-10` dan `VI-11` | 409 |
| `POST` | `/{id}/approve` | `PayrollRun : Approve` | Input manual `Approved` → `Applied` | — |
| `POST` | `/{id}/cancel` | `PayrollRun : Cancel` | Input manual belum `Applied` → `Cancelled` ("Run dibatalkan") | — |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan atas instruksi pemilik | `NOT RUN` | — |
| Aturan hitung: 6 status input manual dan 5 contoh sumber otomatis | Manual: hanya `Approved`/`Applied` dihitung. Otomatis: hasil sama dengan aturan lama (`Submitted` cuti, `Verified`, `Approved` jasa medis dihitung; `Cancelled` tidak) | `PASS` | Skrip simulasi |
| `VI-10`: satu input `Submitted` | 409 `VI-10` (1) | `PASS` | Skrip simulasi |
| `VI-11`: input diubah sesudah / sebelum `CalculatedAt` | 409 / lolos | `PASS` | Skrip simulasi |
| Pengesahan: `Draft`, `Approved`, `Rejected` | `Draft`, `Applied`, `Rejected` | `PASS` | Skrip simulasi |
| Pembatalan: `Draft`, `Submitted`, `Approved`, `Rejected`, `Applied` | `Cancelled` ×3, `Rejected`, `Applied` | `PASS` | Skrip simulasi |
| Perpindahan status di transaksi yang sama | Dipanggil di dalam `InTransactionAsync` sebelum satu-satunya `SaveChangesAsync` aksi itu | `PASS` | Review source |
| Pemetaan 409 | `PayrollRunConflictException` → 409 lewat `PayrollControllerSupport` | `PASS` | Review source |
| Keseimbangan kurung | 0 selisih di 2 berkas | `PASS` | Skrip penghitung |
| Uji proses di DB uji | Menunggu pemilik | `NOT RUN` | Langkah di bawah |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `REQUIRED` — dijalankan pemilik setelah build.

**Langkah uji untuk pemilik** (DB uji, data samaran, satu run yang juga memuat input otomatis):

1. Regresi: hitung run yang berisi input jasa medis, cuti, dan cicilan, tanpa input manual.
   Harapan: total sama dengan sebelum task ini.
2. Buat dua insentif lewat `PAY-BE-19`; ajukan keduanya; setujui satu. Hitung run. Harapan: hanya
   yang disetujui masuk komponen `PAY_VARIABLE_EARNING`; peringatan "1 penghasilan variabel manual
   masih menunggu keputusan…".
3. Ajukan run. Harapan: 409 `VI-10`.
4. Tolak input yang tersisa, lalu ajukan run tanpa menghitung ulang. Harapan: 409 `VI-11`.
5. Hitung ulang, lalu ajukan. Harapan: berhasil.
6. Sahkan run. Harapan: input yang disetujui menjadi `Applied`; yang ditolak tetap `Rejected`.
7. Pada run lain, buat satu input `Draft` dan satu `Approved`, lalu batalkan run. Harapan: keduanya
   `Cancelled` dengan alasan "Run dibatalkan".

**Tidak dijalankan:** `dotnet build`, uji proses, checker QBE resmi.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Input manual `Submitted` tidak dihitung; jasa medis, cicilan, cuti, AR tetap seperti sebelumnya | Terpenuhi (simulasi); regresi di DB uji menunggu | Bagian 5 |
| 2. Ajukan run dengan input `Submitted`: 409 `VI-10` | Terpenuhi (simulasi dan review) | `EnsureManualInputsSettledAsync` |
| 3. Input berubah setelah `CalculatedAt`: 409 `VI-11` | Terpenuhi (simulasi dan review) | `EnsureManualInputsSettledAsync` |
| 4. Run disahkan: `Approved` → `Applied` dalam transaksi yang sama | Terpenuhi (statis) | `ApproveAsync` |
| 5. Run dibatalkan: belum `Applied` → `Cancelled` beralasan "Run dibatalkan" | Terpenuhi (statis) | `CancelAsync` |
| DoD: build hijau | **Belum terpenuhi** | `NOT RUN` atas instruksi pemilik |
| DoD: QBE bersih | Terpenuhi untuk aturan yang dicek; checker resmi `NOT RUN` | Bagian 5 |
| DoD: lima kriteria terbukti | Lewat simulasi dan review; di DB uji **belum** | Langkah uji menunggu pemilik |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Tafsiran kontrak | (1) `VI-11` memakai `UpdateDateTime`, atau `CreateDateTime` bila belum pernah diubah, sehingga input yang **dibuat** sesudah hitung juga menahan pengajuan. (2) `VI-11` berlaku untuk input manual berstatus apa pun, sesuai bunyi kontrak "ada input manual yang berubah". Akibatnya, menolak atau membatalkan input sesudah hitung juga meminta hitung ulang, walau angkanya tidak berubah. Ini aman tetapi bisa terasa ketat |
| Risiko sela `PAY-BE-19` | Ditutup: input manual `Submitted` tidak lagi dihitung |
| Masalah yang diketahui | (1) Input manual `Draft` yang tertinggal saat run disahkan tetap `Draft` di run yang terkunci. Kontrak hanya mengatur `Approved` → `Applied`; layar sebaiknya mengingatkan petugas. (2) Layanan input manual tidak memegang kunci advisory run. Persetujuan input yang terjadi tepat bersamaan dengan pengajuan run masih mungkin lolos penjaga. Peluangnya kecil, dan perbaikannya (kunci bersama) di luar cakupan task ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Task ini: `M` `PayrollCalculationService.cs` (bersama `PAY-BE-17`/`18`) dan `PayrollRunService.cs`; ditambah laporan dan penandaan roadmap. Perubahan `PAY-BE-14`–`19` yang belum di-commit masih ada. Tidak ada yang di-stage atau di-commit |
| Langkah berikutnya | Pemilik: build dan langkah uji bagian 5. Task berikutnya: `PAY-BE-21` (simulasi tanpa jejak) |
