# Laporan Perubahan Backend — `BE-MCU-024`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-024` |
| Judul | Batal komponen dengan refund |
| Slice | `MVP-2` — `EPIC MCU-04` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-024`* |
| Trace | `FR-MCU-034`; `MCU-DEC-015`, `043`; `MCU-AC-009`, `MCU-AC-031`; `ARC-TECH-01`; `AT-MCU-30`, `31`, `37` |
| Contract version | `MCU-API-v1` grup Mcu Examination (`PATCH components/{componentId}/cancel`, `CancelMcuComponentRequest`, `McuOrderComponentResponse`); `MCU-STATE-v1` 2.1–2.2; `MCU-VAL-v1` `VAL-MCU-38`, `40`, `46`; `MCU-PERM-v1` (`McuExamination : CancelComponent`); `MCU-INT-v1` `INT-MCU-08`, `09` — seluruhnya `approved` 2026-10-21; hash kontrak cocok dengan manifest |
| Dependency | `BE-MCU-021` ✅, `BE-MCU-026` ✅ |
| Keputusan user (2026-10-23) | Lab diberi jalur **cabut satu permintaan yang belum masuk wadah** (`LabExaminationService.CancelOrderedProcedureAsync`); Yoga Aji Pratama selaku pemilik Lab |
| Klasifikasi | `HEAVY` — skor 10: berkas diperiksa > 20 (2), berkas diubah 4–8 (1), logika kompleks (2), memakai kontrak yang sudah ada (1), perilaku persistence modul mitra (1), keamanan berkaitan — penjaga dokter (1), workflow terbatas (1), uang — refund (1) |
| Task mode | `BACKEND` |
| Target tulis | MCU: `McuExaminationService.cs`, `McuExaminationController.cs`, `McuExaminationDtos.cs`, `McuOperationResult.cs`. Mitra: `LabExaminationService.cs` (Lab). Laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-23 |
| Status | Selesai — `AT-MCU-30`, `31`, `37` terbukti di harness integrasi; **uji devYoga belum dijalankan** (dua migration masih tertunda, butuh izin) |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE`; modul mitra `LaboratoryManagement` (`Lab`) |
| Keberlakuan | `NEW CODE` (endpoint dan service MCU, method Lab baru) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (`McuExamination : CancelComponent`), `QBE-LOG-001` (alasan batal tidak masuk log), `QBE-VAL-001`, `QBE-TXN-001` (satu transaksi Lab/Rad/Billing/MCU), `QBE-DTO-001`, `QBE-DEL-001` (aktor batal tercatat) |
| Checker QBE | `PASS` — 5 berkas task nol temuan. Kontrol negatif sementara `ZzzKontrol` terkena `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

Dokter MCU belum bisa membatalkan satu pemeriksaan di dalam paket. Peserta yang ternyata hamil tetap memiliki
pesanan Thorax yang aktif, dan uang komponen yang tidak dikerjakan tidak dapat dikembalikan.

**Contoh.** Siti membayar Paket Basic Rp 350.000. Saat pemeriksaan berjalan, ia ternyata hamil. dr. Rina membatalkan
Thorax dengan alasan *kehamilan* → Thorax *Dibatalkan*, pesanan Radiologi batal, dan refund **Rp 150.000** (nilai
pengembalian Thorax di master paket, bukan tarif satuan Thorax Rp 200.000) diajukan ke kasir. Kemudian Audiometri
dibatalkan → refund kedua Rp 50.000. Total refund atas baris paket Rp 200.000, sisanya Rp 150.000.

---

## 2. Proses bisnis

1. Dokter MCU yang **ditetapkan untuk peserta itu** menekan **Batalkan** pada satu komponen dan mengisi alasan.
2. Syarat: order *Dalam Pemeriksaan* atau *Menunggu Resume*; komponen belum *Selesai* dan belum *Dibatalkan*; versi
   order sama dengan yang dibaca dokter.
3. Pemeriksaan dibatalkan di modulnya (satu transaksi):
   - **Laboratorium, darah belum diambil** — hanya permintaan pemeriksaan itu yang dicabut; pemeriksaan lain di
     pesanan yang sama tetap berjalan. Bila itu permintaan terakhir pada pesanan yang belum diproses, pesanan Lab
     dibatalkan utuh.
   - **Laboratorium, wadah sudah dibentuk** — hanya pemeriksaan itu yang dibatalkan; wadah dan pemeriksaan lain tetap.
   - **Radiologi** — pesanannya dibatalkan.
   - **Pemeriksaan MCU** (mis. Audiometri) — tidak ada pesanan di modul lain.
4. Refund sebesar **nilai pengembalian komponen** diajukan ke Billing bila komponen termasuk paket, nilainya lebih
   dari nol, dan invoice paket sudah dibayar. Billing menolak bila nominalnya melebihi sisa baris paket.
5. Komponen dicatat *Dibatalkan* beserta alasan, dokter pembatal, dan pengajuan refund-nya. Bila semua komponen sudah
   tuntas (selesai atau dibatalkan), order pindah ke *Menunggu Resume*.
6. Persetujuan dan pembayaran refund tetap di kasir (`POST refunds/{id}/approve`).

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Bukan dokter MCU yang ditetapkan | `403` `VAL-MCU-38` *"Hanya dokter MCU yang ditetapkan untuk peserta ini yang dapat melakukan tindakan ini."* |
| Alasan kosong | `400` `VAL-MCU-40` *"Alasan wajib diisi."* |
| Versi order berbeda / tabrakan simpan | `409` `VAL-MCU-46` |
| Pemeriksaan Lab sudah divalidasi atau dirilis | `409` *"Pemeriksaan Lab sudah divalidasi dan tidak dapat dibatalkan."* |
| Komponen sudah selesai / sudah dibatalkan | `409` *"Komponen sudah selesai."* / *"Komponen sudah dibatalkan."* (tanpa refund kedua) |
| Order belum dimulai | `409` *"Pemeriksaan belum dimulai."* |
| Lab, Rad, atau Billing menolak di tengah (mis. nominal melebihi sisa baris) | `409` `MCU-PARTNER-REJECTED` dengan pesan mitra; pesanan Lab/Rad, refund, komponen, dan order **tetap seperti semula** |
| Peserta perusahaan (terjamin, belum membayar) | Komponen dibatalkan, pesanan batal, **tanpa refund** |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/backend-roadmap.md` (`BE-MCU-024`), `contracts/api-contract.md`, `state-transition-matrix.md` (1.1, 2.1–2.3),
`validation-matrix.md`, `integration-contract.md` (`INT-MCU-08`, `09`), `permission-audit-matrix.md`,
`testing/acceptance-test-matrix.md`, `00-interview-decisions.md` (`MCU-DEC-015`, `043`), `evidence/2026-10-21-persetujuan-dan-pembuktian.md`
(2.1), `blueprint-manifest.md` (hash), laporan `BE-MCU-025` dan `026`, `McuExaminationService`, `McuExaminationController`,
`McuOrderComponent`, `McuOrderQueryService`, `LabExaminationService.CancelAsync`, `LabOrderedProcedure`, `LabSpecimenService`
(pintu wadah `VAL-68`, pengembalian permintaan), `LabOrderService.CancelAsync`, `RadOrderService.CancelAsync`,
`BillingRefundService.CreateAsync`, `LabPaymentClearanceRules`, governance `AGENTS.md`, `BACKEND_ENGINEERING_CONTRACT.md`,
`MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `rules/backend/`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/Services/McuExaminationService.cs` | `CancelComponentAsync`: penjaga dokter, alasan, versi, status; rencana Lab (pemeriksaan, permintaan, atau pesanan utuh) dan Rad; refund `RefundAmountSnapshot` bila dibayar; satu transaksi; pindah ke *Menunggu Resume*. Dependensi `LabExaminationService` |
| `MedicalCheckupManagement/Controllers/McuExaminationController.cs` | `PATCH components/{componentId}/cancel`, izin `McuExamination : CancelComponent`; log tanpa alasan |
| `MedicalCheckupManagement/DTOs/McuExaminationDtos.cs` | `CancelMcuComponentRequest` (reason, version) |
| `MedicalCheckupManagement/Services/McuOperationResult.cs` | Kode `VAL-MCU-40` |
| `LaboratoryManagement/Services/LabExaminationService.cs` | `CancelOrderedProcedureAsync`: mencabut satu `LabOrderedProcedure` *Ordered* → *Cancelled* beserta riwayat `OrderedProcedure.Cancel` beralasan; ikut transaksi pemanggil |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru sesuai kontrak; request/response sesuai `MCU-API-v1`. Endpoint Lab tidak berubah |
| Database | Tanpa migration (kolom `CancelReason`, `CancelledByDoctorId`, `RefundCaseId` sudah ada sejak `BE-MCU-002`). **Tidak ada eksekusi ke basis data mana pun.** Endpoint ini butuh tabel `BilRefundCaseItem` (`BE-MCU-026`) yang migration-nya belum diterapkan di devYoga |
| Keamanan/Auth | `McuExamination : CancelComponent` + akun harus tertaut ke `McuDoctorId` order. Hak akses diberikan per departemen-posisi di `SysAccessPolicies`; pemberiannya ke jabatan dokter MCU di devYoga belum diperiksa. Alasan batal (bisa berupa kehamilan) tidak masuk log aplikasi |
| Modul mitra | Lab: method baru saja; jalur Lab yang ada tidak berubah |

