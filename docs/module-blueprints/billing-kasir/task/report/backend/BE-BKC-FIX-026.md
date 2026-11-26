# BE-BKC-FIX-026 — Penolakan 422 pembayaran tunai: penyebab dan kontrak error spesifik (backend)

Tanggal: 25 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `c073f8c1` (working tree belum di-commit). Frontend: `yasmina`, HEAD `5a34cb9c8` (lihat FE-BKC-FIX-016).

`dotnet build`: **NOT RUN** (dilarang). `MIGRATION`: tidak ada. `AUTOMATED TEST`: NOT APPLICABLE untuk backend; kontrak diperiksa lewat tes statis FE (`billing-cash-payment-sync.test.mjs`, 9 lulus). Preflight: Area HealthServices/BillingManagement/Billing, `Bil`, `TOUCHED LEGACY`, tanpa entity baru.

## 1. Penyebab (dari source; data runtime TIDAK tersedia)
- Pesan "Total metode pembayaran melebihi saldo yang harus dibayar." hanya ada pada satu cabang: `AddTenderAsync`, `if (request.Amount > maxCollectible)`, sebagai `BillingSettlementValidationException` (HTTP 422).
- Urutan guard sebelum cabang itu menyingkirkan beberapa dugaan: `ExpectedRowVersion` basi = 409; settlement `Settled` = 422 dengan pesan lain; tender tunai CREATED dengan metode sama = 409. Jadi pada kasus aktual: RowVersion cocok, settlement belum lunas, dan **tidak ada** tender tunai CREATED bermetode sama.
- `maxCollectible` tunai = `max(collectible, round(collectible))` dengan `collectible = RequestedAmount − SuccessfulAmount − (tender CREATED/PENDING semua metode)`. Nominal 24.461.828 tidak mungkin melebihi 24.461.829 pada sisa Rp24.461.828,74, jadi **sisa settlement di server < Rp24.461.828**: sebagian sudah tercatat berhasil, atau dipesan tender metode lain, atau `RequestedAmount` lebih kecil dari yang tampil.
- Penyebab pendukung yang TERBUKTI dari kode FE: `add-tender-modal.jsx` memakai `Math.trunc(collectible)` untuk "Gunakan sisa/Copy Nominal", sehingga nominal terkirim 24.461.828 (bukan 24.461.828,74) dan meninggalkan Rp0,74. Hipotesis terkuat (BELUM terbukti): pembayaran tunai Rp24.461.828 pertama sudah tercatat SUCCEEDED (sisa Rp0,74), layar masih menampilkan sisa penuh yang basi, lalu percobaan kedua ditolak 422. Bila benar, guard backend justru mencegah pembayaran ganda.
- Pembuktian membutuhkan baris settlement/tender asli (RequestedAmount, SuccessfulAmount, status tender per metode). Query baca-saja yang disarankan untuk pemilik: pilih `BilSettlement` milik invoice itu (Status, RequestedAmount, SuccessfulAmount, RowVersion) dan `BilTender` miliknya (Status, Amount, CashTenderedAmount, CashChangeAmount, PaymentMethodId, AttemptedAt, KwitansiNumber), urut `AttemptedAt` (sesuaikan nama tabel bila berbeda). **BLOCKED untuk kepastian akar masalah**; perbaikan di bawah tidak bergantung padanya.

