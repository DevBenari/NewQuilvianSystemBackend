# Laporan Perubahan Backend — `BE-FIN-078`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-078` |
| Judul | Pembayaran langsung utang supplier mendapat kontrol yang sama bentuknya dengan piutang |
| Slice | `REV-14D` — `EPIC FIN-23`, pembayaran langsung berkontrol beserta bukti |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "Task REV-14D — `EPIC FIN-23`" |
| Trace | `FIN-DEC-126`, `FIN-DEC-133`, `FIN-DEC-134`; `FIN-DES-085`, `FIN-DES-092` |
| Contract version | `FIN-API-1.6` F.3/F.8. `contract_status: approved 2026-10-02 (Yasmin)` |
| Dependency | `BE-FIN-075` 🟡, `BE-FIN-076` 🟡 (source lengkap, build belum diverifikasi), `BE-FIN-061` ✅ (buku mutasi utang, selesai) |
| Klasifikasi | `HEAVY` — skor numerik mentah `MEDIUM` (repo 0 + diperiksa >20 (2) + diubah ≤3 (0) + logika kompleks (2) + kontrak diubah (2) + database 0 + auth 0 + UI 0 = 6), **dinaikkan** karena kriteria kualitatif "PERUBAHAN MEMUTUS pada endpoint produksi" — konsisten dengan klasifikasi `BE-FIN-077` yang bentuknya identik |
| Task mode | `BACKEND` (target tulis backend; frontend **tidak** disentuh, tetapi urutan rilisnya terikat `FE-FIN-030` — lihat bagian 7) |
| Target tulis | `NewQuilvianSystemBackend` — lihat daftar lengkap bagian 3.2 |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `0e256765` (working tree membawa perubahan tidak ter-commit dari task REV-14A/14B/14C/14D sebelumnya — lihat bagian 7) |
| Tanggal | 2 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap; `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna; dependency `BE-FIN-075`/`076`/`077` juga belum ter-build-verifikasi — lihat bagian 5 |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | Corporate |
| Module | FinanceManagement |
| Submodule | `Payable` |
| Owner/prefix pada registry | `Fin` — `ACTIVE` sejak 2026-09-21 (enam submodul Finance, termasuk `Payable`) |
| Keberlakuan | `TOUCHED LEGACY` — mengubah `FinanceSupplierPayableService.RecordDirectPaymentAsync` dan `FinanceApController` yang **sudah berjalan** (pola pra-QBE yang sama dengan `FinanceArController`: route `api/finance/payable`, resource `Finance.AP`). Route, nama resource, dan struktur controller **tidak dinormalisasi** ke pola QBE yang lebih baru — di luar wewenang literal task, akan jadi refactor tidak diminta |
| Status registry | `ACTIVE`. **Nol** `QBE-MOD-002`/`003` — nol entity/model baru pada task ini |
| QBE ID yang berlaku | `QBE-CODE-002`/`003` (lolos — nol Count/Max/Last+1 baru); role-access-rules tidak relevan — nol `[AccessController]`/`[AccessAction]`/`[AccessPermission]` baru, resource `Finance.AP`/`Payment` yang sudah ada dipakai apa adanya |

---

## 1. Masalah yang diperbaiki

Berbeda dari sisi piutang (`BE-FIN-077`), sisi utang supplier **sudah** meneruskan `BankAccountId`
sebagai `fundingSourceId` ke `RecordSupplierPayableMovementAsync` sebelum task ini — temuan F3 pada
`00-interview-decisions.md` rupanya sudah diperbaiki sebagian di titik ini. Yang **benar-benar hilang**
ditemukan saat memeriksa `RecordSupplierPaymentRequest`:

1. **`BankAccountId` didefinisikan `[Required] Guid` (non-nullable)** — bukan `Guid?`. Artinya
   **setiap** pembayaran, termasuk `CASH`, "mewajibkan" rekening bank pada lapisan DTO, dan
   `FIN-VAL-201` ("CASH tetapi rekening bank diisi → ditolak") **mustahil ditegakkan** karena tidak
   ada cara merepresentasikan "tidak ada rekening" selain `Guid.Empty` yang tidak pernah diperiksa.
2. **`ProofId` tidak ada sama sekali** pada kontrak maupun service.
3. **Nol pemeriksaan ambang** — nominal berapa pun dapat dibayar langsung.
4. **Nol validasi `PaymentMethod`** — string apa pun diterima selama bukan `"CASH"` dianggap transfer.

---

## 2. Proses bisnis

**Tujuan.** Menyamakan persis bentuk kontrol pembayaran langsung utang supplier dengan piutang
(`BE-FIN-077`) — perbedaan bentuk antar kedua jalur dinyatakan sebagai **cacat** oleh acceptance
criteria roadmap, bukan variasi yang sah.

**Pelaku.** `FinanceSupplierPayableService.RecordDirectPaymentAsync` (diperbarui), dipanggil
`FinanceApController.RecordPayment` (`POST api/finance/payable/payment`, kontrak lama yang sudah
berjalan — route dan nama resource **tidak diubah**).

**Pemicu dan langkah berurutan.** Identik dengan `BE-FIN-077`, diterapkan pada utang supplier:

1. Petugas AP mengirim `SupplierPayableId`, `Amount`, `PaymentMethod` (`CASH`/`TRANSFER`),
   `BankAccountId` (**kini opsional**, wajib untuk `TRANSFER`), `ReferenceNumber` (**kini opsional**
   — server sudah lama membangkitkan nomor bawaan bila kosong), `ProofId` (**baru**, wajib), `Notes`
   opsional.
2. Tujuh validasi berurutan — **pesan dan kode identik** `BE-FIN-077`: `PaymentMethod` tidak
   valid (`FIN-VAL-199`, `400`); `TRANSFER` tanpa rekening (`FIN-VAL-200`, `422`); `CASH` dengan
   rekening terisi (`FIN-VAL-201`, `400`); `ProofId` kosong (`FIN-VAL-202`, `422`); ambang tidak
   aktif (`FIN-VAL-197`, `404`, fail-closed); nominal melewati ambang (`FIN-VAL-198`, `422`); bukti
   tidak ditemukan (`FIN-VAL-223`, `404`); bukti sudah terpakai — diperiksa di **kedua** tabel mutasi
   (`FIN-VAL-203`, `409`).
3. Sesudah validasi lolos: transaksi dan advisory lock seperti sebelumnya, tidak berubah.
4. Baris mutasi `PEMBAYARAN-LANGSUNG` membawa `PaymentMethodCode`, `FundingSourceType`,
   `FundingSourceId` (**sudah diteruskan sejak sebelumnya**), `ReferenceNumber`, dan `ProofId`
   (**baru diteruskan**) — lewat `RecordSupplierPayableMovementAsync` yang **tidak diubah sama
   sekali**, sama seperti `RecordReceivableMovementAsync` pada `BE-FIN-077`.
5. Metode `CASH` tetap melahirkan mutasi kas keluar `PEMBAYARAN-TUNAI-LANGSUNG` dari Kas Kasir —
   perilaku **sudah ada**, tidak berubah. Anggaran kas kecil **tidak pernah** tersentuh jalur ini
   (`FIN-DEC-133`) — dipastikan ulang dengan membaca kode: nol referensi ke tabel anggaran kas kecil
   di method ini sebelum maupun sesudah perubahan.

**Aturan yang berlaku.** `FIN-DEC-126` (bentuk sama untuk kedua jalur), `FIN-DEC-133` (kas tunai
pembayaran langsung menggerakkan Kas Kasir, bukan kas kecil), `FIN-DEC-134` (ambang, fail-closed),
`FIN-DES-085`, `FIN-DES-092`.

**Jalur tidak normal.** Identik `BE-FIN-077`: permintaan tanpa `ProofId` selalu `422` (disengaja,
inti perubahan memutus); `ProofId` yang sudah terpakai (di buku piutang **maupun** utang) ditolak
`409`.

**Hasil akhir.** Pembayaran langsung utang supplier kini membawa jejak metode, sumber dana, rujukan,
dan bukti yang dapat ditelusuri, dengan bentuk kontrak **identik** jalur piutang.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/Corporate/FinanceManagement/Payable/Services/FinanceSupplierPayableService.cs` (method `RecordDirectPaymentAsync` penuh — sebelum dan sesudah; tiga exception `PayableBadRequestException`/`PayableValidationException`/`PayableConflictException` yang **sudah ada**)
- `Areas/Corporate/FinanceManagement/Payable/Controllers/FinanceApController.cs` (penuh — pola `Failure`/`IsHandled` generik identik `FinanceArController`, dipakai ulang apa adanya; dipastikan **hanya satu** pemanggil `RecordDirectPaymentAsync` di seluruh repository)
- `Areas/Corporate/FinanceManagement/Payable/Dtos/FinanceApDtos.cs` (`RecordSupplierPaymentRequest` — ditemukan `BankAccountId` bertipe `Guid` non-nullable `[Required]`, akar masalah `FIN-VAL-201` mustahil ditegakkan)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerMovementService.cs` (`RecordSupplierPayableMovementAsync` — dipastikan `proofId` sudah didukung sejak awal, nol perubahan dibutuhkan di sana)
- Laporan task `BE-FIN-077.md` (dipakai sebagai acuan bentuk kontrak — "bentuk sama persis")

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Payable/Services/FinanceSupplierPayableService.cs` | `RecordDirectPaymentAsync`: signature bertambah `Guid proofId`; tujuh validasi baru berurutan (`FIN-VAL-197`..`203`, identik `BE-FIN-077`) sebelum transaksi dibuka; `RecordSupplierPayableMovementAsync` kini dipanggil dengan `proofId` terisi (`fundingSourceId` **sudah** terisi sejak sebelumnya); `fundingSourceType` diturunkan dari `normalizedPaymentMethod`; respons membawa `PaymentMethod`/`FundingSourceId`/`ProofId`. **Nol** method lain disentuh |
| `Areas/Corporate/FinanceManagement/Payable/Dtos/FinanceApDtos.cs` | `RecordSupplierPaymentRequest`: `BankAccountId` berubah dari `[Required] Guid` menjadi `Guid?` (memperbaiki cacat yang membuat `FIN-VAL-201` mustahil ditegakkan); `ReferenceNumber` berubah dari `[Required] string` menjadi `string?` opsional (menyamakan bentuk dengan `BE-FIN-077`, service sudah lama mentolerir kosong); `ProofId` ditambahkan (tanpa `[Required]` — lihat bagian 7). `SupplierPayablePaymentResponse` bertambah `PaymentMethod`, `FundingSourceId`, `ProofId` |
| `Areas/Corporate/FinanceManagement/Payable/Controllers/FinanceApController.cs` | `RecordPayment` meneruskan `request.ProofId` (parameter baru); `[ProducesResponseType]` bertambah `404` dan `409` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **PERUBAHAN MEMUTUS** pada `POST api/finance/payable/payment` — `ProofId` kini wajib; `BankAccountId` kini benar-benar ditegakkan sesuai metode (dan menjadi *opsional pada bentuk request*, meski secara efektif tetap wajib untuk `TRANSFER` lewat validasi runtime — bukan lagi `[Required]` level DTO yang keliru mewajibkannya untuk `CASH` juga); `ReferenceNumber` menjadi opsional. Klien lama yang belum mengirim `ProofId` akan **selalu** menerima `422`. Respons bertambah tiga ruas (aditif, aman) |
| Database | **NOT APPLICABLE.** Nol model/migration baru — seluruh kolom (`FundingSourceId`, `ProofId` pada `FinSupplierPayableMovement`) sudah ada sejak `BE-FIN-058` |
| Keamanan/Auth | **NOT APPLICABLE.** Nol resource/action baru — `Finance.AP : Payment` yang sudah ada dipakai apa adanya |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / AP V2

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/payment` | Mencatat pembayaran langsung utang supplier — kini wajib membawa metode, sumber dana (untuk transfer), dan bukti; ambang ditegakkan. Bentuk identik `POST api/finance/receivable/payment` | `Finance.AP : Payment` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Permintaan eksplisit pengguna. Dependency `BE-FIN-075`/`076`/`077` juga belum ter-build-verifikasi — risiko kumulatif dicatat bagian 7 |
| Pembacaan kode: bentuk kontraknya sama dengan `BE-FIN-077` | Terverifikasi baris demi baris — ketujuh pesan validasi, urutan pemeriksaan, dan nama exception (`PayableBadRequestException`↔`ReceivableBadRequestException`, dst.) identik, hanya nama tipe yang berbeda sesuai domainnya | `PASS` | `FinanceSupplierPayableService.cs` baris 276-331 vs `FinanceReceivableService.cs` baris 733-790 |
| Pembacaan kode: pembayaran tunai **tidak** memotong anggaran kas kecil | Terverifikasi — blok `if (normalizedPaymentMethod == "CASH")` memanggil `RecordCashMovementAsync` dengan `FinCashMovementTypes.PembayaranTunaiLangsung`/`Direction.Out` (kontribusi Kas Kasir); nol referensi ke tabel anggaran kas kecil pada method ini, sebelum maupun sesudah perubahan | `PASS` (diwarisi, tidak diubah) | `FinanceSupplierPayableService.cs` baris 390-407 |
| Pembacaan kode: `ProofId` terpakai ditolak `409` | Terverifikasi — pre-check `AnyAsync` pada **kedua** tabel mutasi (utang lalu piutang) melempar `PayableConflictException`. Catatan race yang sama seperti `BE-FIN-077` berlaku di sini juga | `PASS` dengan catatan | `FinanceSupplierPayableService.cs` baris 323-331 |
| Pembacaan kode: cacat `BankAccountId` non-nullable diperbaiki | Terverifikasi — `RecordSupplierPaymentRequest.BankAccountId` kini `Guid?`, memungkinkan `FIN-VAL-201` benar-benar diperiksa untuk pertama kalinya | `PASS` | `FinanceApDtos.cs` |
| Pembacaan kode: nol pemanggil lain yang perlu disesuaikan | Terverifikasi — pencarian `RecordDirectPaymentAsync` pada seluruh repository hanya menemukan satu pemanggil (`FinanceApController.cs`) | `PASS` | Grep seluruh repository, dilampirkan pada riwayat sesi |

Uji manual: `NOT FEASIBLE` — memerlukan runtime aplikasi aktif, migration `BE-FIN-074` sudah
diterapkan, konfigurasi `FinanceManagement:TransactionProof:*` terisi, dan ambang aktif sudah
ditetapkan lewat `BE-FIN-076`; di luar cakupan perubahan source tanpa eksekusi aplikasi.

**Tidak dijalankan:** `dotnet build` (permintaan eksplisit pengguna), uji manual jalur tolak ambang
dan bukti terpakai (`NOT FEASIBLE`), `dotnet test` (backend tidak memelihara project automated test).

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Bentuk kontraknya sama dengan `BE-FIN-077` | Terpenuhi (source) | Bagian 2, 5 |
| Pembayaran tunai **tidak** memotong anggaran kas kecil (`FIN-DEC-133`) | Terpenuhi (diwarisi, tidak diubah) | Bagian 5 |
| `ProofId` terpakai ditolak `409` | Terpenuhi (pre-check; lihat catatan race bagian 5) | Bagian 5 |
| `dotnet build` PASS | **Belum terpenuhi** — `NOT RUN` atas permintaan eksplisit pengguna | Bagian 5 |
| Urutan rilis tercatat | Terpenuhi | Bagian 7 |
| Laporan task tracked ada | Terpenuhi | Berkas ini |

Butir DoD `dotnet build PASS` **belum terpenuhi** karena sengaja tidak dijalankan atas instruksi
eksplisit pengguna. Ini **bukan** pengecualian DoD permanen (`status-task-roadmap.md` §2.2).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Urutan rilis — PERUBAHAN MEMUTUS (wajib dibaca pemilik rilis)** | **Identik `BE-FIN-077`.** Backend ini **MUST NOT** dirilis ke produksi sendirian sebelum `FE-FIN-030` siap mengirim `ProofId`. Begitu aktif, setiap permintaan `POST api/finance/payable/payment` tanpa `ProofId` akan **selalu** ditolak `422`. Urutan aman: **(a)** rilis bersamaan dengan `FE-FIN-030`, atau **(b)** frontend lebih dulu kompatibel-mundur, baru backend menyusul. **MUST NOT** backend lebih dulu lalu frontend menyusul |
| Peringatan | **Risiko kumulatif tanpa build** berlanjut dari `BE-FIN-074`-`077`. Disarankan menjalankan `dotnet build` sebelum menganggap `REV-14D` selesai sepenuhnya |
| Masalah yang diketahui | **Celah race pre-check `ProofId`** — sama seperti `BE-FIN-077`, bukan temuan baru. **Cacat `BankAccountId` non-nullable** pada DTO lama sudah diperbaiki task ini (lihat bagian 1 dan 3.2) |
| Risiko tersisa | Klien lama (bila ada) yang memanggil `POST api/finance/payable/payment` tanpa `ProofId`, atau yang selalu mengirim `BankAccountId` walau metode `CASH` (dulu "wajib" secara DTO), akan berhenti berfungsi/berubah perilaku segera setelah backend ini aktif — lihat baris "Urutan rilis" di atas |
| Perubahan sampingan | `NONE` — ketiga berkas yang diubah murni perluasan/perbaikan untuk task ini; nol method lain pada `FinanceSupplierPayableService.cs`/`FinanceApController.cs` disentuh |
| Interupsi | `NONE` |
| Status Git | `git status --short` menunjukkan tiga berkas task ini (seluruhnya modifikasi, nol berkas baru), tercampur dengan modifikasi tidak ter-commit dari task `BE-FIN-058`..`077` yang memang belum di-commit sejak sebelum task ini dimulai. Tidak ada file di luar scope Finance Management yang tersentuh |
| Langkah berikutnya | (1) Pengguna menjalankan `dotnet build` (mencakup seluruh `BE-FIN-074`-`078`) dan mengonfirmasi hasilnya; (2) pemilik rilis mengoordinasikan `FE-FIN-030` sesuai urutan rilis di atas untuk **kedua** endpoint (`BE-FIN-077` dan `078`) sebelum mengaktifkan di produksi; (3) dengan ini, seluruh task `REV-14D` (`BE-FIN-074`-`078`) sudah tersentuh source-nya — langkah berikut adalah verifikasi build menyeluruh, bukan task baru |
