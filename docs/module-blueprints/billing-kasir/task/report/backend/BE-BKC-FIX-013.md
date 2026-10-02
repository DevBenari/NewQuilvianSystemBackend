# Laporan Perubahan Backend — `BE-BKC-FIX-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-013` |
| Judul | Perbaikan gerbang CI "Verify authorization registry" — resource `BillingRefund`/`BillingAdjustment`/`BillingWriteOff` terdaftar pada dua module sekaligus |
| Slice | Perbaikan bug ad-hoc, di luar penomoran roadmap asli — mengikuti pola `BE-BKC-FIX-XXX` yang sudah dipakai modul ini (`BE-BKC-FIX-001`–`012`) |
| Roadmap | Tidak ada baris roadmap resmi — bug ditemukan lewat kegagalan CI (`tools/authorization-verifier/verify-authorization.sh`, job "Verify authorization registry"), dilaporkan pengguna lewat screenshot, bukan dari task roadmap manapun. Ditandai ad-hoc dengan Task ID `BE-BKC-FIX-013` |
| Trace | `tools/authorization-verifier/README.md` invarian #3 ("Identitas kanonik tidak ganda dan tidak bertentangan") — identitas kanonik `SysControllerAccess` adalah pasangan `(ModuleId, ResourceName)`, dan pelanggarannya "meyatimkan policy" |
| Contract version | `NOT APPLICABLE` — nol perubahan bentuk request/response DTO |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); berkas diperiksa 9–20 (skor 1: dua controller, dua service, satu slice frontend read-only sebagai bukti, tools verifier, README-nya); berkas diubah 2 (skor 0); logika sederhana — memindahkan method utuh + menghapus dua endpoint mati, nol logika bisnis baru (skor 1, karena menyentuh permukaan endpoint meski bukan logika); kontrak API murni pemindahan lokasi fisik, URL/bentuk tidak berubah (skor 1, memakai kontrak yang sudah ada); database `NOT APPLICABLE` (skor 0); keamanan/auth **inti** — memperbaiki pelanggaran invarian registry hak akses (skor 2); UI/workflow `NOT APPLICABLE`, backend murni (skor 0). Total skor 5 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs`, `.../BillingFinancialExceptionsController.cs` |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `d6cdfaf9fda8889d6fe73db1e97cc28c4f022852` (branch `Yasmina`) |
| Tanggal | 2026-09-25 |
| Status | ✅ **SELESAI 25 September 2026.** `dotnet build -c Release` lulus (0 error, 229 warning pre-existing tidak terkait); `bash tools/authorization-verifier/verify-authorization.sh --configuration Release` **PASS**, exit code 0. Dijalankan atas permintaan eksplisit pengguna khusus untuk task perbaikan CI ini — pengecualian dari instruksi umum "jangan build backend otomatis" yang berlaku untuk task lain sepanjang sesi |

---

## 1. Masalah yang diperbaiki

CI job "Verify authorization registry" gagal (exit code 1) dengan pesan:

```
AUTHORIZATION VERIFIER: FAIL
[3] Identitas kanonik ganda:
      BillingAdjustment|Create
      BillingRefund|Create
      BillingRefund|Read
      BillingWriteOff|Create
[3] Resource terdaftar pada lebih dari satu modul:
      BillingAdjustment  ->  HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING, HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_FINANCIAL_EXCEPTION
      BillingRefund  ->  HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING, HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_FINANCIAL_EXCEPTION
      BillingWriteOff  ->  HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING, HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_FINANCIAL_EXCEPTION
