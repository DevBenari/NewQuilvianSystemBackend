# Laporan Perubahan Backend — `BE-FIN-077`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-077` |
| Judul | Pembayaran langsung piutang tidak lagi kehilangan metode, sumber dana, dan buktinya, dan nilai di atas ambang ditolak |
| Slice | `REV-14D` — `EPIC FIN-23`, pembayaran langsung berkontrol beserta bukti |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "Task REV-14D — `EPIC FIN-23`" |
| Trace | `FIN-DEC-126`, `FIN-DEC-132`, `FIN-DEC-134`; `FIN-DES-085`, `FIN-DES-092` |
| Contract version | `FIN-API-1.6` F.3/F.8; `FIN-VAL-1.8` `FIN-VAL-197`..`203`, `FIN-VAL-222`/`223`. `contract_status: approved 2026-10-02 (Yasmin)` |
| Dependency | `BE-FIN-075` 🟡, `BE-FIN-076` 🟡 (keduanya source lengkap, build belum diverifikasi), `BE-FIN-060` ✅ (buku mutasi piutang, selesai) |
| Klasifikasi | `HEAVY` — skor numerik mentah `MEDIUM` (repo 0 + diperiksa >20 (2) + diubah ≤3 (0) + logika kompleks (2) + kontrak diubah (2) + database 0 + auth 0 + UI 0 = 6), **dinaikkan** karena kriteria kualitatif "PERUBAHAN MEMUTUS pada endpoint produksi" dan "perubahan kontrak API" pada `TASK_CLASSIFICATION.md` — aturan "pakai faktor tertinggi yang berlaku" |
| Task mode | `BACKEND` (target tulis backend; frontend **tidak** disentuh, tetapi urutan rilisnya terikat `FE-FIN-030` — lihat bagian 7) |
| Target tulis | `NewQuilvianSystemBackend` — lihat daftar lengkap bagian 3.2 |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `0e256765` (working tree membawa perubahan tidak ter-commit dari task REV-14A/14B/14C/14D sebelumnya — lihat bagian 7) |
| Tanggal | 2 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap; `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna; dependency `BE-FIN-075`/`076` juga belum ter-build-verifikasi — lihat bagian 5 |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | Corporate |
| Module | FinanceManagement |
| Submodule | `Receivable` |
| Owner/prefix pada registry | `Fin` — `ACTIVE` sejak 2026-09-21 (enam submodul Finance, termasuk `Receivable`) |
| Keberlakuan | `TOUCHED LEGACY` — mengubah `FinanceReceivableService.RecordPaymentAsync` dan `FinanceArController` yang **sudah berjalan** sejak task lama (`FinanceArController` memakai pola pra-QBE: route `api/finance/receivable` bukan `api/v1/corporate/...`, resource `Finance.AR` bukan `FinanceReceivable`). Mengikuti `AGENTS.md`: "Ikuti kode yang sudah ada" — route, nama resource, dan struktur controller **tidak** dinormalisasi ke pola QBE yang lebih baru pada task ini, karena itu di luar wewenang literal task dan akan menjadi refactor tidak diminta |
| Status registry | `ACTIVE`. **Nol** `QBE-MOD-002`/`003` — nol entity/model baru pada task ini |
| QBE ID yang berlaku | `QBE-CODE-002`/`003` (lolos — nol Count/Max/Last+1 baru diperkenalkan); role-access-rules tidak relevan — nol `[AccessController]`/`[AccessAction]`/`[AccessPermission]` baru, resource `Finance.AR`/`Payment` yang sudah ada dipakai apa adanya |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, `POST api/finance/receivable/payment` **menerima** `BankAccountId` pada
`RecordReceivablePaymentRequest` tetapi **tidak pernah meneruskannya** ke
`FinanceReceivableService.RecordPaymentAsync` — nilainya diam-diam dibuang (temuan F3,
`00-interview-decisions.md`). Tidak ada ruas `ProofId` sama sekali pada kontrak. Tidak ada
pemeriksaan ambang nilai — petugas dapat membayar piutang berapa pun nominalnya lewat jalur
langsung tanpa kontrol, dan tanpa bukti yang tersimpan.

Baris mutasi (`FinReceivableMovement`) sendiri **sudah** punya kolom `FundingSourceId` dan `ProofId`
sejak `BE-FIN-058`, dan `FinanceSubledgerMovementService.RecordReceivableMovementAsync` **sudah**
menerima kedua parameter itu sejak awal — keduanya hanya belum pernah diisi oleh pemanggil manapun.
Task ini bukan membangun infrastruktur baru, melainkan **menyambungkan** jalur yang sudah ada.

---

## 2. Proses bisnis

**Tujuan.** Memastikan metode pembayaran, sumber dana, nomor rujukan, dan bukti benar-benar tersimpan
di baris mutasi saat pembayaran langsung piutang dicatat, dan menolak nominal yang melewati ambang
yang berlaku.

**Pelaku.** `FinanceReceivableService.RecordPaymentAsync` (diperbarui), dipanggil
`FinanceArController.RecordPayment` (`POST api/finance/receivable/payment`, kontrak lama yang
sudah berjalan — route dan nama resource **tidak diubah**).

**Pemicu dan langkah berurutan:**

1. Petugas AR mengirim `ReceivableId`, `Amount`, `PaymentMethod` (`CASH`/`TRANSFER`), `BankAccountId`
   (wajib untuk `TRANSFER`), `ReferenceNumber`, `ProofId` (**baru**, wajib), dan `Notes` opsional.
2. Validasi berurutan, independen satu sama lain kecuali yang secara eksplisit bergantung:
   - `Amount` harus positif (sudah ada, tidak diubah).
   - `PaymentMethod` harus `CASH` atau `TRANSFER`, tidak boleh kosong (`FIN-VAL-199`, `400`).
   - `TRANSFER` tanpa `BankAccountId` ditolak (`FIN-VAL-200`, `422`); `CASH` dengan `BankAccountId`
     terisi ditolak (`FIN-VAL-201`, `400`).
   - `ProofId` tidak boleh kosong (`FIN-VAL-202`, `422`).
   - Ambang aktif **wajib ada** — tanpa baris aktif, jalur ini ditolak seluruhnya (`FIN-VAL-197`,
     `404`, fail-closed sesuai `FIN-DES-086`).
   - Nominal **tidak boleh melewati** ambang aktif (`FIN-VAL-198`, `422`) — pesan mengarahkan ke
     jalur `FinPayment` berjenjang.
   - `ProofId` harus menunjuk bukti yang ada dan belum dihapus (`FIN-VAL-223`, `404`).
   - `ProofId` **tidak boleh** sudah dipakai mutasi piutang **maupun** mutasi utang supplier lain
     (`FIN-VAL-203`, `409`) — diperiksa di kedua tabel, karena satu bukti hanya untuk tepat satu
     pembayaran, bukan satu per jenis buku (`FIN-DES-087`).
3. Sesudah seluruh validasi lolos: transaksi dimulai, advisory lock diambil, piutang dibaca dan
   diperiksa ulang (status, sisa tagihan) — perilaku yang **sudah ada**, tidak berubah.
4. Baris mutasi `PEMBAYARAN-LANGSUNG` dicatat membawa **keempat ruas**: `PaymentMethodCode`,
   `FundingSourceType` (diturunkan dari metode), `FundingSourceId`, `ReferenceNumber`, dan `ProofId`
   — lewat `RecordReceivableMovementAsync` yang **tidak diubah sama sekali**, hanya dipanggil dengan
   parameter yang sebelumnya selalu `null`.
5. Metode `CASH` tetap melahirkan satu mutasi kas masuk `PENERIMAAN-TUNAI-LANGSUNG` ke Kas Kasir —
   perilaku yang **sudah ada**, tidak berubah. Anggaran kas kecil tidak pernah tersentuh jalur ini.

**Aturan yang berlaku.** `FIN-DEC-126`/`132` (metode, sumber dana, bukti dibawa baris mutasi, kas
tunai menggerakkan Kas Kasir), `FIN-DEC-134` (ambang menolak nominal berlebih, fail-closed tanpa
ambang), `FIN-DES-085` (keputusan arsitektur ruas-ruas ini), `FIN-DES-092` (gerbang proof terkait).

**Jalur tidak normal.** Permintaan tanpa `ProofId` (klien lama yang belum diperbarui) akan **selalu**
ditolak `422` — ini **disengaja** dan merupakan inti perubahan memutus task ini, bukan bug. Permintaan
dengan `ProofId` yang menunjuk bukti sudah terpakai ditolak `409` beserta pesan yang mengarahkan
mengunggah bukti baru.

**Hasil akhir.** Setiap pembayaran langsung piutang kini membawa jejak metode, sumber dana, rujukan,
dan bukti yang dapat ditelusuri — dan nilai di luar kendali ambang tidak akan pernah tercatat lewat
jalur ini.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/02-backend-architecture.md` (`FIN-DES-085` penuh)
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` (`F.5`, `FIN-VAL-197`..`206`)
- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` (method `RecordPaymentAsync` penuh — sebelum dan sesudah; tiga exception `ReceivableBadRequestException`/`ReceivableValidationException`/`ReceivableConflictException` yang **sudah ada**)
- `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceArController.cs` (penuh — pola `Failure`/`IsHandled` generik yang **sudah ada**, dipakai ulang apa adanya; dipastikan **hanya satu** pemanggil `RecordPaymentAsync` di seluruh repository)
- `Areas/Corporate/FinanceManagement/Receivable/DTOs/FinanceArDtos.cs` (`RecordReceivablePaymentRequest` — dipastikan `BankAccountId` sudah ada tetapi tidak pernah diteruskan)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerMovementService.cs` (`RecordReceivableMovementAsync` — dipastikan `fundingSourceId`/`proofId` sudah didukung sejak awal, nol perubahan dibutuhkan di sana)
- `Repositories/ApplicationDbContext.cs` (dipastikan `MstDirectPaymentThresholds`, `FinTransactionProofs`, `FinReceivableMovements`, `FinSupplierPayableMovements` seluruhnya sudah terdaftar sebagai `DbSet`, nol perubahan dibutuhkan)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` | `RecordPaymentAsync`: signature bertambah `Guid? fundingSourceId` dan `Guid proofId`; tujuh validasi baru berurutan sebelum transaksi dibuka (`FIN-VAL-197`..`203`); `RecordReceivableMovementAsync` kini dipanggil dengan `fundingSourceId`/`proofId` terisi; `fundingSourceType` diturunkan dari `normalizedPaymentMethod`; respons membawa `PaymentMethod`/`FundingSourceId`/`ProofId`. **Nol** method lain disentuh |
| `Areas/Corporate/FinanceManagement/Receivable/DTOs/FinanceArDtos.cs` | `RecordReceivablePaymentRequest` bertambah `ProofId` (tanpa `[Required]` — lihat bagian 7); `ReceivablePaymentResponse` bertambah `PaymentMethod`, `FundingSourceId`, `ProofId` |
| `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceArController.cs` | `RecordPayment` meneruskan `request.BankAccountId` dan `request.ProofId` (sebelumnya `BankAccountId` tidak diteruskan sama sekali); `[ProducesResponseType]` bertambah `404` dan `409` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **PERUBAHAN MEMUTUS** pada `POST api/finance/receivable/payment` — `ProofId` kini wajib; `BankAccountId` kini benar-benar ditegakkan (wajib untuk `TRANSFER`, dilarang untuk `CASH`). Klien lama yang belum mengirim `ProofId` akan **selalu** menerima `422`. Respons bertambah tiga ruas (aditif, aman) |
| Database | **NOT APPLICABLE.** Nol model/migration baru — seluruh kolom (`FundingSourceId`, `ProofId` pada `FinReceivableMovement`) sudah ada sejak `BE-FIN-058` |
| Keamanan/Auth | **NOT APPLICABLE.** Nol resource/action baru — `Finance.AR : Payment` yang sudah ada dipakai apa adanya |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / AR V2

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/payment` | Mencatat pembayaran langsung piutang — kini wajib membawa metode, sumber dana (untuk transfer), nomor rujukan, dan bukti; ambang ditegakkan | `Finance.AR : Payment` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Permintaan eksplisit pengguna. Dependency `BE-FIN-075`/`076` juga belum ter-build-verifikasi — risiko kumulatif dicatat bagian 7 |
| Pembacaan kode: metode dan sumber dana tersimpan, bukan diterima lalu dibuang | Terverifikasi — `fundingSourceId` dan `proofId` kini diteruskan ke `RecordReceivableMovementAsync` yang menulisnya ke `FinReceivableMovement` | `PASS` | `FinanceReceivableService.cs` baris 835-850 |
| Pembacaan kode: nominal di atas ambang ditolak dan diarahkan ke jalur berjenjang | Terverifikasi — perbandingan `amount > threshold.Amount` melempar `ReceivableValidationException` dengan pesan yang menyebut jalur `FinPayment` berjenjang | `PASS` | `FinanceReceivableService.cs` baris 765-770 |
| Pembacaan kode: tanpa ambang aktif seluruh pembayaran ditolak | Terverifikasi — `SingleOrDefaultAsync(...) ?? throw new KeyNotFoundException(...)` dieksekusi SEBELUM nominal dibandingkan, sebelum transaksi dibuka | `PASS` | `FinanceReceivableService.cs` baris 760-763 |
| Pembacaan kode: `ProofId` yang sudah terpakai ditolak `409` | Terverifikasi — pre-check `AnyAsync` pada **kedua** tabel mutasi (piutang dan utang supplier) melempar `ReceivableConflictException`. **Catatan jujur:** ini pre-check, bukan jaminan atomik — jaring pengaman sebenarnya tetap unique index database; celah race theoretically ada tetapi akan jatuh ke `DbUpdateException` generik (bukan `409` bersih) bila benar-benar terjadi, pola yang sama dengan pre-check idempotensi lain di rumpun ini (mis. `FinanceReceiptService.CreateSucceededReceiptAsync`) | `PASS` dengan catatan | `FinanceReceivableService.cs` baris 780-790 |
| Pembacaan kode: pembayaran tunai menggerakkan Kas Kasir dan tidak menyentuh anggaran kas kecil | Terverifikasi — blok `if (normalizedPaymentMethod == "CASH")` memanggil `RecordCashMovementAsync` dengan `FinCashMovementTypes.PenerimaanTunaiLangsung` (kontribusi Kas Kasir per `FIN-DES-081`); **nol** referensi ke tabel anggaran kas kecil pada method ini — tidak diubah dari perilaku sebelumnya | `PASS` (diwarisi, tidak diubah) | `FinanceReceivableService.cs` baris 852-870 |
| Pembacaan kode: `PaymentMethod` kosong atau di luar `TRANSFER`/`CASH` ditolak `400` | Terverifikasi — pola `is not ("CASH" or "TRANSFER")` menangkap `null`, string kosong, dan nilai lain | `PASS` | `FinanceReceivableService.cs` baris 733-738 |
| Pembacaan kode: nol pemanggil lain yang perlu disesuaikan | Terverifikasi — pencarian `RecordPaymentAsync` pada seluruh repository hanya menemukan satu pemanggil (`FinanceArController.cs`) | `PASS` | Grep seluruh repository, dilampirkan pada riwayat sesi |

Uji manual: `NOT FEASIBLE` — memerlukan runtime aplikasi aktif, migration `BE-FIN-074` sudah
diterapkan, konfigurasi `FinanceManagement:TransactionProof:*` terisi, dan ambang aktif sudah
ditetapkan lewat `BE-FIN-076`; semuanya di luar cakupan perubahan source tanpa eksekusi aplikasi.

**Tidak dijalankan:** `dotnet build` (permintaan eksplisit pengguna), uji manual jalur tolak ambang
dan bukti terpakai (`NOT FEASIBLE`), `dotnet test` (backend tidak memelihara project automated test).

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Metode dan sumber dana tersimpan, bukan diterima lalu dibuang | Terpenuhi (source) | Bagian 2, 5 |
| Nominal di atas ambang ditolak dan diarahkan ke jalur berjenjang | Terpenuhi | Bagian 5 |
| Tanpa ambang aktif seluruh pembayaran ditolak | Terpenuhi | Bagian 5 |
| `ProofId` yang sudah terpakai ditolak `409` | Terpenuhi (pre-check; lihat catatan race bagian 5) | Bagian 5 |
| Pembayaran tunai menggerakkan Kas Kasir dan tidak menyentuh anggaran kas kecil | Terpenuhi (diwarisi, tidak diubah) | Bagian 5 |
| `dotnet build` PASS | **Belum terpenuhi** — `NOT RUN` atas permintaan eksplisit pengguna | Bagian 5 |
| Urutan rilis tercatat | Terpenuhi | Bagian 7 |
| Laporan task tracked ada | Terpenuhi | Berkas ini |

Butir DoD `dotnet build PASS` **belum terpenuhi** karena sengaja tidak dijalankan atas instruksi
eksplisit pengguna. Ini **bukan** pengecualian DoD permanen (`status-task-roadmap.md` §2.2).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Urutan rilis — PERUBAHAN MEMUTUS (wajib dibaca pemilik rilis)** | Backend ini **MUST NOT** dirilis ke produksi sendirian sebelum `FE-FIN-030` siap mengirim `ProofId`. Begitu backend ini aktif, **setiap** permintaan `POST api/finance/receivable/payment` yang tidak menyertakan `ProofId` akan **selalu** ditolak `422` — termasuk dari layar lama yang belum diperbarui. Urutan yang aman: **(a)** rilis bersamaan (backend dan `FE-FIN-030` dalam satu jendela rilis), atau **(b)** `FE-FIN-030` lebih dulu memakai mode kompatibel-mundur bila backend-nya belum aktif, baru backend menyusul. **MUST NOT** merilis backend lebih dulu lalu `FE-FIN-030` menyusul — itu akan mematikan jalur pembayaran langsung piutang di produksi selama jeda rilis |
| Peringatan | **Risiko kumulatif tanpa build** berlanjut dari `BE-FIN-074`/`075`/`076`: `MstDirectPaymentThreshold`, `FinTransactionProof`, dan `FinanceTransactionProofOptions` yang dipakai task ini juga belum pernah di-build. Disarankan menjalankan `dotnet build` sebelum melanjutkan ke `BE-FIN-078` |
| Masalah yang diketahui | **Celah race pre-check `ProofId`** (lihat bagian 5) — bukan temuan baru, pola yang sama sudah ada di idempotensi lain pada rumpun Finance ini. Dicatat terbuka, bukan diperbaiki dengan infrastruktur penangkap `DbUpdateException` tambahan yang tidak diminta task ini |
| Risiko tersisa | Klien lama (bila ada) yang memanggil `POST api/finance/receivable/payment` tanpa `ProofId` akan berhenti berfungsi segera setelah backend ini aktif — lihat baris "Urutan rilis" di atas |
| Perubahan sampingan | `NONE` — ketiga berkas yang diubah murni perluasan untuk task ini; nol method lain pada `FinanceReceivableService.cs`/`FinanceArController.cs` disentuh |
| Interupsi | `NONE` |
| Status Git | `git status --short` menunjukkan tiga berkas task ini (seluruhnya modifikasi, nol berkas baru), tercampur dengan modifikasi tidak ter-commit dari task `BE-FIN-058`..`076` yang memang belum di-commit sejak sebelum task ini dimulai. Tidak ada file di luar scope Finance Management yang tersentuh |
| Langkah berikutnya | (1) Pengguna menjalankan `dotnet build` (mencakup `BE-FIN-074`-`077` sekaligus) dan mengonfirmasi hasilnya; (2) pemilik rilis mengoordinasikan `FE-FIN-030` sesuai urutan rilis di atas sebelum mengaktifkan backend ini di produksi; (3) `BE-FIN-078` melanjutkan dengan bentuk **sama persis** pada jalur pembayaran langsung utang supplier |