### 3.4 Delta terhadap kontrak dan roadmap

| Butir | Roadmap/kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Batal Lab per pemeriksaan | Lewat `LabExaminationService.CancelAsync` | `CancelAsync` bila wadah sudah ada; `CancelOrderedProcedureAsync` (baru) bila belum; `LabOrderService.CancelAsync` bila komponen itu yang terakhir pada pesanan *Requested/Confirmed* | Sebelum wadah dibentuk belum ada `LabExamination`; keputusan pemilik Lab 2026-10-23 |
| Cakupan berkas | `McuExaminationService`, controller | + `LabExaminationService` | Idem |
| Penanda refund | `INT-MCU-09`: `SelectedBillingItemIds`, `RequestedAmount`, `Reason` | `CorrelationId` = komponen, `CausationId` = order, idempotency = komponen | Billing menolak `CorrelationId` yang sudah dipakai case lain; dengan korelasi = order, komponen kedua selalu ditolak |
| Status order | Batal komponen dari *Dalam Pemeriksaan* | *Dalam Pemeriksaan* atau *Menunggu Resume*; *Hadir* ditolak *"Pemeriksaan belum dimulai."* | Sebelum dimulai, peserta tunai belum tentu membayar dan baris paket tidak berkurang; jalurnya batal penuh atau mulai lebih dulu |
| Peserta perusahaan | Sesuai aturan Billing (`MCU-COORD-003`) | Tanpa refund, tagihan kolektif tidak dikurangi | Perlakuan tagihan kolektif belum ada di Billing (`BE-MCU-026` keputusan 3) |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Examination

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `PATCH` | `/orders/{id}/components/{componentId}/cancel` | Dokter MCU membatalkan satu komponen; refund nilai pengembalian diajukan bila sudah dibayar | `McuExamination : CancelComponent` | `CancelMcuComponentRequest` (reason, version) | `McuOrderComponentResponse` (status *Dibatalkan*, `cancelReason`) | Tersedia |

