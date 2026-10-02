# FE-BKC-009 — Preview dan Finalisasi Invoice

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-009` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `0.4`) |
| Task type | Frontend, vertical slice, panel dan modal baru pada halaman invoice detail yang sudah ada |
| Task mode | `FRONTEND` (backend read-only, dipakai sebagai bukti kontrak dan perilaku *as-is*) |
| Write target | `QuilvianSystemFrontendDev` (source); laporan ini + evidence roadmap ditulis di `NewQuilvianSystemBackend` mengikuti presedens task sebelumnya |
| Branch frontend | `yasmina` |
| Lokasi UI | Panel "Preview dan Finalisasi Invoice" + modal "Finalisasi Invoice" pada `/health-services/billing-management/billing/invoices/[slug]` |
| Status task sebelum sesi ini | Source sudah ada dan sudah **ter-commit** (bagian dari commit `2dcea2f8f` "update billing-kasir part 3 fe"), lulus lint/build/`test:unit`, tetapi **belum pernah dilaporkan** — `roadmap/frontend-roadmap.md` dan `MODULE-STATUS.md` masih menyatakan `FE-BKC-009` "belum dibangun". Pola ini sama dengan rekonsiliasi `ISSUE-FE-003` sebelumnya (dokumentasi tertinggal dari source). |
| Status task setelah sesi ini | Source diverifikasi terhadap kontrak `0.4`, satu gap kecil ditemukan dan diperbaiki (lihat Temuan), lulus lint/`test:unit`/build ulang. Verifikasi manual ter-autentikasi tetap belum dilakukan. |

## Ringkasan untuk pembaca umum

Panel pada halaman detail invoice yang menampilkan checklist kesiapan sebelum invoice dikunci
(status `OPEN` → `FINAL`): apakah semua order sudah selesai, apakah kalkulasi terkini, versi
kalkulasi, dan sisa tagihan (*outstanding*). Bila outstanding masih ada, Billing wajib mencatat
**departure exception** (meninggal dunia, rujuk darurat, atau pulang paksa/DAMA) beserta identitas
dan hubungan penanggung sisa tagihan sebelum bisa melanjutkan. Setelah difinalisasi, invoice
menjadi read-only dan panel menampilkan status handoff AR (piutang pasien/penjamin) dan AP (jasa
dokter) beserta koreksi (adjustment) yang terjadi setelah finalisasi.

## Temuan dan perbaikan pada sesi ini

**Gap kecil ditemukan dan diperbaiki**: variabel `isFinal` pada
`billing-invoice-detail-view.jsx` sebelumnya hanya memeriksa `rawStatus === "FINAL"`, padahal
`contracts/state-transition-matrix.md` baris 12 menyatakan invoice berlanjut dari `FINAL` ke
`CLOSED` begitu handoff AR/AP sukses diposting (`BilInvoice.Closed = "CLOSED"` di
`Areas/HealthServices/BillingManagement/Billing/Models/BilInvoice.cs:28`). Karena `isFinal` adalah
satu-satunya sumber kebenaran yang dikirim ke `BillingFinalizationPanel` untuk menentukan apakah
panel menampilkan checklist/tombol "Ajukan Finalisasi" atau banner read-only, invoice berstatus
`CLOSED` akan salah ditampilkan seolah belum difinalisasi (checklist kosong, bukan banner final).

Diperbaiki dengan satu baris di `billing-invoice-detail-view.jsx`:

```js
const isFinal = rawStatus.toUpperCase() === FINAL_STATUS || rawStatus.toUpperCase() === CLOSED_STATUS;
```

**Catatan tingkat risiko**: gap ini saat ini **tidak dapat terjadi di runtime** karena dikonfirmasi
langsung dari source — tidak ada satu pun tempat di backend yang benar-benar meng-*assign*
`BilInvoice.Status = Closed` (hanya dibaca sebagai guard di
`BillingFinancialExceptionService.cs:675`). Ini konsisten dengan `BKC-BLK-INT-001` (kontrak
konsumen AR/AP belum dibuktikan, menahan aktivasi transisi `FINAL → CLOSED` yang sebenarnya).
Perbaikan ini bersifat forward-compatible agar panel tetap benar begitu `BE-BKC-016`
mengaktifkan transisi tersebut.

## Endpoint yang dikonsumsi

### Health Services / Billing Management / Billing / Finalizations

Base URL: `api/v1/health-services/billing-management/billing/finalizations`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/invoices/{invoiceId}/preview` | Checklist kesiapan finalisasi | `BillingFinalization : Read` | — | `ApiResponse<FinalizationPreviewResponse>` |
| `POST` | `/invoices/{invoiceId}` | Finalisasi invoice (memicu handoff) | `BillingFinalization : Create` | `FinalizeInvoiceRequest` (`expectedRowVersion`, `departureReason`, `debtorIdentity`, `debtorRelationship`, `reason`, `correlationId`, `causationId`) + header `Idempotency-Key` | `ApiResponse<FinalizationResponse>` |
| `GET` | `/{id}/handoffs` | Status handoff AR/AP + koreksi pasca-finalisasi | `BillingFinalization : Read` | — | `ApiResponse<HandoffStatusResponse>` |

