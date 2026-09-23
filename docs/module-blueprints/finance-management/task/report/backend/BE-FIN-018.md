# Laporan Perubahan Backend — `BE-FIN-018`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-018` |
| Judul | Alokasi, koreksi, penghapusan piutang — `AllocateAsync`/`ReverseAllocationAsync` |
| Slice | `MVP-2` — sebelumnya `BLOCKED — turunan MVP-2` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian 4 (`MVP-2`, `MVP-3` — tertahan) |
| Trace | `FIN-DES-014`, `FR-FIN-040`..`046`; kontrak `FIN-STATE-1.0` §2/§3/§4 (state-transition-matrix.md) — **terkunci** |
| Contract version | `FIN-STATE-1.0` — `draft` tapi dipakai apa adanya (satu-satunya sumber transisi status yang ada); nol perubahan diusulkan padanya |
| Dependency | `BE-FIN-017` — 🟡 sebagian 22 September 2026, `FinanceReceiptService` sudah ada (lihat [laporan](BE-FIN-017.md)) |
| Klasifikasi | `HEAVY` — logika bisnis lintas dua aggregate (`FinReceipt`, `FinReceivable`) dan dua service, dengan invariant satu-penulis yang harus dijaga; nol entity/migration baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` (disunting — `ApplyAllocationAsync`/`ReverseAllocationAsync`); `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` (disunting — `AllocateAsync`/`ReverseAllocationAsync`, `AllocationLineRequest`) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `a743388b57da91e6a0d7a42813604dc94563e38d` |
| Tanggal | 22 September 2026 |
| Status | 🟡 **SEBAGIAN — `AllocateAsync`/`ReverseAllocationAsync` selesai penuh di kedua service; QBE `PASS`.** Belum ada controller (pola sama dengan `BE-FIN-008` sebelum `BE-FIN-009`) — lihat bagian 1.4 |

---

## 0. Verifikasi dependency dan penyelesaian kontradiksi kepemilikan tulis

Pertanyaan pengguna "Apakah bisa melanjutkan BE-FIN-018?" dijawab dengan riset menyeluruh
sebelum menulis kode, karena roadmap `BE-FIN-018` sendiri bertuliskan "Kontrak: `FIN-STATE-1.0`
§piutang — **terkunci**; yang menahan hanya dependency-nya" — klaim yang MUST diverifikasi, bukan
diterima mentah.

1. **Dependency `BE-FIN-017` terpenuhi.** `FinanceReceiptService` sudah ada sejak `BE-FIN-017`.
2. **Kontradiksi ditemukan di komentar kelas `FinanceReceivableService`** (ditulis `BE-FIN-008`):
   baris "Satu-satunya penulis `FinReceivable.OutstandingAmount`" berdampingan dengan baris "MUST
   NOT dipakai untuk alokasi penerimaan — itu `FinanceReceiptService`". Kedua klaim itu tidak bisa
   sama-sama benar secara harfiah bila "alokasi" berarti mengubah `OutstandingAmount` — dan
   `state-transition-matrix.md` §2 mengonfirmasi alokasi memang mengubah `OutstandingAmount`
   (`OUTSTANDING`→`PARTIAL`→`SETTLED`).
3. **`state-transition-matrix.md` (`FIN-STATE-1.0`) dibaca penuh** §2 (piutang), §3 (koreksi/
   penghapusan — **sudah dibangun `BE-FIN-008`**, maker-checker `REQUESTED`→`APPROVED`/`REJECTED`),
   §4 (penerimaan). Temuan penting: **alokasi BUKAN maker-checker** — §2 baris "Alokasi penerimaan
   sebagian/penuh... Siapa yang boleh: Petugas AR" menunjukkan aksi LANGSUNG, berbeda dari §3 yang
   memang `REQUESTED`→`APPROVED`. Ini menyelaraskan dengan `FIN-DES-013` (alokasi manual tanpa
   pencocokan otomatis, bukan tanpa proses persetujuan — dua hal yang berbeda).

**Resolusi yang diambil (teknis, bukan kebijakan bisnis baru — nol keputusan bisnis yang
diarang)**: kepemilikan ORKESTRASI (validasi jumlah, pembuatan `FinReceiptAllocation`, mutasi
`FinReceipt`) tetap di `FinanceReceiptService` sesuai kalimat literal roadmap ("Logika pembagian
di `FinanceReceiptService`"); kepemilikan TULIS `FinReceivable.OutstandingAmount` tetap
di `FinanceReceivableService` sesuai invariant yang berulang kali ditegaskan di seluruh roadmap
modul ini sebagai "Yang mudah salah". Dua method baru,
`FinanceReceivableService.ApplyAllocationAsync`/`ReverseAllocationAsync`, menjadi SATU-SATUNYA
jalur yang menyentuh `FinReceivable` dari alokasi — dipanggil oleh `FinanceReceiptService`, tidak
pernah ditulis langsung olehnya. Ini dianggap keputusan teknis implementasi (diizinkan
"menambah permukaan teknis" sesuai `build-module-backend` langkah 4), bukan keputusan bisnis,
karena kebijakan bisnisnya sendiri (manual, tanpa auto-matching, langsung tanpa approval) sudah
terkunci penuh di `FIN-DEC-011`/`FIN-DES-013`/`state-transition-matrix.md` — tidak ada satu pun
aturan bisnis yang diputuskan sepihak di sini, hanya *siapa memanggil siapa*.

---

## 1. Keputusan desain dan batas cakupan (didokumentasikan, bukan didiamkan)

### 1.1 FR-FIN-043/044/046 sudah terpenuhi sejak `BE-FIN-008` — tidak ditulis ulang

Roadmap `BE-FIN-018` bertrace `FR-FIN-040`..`046` (tujuh butir, seluruh `EPIC FIN-06`), tetapi
tiga di antaranya **sudah lengkap** sebelum task ini dibuka:

| Requirement | Bukti sudah ada |
| --- | --- |
| `FR-FIN-043` — Pengaju tidak boleh menyetujui permohonannya sendiri | `DecideAdjustmentAsync`/`DecideWriteOffAsync` baris "if (actorUserId == adjustment.RequestedBy) throw..." — `BE-FIN-008` |
| `FR-FIN-044` — Nilai piutang belum berubah selama permohonan belum diputus | `RequestAdjustmentAsync`/`RequestWriteOffAsync` tidak pernah menyentuh `OutstandingAmount`, hanya `DecideAdjustmentAsync`/`DecideWriteOffAsync` (`approve = true`) yang mengubahnya — `BE-FIN-008` |
| `FR-FIN-046` — Piutang lunas tidak dapat dihapusbukukan | `RequestWriteOffAsync` dan `DecideWriteOffAsync` sama-sama memeriksa `receivable.Status == Settled` → `ReceivableValidationException` — `BE-FIN-008` |

Task ini **tidak menulis ulang** ketiganya — hanya mengonfirmasi lewat pembacaan source, sesuai
prinsip "jangan menggandakan pekerjaan yang sudah selesai".

### 1.2 Cakupan baru: `FR-FIN-040`, `041`, `042`, `045` (alokasi dan pembaliknya)

| Requirement | Ditegakkan di | Cara |
| --- | --- | --- |
| `FR-FIN-040` — alokasi dipilih petugas, bisa dipecah ke banyak piutang sekaligus | `FinanceReceiptService.AllocateAsync` | Menerima `IReadOnlyList<AllocationLineRequest>`, satu baris per target |
| `FR-FIN-041` — alokasi tidak boleh melebihi sisa penerimaan | `AllocateAsync` | `totalRequested > receipt.UnallocatedAmount` → `ReceivableValidationException` menyebut sisa sebenarnya |
| `FR-FIN-042` — alokasi tidak boleh melebihi sisa piutang | `FinanceReceivableService.ApplyAllocationAsync` | `amount > receivable.OutstandingAmount` → `ReceivableValidationException` menyebut sisa sebenarnya |
| `FR-FIN-045` — pembalikan tidak menghapus riwayat | `FinanceReceiptService.ReverseAllocationAsync` | Baris `FinReceiptAllocation` baru (`IsReversal = true`), baris asli tidak pernah diubah/dihapus |

### 1.3 `TargetType` ditentukan dari ada-tidaknya `ReceivableId` — bukan dipilih terpisah oleh petugas

`AllocationLineRequest.ReceivableId` bertipe `Guid?`. Bila terisi → `TargetType = RECEIVABLE`
dan `ApplyAllocationAsync` dipanggil. Bila kosong → `TargetType = INVOICE_DIRECT`, nol
pemanggilan ke `FinanceReceivableService` (pasien bayar lunas tanpa piutang, `FIN-DES-011`).
Ini konsisten dengan bentuk data `FinReceiptAllocation` yang terkunci (`data-dictionary.md` §3.2)
— tidak ada field terpisah untuk "jenis alokasi" selain `ReceivableId` itu sendiri.

### 1.4 Belum ada controller — pola yang sama dengan `BE-FIN-008`

Tidak ada task roadmap yang eksplisit memiliki `FinanceReceiptsController`
(`02-backend-architecture.md` §4.23 menyebutnya, tapi tidak ada baris `BE-FIN-0nn` yang
memilikinya). `AllocateAsync`/`ReverseAllocationAsync` karena itu murni service, belum dapat
dipanggil lewat HTTP — identik dengan `FinanceReceivableService` yang selesai penuh di
`BE-FIN-008` sebelum `FinanceReceivablesController` dibangun `BE-FIN-009`. Dicatat sebagai
langkah berikutnya, bukan gap yang menahan task ini selesai secara source.

### 1.5 Kunci advisory diurutkan menaik untuk mencegah deadlock multi-piutang

`AllocateAsync` mengunci `FIN_RECEIPT_{receiptId}` lebih dulu, lalu setiap `FIN_RECEIVABLE_{id}`
dalam urutan `Guid` menaik (bukan urutan kemunculan di request) — supaya dua permintaan alokasi
yang bersamaan menyentuh piutang yang sama tidak saling deadlock lock advisory Postgres.

### 1.6 Reversal memeriksa "sudah pernah dibalik?" sebelum mengunci — diserahkan ke Serializable

Sama seperti `DecideAdjustmentAsync`/`DecideWriteOffAsync` (`BE-FIN-008`), pemeriksaan duplikasi
dilakukan sebelum lock advisory diambil; race yang tersisa ditangkap oleh isolasi `Serializable`
milik transaksi (percobaan yang kalah akan menerima galat serialisasi dari Postgres, bukan data
yang salah tersimpan) — pola yang sudah ada, bukan risiko baru yang diperkenalkan di sini.

---

## 2. Ringkasan pekerjaan

### 2.1 `FinanceReceivableService` (disunting)

| Method (baru) | Kegunaan |
| --- | --- |
| `ApplyAllocationAsync(receivableId, amount, actorUserId, ct)` | FR-FIN-042; satu-satunya jalur alokasi menulis `OutstandingAmount` |
| `ReverseAllocationAsync(receivableId, amount, actorUserId, ct)` | Kebalikan `ApplyAllocationAsync`, dipanggil `FinanceReceiptService.ReverseAllocationAsync` |

Keduanya **MUST NOT** membuka/commit/rollback transaksi sendiri — mengikuti pola
`FinanceAccountingOutboxService.StageEventAsync`.

### 2.2 `FinanceReceiptService` (disunting)

| Method (baru) | Kegunaan |
| --- | --- |
| `AllocateAsync(receiptId, lines, actorUserId, ct)` | Orkestrasi FR-FIN-040/041, membuka transaksi `Serializable` sendiri |
| `ReverseAllocationAsync(allocationId, actorUserId, ct)` | Orkestrasi FR-FIN-045, membuka transaksi `Serializable` sendiri |

Konstruktor menerima `FinanceReceivableService` tambahan (DI, sudah terdaftar sejak `BE-FIN-008`,
nol perubahan registrasi diperlukan).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` §2, §3, §4 (dibaca penuh — sumber utama temuan bagian 0)
- `docs/module-blueprints/finance-management/04-prd-to-mvp.md` — `FR-FIN-040`..`046` (EPIC FIN-06)
- `docs/module-blueprints/finance-management/00-interview-decisions.md` — `FIN-DEC-011`, `FIN-DES-013` (02-backend-architecture.md §2.4)
- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` (dibaca penuh — sumber kontradiksi bagian 0, dan konfirmasi FR-FIN-043/044/046 sudah ada)
- `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs`, `Models/FinReceiptAllocation.cs`, `Models/FinReceipt.cs` (dibaca penuh sebelum menyunting)
- `docs/module-blueprints/finance-management/erd/data-dictionary.md` §3.2 — bentuk `FinReceiptAllocation` terkunci

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` | +`ApplyAllocationAsync`, +`ReverseAllocationAsync`; docstring kelas diperbarui meluruskan kontradiksi bagian 0 |
| `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` | +field/parameter konstruktor `FinanceReceivableService`; +`AllocateAsync`, +`ReverseAllocationAsync`, +`AllocationLineRequest`; +infrastruktur transaksi/lock (`BeginTransactionAsync`/`AcquireLockAsync`/`CommitAsync`/`RollbackAsync`, belum ada di berkas ini sebelumnya); docstring kelas diperbarui |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada Controller/DTO API baru (bagian 1.4) |
| Database | `NONE` — nol perubahan skema, nol migration baru |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada endpoint. Nol data pasien disentuh |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — lihat bagian 1.4.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **Tidak dijalankan oleh saya** | `NOT RUN` | Atas instruksi pengguna sejak `BE-FIN-002` |
| `powershell.exe -NoProfile -File tooling/qbe/Invoke-QbeConformanceCheck.ps1` | Lihat kutipan bagian bawah | `PASS` | Dijalankan latar belakang, hasil disalin apa adanya |
| Review manual: `FinanceReceivableService.OutstandingAmount` hanya ditulis dari `Decide*Async` dan `Apply/ReverseAllocationAsync` — nol titik lain | Dikonfirmasi lewat grep `OutstandingAmount =` di seluruh berkas | `PASS` | Bagian 0 |
| Review manual: `FinanceReceiptService` nol menulis kolom `FinReceivable` langsung | Dikonfirmasi — setiap penyentuhan `FinReceivable` selalu lewat `_receivableService.*` | `PASS` | Bagian 2.1/2.2 |
| Review manual: FR-FIN-041 dan FR-FIN-042 diuji dengan skenario contoh persis dari `04-prd-to-mvp.md` (Rp 5jt teralokasi Rp 4jt, tambahan Rp 1,5jt ditolak; sisa piutang Rp 2jt, alokasi Rp 3jt ditolak) | Dikonfirmasi lewat pembacaan kode — kedua kondisi `>` menghasilkan pesan yang menyebut sisa sebenarnya, sesuai contoh | `PASS` | Bagian 1.2 |
| Review manual: alokasi ke piutang `WRITTEN_OFF`/`CANCELLED` ditolak | Dikonfirmasi — `ApplyAllocationAsync` memeriksa `Status is WrittenOff or Cancelled` sebelum menghitung | `PASS` | `state-transition-matrix.md` §2 baris 44 |
| Review manual: alokasi ke penerimaan `REVERSED` ditolak | Dikonfirmasi — `AllocateAsync` memeriksa `receipt.Status == Reversed` | `PASS` | `state-transition-matrix.md` §4 baris 83 |
| `UAT-08`..`UAT-12` | **Tidak dapat dijalankan** — memerlukan database sungguhan dan (belum ada) controller/klien HTTP | `NOT RUN` | Bagian 1.4 |

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

