# Medical Fee — Matriks Validasi

| Field | Nilai |
|---|---|
| Kontrak | `MDF-VAL-1.0` — `locked` 20 September 2026 |
| Blueprint ID | `MF-BP-001` |

Kolom **Lapis** menyatakan di mana aturan ditegakkan: `DTO` (anotasi), `SVC` (service), `DB`
(check constraint atau index). Aturan yang menyangkut uang atau wewenang ditegakkan di
sekurang-kurangnya dua lapis.

---

## 1. Peran

| # | Aturan | Lapis | Kode gagal |
|---:|---|---|---|
| 1.1 | `roleCode` wajib, 2–30 karakter, huruf besar dan garis bawah | DTO | `400` |
| 1.2 | `roleName` wajib, maksimum 100 karakter | DTO | `400` |
| 1.3 | `roleCode` unik di antara baris yang belum dihapus | SVC + DB | `MDF_ROLE_CODE_DUPLICATE` |
| 1.4 | `oprTeamRoleMapping` bila diisi MUST salah satu nilai `OprTeamRole` yang ada | SVC | `400` |
| 1.5 | Satu nilai `OprTeamRole` dipetakan paling banyak satu peran | SVC + DB | `MDF_ROLE_MAPPING_DUPLICATE` |
| 1.6 | Peran yang dipakai baris tarif tidak dapat dihapus | SVC + DB (FK `Restrict`) | `MDF_ROLE_IN_USE` |
| 1.7 | Peran nonaktif tidak dapat dipilih pada baris tarif baru | SVC | `422` |

## 2. Kesepakatan tarif sharing

| # | Aturan | Lapis | Kode gagal |
|---:|---|---|---|
| 2.1 | `agreementNumber` wajib, unik | DTO + DB | `409` |
| 2.2 | `sourceContractHistoryId` wajib dan MUST menunjuk baris `WfpContractHistory` yang ada | DTO + DB (FK) | `400` / `404` |
| 2.3 | `payeeType` salah satu `Doctor`, `Workforce` | DTO + DB | `400` |
| 2.4 | `payeeReferenceId` MUST ada di tabel sesuai `payeeType` | SVC | `404` |
| 2.5 | `payeeReferenceId` MUST cocok dengan tenaga kerja pada kontrak yang ditunjuk | SVC | `422` |
| 2.6 | `effectiveStart` MUST NOT mendahului `StartDate` kontrak | SVC | `MDF_AGREEMENT_CONTRACT_MISMATCH` |
| 2.7 | `effectiveEnd` MUST NOT melewati `EndDate` kontrak bila kontrak punya akhir | SVC | `MDF_AGREEMENT_CONTRACT_MISMATCH` |
| 2.8 | `effectiveEnd` kosong atau `>= effectiveStart` | DTO + DB | `400` |
| 2.9 | Satu penerima MUST NOT punya dua kesepakatan `Active` yang masa berlakunya tumpang-tindih | SVC | `422` |
| 2.10 | Aktivasi menuntut sekurang-kurangnya satu baris tarif | SVC | `MDF_AGREEMENT_NO_RULES` |
| 2.11 | Penghentian menuntut alasan | DTO | `400` |

Aturan 2.5 yang paling mudah terlewat: tanpa itu, kesepakatan tarif seseorang bisa menunjuk
kontrak milik orang lain, dan angka jasanya tetap terhitung seolah sah.

## 3. Baris tarif

| # | Aturan | Lapis | Kode gagal |
|---:|---|---|---|
| 3.1 | `sharingPercentage` antara 0 dan 100, 2 desimal | DTO + DB | `400` |
| 3.2 | `roleId` wajib dan MUST menunjuk peran aktif | DTO + SVC | `400` / `422` |
| 3.3 | `tariffId` bila diisi MUST ada | DB (FK) | `400` |
| 3.4 | `tariffCategoryId` bila diisi MUST ada | DB (FK) | `400` |
| 3.5 | Dua baris dengan lingkup dan peran sama MUST NOT tumpang-tindih masa berlakunya | SVC | `MDF_RULE_OVERLAP` |
| 3.6 | Jumlah persentase seluruh peran pada satu lingkup dan satu tanggal MUST NOT melebihi 100% | SVC | `MDF_RULE_PERCENTAGE_EXCEEDED` |
| 3.7 | `effectiveStart` baris pengganti MUST setelah `effectiveStart` baris yang digantikan | SVC | `422` |
| 3.8 | Baris yang sudah dipakai rincian hasil jasa MUST NOT diubah nilainya | SVC | `422` |
| 3.9 | Masa berlaku baris tarif MUST berada di dalam masa berlaku kesepakatannya | SVC | `422` |

