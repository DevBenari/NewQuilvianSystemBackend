# Medical Fee — Kontrak Integrasi

| Field | Nilai |
|---|---|
| Kontrak | `MDF-INTEGRATION-1.0` — `locked` 20 September 2026 |
| Blueprint ID | `MF-BP-001` |
| Kontrak tetangga | `BIL-INTEGRATION-0.4`, `FIN-INTEGRATION-0.2`, `BIL-CASH-001`, `FIN-BP-001` revisi 2 |

---

## 1. Ringkasan seluruh integrasi

| # | Mitra | Arah | Jenis | Status |
|---:|---|---|---|---|
| 1 | Operating Room | Masuk | Baca langsung | Siap — datanya lengkap |
| 2 | Clinical | Masuk | Baca langsung | Sebagian — menunggu `MF-CQ-07` untuk tim |
| 3 | Laboratory | Masuk | Baca langsung | Sebagian — menunggu `MF-CQ-07` |
| 4 | Radiology | — | — | **Ditunda** `MF-DEC-015` |
| 5 | Billing — baca nilai layanan | Masuk | Baca langsung | Siap |
| 6 | Billing — tulis `DoctorShare` | **Keluar** | `OPEN DECISION` | **Menunggu `MF-CQ-08`** |
| 7 | Billing — pelaksana entri kasir | Masuk | `OPEN DECISION` | **Menunggu `MF-CQ-05`** |
| 8 | HR — kontrak kerja | Masuk | Baca + FK | Siap |
| 9 | MasterData — tarif | Masuk | Baca + FK | Siap |
| 10 | Finance — penyerahan hasil jasa | Keluar | Tabel handoff | Siap; Finance sudah menyesuaikan |

Tujuh dari sepuluh siap. Tiga yang tertahan seluruhnya menyangkut Billing dan modul klinis.

---

## 2. Masuk — sumber layanan

Seluruhnya dibaca lewat `MedicalFeeServiceSourceAdapter`, satu-satunya tempat yang mengetahui
perbedaan antar sumber.

### 2.1 Operating Room — `OPERATING_ROOM`

| Aspek | Nilai |
|---|---|
| Tabel dibaca | `OprCase`, `OprTeamMember` |
| Pelaksana | `OprTeamMember.WorkforceId` |
| Peran | `OprTeamMember.Role` (`OprTeamRole`), dipetakan lewat `MstMedicalFeeRole.OprTeamRoleMapping` |
| Pelaksana utama | `OprTeamMember.IsLead` |
| `SourceDetailId` | `"<OprCaseId>/<OprTeamMemberId>"` |
| Dukungan tim | **Penuh** — satu tindakan menghasilkan beberapa rincian, satu per anggota tim |

Inilah satu-satunya sumber yang sudah mendukung pembagian tim tanpa perubahan apa pun.

### 2.2 Clinical — `PROCEDURE`

| Aspek | Nilai |
|---|---|
| Tabel dibaca | `TrxPatientProcedure` |
| Pelaksana | `TrxPatientProcedure.DoctorId` (wajib), `PerformedByUserId` (opsional) |
| Peran | **Tidak ada** — dianggap peran yang `IsPrimaryRole = true` |
| `SourceDetailId` | `"<TrxPatientProcedureId>"` |
| Dukungan tim | **Tidak** — satu pelaksana saja sampai `MF-CQ-07` turun |

Bila `MF-CQ-07` disetujui, `SourceDetailId` berubah menjadi `"<ProcedureId>/<TeamMemberId>"`
dan peran diambil dari pencatatan tim. Bentuk rincian tidak berubah.

### 2.3 Laboratory — `LABORATORY`

| Aspek | Nilai |
|---|---|
| Tabel dibaca | `LabOrder` |
| Pelaksana | `LabOrder.ExaminerDoctorId` — **boleh kosong** |
| Peran | Tidak ada — dianggap peran utama |
| `SourceDetailId` | `"<LabOrderId>"` |
| Bila pemeriksa kosong | `MdfUnresolvedService` `PERFORMER_MISSING` |

### 2.4 Radiology — tidak dibaca

`MF-DEC-015`. Modul Radiology tidak menyimpan pelaksana sama sekali. Selama ditunda, layanan
radiologi **tidak** masuk perhitungan dan **tidak** dicatat sebagai belum terhitung — ia
diabaikan sepenuhnya, karena mencatat ribuan baris `SOURCE_UNSUPPORTED` setiap bulan hanya akan
menenggelamkan daftar yang benar-benar perlu ditindaklanjuti.

`SOURCE_UNSUPPORTED` disediakan pada kamus data untuk kelak, bukan untuk revisi ini.

---

## 3. Masuk — Billing, nilai layanan