Uji manual: `NOT APPLICABLE` — nol controller, migration `BE-FIN-016` belum dijalankan, tidak ada
jalur HTTP maupun database sungguhan untuk diuji end-to-end.

Keluaran `Invoke-QbeConformanceCheck.ps1` (dijalankan latar belakang atas working tree penuh):

```
QBE Conformance Report
Checker mode: ReportOnly
Scope: WorkingTree
Files evaluated: 13
Generated files excluded (bin/obj): 0
Test-scope files excluded from QBE-ENT-001/QBE-CFG-001/QBE-MOD-002: 0
VIOLATION: 0
REVIEW: 0
INFO: 0
Findings: none
REPORT ONLY: No enforcement/blocking performed.
Final result: PASS
```

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (persis roadmap) | Status | Bukti |
| --- | --- | --- |
| Pengaju tidak dapat menyetujui permohonannya sendiri | **Terpenuhi sejak `BE-FIN-008`** — dikonfirmasi ulang, bukan ditulis baru (bagian 1.1) | Bagian 1.1 |
| Nilai piutang tidak berubah selama belum diputus | **Terpenuhi sejak `BE-FIN-008`** untuk koreksi/penghapusan; alokasi memang tidak melalui status "belum diputus" (langsung, bukan maker-checker — bagian 0) | Bagian 0, 1.1 |
| Pembalikan tidak menghapus riwayat | **Terpenuhi** — `ReverseAllocationAsync` (bagian 1.2) | Bagian 1.2 |
| Cakupan `FinanceReceiptService` (logika pembagian) | **Terpenuhi** — `AllocateAsync`/`ReverseAllocationAsync`, dengan mutasi `FinReceivable` didelegasikan (bagian 0) | Bagian 2 |
| DoD: "Maker-checker tiga lapis" | **Sebagian** — berlaku untuk koreksi/penghapusan (sudah ada), **tidak berlaku** untuk alokasi itu sendiri karena `state-transition-matrix.md` §2 menetapkannya sebagai aksi langsung, bukan maker-checker. Dicatat sebagai temuan kontrak, bukan diabaikan (bagian 0) | Bagian 0 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | DoD roadmap "Maker-checker tiga lapis" **berpotensi tidak akurat** untuk alokasi — `state-transition-matrix.md` §2 (kontrak yang sama-sama terkunci) menyatakan alokasi adalah aksi langsung Petugas AR. Bila pemilik repository memang menginginkan maker-checker untuk alokasi juga, itu perubahan kontrak `FIN-STATE-1.0` yang butuh keputusan eksplisit, bukan sesuatu yang dapat diasumsikan dari DoD ringkas roadmap semata |
| Masalah yang diketahui | Belum ada controller (bagian 1.4) — `AllocateAsync`/`ReverseAllocationAsync` tidak dapat dipanggil dari luar proses sampai task controller dibuat |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada task ini |
| Status Git | 2 berkas disunting (`FinanceReceivableService.cs`, `FinanceReceiptService.cs`), nol berkas baru |
| Langkah berikutnya | (1) `dotnet build` oleh pengguna. (2) **Klarifikasi pemilik repository** atas temuan "Maker-checker tiga lapis" (Peringatan di atas) — apakah DoD roadmap perlu diperbarui atau `FIN-STATE-1.0` perlu diamandemen. (3) Task controller baru (`FinanceReceiptsController`, belum ada task pemilik eksplisit di roadmap manapun) untuk menjadikan `AllocateAsync`/`ReverseAllocationAsync` dapat diakses HTTP — pola yang sama seperti `BE-FIN-009` menyusul `BE-FIN-008`. (4) Otorisasi eksekusi migration `AddFinanceCollection`/`AddBillCollectionPrescriptionHandoff` supaya `UAT-08`..`UAT-12` dapat dibuktikan sungguhan |

