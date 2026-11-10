# Optik — Matriks Hak Akses dan Audit

| Field | Nilai |
| --- | --- |
| `contract_version` | `OPT-PERM-v1.4` — status `approved` (sisi Optik), Yoga Aji Pratama 2026-11-06. Dasarnya: `OPT-PERM-v1.3` `approved` (sisi Optik), Yoga Aji Pratama 2026-11-06. Registri Optik tetap 45 endpoint, dan tidak ada izin baru di kode. Enam izin modul lain yang sudah ada dicantumkan di tabel peran (`OPT-DEC-085`); persetujuannya cukup Yoga Aji Pratama dan admin akses, dan pemilik modul diberi tahu (`OPT-DEC-086`) |
| Owner | Yoga Aji Pratama; security: pemilik arsitektur backend |
| `approved_by` / `approved_at` | Yoga Aji Pratama / 2026-11-06 untuk `v1.4`, `v1.3`, `v1.2`, dan `v1.1`; `v1` 2026-11-03 |
| Konstanta | `Areas/HealthServices/OpticalManagement/Constants/OpticalPermissions.cs`, `ModuleCode = "HEALTH_SERVICE_OPTICAL_MANAGEMENT"`, `ModuleName = "Health Service Optical Management"`, `AreaName = "HealthServices"` |

String disalin apa adanya ke `[AccessController(ControllerName = …)]`, `[AccessAction]`, dan
`[AccessPermission]`. Selisih satu huruf menghasilkan 403 permanen. Pemetaan ke departemen-jabatan
diatur lewat `SysAccessPolicies`. POST Role Access menimpa seluruh set, jadi baca dulu lalu kirim
ulang utuh.

## 1. Endpoint

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
| --- | --- | --- | --- | :---: |
| `GET visits/` | `OptVisit` | `Read` | `[AccessPermission("OptVisit", "Read")]` | Tidak |
| `GET visits/{encounterId}` | `OptVisit` | `Read` | `[AccessPermission("OptVisit", "Read")]` | Tidak |
| `POST visits/` | `OptVisit` | `Create` | `[AccessPermission("OptVisit", "Create")]` | Ya |
| `GET refraction-exams/`, `/{id}` | `OptRefractionExam` | `Read` | `[AccessPermission("OptRefractionExam", "Read")]` | Tidak |
| `POST refraction-exams/` | `OptRefractionExam` | `Create` | `[AccessPermission("OptRefractionExam", "Create")]` | Ya |
| `PUT refraction-exams/{id}/measurements` | `OptRefractionExam` | `Update` | `[AccessPermission("OptRefractionExam", "Update")]` | Ya |
| `POST refraction-exams/{id}/finalize` | `OptRefractionExam` | `Finalize` | `[AccessPermission("OptRefractionExam", "Finalize")]` | Ya |
| `POST refraction-exams/{id}/revisions` | `OptRefractionExam` | `Revise` | `[AccessPermission("OptRefractionExam", "Revise")]` | Ya |
| `POST refraction-exams/{id}/cancel` | `OptRefractionExam` | `Cancel` | `[AccessPermission("OptRefractionExam", "Cancel")]` | Ya |
| `GET prescriptions/`, `/{id}`, `/options` | `OptPrescription` | `Read` | `[AccessPermission("OptPrescription", "Read")]` | Tidak |
| `POST prescriptions/`, `/external` | `OptPrescription` | `Create` | `[AccessPermission("OptPrescription", "Create")]` | Ya |
| `POST prescriptions/{id}/revisions` | `OptPrescription` | `Revise` | `[AccessPermission("OptPrescription", "Revise")]` | Ya |
| `GET prescriptions/range-override`; syarat tambahan di dalam create/revise | `OptPrescription` | `OverrideRange` | `[AccessPermission("OptPrescription", "OverrideRange")]` — dicek service, bukan atribut endpoint | Ya |
| `GET orders/`, `/{id}`, `/{id}/payment-status`, `/{id}/price-preview` | `OptOrder` | `Read` | `[AccessPermission("OptOrder", "Read")]` | Tidak |
| `POST orders/` | `OptOrder` | `Create` | `[AccessPermission("OptOrder", "Create")]` | Ya |
| `PUT orders/{id}` | `OptOrder` | `Update` | `[AccessPermission("OptOrder", "Update")]` | Ya |
| `POST orders/{id}/bill` | `OptOrder` | `Bill` | `[AccessPermission("OptOrder", "Bill")]` | Ya |
| `POST orders/{id}/send-to-work` | `OptOrder` | `SendToWork` | `[AccessPermission("OptOrder", "SendToWork")]` | Ya |
| `POST orders/{id}/mark-ready` | `OptOrder` | `MarkReady` | `[AccessPermission("OptOrder", "MarkReady")]` | Ya |
| `POST orders/{id}/hand-over` | `OptOrder` | `HandOver` | `[AccessPermission("OptOrder", "HandOver")]` | Ya |
| `POST orders/{id}/cancel` | `OptOrder` | `Cancel` | `[AccessPermission("OptOrder", "Cancel")]` | Ya |
| `POST orders/{id}/cancellation-requests` | `OptOrder` | `RequestCancel` | `[AccessPermission("OptOrder", "RequestCancel")]` | Ya |
| `POST orders/{id}/cancellation-requests/{requestId}/decision` | `OptOrder` | `DecideCancel` | `[AccessPermission("OptOrder", "DecideCancel")]` | Ya |
| `POST orders/{id}/close-not-collected` | `OptOrder` | `CloseNotCollected` | `[AccessPermission("OptOrder", "CloseNotCollected")]` | Ya |
| `GET warranty-claims/`, `/{id}` | `OptWarrantyClaim` | `Read` | `[AccessPermission("OptWarrantyClaim", "Read")]` | Tidak |
| `POST warranty-claims/` | `OptWarrantyClaim` | `Create` | `[AccessPermission("OptWarrantyClaim", "Create")]` | Ya |
| `POST warranty-claims/{id}/examination` | `OptWarrantyClaim` | `Examine` | `[AccessPermission("OptWarrantyClaim", "Examine")]` | Ya |
| `POST warranty-claims/{id}/decision` | `OptWarrantyClaim` | `Decide` | `[AccessPermission("OptWarrantyClaim", "Decide")]` | Ya |
| `GET reference-data/metadata`, `/item-catalog`, `/settings` | `OptReferenceData` | `Read` | `[AccessPermission("OptReferenceData", "Read")]` | Tidak |
| `PUT reference-data/item-profiles/{drugId}` | `OptReferenceData` | `Update` | `[AccessPermission("OptReferenceData", "Update")]` | Ya |
| `PUT reference-data/settings` | `OptReferenceData` | `UpdateSetting` | `[AccessPermission("OptReferenceData", "UpdateSetting")]` | Ya |
| `GET reports/*` | `OptReport` | `Read` | `[AccessPermission("OptReport", "Read")]` | Tidak |