Aturan 3.6 memeriksa `<= 100`, bukan `= 100` — sisanya adalah porsi rumah sakit, dan itu keadaan
yang lazim.

## 4. Periode

| # | Aturan | Lapis | Kode gagal |
|---:|---|---|---|
| 4.1 | `periodCode` wajib, unik, berpola `YYYY-MM` | DTO + DB | `409` |
| 4.2 | `periodEnd >= periodStart` | DTO + DB | `400` |
| 4.3 | Rentang MUST NOT tumpang-tindih periode lain yang belum dihapus | SVC | `MDF_PERIOD_OVERLAP` |
| 4.4 | Transisi status MUST ada di `state-transition-matrix.md` | SVC | `MDF_PERIOD_STATE_INVALID` |
| 4.5 | `expectedRowVersion` MUST cocok | SVC | `MDF_ROWVERSION_MISMATCH` |
| 4.6 | Perhitungan MUST membawa `Idempotency-Key` | SVC | `400` |
| 4.7 | Verifikasi menuntut seluruh hasil jasa periode itu sudah `Verified` | SVC | `422` |
| 4.8 | Penyetuju MUST NOT sama dengan pemverifikasi | SVC + DB | `MDF_MAKER_CHECKER_VIOLATION` |
| 4.9 | Penutupan ditolak bila ada `MdfUnresolvedService` berstatus `Open` | SVC | `MDF_PERIOD_HAS_UNRESOLVED` |
| 4.10 | Perhitungan ulang ditolak bila status `Verified` atau lebih tinggi | SVC | `MDF_PERIOD_STATE_INVALID` |
| 4.11 | Periode `Closed` MUST NOT diubah oleh operasi apa pun | SVC | `422` |

## 5. Perhitungan

| # | Aturan | Lapis | Kode gagal / perlakuan |
|---:|---|---|---|
| 5.1 | Layanan tanpa pelaksana → `MdfUnresolvedService` `PERFORMER_MISSING` | SVC | Bukan kesalahan — dicatat |
| 5.2 | Penerima tanpa kesepakatan berlaku → `AGREEMENT_MISSING` | SVC | Dicatat |
| 5.3 | Tidak ada baris tarif yang cocok → `RULE_MISSING` | SVC | Dicatat |
| 5.4 | Tanggal layanan setelah kontrak berakhir → `CONTRACT_EXPIRED` | SVC | Dicatat |
| 5.5 | Peran sumber tidak terpetakan ke `MstMedicalFeeRole` → `ROLE_UNMAPPED` | SVC | Dicatat |
| 5.6 | `SourceDomain` yang belum didukung → `SOURCE_UNSUPPORTED` | SVC | Dicatat |
| 5.7 | Dua baris tarif cocok pada tingkat kekhususan yang sama untuk peran sama | SVC | `422` — **bukan** dicatat, ini cacat data |
| 5.8 | `BaseAmount` diambil dari `Quantity × UnitPrice`, sebelum diskon | SVC | — |
| 5.9 | `CalculatedAmount` dibulatkan 2 desimal `AwayFromZero` | SVC | — |
| 5.10 | `GrossAmount` MUST sama dengan `Σ CalculatedAmount` | SVC | Transaksi dibatalkan |
| 5.11 | `SharingRuleId` pada rincian MUST terisi | SVC + DB (`IsRequired`) | Transaksi dibatalkan |
| 5.12 | Satu layanan MUST NOT muncul dua kali di daftar belum terhitung pada periode yang sama | DB (unique) | `409` |