Ketiga path ini cocok persis dengan `contracts/api-contract.md:67-69` dan diimplementasikan di
`Areas/HealthServices/BillingManagement/Billing/Controllers/BillingFinalizationsController.cs`
serta `Services/BillingFinalizationService.cs` (backend snapshot `8e48237`).

**Keterbatasan yang sama dengan `FE-BKC-007`/`FE-BKC-008` (bukan bug baru)**: tidak ada endpoint
"get finalization record by invoice id". Satu-satunya cara memulihkan `FinalizationRecordId`
(dibutuhkan untuk `GET .../handoffs`) setelah refresh browser adalah menyimpannya di
`localStorage` per invoice begitu `POST` finalisasi berhasil (`use-billing-finalization.js:39-61`,
mengikuti pola draft `FE-BKC-006`). Jika invoice difinalisasi dari sesi/perangkat lain, status
handoff tidak bisa dilihat tanpa `FinalizationRecordId` disampaikan secara manual —
`handoffsUnavailableReason` di panel sudah menjelaskan ini ke pengguna.

Kode status yang mungkin muncul:

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Finalisasi berhasil, termasuk replay dari `Idempotency-Key` yang sama (`result.isReplay`). |
| `403` | Invoice tidak dalam wewenang pengguna atau bukan `BillingFinalization : Create`. |
| `409` | `RowVersion` usang — perlu reload invoice. |
| `422` | Checklist belum terpenuhi (order belum selesai/kalkulasi stale/outstanding tanpa departure exception) — backend mengembalikan checklist terbaru di body, ditangkap di `billing-finalization-slice.jsx:155-160` supaya panel menampilkan alasan blokir tanpa panggilan preview terpisah. |

## Acceptance criteria (dari `roadmap/frontend-roadmap.md`, `FE-BKC-009`)