## 2. Pemetaan peran usulan

Peran berasal dari keputusan pemilik (`OPT-DEC-005`, `010`, `015`, `018`, `027`, `040`). Jabatan
nyata di HR belum diperiksa (`OPT-CAP-07`). Selain izin, service menegakkan profesi RO/Sp.M dan izin
praktik berlaku.

| Resource : Action | RO | Sp.M | Petugas penjualan | Kepala Optik | Admin Optik | Admisi |
| --- | :---: | :---: | :---: | :---: | :---: | :---: |
| `OptVisit : Read` | ✔ | ✔ | ✔ | ✔ | — | ✔ |
| `OptVisit : Create` | — | — | ✔ | ✔ | — | ✔ |
| `OptRefractionExam : Read` | ✔ | ✔ | ✔ | ✔ | — | — |
| `OptRefractionExam : Create/Update/Finalize/Revise/Cancel` | ✔ | ✔ | ✘ | — | — | — |
| `OptPrescription : Read` | ✔ | ✔ | ✔ | ✔ | — | — |
| `OptPrescription : Create/Revise` | ✔ | ✔ | ✘ | — | — | — |
| `OptPrescription : OverrideRange` | ✘ | ✔ | ✘ | ✘ | ✘ | ✘ |
| `OptOrder : Read` | ✔ | ✔ | ✔ | ✔ | — | — |
| `OptOrder : Create/Update/Bill/SendToWork/MarkReady/HandOver/Cancel/RequestCancel` | — | — | ✔ | ✔ | — | — |
| `OptOrder : DecideCancel/CloseNotCollected` | ✘ | ✘ | ✘ | ✔ | — | — |
| `OptWarrantyClaim : Read/Create` | ✔ | ✔ | ✔ | ✔ | — | — |
| `OptWarrantyClaim : Examine` | ✔ | ✔ | ✘ | — | — | — |
| `OptWarrantyClaim : Decide` | ✘ | ✘ | ✘ | ✔ | — | — |
| `OptReferenceData : Read` | ✔ | ✔ | ✔ | ✔ | ✔ | — |
| `OptReferenceData : Update/UpdateSetting` | — | — | — | — | ✔ | — |
| `OptReport : Read` | — | — | — | ✔ | ✔ | — |
| `BillingDiscount : Create` (Billing) | — | — | ✔ | ✔ | — | — |
| `DiscountPolicy : Read` (Billing), untuk daftar pilihan modal Ajukan Diskon (`OPT-DEC-079`) | — | — | ✔ | ✔ | — | — |
| `Drug : Read` (Master Data), untuk pilihan barang Daftarkan Barang (`OPT-DEC-080`) | — | — | — | — | ✔ | — |
| `Patient : Read` (Pasien), untuk mencari pasien saat mendaftar (`OPT-DEC-085`) | — | — | ✔ | ✔ | — | ✔ |
| `PatientInsurance : Read` (Pasien), untuk memilih kartu asuransi (`OPT-DEC-085`) | — | — | ✔ | ✔ | — | ✔ |
| `PatientCompanyGuarantor : Read` (Pasien), untuk memilih Kartu Penjamin Perusahaan (`OPT-DEC-085`) | — | — | ✔ | ✔ | — | ✔ |
| `PatientEncounter : Read` (Registrasi), untuk memilih kunjungan asal rujukan (`OPT-DEC-085`) | — | — | ✔ | ✔ | — | ✔ |
| `BillingInvoice : Read` (Billing), untuk harga dan diskon baris di detail pesanan (`OPT-DEC-085`) | ✘ | ✘ | ✔ | ✔ | — | — |
| `Supplier : Read` (Administrator), untuk memilih lab lensa luar saat kirim ke pengerjaan (`OPT-DEC-085`) | — | — | ✔ | ✔ | — | — |