Kode status: `200`, `400`, `403`, `404`, `409`. Log aplikasi: nama komponen, jenis, nomor order, nomor pesanan — tanpa
alasan, nama, atau NIK.

---

## 5. Verifikasi

Harness uji **sementara** disusun ulang (`Tests/QuilvianSystemBackend.McuTests`): SQLite, rakitan DI seperti `Program.cs`
— `AddBillingManagement()` (refund sungguhan dengan akumulasi per baris `BE-MCU-026`), `LabOrderService`,
`LabExaminationService`, `RadOrderService`, `EncounterIntakeService`, `McuBookingService`/`McuCheckInService` produksi
(hanya seam kewenangan yang diganti). Order lahir lewat *walk-in* sungguhan, dibayar (settlement + tender tunai), lalu
dimulai lewat `StartAsync`. Model uji menyimpan `DateTimeOffset` sebagai bilangan, dan kunci penasihat PostgreSQL
(`hashtext`, `pg_advisory_xact_lock`) diganti fungsi tanpa efek (keterbatasan SQLite). Harness **dihapus** sesudahnya.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 13 lulus | `PASS` | `Passed! - Failed: 0, Passed: 13` |
| `AT-MCU-30` | Thorax batal → pesanan Rad *Cancelled*, refund **Rp 150.000** (rincian baris paket 150.000, `RefundCaseId` tercatat, dokter pembatal dr. Rina); Audiometri batal → refund kedua Rp 50.000; total atas baris paket Rp 200.000; order tetap *Dalam Pemeriksaan* | `PASS` | `AT_MCU_30_…` |
| `AT-MCU-37` sebelum wadah | Darah Lengkap batal → permintaan DL *Cancelled*, Gula Darah Puasa tetap *Ordered*, pesanan Lab tetap *Requested*; riwayat Lab `OrderedProcedure.Cancel` beralasan; refund Rp 100.000 | `PASS` | `AT_MCU_37_sebelum_…` |
| `AT-MCU-37` sesudah wadah | DL *Cancelled*, GDP tetap *ChargeEligible*, pesanan tetap *InProcess*, wadah tetap *Accepted* | `PASS` | `AT_MCU_37_sesudah_…` |
| Lab tervalidasi | `409` pesan persis; pemeriksaan, komponen, versi order tidak berubah; nol refund | `PASS` | `Pemeriksaan_Lab_tervalidasi_…` |
| Semua komponen dibatalkan | Komponen Lab terakhir → pesanan Lab batal utuh; order → *Menunggu Resume* dengan riwayat; 4 refund total Rp 340.000 ≤ Rp 350.000 | `PASS` | `Semua_komponen_…` |
| `AT-MCU-31` | Akun dokter lain → `403` `VAL-MCU-38`; nol perubahan | `PASS` | `AT_MCU_31_…` |
| Validasi | Alasan kosong `VAL-MCU-40`; versi lama `VAL-MCU-46`; komponen *Selesai* → `409`; batal ulang → `409`, tetap satu refund; order belum dimulai → `409` | `PASS` | `Validasi_…`, `Order_belum_dimulai_…` |
| Billing menolak di akhir | Refund lain menyisakan Rp 100.000 → batal Thorax `409` `MCU-PARTNER-REJECTED` *"…melebihi sisa…"*; pesanan Rad **tidak** batal, komponen tetap *Dipesan*, versi order sama, tidak ada refund baru | `PASS` | `Billing_menolak_…` |
| Peserta perusahaan | Terjamin tanpa bayar → komponen batal, pesanan Rad batal, nol refund | `PASS` | `Peserta_perusahaan_…` |
| Regresi `BE-MCU-025` | Batal penuh tanpa batal komponen sebelumnya tetap mengajukan refund Rp 350.000 | `PASS` | `Regresi_batal_penuh_…` |
| Kontrak endpoint | Template `components/{componentId:guid}/cancel`, izin `McuExamination : CancelComponent` | `PASS` | `Endpoint_PATCH_…` |
| Uji mutasi | Penjaga *tervalidasi*, pencabutan permintaan Lab, korelasi refund per komponen, dan penjaga dokter dirusak → 5 uji **gagal**; dipulihkan identik (`cmp`), kembali lulus | `PASS` | Keluaran `dotnet test` |
| Checker QBE `-Path` (5 berkas + kontrol) | Nol temuan; kontrol terkena 3 aturan | `PASS` | Keluaran checker |
| Uji devYoga satu refund (roadmap *Verifikasi*) | Tidak dijalankan: migration `AddCoveringMcuOrderToLabRadOrders` dan `AddBillingRefundCaseItem` tertunda di devYoga, hak akses `CancelComponent` untuk jabatan dokter MCU belum diperiksa, dan eksekusi DB butuh izin | `NOT RUN` | `migrations list` 2026-10-23 |
| PostgreSQL (kunci baris, kunci penasihat, Serializable), `403` lewat HTTP | Tidak dijalankan | `NOT RUN` | — |

