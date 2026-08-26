# Arsitektur Frontend — Rawat Jalan Billing

| Field | Nilai |
|---|---|
| Blueprint | `RJ-BIL-BP-001` revision `11` |
| Status | `draft` |
| Backend contract evidence | Working tree Billing Operational; source commit `9b26be3...` |
| Frontend source evidence | `ab4bd836e05c72d0679e02899258f3773f3869a2` |
| Domain architecture | revision `1`, core internal/manual independen dari partial external adapter |

Dokumen ini menentukan kontrak perilaku frontend. Ia tidak mengunci route, layout, warna,
sidebar, tab, atau komponen visual yang belum diberi UI authority.

## 1. Kebutuhan fungsional

| Kemampuan | Pengguna | Data yang dikonsumsi | Aksi utama |
|---|---|---|---|
| Melihat folio berdasarkan encounter | Billing, cashier, authorized clinical read | Folio status, charge line, component, processing outcome | Buka detail, refresh, lihat alasan review |
| Menampilkan milestone processing | Billing/integration operator | Outcome, replay, version conflict, error code, correlation | Retry terkontrol, buka reconciliation case |
| Menampilkan allocation | Billing/payer/cashier | Payer allocation, patient responsibility, residual | Lihat versi, jangan overwrite histori |
| Mengajukan financial action | Billing maker | Charge state, approval policy, reason, amount | Submit request; tidak langsung mengubah state |
| Memproses approval | Checker berwenang | Request, maker, impact, policy version | Approve/reject/return; self-approval ditolak |
| Melihat projection di Pharmacy | Pharmacy | Status/version/reference dari Billing/Payer | Baca saja; urgent exception melalui workflow |

## 2. Data dan status frontend

Frontend wajib membedakan:

- clinical order/fulfillment;
- milestone processing (`Received`, `InProgress`, `OutcomeUnknown`, `PendingReconciliation`);
- charge calculation (`PendingFinancialReview`, `Recognized`, `Superseded`, `Voided`, `Reversed`);
- allocation/patient responsibility;
- payment/claim settlement;
- financial action approval.

`OutcomeUnknown` ditampilkan sebagai “hasil pemrosesan belum dapat dipastikan”, bukan gagal dan
bukan berhasil. `PendingFinancialReview` ditampilkan sebagai “menunggu tinjauan finansial”.

## 3. State handling

| State UI | Perilaku |
|---|---|
| Loading | Tampilkan indikator tanpa menghapus data terakhir yang masih relevan |
| Empty | Jelaskan folio belum terbentuk; jangan membuat folio dari frontend |
| Error 400 | Tampilkan koreksi input |
| Error 401/403 | Tampilkan akses tidak tersedia; jangan menyembunyikan sebagai empty |
| Error 404 | Tampilkan encounter/folio tidak ditemukan |
| Error 409 | Tampilkan konflik versi/outcome dan tombol rekonsiliasi/reload terkontrol |
| Timeout/network | Pertahankan request identity; jangan auto-submit dengan idempotency key baru |
| Stale response | Tolak response versi lebih lama menimpa state yang lebih baru |

## 4. Aksi per peran dan permission

| Peran/capability | Boleh | Tidak boleh |
|---|---|---|
| Billing reader | Baca folio dan histori | Mengubah charge atau approval |
| Billing maker | Ajukan allocation/action | Menyetujui request sendiri |
| Billing checker | Approve/reject/return | Mengubah clinical fact |
| Payer operator | Catat decision/manual claim | Menetapkan Paid langsung pada Pharmacy |
| Cashier | Collection/receipt/refund execution setelah approval | Mengubah order klinis |
| Pharmacy | Clinical fulfillment dan baca projection | Menulis canonical financial status |
| Clinical user | Mengirim clinical fact/correction request | Void/refund/waiver financial |

## 5. Duplicate submit, cache, dan invalidation

1. Tombol submit milestone/financial action dinonaktifkan setelah request diterima.
2. Client memakai idempotency key yang stabil untuk satu operasi dan tidak menggantinya saat
   timeout sebelum status query selesai.
3. Query folio di-invalidate setelah response canonical berhasil, replay, atau version conflict.
4. Response versi lama tidak boleh menimpa allocation/projection versi baru.
5. Refresh adalah operasi read-only dan tidak mengulang mutation.

## 6. Privacy, accessibility, dan responsive behavior

Data klinis sensitif tidak masuk custom logger dan hanya ditampilkan sesuai permission. UI harus
memakai label manusia, status text, reason, dan waktu; tidak menjadikan UUID sebagai satu-satunya
informasi. Kontras, keyboard navigation, focus/error announcement, dan tampilan layar kecil
menjadi persyaratan engineering standar. Detail visual final tetap `DEV_DISCRETION` sampai UI
authority ditetapkan.

## 7. UI yang sengaja belum dikunci

Route final, lokasi menu, susunan sidebar, bentuk modal/drawer, warna status, dan component
library tidak ditentukan oleh dokumen ini. Implementer frontend harus mengusulkan opsi dan
menunggu authority UI yang sah. Adapter payer eksternal tidak boleh memiliki tombol aktivasi
produksi sebelum `RJ-BIL-DEP-009` selesai.