"—" = tidak diatur keputusan; bawaan tidak diberikan (*fail-closed*).

**Izin modul lain (`OPT-DEC-085`).** Baris bertanda modul di dalam kurung adalah izin yang sudah ada di modul itu; layar Optik memanggil endpoint-nya. Superadmin melewati pemeriksaan izin, sehingga kekurangan izin hanya terlihat dengan akun petugas (`OPT-AC-042`). **Contoh:** petugas penjualan tanpa `Patient : Read` membuka Daftarkan Kunjungan, lalu pencarian pasien dijawab `403`. Keenam izin terbaru cukup disetujui Yoga Aji Pratama dan diberikan admin akses, dan pemilik modul diberi tahu (`OPT-DEC-086`). Izin tulis lintas modul tidak termasuk.

**RO dan Sp.M tanpa `BillingInvoice : Read`.** Rincian tagihan kunjungan tidak terbaca, dan bagian harga di detail pesanan menampilkan pesan penolakan. Status Bayar tetap tampil, karena `GET orders/{id}/payment-status` memakai `OptOrder : Read`: isinya bagian pasien, minimum uang muka, dan jumlah terbayar. **Contoh:** RO melihat bagian pasien Rp 1.050.000 dan terbayar Rp 525.000 pada pesanan itu, tetapi tidak melihat baris lain pada tagihan kunjungan.

## 3. Audit

| Kejadian | Jejak tahan lama | Logger (teks pesan) |
| --- | --- | --- |
| Pemeriksaan final/revisi | `OptRefractionExamRevision` (pelaku, waktu, alasan) + snapshot izin | "OPT-EXM-… difinalkan oleh {aktor}" |
| Kirim ulang tarif pemeriksaan oleh pekerja latar (`OPT-DEC-082`) | Fakta tagihan: pelaku `FinalizedBy`; `BillingFactEmittedAt` | "Tarif OPT-EXM-… dikirim ulang pekerja latar atas nama pemeriksa yang memfinalkan". Tanpa nilai ukur |
| Resep dibuat/disalin/direvisi/lewat rentang | `OptPrescriptionVersion` | "OPT-RX-… versi {n} oleh {aktor}" |
| Transisi pesanan | `OptOrderStatusHistory` | "OPT-ORD-… {dari} → {ke} oleh {aktor}" |
| Pengajuan/keputusan batal | `OptOrderCancellation` | "Batal OPT-ORD-… {disetujui/ditolak} oleh {aktor}" |
| Keluhan | `OptWarrantyClaim` | "OPT-CLM-… {status} oleh {aktor}" |
| Pengaturan | `OptSettingRevision` | "Pengaturan Optik diubah oleh {aktor}" |

Payload logger **tidak boleh** memuat kolom bertanda Sensitif (nilai ukur, resep, keluhan, temuan,
nama pengambil). `LoggerService.AuditAsync` mengabaikan objek `data`, jadi isi pesan ditulis di teks
pesan. Ruas `Id`/`Name`/`Path` di payload jangan dipakai karena menimpa pelaku.