| Aspek | Nilai |
|---|---|
| Tabel dibaca | `BilInvoiceItem`, `BilDiscountApplication` |
| Penaut ke layanan | `BilInvoiceItem.SourceDomain` + `SourceDetailId` — pola yang sudah ada |
| Nilai yang dipakai | `Quantity × UnitPrice` — **kotor, sebelum diskon** (`MDF-DES-012`) |
| Diskon | **Tidak dikurangkan** oleh Medical Fee |
| Tagihan yang dibaca | Hanya yang sudah difinalisasi pada rentang periode |

Perlakuan diskon sengaja mengikuti perilaku Billing apa adanya (`MF-DEC-013`): audit menemukan
diskon promo tidak menyentuh `DoctorShare`, dan hanya diskon bertipe dokter yang menguranginya.
Medical Fee **tidak mengubah** perilaku itu, dan tidak mengurangkan diskon apa pun dari basis
perhitungannya.

Konsekuensi yang diterima dan sudah disampaikan ke owner Billing: nilai jasa yang dihitung
Medical Fee dapat berbeda dari `DoctorShare` setelah diskon dokter. Selisihnya bukan kekeliruan
— keduanya menjawab pertanyaan berbeda.

---

## 4. Keluar — Billing, sumber `DoctorShare`

> **`OPEN DECISION` — `MF-CQ-08`.** Yang tertulis di bawah adalah **bentuk yang diusulkan**,
> bukan kesepakatan. Ia MUST NOT diimplementasikan sebelum owner Billing menjawab.

### 4.1 Keadaan sekarang

| Fakta | Bukti |
|---|---|
| `DoctorShare` diketik petugas | `BillingInvoiceService.cs:1855` |
| Satu-satunya batas: tidak melebihi nilai kotor baris | `BillingInvoiceService.cs:1880-1881` |
| Nilai itu mengalir ke Finance saat finalisasi | `BillingArApHandoffService.cs:97-105` |

### 4.2 Tiga bentuk yang ditawarkan

| Bentuk | Cara kerja | Untung | Rugi |
|---|---|---|---|
| A — Billing menarik | Saat item dicatat, Billing memanggil Medical Fee untuk menanyakan persentase yang berlaku | Billing tetap pemilik tulis; nilai terisi seketika | Billing bergantung pada ketersediaan Medical Fee saat input |
| B — Medical Fee mendorong | Medical Fee memanggil Billing saat perhitungan selesai | Billing tidak terganggu saat input | `DoctorShare` kosong sampai periode dihitung |
| C — Billing membaca tabel aturan | Billing membaca `MdfSharingRule` langsung | Tanpa panggilan antar service | Aturan pemilihan baris tarif terduplikasi di dua modul |

Rekomendasi Medical Fee adalah **A**, karena ia mempertahankan Billing sebagai satu-satunya
penulis `BilInvoiceItem` dan tetap membuat nilainya terisi saat petugas menginput. Bentuk C
ditolak karena menduplikasi logika pemilihan baris tarif — tempat kekeliruan paling mudah
tumbuh diam-diam.

Keputusannya milik owner Billing, bukan Medical Fee.

### 4.3 Bentuk panggilan untuk bentuk A

```text
GET  api/v1/health-services/medical-fee-management/sharing-rules/resolve
     ?payeeType=Doctor&payeeReferenceId=...&tariffId=...&serviceDate=2026-09-14&roleCode=OPERATOR
```

```json
{
  "success": true,
  "data": {
    "resolved": true,
    "sharingPercentage": 40.00,
    "sharingRuleId": "...",
    "agreementNumber": "PKS-2026-0142"
  }
}
```

`resolved: false` berarti tidak ada aturan yang berlaku. **Perlakuan untuk kasus itu adalah
pertanyaan ketiga kepada owner Billing** (`evidence/01-permintaan-untuk-owner-billing.md`
bagian 2.4): nol, kosong, atau tetap boleh diketik.

## 5. Masuk — Billing, pelaksana entri kasir

> **`OPEN DECISION` — `MF-CQ-05`.**

Yang dibutuhkan pada entri `ADHOC` dan `ADHOC_CATALOG`:

| Bidang | Tipe | Wajib | Kegunaan |
|---|---|:---:|---|
| Rujukan tenaga medis pelaksana | `Guid` | Bila layanan berjasa | Penerima jasa |
| Peran pelaksana | `string` | Opsional | Kosong → peran utama |

Nama dan bentuknya terserah Billing. Selama belum ada, seluruh entri `ADHOC` dan
`ADHOC_CATALOG` jatuh ke `MdfUnresolvedService` `PERFORMER_MISSING`.

---

## 6. Masuk — HR dan MasterData