Uji manual: `NOT FEASIBLE` di sesi ini (lihat uji devYoga di atas).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MCU-30` — Thorax batal: komponen *Dibatalkan*, pesanan Rad batal, refund tercatat Rp 150.000; komponen kedua → refund kedua, total ≤ nilai baris | Terpenuhi | §5 |
| `AT-MCU-31` — dokter lain `403` | Terpenuhi | §5 |
| `AT-MCU-37` — hanya pemeriksaan itu yang batal, pemeriksaan lain tetap berjalan | Terpenuhi (sebelum dan sesudah wadah) | §5 |
| Lab tervalidasi → `409` *"Pemeriksaan Lab sudah divalidasi dan tidak dapat dibatalkan."* | Terpenuhi | §5 |
| Verifikasi roadmap: satu uji devYoga refund dengan izin | **Belum** — menunggu dua migration dan izin | §5 |
| DoD: kriteria terbukti | Terpenuhi di harness integrasi | §5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Batal penuh sesudah batal komponen ditolak.** Uji pengamatan: Thorax dibatalkan (refund Rp 150.000), lalu batal penuh order meminta Rp 350.000 sesuai `INT-MCU-09` → Billing menolak *melebihi sisa*, `409`, tidak ada yang berubah. Perbaikannya butuh perubahan kontrak `INT-MCU-09` (batal penuh = **sisa** baris paket); tidak diubah di task ini |
| Masalah yang diketahui | Peserta perusahaan tidak menerima pengurangan tagihan kolektif (`MCU-COORD-003`). Pembatalan pemeriksaan Lab yang sudah layak tagih untuk pemeriksaan **di luar paket** tidak mengoreksi tagihannya — itu milik Lab (`BE-LAB-13`). Pesanan Lab *InProcess* yang semua pemeriksaannya dibatalkan tetap *InProcess* (perilaku Lab, `MCU-EVD-002` 2.1 butir 3) |
| Risiko tersisa | Audit Lab dan Billing ditulis sebelum transaksi MCU commit; bila langkah sesudahnya gagal, log bisa menyebut pembatalan/refund yang akhirnya dibatalkan. Kunci baris dan isolasi hanya terbukti di SQLite |
| Perubahan sampingan | Folder uji sementara dibuat lalu dihapus (salinan di scratchpad sesi); kontrol negatif QBE dibuat lalu dihapus |
| Interupsi | Skrip mutasi pertama gagal mencocokkan penanda (akhir baris LF) sebelum menulis apa pun; diulang dengan penanda tanpa baris baru. Folder uji sempat terkunci build server, dihapus sesudah `dotnet build-server shutdown` |
| Status Git | Baru dari task ini: ` M` `LabExaminationService.cs`; perubahan di folder `MedicalCheckupManagement/` (`??`); laporan ini; tanda roadmap/traceability. Berkas `M`/`??` lain sudah ada sebelum task |
| Langkah berikutnya | (1) Putuskan perbaikan batal penuh sesudah batal komponen (ubah `INT-MCU-09`); (2) uji devYoga sesudah dua migration diterapkan dan izin `CancelComponent` dipastikan; (3) `BE-MCU-030` (resume) siap |