| Acceptance criteria | Status | Bukti |
| --- | --- | --- |
| Missing order/debtor blocks | **Terpenuhi (via backend 422)** | Tombol "Ajukan Finalisasi" tidak memblokir klik berdasar `blockingReasons` (backend adalah penegak akhir dan bisa berubah antara preview dan submit), tetapi submit yang diblokir langsung menampilkan checklist terbaru dari body `422` (`billing-finalization-slice.jsx:151-160`). `blockingReasons` juga ditampilkan sebagai `InformationAlert` warning di panel sebelum submit. |
| Death/DAMA reason visible | **Terpenuhi** | `DEPARTURE_REASON_OPTIONS` (`DEATH`, `EMERGENCY_TRANSFER`, `DAMA`) ditampilkan sebagai pilihan wajib di `finalize-invoice-modal.jsx` ketika outstanding > 0 atau dicentang manual; hasil `departureReason` ditampilkan kembali di banner final panel. |
| AP not-ready distinct | **Terpenuhi** | Kolom "Kesiapan" pada tabel AP memakai `AP_READINESS_STATUS_BADGE_CONFIG` (`not_ready`/`ready`) terpisah dari kolom "Status Handoff" (`created`/`acknowledged`) — dua badge berbeda, bukan digabung. |
| Retry tidak membuat second finalization | **Terpenuhi** | `Idempotency-Key`/`CorrelationId` dibuat sekali per invoice dan disimpan di draft `localStorage`; `openFinalize` memakai kembali key yang sama bila submit sebelumnya gagal terkirim (`use-billing-finalization.js:136-140`), sehingga retry memicu jalur replay backend, bukan finalisasi kedua. |
| Read-only final state | **Terpenuhi setelah perbaikan sesi ini** | `isFinal` kini mencakup `FINAL` dan `CLOSED` (lihat Temuan). Banner final menampilkan tanggal finalisasi, versi kalkulasi, outstanding saat finalisasi, dan departure reason bila ada. |
| Preview checklist, calculation version, debtor breakdown, confirm | **Terpenuhi** | Lima field checklist (`allOrdersComplete`, `calculationCurrent`, `calculationVersion`, `outstanding`, `isReadyForNormalFinalization`) ditampilkan di `<dl>`; debtor breakdown tampil sebagai tabel AR terpisah dari AP setelah finalisasi. |

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npx eslint` file yang diubah (`billing-invoice-detail-view.jsx`) | **PASS** | Tanpa output. |
| `npm run lint:errors` | **PASS** | Exit code 0, seluruh repo. |
| `npm run test:unit` | **PASS** | 44 test, 44 pass, 0 fail. **Tidak ada test unit khusus** untuk `billing-finalization-slice.jsx`/`use-billing-finalization.js`/panel — gap yang sama seperti `FE-BKC-008` (tidak diperbaiki pada sesi ini, di luar scope perbaikan satu-baris yang diotorisasi; lihat Langkah berikutnya). |
| `npm run build` | **PASS** | Build Next.js selesai (`exit code 0`), `postbuild`/`prepare-standalone` sukses. |
| Smoke-test browser headless tanpa login | **NOT DONE pada sesi ini** | Tidak dijalankan ulang karena tidak ada perubahan pada route publik; halaman detail invoice (tempat panel dirender) tetap butuh auth, konsisten dengan keterbatasan smoke-test pada task-task sebelumnya. |
| Verifikasi manual ter-autentikasi (ajukan finalisasi normal, dengan departure exception, retry, lihat handoff AR/AP) | **NOT FEASIBLE pada sesi ini** | Tidak ada kredensial/environment ter-autentikasi maupun invoice `OPEN` nyata dengan order lengkap yang tersedia untuk sesi ini. |

## Git status

```
 M src/components/view/health-services/billing-management/billing-invoices/detail/billing-invoice-detail-view.jsx
```

Satu baris (`isFinal`) diubah; belum di-*stage*/commit sesuai batasan wewenang task ini (git add/commit/push memerlukan instruksi eksplisit terpisah).

## Langkah berikutnya yang direkomendasikan

1. Commit perubahan `isFinal` ini bersama tujuh task frontend lain yang sudah menumpuk (lihat
   `MODULE-STATUS.md`), atau secara terpisah bila pemilik modul ingin memisahkan perbaikan kecil
   ini dari fitur besar.
2. Tambahkan unit test untuk `billing-finalization-slice.jsx` dan `use-billing-finalization.js`
   (retry idempotency, penanganan checklist pada `422`, transisi `isFinal`) — belum ada sama
   sekali saat ini.
3. Verifikasi manual ter-autentikasi begitu tersedia environment dengan invoice `OPEN` nyata:
   alur normal, alur departure exception, retry setelah koneksi putus, dan tampilan handoff AR/AP
   setelah `BE-BKC-016`/`BKC-BLK-INT-001` benar-benar mengaktifkan transisi ke `CLOSED`.
4. Lanjut ke `FE-BKC-010` (accessibility, privacy, dan regression lintas workspace) — satu-satunya
   task frontend roadmap revisi 1 yang belum punya source sama sekali.