## 2. Perubahan (`Services/BillingSettlementService.cs`)
- `AddTenderAsync`: cabang `request.Amount > maxCollectible` kini memanggil `BuildOverCollectibleException` setelah pengecekan tender tunai CREATED yang ada (tidak diubah). Batas, rumus `maxCollectible`, `ResolveCashBreakdown`, urutan guard (RowVersion dan status Settled lebih dulu), idempotensi, dan pemulihan tender CREATED **tidak diubah**.
- `BuildOverCollectibleException` (baru, murni pesan): (a) ada tender CREATED/PENDING yang memesan saldo → **409** dengan nominal tertahan dan sisa; (b) sebagian sudah SUCCEEDED → **422** menyebut jumlah yang sudah tercatat, sisa, dan nominal yang dikirim ("muat ulang halaman pembayaran"); (c) lainnya → **422** dengan nominal dan sisa serta penegasan bahwa uang tunai diterima boleh lebih besar tetapi nominal tagihan tidak. Awalan "Total metode pembayaran melebihi saldo yang harus dibayar." dipertahankan agar klien yang mengenalinya tetap bekerja.
- Pembedaan NETO vs uang fisik tetap di `ResolveCashBreakdown`: uang diterima wajib, tidak boleh kurang dari Amount, kembalian = diterima − Amount dihitung server, konfirmasi kembalian wajib bila positif; presisi maksimal dua desimal (`ValidateMoney`).

## 3. Kebijakan presisi dan pembulatan
Kebijakan resmi yang sudah ada dipertahankan: nominal tunai boleh sampai `Round(sisa, 0, AwayFromZero)`; bila `Amount == sisa dibulatkan` dan selisih < Rp1, rekonsiliasi memposting `BilAdjustment` pembulatan (audit). Bila `Amount` = sisa persis (dua desimal), tidak ada penyesuaian. Nominal di bawah sisa (mis. dipotong ke bilangan bulat) hanyalah pembayaran sebagian dan meninggalkan sisa; itu yang dicegah di sisi FE.

## 4. Kontrak
Request benar (kasus aktual, NETO persis):
```
POST /api/v1/health-services/billing-management/billing/patient-funds/settlements/{id}/tenders
{ "paymentMethodId": "…", "amount": 24461828.74, "cashTenderedAmount": 25000000, "cashChangeGivenConfirmed": true, "expectedRowVersion": "…", "correlationId": "…", "causationId": "…" }
```
Respons: tender SUCCEEDED dengan `amount` 24461828.74, `cashTenderedAmount` 25000000, `cashChangeAmount` 538171.26.

Penolakan yang tetap benar: 422 uang diterima < Amount; 422 kembalian tanpa konfirmasi; 422 Amount > sisa (pesan spesifik di atas); 409 tender menunggu memesan saldo; 409 RowVersion basi; 422/409 settlement sudah lunas.

## 5. Skenario TC-01..20 (sisi backend)
| TC | Status |
| --- | --- |
| 01-03 | NO CHANGE REQUIRED, VERIFIED STATIC (`ResolveCashBreakdown`) |
| 04 | NO CHANGE REQUIRED, VERIFIED STATIC |
| 05, 08 | VERIFIED STATIC (guard tidak diubah; pesan lebih spesifik) |
| 06-07 | NO CHANGE REQUIRED (guard per tender; kembalian dari uang fisik, bukan alokasi) |
| 09-10 | NO CHANGE REQUIRED (pemulihan CREATED, idempotency key, satu Kwitansi per tender baru) |
| 11-12 | NO CHANGE REQUIRED (advisory lock + Serializable + RowVersion) |
| 13-15 | VERIFIED STATIC (dua desimal; pembulatan resmi); TC-15 aturan bisnis pembulatan NEEDS CONFIRMATION bila pemilik ingin mengubah |
| 16 | NOT RUN (butuh transaksi nyata) |
| 17-20 | FE (lihat FE-BKC-FIX-016) |

## 6. Risiko
- Akar masalah kasus aktual belum terbukti tanpa data settlement/tender. Bila setelah ini masih 422, pesan baru memuat angka (diminta, berhasil, sisa) yang menentukan penyebab.
- Settlement yang sudah SUCCEEDED Rp24.461.828 dan sisa Rp0,74 tidak diperbaiki otomatis; sisa itu diselesaikan kasir lewat pembayaran Rp0,74 atau mekanisme penyesuaian/pembulatan yang disahkan. Tidak ada data diubah.
- Belum dibuild; kompilasi `BuildOverCollectibleException` (record exception, `CultureInfo`) perlu build.
