# Laporan Perubahan Backend — `BE-BUI-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BUI-002` |
| Judul | Field `TransactionDate` pada Refundable Items |
| Slice | Gelombang `MVP-30` — Revisi UI Billing: Perbaikan Logika Backend (`docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `Gelombang MVP-30`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BUI-002` |
| Trace | `BUI-DEC-012` (keputusan bisnis, approved 24 September 2026); `BUI-DES-002` (keputusan arsitektur, **approved pemilik modul 25 September 2026** — gerbang wajib sebelum task ini boleh dikerjakan, terpisah dari approval bisnis) |
| Contract version | `BIL-API-1.5` (draft) — perubahan **aditif, non-breaking** pada `BillingRefundableItemResponse` |
| Dependency | Tidak ada — task independen dari `BE-BUI-001`, menyentuh berkas berbeda (`BillingRefundDtos.cs`, `BillingRefundService.cs` vs `BillingPayerEditService.cs`) |
| Klasifikasi | `LIGHT` — satu repository (skor 0); berkas diperiksa ≤8 (skor 0: satu DTO, satu service, satu roadmap card); berkas diubah 2 (skor 0); logika bisnis sederhana — satu field aditif, satu sumber data yang sudah dimuat (skor 0); kontrak API bertambah field aditif non-breaking (skor 1 — memakai kontrak yang sudah ada, bukan mengubahnya); database tidak ada dampak, nol migration (skor 0); keamanan/auth `NOT APPLICABLE` (skor 0); UI/workflow `NOT APPLICABLE` untuk backend murni (skor 0). Total skor 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Dtos/BillingRefundDtos.cs`, `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/BillingRefundService.cs` |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `d6cdfaf9fda8889d6fe73db1e97cc28c4f022852` (branch `Yasmina`) |
| Tanggal | 2026-09-25 |
| Status | 🟡 **SEBAGIAN — build lulus, verifikasi manual belum.** Source selesai persis sesuai scope roadmap. `dotnet build` dijalankan pengguna sendiri dan **lulus**, dikonfirmasi 25 September 2026. Verifikasi manual `GET /{id}/refundable-items` terhadap data nyata belum dijalankan — lihat §5 |

---

## 1. Masalah yang diperbaiki

Datatable pemilihan item refund pada frontend (rencana `BUI-DES-011`) perlu menampilkan kolom "Tanggal" transaksi setiap item tagihan yang dapat direfund. Data ini sebetulnya sudah tersedia di database (`BilInvoiceItem.CreateDateTime`, warisan `IdentityModel`, terisi otomatis sejak baris item dibuat) dan sudah dimuat backend lewat `Include(x => x.Items)` pada `GetBillingRefundableItemsAsync` — tetapi belum pernah diproyeksikan ke response `BillingRefundableItemResponse`. Tanpa perubahan ini, frontend harus melakukan query tambahan atau tidak bisa menampilkan kolom tersebut sama sekali.

---

## 2. Proses bisnis

1. Kasir/Billing membuka modal refund pada sebuah invoice, memilih sumber refund "Billing" (dua sumber refund: Billing atau Deposito, sesuai `BUI-DEC-011`/`012`).
2. Frontend memanggil `GET /{id}/refundable-items` untuk menampilkan datatable item tagihan yang dapat direfund.
3. **Perubahan pada task ini**: setiap baris response kini menyertakan `TransactionDate`, diisi dari `item.CreateDateTime` — tanggal saat baris item tagihan itu sendiri dibuat, **bukan** tanggal invoice induknya dibuat (keduanya bisa berbeda pada kasus entri manual belakangan, misalnya item susulan yang ditambahkan setelah invoice awal dibuka).
4. Field lama (`BillingItemId`, `ItemName`, `Qty`, `Amount`, `RefundableAmount`) dan logika penentuan `RefundableAmount` (item yang sudah pernah diajukan refund pada `BilRefundCase` lain menjadi `0`) **tidak disentuh sama sekali** — perubahan murni aditif.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingRefundDtos.cs` (definisi `BillingRefundableItemResponse`)
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingRefundService.cs` (method `GetBillingRefundableItemsAsync`, baris 34-85)
- `Models/IdentityModel.cs` (memastikan tipe `CreateDateTime` adalah `DateTime`, sama dengan tipe field baru)
- `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `Gelombang MVP-30`, kartu `BE-BUI-002`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingRefundDtos.cs` | Menambah properti `public DateTime TransactionDate { get; set; }` pada `BillingRefundableItemResponse` — satu baris, ditambahkan setelah `RefundableAmount` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingRefundService.cs` | Pada proyeksi `activeItems.Select(item => ...)` di `GetBillingRefundableItemsAsync`, menambah `TransactionDate = item.CreateDateTime` pada konstruksi `BillingRefundableItemResponse` — satu baris, sumbernya eksplisit `item` (baris tagihan), bukan `invoice` |

Hanya satu construction site `new BillingRefundableItemResponse` ditemukan di seluruh repository (`grep -rn "new BillingRefundableItemResponse"`), sehingga tidak ada tempat lain yang perlu disentuh agar field baru selalu terisi.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `BillingRefundableItemResponse` bertambah satu field aditif (`TransactionDate`). Bentuk field lama, tipe, dan urutan tidak berubah — konsumen lama yang mengabaikan field baru tidak terpengaruh (tidak ada breaking change) |
| Database | `NOT APPLICABLE` — nol migration. `CreateDateTime` adalah kolom yang sudah ada di database sejak `BilInvoiceItem` dibuat lewat `IdentityModel` |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada perubahan otorisasi/endpoint |

---

## 4. Dokumentasi endpoint

Endpoint `GET /{id}/refundable-items` (bagian dari grup Refund pada `Billing`) **tidak berubah rute maupun verb**; response-nya bertambah satu field aditif `TransactionDate` (tipe `DateTime`) pada setiap baris `BillingRefundableItemResponse`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Lulus | `PASS` | Dijalankan pengguna sendiri, dikonfirmasi 25 September 2026 |
| Review diff — perubahan hanya 2 baris pada dua berkas, sesuai scope roadmap persis | Sesuai | `PASS` | §3.2 |
| Pencarian construction site `BillingRefundableItemResponse` lain yang mungkin luput | Hanya satu titik konstruksi di seluruh repository | `PASS` | `grep -rn "new BillingRefundableItemResponse"` |
| Sumber tanggal terverifikasi `item.CreateDateTime`, bukan `invoice.CreateDateTime` | Sesuai — variabel `item` adalah elemen loop `activeItems` (baris `BilInvoiceItem`), tidak ada referensi ke `invoice.CreateDateTime` pada baris yang diubah | `PASS` | §3.2, pembacaan manual |
| Tipe `TransactionDate` (`DateTime`) cocok dengan tipe `CreateDateTime` sumbernya | Sesuai — keduanya `DateTime`, tidak perlu konversi | `PASS` | `Models/IdentityModel.cs` baris 12/19 |
| Field lama (`BillingItemId`, `ItemName`, `Qty`, `Amount`, `RefundableAmount`) tidak berubah bentuk/nilai | Sesuai — tidak satu pun baris yang menyusun field-field itu tersentuh | `PASS` | `git diff` atas kedua berkas |

Uji manual: `REQUIRED`, belum dijalankan — `NOT RUN`. Belum ada pemanggilan nyata `GET /{id}/refundable-items` terhadap invoice sungguhan untuk mengonfirmasi `TransactionDate` yang dikembalikan benar-benar sama dengan `CreateDateTime` baris `BilInvoiceItem` yang bersangkutan.

**Tidak dijalankan:** `dotnet build`; uji unit proyeksi (disebut roadmap sebagai bukti verifikasi) — **tidak ada project test otomatis pada repository ini** (`rules/backend/TEST_POLICY.md`), sehingga bukti verifikasi diganti review diff + pencarian construction site sesuai kebijakan; regresi manual terhadap data nyata.

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`); tidak diminta secara eksplisit pada task ini.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `GET /{id}/refundable-items` mengembalikan `TransactionDate` terisi (bukan default/null) untuk setiap baris | Terpenuhi lewat pembacaan kode, **belum diuji nyata** | §3.2, §5 |
| Nilai `TransactionDate` sama dengan `CreateDateTime` baris `BilInvoiceItem` yang bersangkutan | Terpenuhi — sumbernya eksplisit `item.CreateDateTime` | §5 |
| Field lama tidak berubah bentuk maupun nilai | Terpenuhi | §3.3, §5 |
| Nol migration dijalankan | Terpenuhi | §3.3 |
| QBE preflight PASS | Terpenuhi — `TOUCHED LEGACY`, `Area HealthServices/BillingManagement/Billing`, prefix `Bil` sudah `ACTIVE` pada registry, nol entity baru | Preflight ini |
| Build backend terverifikasi hijau | Terpenuhi | `dotnet build` dijalankan pengguna sendiri dan lulus, dikonfirmasi 25 September 2026 |
| Verifikasi manual/regresi nyata terhadap `GET /{id}/refundable-items` | **Belum terpenuhi** | Menunggu pengguna |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `dotnet build` sudah lulus (dikonfirmasi pengguna 25 September 2026), tapi nilai `TransactionDate` yang benar-benar dikembalikan API belum diamati langsung lewat pemanggilan nyata |
| Masalah yang diketahui | Tidak ada |
| Risiko tersisa | Bila di masa depan ada jalur lain yang membangun `BillingRefundableItemResponse` di luar `GetBillingRefundableItemsAsync` (belum ada saat ini, dikonfirmasi lewat pencarian repository), jalur itu perlu diberi `TransactionDate` juga secara manual — tidak ada nilai default otomatis selain `default(DateTime)` (`0001-01-01`) bila diabaikan |
| Perubahan sampingan | `NONE` |
| Interupsi | Tidak ada — task ini dikerjakan langsung sesudah `BE-BUI-001` pada sesi yang sama, sesudah gerbang arsitektur `BUI-DES-001`/`002` dicatat disetujui |
| Status Git | `M Areas/HealthServices/BillingManagement/Billing/Dtos/BillingRefundDtos.cs`, `M Areas/HealthServices/BillingManagement/Billing/Services/BillingRefundService.cs` |
| Langkah berikutnya | Verifikasi manual `GET /{id}/refundable-items` terhadap invoice nyata yang punya item dengan `CreateDateTime` berbeda dari invoice induknya (kasus entri manual belakangan) untuk memastikan `TransactionDate` bukan sekadar terisi, tapi terisi dari baris yang benar — satu-satunya butir DoD yang masih terbuka. Gelombang `MVP-30` backend (`BE-BUI-001`, `BE-BUI-002`) sudah build-verified secara bersamaan |