```

**Akar masalah.** Identitas kanonik hak akses pada tabel `SysControllerAccess` adalah pasangan `(ModuleId, ResourceName)` — satu Resource **wajib** dimiliki tepat satu Module. `BillingFinancialExceptionsController` (module `HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_FINANCIAL_EXCEPTION`) adalah pemilik kanonik resource `BillingRefund`, `BillingAdjustment`, dan `BillingWriteOff` — punya aksi Read/Create/Approve untuk ketiganya. Namun `BillingInvoicesController` (module `HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING`) **juga** punya lima aksi yang memakai ulang resource yang sama persis:

| Aksi di `BillingInvoicesController` | Resource\|Action | Dipanggil frontend? |
| --- | --- | --- |
| `GetRefundableItems` (`GET {id}/refundable-items`) | `BillingRefund\|Read` | **Ya** — `billing-financial-exception-slice.jsx` |
| `GetRemainingDeposit` (`GET {id}/remaining-deposit`) | `BillingRefund\|Read` | **Ya** — sda |
| `CreateRefund` (`POST {id}/refunds`) | `BillingRefund\|Create` | **Ya** — sda (`createBillingRefund`) |
| `CreateAdjustment` (`POST {id}/adjustments`) | `BillingAdjustment\|Create` | **Tidak** — frontend memakai `POST /financial-exceptions/adjustments` milik `BillingFinancialExceptionsController` |
| `CreateWriteOff` (`POST {id}/write-offs`) | `BillingWriteOff\|Create` | **Tidak** — frontend memakai `POST /financial-exceptions/write-offs` milik `BillingFinancialExceptionsController` |

Dua yang terakhir (`CreateAdjustment`, `CreateWriteOff`) adalah **kode mati** — dibuktikan lewat pencarian menyeluruh `QuilvianSystemFrontendDev/src` yang menunjukkan nol pemanggil untuk rute `{invoiceId}/adjustments` dan `{invoiceId}/write-offs` milik `BillingInvoicesController` (frontend selalu memanggil endpoint `BillingFinancialExceptionsController` untuk kedua aksi itu). Tiga yang pertama **aktif dipakai** frontend lewat rute `invoices/{id}/...`.

Dampak nyata bila dibiarkan: pipeline CI (`.github/workflows/integration-to-dev.yml`, tahap sesudah build backend dan sebelum keputusan migration/promosi ke DEV) selalu gagal pada job ini, memblokir promosi kandidat apa pun ke DEV — bukan hanya perubahan Billing.

---

## 2. Proses bisnis

Tidak ada perilaku bisnis yang berubah bagi pengguna akhir (kasir/admin). Task ini murni memperbaiki struktur internal registrasi hak akses:

- Kasir yang mengklik "Ajukan Refund" pada layar Riwayat Pembayaran/Menu Pembayaran tetap memanggil `POST /invoices/{id}/refunds` — URL, method, request/response body, dan syarat hak akses (`BillingRefund : Create`) **identik** dengan sebelum perbaikan.
- Layar yang menampilkan item refundable dan sisa deposito tetap memanggil `GET /invoices/{id}/refundable-items` dan `GET /invoices/{id}/remaining-deposit` — identik.
- Dua endpoint mati (`{id}/adjustments`, `{id}/write-offs` pada `BillingInvoicesController`) yang **tidak pernah dipanggil siapa pun** dihapus. Kasir tetap mengajukan Adjustment/Write-Off lewat jalur yang selama ini memang benar-benar dipakai (`POST /financial-exceptions/adjustments`, `.../write-offs`) — tidak ada perubahan di sana sama sekali.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `tools/authorization-verifier/README.md` (invarian yang dilanggar, mekanisme penemuan registry)
- `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs` (seluruh berkas)
- `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingFinancialExceptionsController.cs` (seluruh berkas)
- `Attributes/AccessPermissionAttribute.cs`, `Attributes/AccessActionAttribute.cs` (memastikan tidak ada mekanisme override module per-method yang terlewat)
- `QuilvianSystemFrontendDev/src/lib/state/slice/health-services/billing-management/billing-financial-exception-slice.jsx` (read-only, memastikan rute mana yang benar-benar dipanggil frontend sebelum memutuskan pindah vs hapus)
- `QuilvianSystemFrontendDev/src` (pencarian menyeluruh `grep -rn` untuk memastikan `{invoiceId}/adjustments` dan `{invoiceId}/write-offs` milik `BillingInvoicesController` benar-benar tidak dipanggil dari mana pun)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs` | Field `_refundService` (`BillingRefundService`) dan `_financialExceptionService` (`BillingFinancialExceptionService`) beserta parameter constructor-nya dihapus (sudah tidak dipakai lagi). Lima method dihapus: `GetRefundableItems`, `GetRemainingDeposit`, `CreateRefund` (dipindahkan utuh ke `BillingFinancialExceptionsController`, lihat baris berikutnya), `CreateAdjustment`, `CreateWriteOff` (dihapus permanen — kode mati, dikonfirmasi nol pemanggil frontend) |
| `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingFinancialExceptionsController.cs` | Tiga method baru ditambahkan, isi **identik** dengan sebelum dipindah (byte-for-byte body sama, hanya nama field `_refundService` → `_service`, yang sudah ada di controller ini dengan tipe sama): `GetRefundableItems`, `GetRemainingDeposit` (nama tetap sama, aman — tidak ada tabrakan nama di controller tujuan), dan `CreateRefund` → diberi nama baru `CreateRefundFromInvoice` (menghindari tabrakan nama dengan `CreateRefund` yang sudah ada di controller ini, `POST refunds` berbasis body). Ketiganya memakai `[HttpGet]`/`[HttpPost]` dengan **rute absolut** (diawali `/`) persis `/api/v1/health-services/billing-management/billing/invoices/{id:guid}/...` — mempertahankan URL yang sama persis seperti sebelum pemindahan, sehingga frontend tidak perlu diubah. `[AccessAction]` diberi `ActionName` baru yang belum dipakai (`ReadRefundableItems`, `ReadRemainingDeposit`, `CreateRefundFromInvoice`) dan `SortOrder` baru (13, 14, 15 — melanjutkan urutan tertinggi yang sudah ada di controller ini, 1–12) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol perubahan yang terlihat konsumen.** URL, HTTP method, request/response body untuk tiga endpoint yang dipindah identik sebelum dan sesudah. Dua endpoint yang dihapus (`{id}/adjustments`, `{id}/write-offs` pada `BillingInvoicesController`) terbukti tidak pernah dipanggil — menghapusnya bukan breaking change |
| Database | `NOT APPLICABLE` — nol migration, nol perubahan schema |
| Keamanan/Auth | **Inti perbaikan.** Resource `BillingRefund`, `BillingAdjustment`, `BillingWriteOff` kini masing-masing terdaftar tepat pada satu Module (`HEALTH_SERVICE_BILLING_MANAGEMENT_BILLING_FINANCIAL_EXCEPTION`), menutup pelanggaran invarian registry `(ModuleId, ResourceName)`. Syarat hak akses (`BillingRefund : Create`/`Read`) yang berlaku untuk ketiga endpoint aktif **tidak berubah** — admin yang sudah memberi izin `BillingRefund` sebelumnya tetap menggerbangi ketiga endpoint ini tanpa perlu tindakan apa pun |