| Sumber | Yang dibaca | FK | Aturan |
|---|---|:---:|---|
| `WfpContractHistory` | `WorkforceProfileId`, `StartDate`, `EndDate`, `ContractStatus`, `IsCurrent` | Ya, `Restrict` | Layanan setelah `EndDate` → `CONTRACT_EXPIRED` |
| `MstDoctor` | Identitas penerima bertipe `Doctor` | Tidak (polimorfik) | Divalidasi service |
| `MstTariff`, `MstTariffCategory` | Lingkup baris tarif | Ya, `Restrict` | — |

Medical Fee **tidak pernah menulis** ke satu pun tabel HR atau MasterData.

Satu pertanyaan terbuka yang perlu diketahui saat implementasi: apakah dokter tamu dan dokter
paruh waktu punya baris `WfpContractHistory`. Bila tidak, kelompok itu tidak akan punya
kesepakatan tarif yang sah, padahal justru merekalah yang tarifnya paling sering berbeda-beda.
Pertanyaan itu ada di `evidence/02-pemberitahuan-untuk-owner-hr.md` bagian 5 nomor 2.

---

## 7. Keluar — Finance

| Aspek | Nilai |
|---|---|
| Mekanisme | Tabel handoff `MdfFinanceHandoff` (`MDF-DES-018`) |
| Kapan ditulis | Di dalam transaksi yang sama dengan persetujuan hasil jasa |
| Kunci idempoten | `HandoffKey` (`Guid`) |
| Nilai | `MdfServiceFee.FinalAmount` — **kotor, sebelum potongan** (`MF-DEC-005`) |
| Yang mengonsumsi | Finance, membuat `FinMedicalServicePayable` |
| Konfirmasi | Finance memanggil `POST /finance-handoffs/{id}/acknowledgement` |
| Arah tulis | Medical Fee **tidak pernah** menulis ke tabel Finance |

### 7.1 Bentuk baris handoff

```json
{
  "handoffKey": "8f14e45f-...",
  "serviceFeeId": "...",
  "payeeType": "Doctor",
  "payeeReferenceId": "...",
  "periodCode": "2026-09",
  "grossAmount": 42750000.00,
  "status": "Created",
  "correlationId": "...",
  "createdAt": "2026-10-03T16:02:11+07:00"
}
```

`periodCode` dan `payeeType` disalin dengan sengaja: Finance dapat membaca satu baris tanpa
menyentuh tabel Medical Fee mana pun.

### 7.2 Pembagian tanggung jawab dengan Finance

| Tanggung jawab | Medical Fee | Finance |
|---|:---:|:---:|
| Menghitung jasa dari layanan | **Ya** | Tidak |
| Menentukan persentase sharing | **Ya** | Tidak |
| Menyimpan nilai kotor per penerima per periode | **Ya** | Tidak |
| Menghitung PPh 21 | Tidak | **Ya** (`FIN-DES-026`) |
| Kasbon, iuran, KSO, potongan lain | Tidak | **Ya** |
| Membuat utang jasa medis | Tidak | **Ya** (`FinMedicalServicePayable`, `FIN-DES-025`) |
| Membayar | Tidak | **Ya** |
| Jurnal | Tidak | **Ya** |

Finance sudah menyesuaikan diri lebih dulu: `FIN-DES-025`..`028` pada `FIN-BP-001` revisi 2
mengganti `FinDoctorPayable` menjadi `FinMedicalServicePayable` dan menambahkan
`FinPaymentDeduction`. Tidak ada perubahan yang diminta ke Finance oleh revisi ini.

---

## 8. Yang tidak ada dan memang tidak seharusnya ada

| Integrasi | Mengapa tidak |
|---|---|
| Medical Fee → Accounting | Jurnal dibuat dari sisi Finance saat utang terbit. Dua sumber jurnal untuk satu peristiwa akan menggandakan pencatatan |
| Medical Fee → Registration | Modul ini tidak menyentuh data pasien sama sekali |
| Medical Fee menulis ke tabel HR | Kontrak milik HR; hubungan satu arah |
| Medical Fee menulis ke tabel Finance | Finance mengonsumsi handoff, bukan ditulisi |
| Billing → Medical Fee untuk diskon | Diskon tidak mengurangi basis perhitungan (`MF-DEC-013`) |

---

## 9. Pemicu peninjauan kontrak ini

| Pemicu | Yang ditinjau |
|---|---|
| Owner Billing menjawab `MF-CQ-08` | Bagian 4 seluruhnya; naik ke `MDF-INTEGRATION-0.2` |
| Owner Billing menjawab `MF-CQ-05` | Bagian 5 |
| Clinical atau Laboratory menambahkan pencatatan tim | Bagian 2.2 dan 2.3 |
| Radiology menambahkan pencatatan pelaksana | Bagian 2.4; `MF-DEC-015` dapat ditinjau ulang |
| Billing mengubah perlakuan diskon dokter | Bagian 3 |
| HR mengubah `WfpContractHistory` | Bagian 6 |
| `FIN-BP-001` menyentuh `FinMedicalServicePayable` | Bagian 7 |
