# Pemilihan Penyedia — Matriks Perpindahan Status

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-PS-STATE` · `last_changed_in: 1.0` · status `draft` |
| Pemilik | Pengguna sesi (Product Owner Pengadaan) · `approved_by: —` · `approved_at: —` |
| Masukan | `PBJ-DEC-070`..`085`; `PBJ-ASM-005`, `008`, `009`; `02-backend-architecture.md` bagian 8 · `input_revision`: decision log 26 November 2026 |
| Kompatibilitas | Seluruh status baru. Satu perubahan pada `perencanaan-permintaan`: PR `APPROVED` → `CONVERTED_TO_PACKAGE` kini dipicu "semua baris beli sudah punya paket", dan dapat kembali ke `APPROVED` saat paket batal (`PBJ-DEC-077`) |

Setiap perpindahan menulis satu baris `PrcDocumentHistory` pada dokumennya. Transisi yang tidak tercantum **tidak sah** dan
dijawab 409 `PBJ-VAL-128` kecuali disebut kode lain.

## 1. Paket (`PrcPackage`)

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Buat paket | `DRAFT` | `ProcurementPackage : Create` | Baris PR `APPROVED`, belum dipaketkan, satu jenis dan satu tahun | 409 `PBJ-VAL-120`/`121`, 422 `PBJ-VAL-122`/`123` |
| `DRAFT`, `PREPARATION_RETURNED` | Ubah persiapan, tambah/lepas baris | sama | `ProcurementPackage : Update` | — | 409 `PBJ-VAL-090` bila data basi |
| `DRAFT`, `PREPARATION_RETURNED` | Ajukan persiapan | `PREPARATION_PENDING_APPROVAL` | `ProcurementPackage : Submit` | HPS > 0 semua item, metode aktif, alasan bila di luar rentang, checklist wajib lengkap, definisi `PBJ-PKT-PERSIAPAN` aktif | 422 `PBJ-VAL-124`..`127`, 422 `PBJ-VAL-091` |
| `PREPARATION_PENDING_APPROVAL` | Tarik pengajuan | `DRAFT` | Pengaju, `ProcurementPackage : Submit` | Instance belum diputus | 409 `PBJ-VAL-128` |
| `PREPARATION_PENDING_APPROVAL` | Disetujui `Wfl` | `READY_FOR_RFQ` | Penyetuju sesuai matriks (bukan pengaju) | — | — |
| `PREPARATION_PENDING_APPROVAL` | Dikembalikan `Wfl` | `PREPARATION_RETURNED` | Penyetuju | — | — |
| `PREPARATION_PENDING_APPROVAL` | Ditolak `Wfl` | `REJECTED` | Penyetuju | Baris PR dilepas | — |
| `READY_FOR_RFQ` | Kirim RFQ (bagian 2) | `RFQ_OPEN` | `ProcurementRfq : Send` | — | 409 `PBJ-VAL-129` bila dari status lain |
| `READY_FOR_RFQ`, `EVALUATION` | Buka ulang persiapan (ubah HPS/metode) | `DRAFT` | `ProcurementPackage : Revise` | Alasan wajib; RFQ terbuka dibatalkan; riwayat RFQ tetap | 422 `PBJ-VAL-148` |
| `RFQ_OPEN` | RFQ ditutup | `EVALUATION` | `ProcurementRfq : Close` | Batas waktu lewat, atau semua undangan sudah membalas | 409 `PBJ-VAL-128` |
| `RFQ_OPEN`, `EVALUATION` | RFQ dinyatakan gagal (`PBJ-DEC-075`) | `READY_FOR_RFQ` | `ProcurementRfq : Close` | Alasan wajib | 422 `PBJ-VAL-148` |
| `EVALUATION` | Ajukan penetapan (bagian 4) | `AWARD_PENDING_APPROVAL` | `ProcurementPackageAward : Submit` | Bagian 4 | Bagian 4 |
| `AWARD_PENDING_APPROVAL` | Penetapan ditarik / dikembalikan / ditolak / tidak dapat diterapkan | `EVALUATION` | Pengaju / penyetuju / sistem | — | — |
| `AWARD_PENDING_APPROVAL` | Penetapan disetujui dan diterapkan | `AWARDED` | Penyetuju; sistem menerapkan | Vendor masih layak | `NOT_APPLIED` (bagian 4) |
| `AWARDED` | Cabut penetapan **[USULAN `G-P09`]** | `EVALUATION` | `ProcurementPackageAward : Revoke` | Nomor PO belum dicatat; alasan wajib; komitmen dilepas | 409 `PBJ-VAL-146` |
| `AWARDED` | Catat nomor PO BLM | `ORDERED` | `ProcurementPackage : RecordOrder` | Nomor belum dipakai paket lain | 409 `PBJ-VAL-145` |
| `DRAFT`..`AWARDED` kecuali `*_PENDING_APPROVAL` | Batalkan paket | `CANCELLED` | `ProcurementPackage : Cancel` | Alasan wajib; tidak ada persetujuan yang sedang berjalan **[USULAN]**; baris PR dan komitmen dilepas | 409 `PBJ-VAL-128`, 422 `PBJ-VAL-148` |
| `DRAFT`, `PREPARATION_RETURNED`, `READY_FOR_RFQ`, `RFQ_OPEN`, `EVALUATION` | Pecah paket | sama (paket baru ikut status sumber) | `ProcurementPackage : Update` | Bukan semua item; tidak ada persetujuan berjalan; penetapan belum diajukan | 409 `PBJ-VAL-144` |
| `ORDERED`, `CANCELLED`, `REJECTED` | Apa pun | — | — | Status akhir | 409 `PBJ-VAL-128`; batal setelah PO 409 `PBJ-VAL-147` |

**Contoh tidak sah:** Andi menekan "Kirim RFQ" pada PKT-000012 yang masih `PREPARATION_PENDING_APPROVAL` → 409
`PBJ-VAL-129`. Andi mencoba membatalkan PKT-000012 yang sudah `ORDERED` (PO-2026-0412) → 409 `PBJ-VAL-147`.

**Pecah paket:** paket baru mewarisi status, salinan metode, `ApprovedHpsTotal` bagian item yang dipindah, evaluator, RFQ
(sebagai RFQ warisan berstatus sama), penawaran, dan hasil evaluasi baris yang dipindah (`PBJ-DEC-083`). Paket sumber
menghitung ulang `HpsTotal` dan `ApprovedHpsTotal` tanpa menghitung ulang metode.

## 2. RFQ (`PrcRfq`)

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Buat RFQ | `DRAFT` | `ProcurementRfq : Create` | Paket `READY_FOR_RFQ`; tidak ada RFQ `DRAFT`/`SENT` lain | 409 `PBJ-VAL-129`/`128` |
| `DRAFT` | Ubah undangan dan batas waktu | `DRAFT` | `ProcurementRfq : Update` | Vendor layak semua kategori; alasan bila Dibatasi | 422 `PBJ-VAL-130`/`131` |
| `DRAFT` | Kirim | `SENT` | `ProcurementRfq : Send` | ≥ 1 undangan; batas waktu di masa depan; nomor `RFQ` dialokasikan | 422 `PBJ-VAL-132` |
| `SENT` | Perpanjang batas waktu | `SENT` | `ProcurementRfq : Update` | **Sebelum** batas waktu lewat; alasan wajib; berlaku untuk semua undangan | 422 `PBJ-VAL-132`/`148` |
| `SENT` | Tutup | `CLOSED` | `ProcurementRfq : Close` | Batas waktu lewat atau semua undangan membalas | 409 `PBJ-VAL-128` |
| `SENT`, `CLOSED` | Nyatakan gagal | `FAILED` | `ProcurementRfq : Close` | Alasan wajib | 422 `PBJ-VAL-148` |
| `DRAFT`, `SENT` | Paket dibatalkan / persiapan dibuka ulang | `CANCELLED` | Sistem | — | — |
| `CLOSED`, `FAILED`, `CANCELLED` | Apa pun selain catat koreksi penawaran | — | — | Status akhir | 409 `PBJ-VAL-128` |

## 3. Penawaran (`PrcQuotation`)

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Catat penawaran | `RECORDED` | `ProcurementQuotation : Create` | RFQ `SENT`/`CLOSED`; vendor diundang; belum ada catatan vendor itu; `IsLate` dihitung | 422 `PBJ-VAL-133`, 409 `PBJ-VAL-134` |
| `RECORDED` | Ubah catatan | `RECORDED` | Pencatatnya, `ProcurementQuotation : Update` | Belum ada hasil evaluasi maupun negosiasi | 409 `PBJ-VAL-128` |
| `RECORDED` | Batalkan catatan salah | `VOID` | `ProcurementQuotation : Cancel` | Alasan wajib; tidak sedang dipakai penetapan hidup | 409 `PBJ-VAL-128`, 422 `PBJ-VAL-148` |
| `RECORDED` | Catat negosiasi | `RECORDED` | `ProcurementQuotation : Negotiate` | Sah (tidak terlambat, lengkap), *Lulus*; harga tidak naik | 422 `PBJ-VAL-137`/`138` |

**Sah** = `RECORDED`, `IsLate = false`, `IsComplete = true`. **Lulus** = setiap baris lulus setiap kriteria dari setiap
evaluator aktif; bila metode tidak mewajibkan evaluasi teknis, penawaran sah dianggap *Lulus*.

## 4. Penetapan (`PrcPackageAward`)

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Ajukan | `PENDING_APPROVAL` | `ProcurementPackageAward : Submit` | Paket `EVALUATION`; penawaran sah dan *Lulus*; jumlah penawaran sah ≥ minimum metode; ≤ HPS atau alasan; terendah atau alasan; vendor layak (Dibatasi: alasan); definisi `PBJ-PKT-PENETAPAN` aktif | 422 `PBJ-VAL-139`..`143`, `PBJ-VAL-131`, `PBJ-VAL-091` |
| `PENDING_APPROVAL` | Tarik | `WITHDRAWN` | Pengaju | Belum diputus | 409 `PBJ-VAL-128` |
| `PENDING_APPROVAL` | Dikembalikan `Wfl` | `RETURNED` | Penyetuju | — | — |
| `RETURNED` | Ajukan ulang (alasan/pilihan diubah) | `PENDING_APPROVAL` | Pengaju | Aturan pengajuan diperiksa ulang | Sama dengan Ajukan |
| `PENDING_APPROVAL` | Ditolak `Wfl` | `REJECTED` | Penyetuju | — | — |
| `PENDING_APPROVAL` | Disetujui, vendor masih layak | `APPROVED` | Penyetuju; sistem menerapkan | Komitmen dicatat per garis pagu | — |
| `PENDING_APPROVAL` | Disetujui, vendor sudah tidak layak **[USULAN `G-P08`]** | `NOT_APPLIED` | Sistem | Alasan tercatat; tanpa komitmen | — |
| `APPROVED` | Cabut **[USULAN `G-P09`]** | `REVOKED` | `ProcurementPackageAward : Revoke` | Paket belum `ORDERED`; alasan; komitmen dilepas | 409 `PBJ-VAL-146` |
| `WITHDRAWN`, `REJECTED`, `NOT_APPLIED`, `REVOKED` | Apa pun | — | — | Status akhir; penetapan baru dibuat sebagai baris baru | 409 `PBJ-VAL-128` |

**Contoh:** penetapan PT Farma Jaya disetujui Direktur pada 02-04-2027, tetapi izin PBF-nya kedaluwarsa 31-03-2027. Sistem
mencatat award `NOT_APPLIED` "Persetujuan tidak dapat diterapkan: vendor tidak layak — Izin PBF kedaluwarsa", paket kembali
`EVALUATION`, dan tidak ada komitmen. Setelah izin baru diverifikasi, Andi mengajukan penetapan baru untuk penawaran yang sama.

## 5. Baris PR (`perencanaan-permintaan`)

| Dari | Tindakan | Ke | Pemicu |
| --- | --- | --- | --- |
| Baris tanpa paket, PR `APPROVED` | Masuk paket | Baris berpaket; PR tetap `APPROVED` sampai semua baris beli berpaket, lalu `CONVERTED_TO_PACKAGE` | `MarkLinesPackagedAsync` |
| Baris berpaket | Paket batal/ditolak, atau baris dilepas dari paket `DRAFT` | Baris tanpa paket; PR kembali `APPROVED` bila sebelumnya `CONVERTED_TO_PACKAGE` | `ReleaseLinesAsync` |
| Baris berpaket | Paket dipecah | Baris pindah paket (`PackageId` baru) | `MarkLinesPackagedAsync` dengan paket baru |