---

## 4. Dokumentasi endpoint

#### Health Services / Billing Management / Billing / Invoices

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| ~~`GET`~~ | ~~`{id}/refundable-items`~~ | **Dipindahkan** ke grup Financial Exceptions, URL tidak berubah | ~~`BillingRefund : Read`~~ |
| ~~`GET`~~ | ~~`{id}/remaining-deposit`~~ | **Dipindahkan** ke grup Financial Exceptions, URL tidak berubah | ~~`BillingRefund : Read`~~ |
| ~~`POST`~~ | ~~`{id}/refunds`~~ | **Dipindahkan** ke grup Financial Exceptions, URL tidak berubah | ~~`BillingRefund : Create`~~ |
| ~~`POST`~~ | ~~`{id}/adjustments`~~ | **Dihapus** — kode mati, nol pemanggil | ~~`BillingAdjustment : Create`~~ |
| ~~`POST`~~ | ~~`{id}/write-offs`~~ | **Dihapus** — kode mati, nol pemanggil | ~~`BillingWriteOff : Create`~~ |

#### Health Services / Billing Management / Billing / Financial Exceptions

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/invoices/{id}/refundable-items` | Daftar item tagihan yang dapat direfund untuk satu invoice (URL sama seperti sebelumnya, kini dimiliki controller ini) | `BillingRefund : Read` |
| `GET` | `/invoices/{id}/remaining-deposit` | Sisa deposito pasien untuk satu invoice (URL sama seperti sebelumnya) | `BillingRefund : Read` |
| `POST` | `/invoices/{id}/refunds` | Ajukan refund untuk satu invoice, dipicu dari Riwayat Pembayaran/Menu Pembayaran (URL sama seperti sebelumnya; nama action internal berubah dari `CreateRefund` menjadi `CreateRefundFromInvoice` untuk menghindari tabrakan nama dengan `POST refunds` berbasis body yang sudah ada di controller ini — tidak terlihat konsumen) | `BillingRefund : Create` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ./QuilvianSystemBackend.csproj -c Release` | `0 Error(s)`, 229 warning (seluruhnya `CS1573`/`CS1574` XML-doc pre-existing, tidak satu pun menyinggung berkas yang diubah task ini) | `PASS` | Keluaran perintah lengkap |
| `bash tools/authorization-verifier/verify-authorization.sh --configuration Release` | `AUTHORIZATION VERIFIER: PASS` — `[1] metadata gap: 0`; `[2] fallback himpunan disetujui: 69 cocok persis`; `[3] identitas kanonik: tidak ganda, satu resource satu modul`; `[4] identitas wajib BE-SEC-003: 24/24`; `[5] endpoint bisnis telanjang: 0 baru, 0 utang baseline` | `PASS` | Keluaran perintah lengkap, exit code 0. Registry diagnostics: Resources turun dari 411→408 (dua endpoint mati terhapus), Actions turun dari 1544→1540 |
| Pencarian menyeluruh pemanggil frontend `{invoiceId}/adjustments` dan `{invoiceId}/write-offs` milik `BillingInvoicesController` sebelum dihapus | Nol pemanggil ditemukan di seluruh `QuilvianSystemFrontendDev/src` | `PASS` | `grep -rn` menyeluruh, dibandingkan dengan pemanggil aktual `billing-financial-exception-slice.jsx` yang memakai endpoint `BillingFinancialExceptionsController` untuk kedua aksi itu |
| Review diff — perubahan hanya pada dua controller, tanpa menyentuh service/DTO/model | Sesuai | `PASS` | `git status --short` di §7 |