Perbedaan 5.7 dari 5.1–5.6 disengaja: enam yang pertama adalah **keadaan data yang wajar terjadi
dan perlu dilengkapi**; yang ketujuh adalah aturan tarif yang saling bertabrakan, dan
membiarkannya berarti memilih angka secara acak.

## 6. Hasil jasa dan koreksi

| # | Aturan | Lapis | Kode gagal |
|---:|---|---|---|
| 6.1 | `finalAmount = grossAmount + adjustmentAmount` | SVC + DB | Transaksi dibatalkan |
| 6.2 | `grossAmount >= 0` | DB | Transaksi dibatalkan |
| 6.3 | Satu penerima satu hasil jasa per periode | SVC + DB | `409` |
| 6.4 | Hasil jasa `HandedOff` MUST NOT diubah nilainya langsung | SVC | `MDF_FEE_LOCKED` |
| 6.5 | `direction` salah satu `Addition`, `Deduction` | DTO + DB | `400` |
| 6.6 | `amount` koreksi MUST > 0 | DTO + DB | `400` |
| 6.7 | `reason` koreksi wajib, 10–500 karakter | DTO | `400` |
| 6.8 | Penyetuju koreksi MUST NOT sama dengan pengaju | SVC + DB | `MDF_MAKER_CHECKER_VIOLATION` |
| 6.9 | Koreksi `Approved` atau `Rejected` MUST NOT diubah | SVC | `MDF_ADJUSTMENT_STATE_INVALID` |
| 6.10 | Penolakan menuntut `rejectionReason` | SVC | `400` |
| 6.11 | Koreksi `Deduction` MUST NOT membuat `finalAmount` negatif | SVC | `422` |
| 6.12 | Pengajuan koreksi membawa `Idempotency-Key` | SVC | `400` |

Aturan 6.11 adalah batas yang sengaja dipasang: potongan yang melebihi jasa bukan urusan modul
ini — potongan semacam itu ditangani Finance lewat `FinPaymentDeduction` (`MF-DEC-005`).

## 7. Layanan belum terhitung dan penyerahan

| # | Aturan | Lapis | Kode gagal |
|---:|---|---|---|
| 7.1 | Pengesampingan menuntut `resolutionNote` | SVC | `MDF_WAIVER_NOTE_REQUIRED` |
| 7.2 | Pengesampingan menuntut izin tersendiri, bukan izin verifikasi biasa | SVC | `403` |
| 7.3 | Baris `Resolved` atau `Waived` MUST NOT dikembalikan ke `Open` secara manual | SVC | `422` |
| 7.4 | Penyerahan hanya dibuat untuk hasil jasa `Approved` | SVC | `422` |
| 7.5 | Satu hasil jasa paling banyak satu penyerahan | SVC + DB | `MDF_HANDOFF_ALREADY_EXISTS` |
| 7.6 | `HandoffKey` unik | DB | `409` |
| 7.7 | Pengiriman ulang MUST NOT mengubah `HandoffKey` | SVC | — |
| 7.8 | ACK dari Finance MUST menyertakan `HandoffKey` yang cocok | SVC | `422` |
| 7.9 | Penolakan dari Finance menuntut `failureReason` | SVC | `400` |

## 8. Aturan lintas modul yang tidak divalidasi di sini

| Aturan | Milik siapa | Mengapa tidak di sini |
|---|---|---|
| `DoctorShare` tidak melebihi nilai kotor baris | Billing (`BillingInvoiceService.cs:1880-1881`) | Sudah berjalan; Medical Fee tidak menduplikasinya |
| Perlakuan diskon dokter terhadap bagian dokter | Billing | `MF-DEC-013` mengikuti perilaku Billing apa adanya |
| Masa berlaku dan status kontrak kerja | HR | Medical Fee membaca, tidak memvalidasi isinya |
| Potongan PPh 21, kasbon, iuran | Finance | `MF-DEC-005` |
| Jurnal atas beban jasa | Accounting | Dibuat dari sisi Finance |