Uji manual: `NOT FEASIBLE` — sesi ini tidak memiliki tool browser/E2E maupun akses database untuk memanggil endpoint secara nyata. Kebenaran perpindahan diverifikasi lewat kombinasi: (a) `dotnet build` sukses (membuktikan kompilasi dan resolusi tipe/method benar), (b) verifier otorisasi PASS (membuktikan registry hak akses benar), (c) perbandingan tekstual body method sebelum/sesudah pindah (byte-for-byte identik kecuali nama field `_refundService`→`_service` dan nama method `CreateRefund`→`CreateRefundFromInvoice`), (d) rute absolut yang secara eksplisit menyalin URL lama persis.

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`); tidak diminta secara eksplisit pada task ini.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `bash tools/authorization-verifier/verify-authorization.sh` keluar dengan exit code 0 | Terpenuhi | §5 |
| Resource `BillingRefund`/`BillingAdjustment`/`BillingWriteOff` masing-masing terdaftar tepat satu Module | Terpenuhi | §5, diagnostic verifier "[3] identitas kanonik: tidak ganda, satu resource satu modul" |
| Nol perubahan URL/kontrak untuk tiga endpoint aktif yang dipindah | Terpenuhi | §3.2, §4 |
| Dua endpoint mati dihapus tanpa memutus konsumen | Terpenuhi | §1, §3.2 — dikonfirmasi nol pemanggil sebelum dihapus |
| `dotnet build` lulus | Terpenuhi | §5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | Tidak ada task roadmap resmi yang menaungi bug ini — ditemukan lewat kegagalan CI, bukan dari slice roadmap manapun |
| Risiko tersisa | Bila ada konsumen API pihak ketiga/eksternal (di luar `QuilvianSystemFrontendDev`) yang memanggil `{invoiceId}/adjustments` atau `{invoiceId}/write-offs` milik `BillingInvoicesController` secara langsung (bukan lewat frontend repo ini), endpoint itu kini mengembalikan 404 — pencarian saya terbatas pada `QuilvianSystemFrontendDev`, tidak mencakup konsumen eksternal yang mungkin tidak terlihat dari repository manapun yang saya akses |
| Perubahan sampingan | `NONE` — field `_refundService`/`_financialExceptionService` yang dihapus dari `BillingInvoicesController` murni konsekuensi langsung pemindahan/penghapusan lima method di atasnya, bukan cleanup tidak terkait |
| Interupsi | `NONE` — task ini murni bug fix yang diminta langsung di tengah sesi, dikerjakan sampai tuntas dalam satu rangkaian |
| Status Git | `M Areas/HealthServices/BillingManagement/Billing/Controllers/BillingFinancialExceptionsController.cs`, `M Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs` |
| Langkah berikutnya | Task ini sudah tuntas — CI "Verify authorization registry" sudah PASS lokal, seharusnya lulus juga saat dijalankan ulang di pipeline. Tidak ada langkah susulan yang diperlukan |
