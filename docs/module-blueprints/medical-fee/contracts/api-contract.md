# Medical Fee — Kontrak API

| Field | Nilai |
|---|---|
| Kontrak | `MDF-API-1.0` — `locked` 20 September 2026 |
| Blueprint ID | `MF-BP-001` |
| Route dasar | `api/v1/health-services/medical-fee-management` |
| Pembungkus | `ApiResponse<T>` untuk tunggal, `ApiResponse<PagedResult<T>>` untuk daftar |

Route memakai bentuk **hyphenated**, mengikuti koreksi yang sama dengan `FIN-API-0.2`
(`api/v1/corporate/finance-management/...`). Tidak ada route tanpa tanda hubung.

---

## 1. Aturan yang berlaku untuk seluruh endpoint

| Aspek | Ketentuan |
|---|---|
| Autentikasi | JWT Bearer, wajib pada seluruh endpoint |
| Otorisasi | `[AccessPermission("<Resource>", "<Action>")]` — lihat `permission-audit-matrix.md` |
| Bentuk sukses | `ApiResponse<T>.Ok(data, message)` |
| Bentuk gagal | `ApiResponse<T>.Fail(code, message)` |
| Paging | `?page=1&pageSize=25`, maksimum `pageSize` 200 |
| Urutan bawaan | Terbaru lebih dulu berdasarkan `CreateDateTime` |
| Soft delete | Seluruh query mengecualikan `IsDelete = true` |
| Concurrency | Perintah pengubah membawa `expectedRowVersion` di body; tidak cocok → `409` |
| Idempotensi | Perintah pengubah nilai uang membawa header `Idempotency-Key: <Guid>` (`MDF-DES-006`) |
| Waktu | Seluruh `timestamptz` dikirim sebagai ISO 8601 dengan offset |

**Kode HTTP yang dipakai**

| Kode | Kapan |
|---|---|
| `200` | Berhasil |
| `201` | Sumber daya baru dibuat |
| `400` | Bentuk permintaan salah |
| `401` / `403` | Tidak terautentikasi / tidak berwenang |
| `404` | Tidak ditemukan atau sudah dihapus |
| `409` | Bentrok concurrency, atau melanggar keunikan |
| `422` | Melanggar aturan bisnis — termasuk maker-checker dan transisi status terlarang |

---

## 2. Data induk — peran

| Method | Route | Fungsi |
|---|---|---|
| `GET` | `/master-data/fee-roles` | Daftar peran, filter `isActive`, `search` |
| `GET` | `/master-data/fee-roles/{id}` | Satu peran |
| `POST` | `/master-data/fee-roles` | Tambah peran |
| `PUT` | `/master-data/fee-roles/{id}` | Ubah peran |
| `PATCH` | `/master-data/fee-roles/{id}/activation` | Aktif / nonaktif |
| `DELETE` | `/master-data/fee-roles/{id}` | Soft delete — ditolak `422` bila sudah dipakai baris tarif |

**`POST /master-data/fee-roles`**

```json
{
  "roleCode": "OPERATOR",
  "roleName": "Operator Utama",
  "oprTeamRoleMapping": "PrimarySurgeon",
  "isPrimaryRole": true,
  "description": "Pelaksana utama satu tindakan"
}
```

`409` bila `roleCode` sudah ada, atau bila `oprTeamRoleMapping` sudah dipetakan peran lain.

---

## 3. Kesepakatan tarif sharing

| Method | Route | Fungsi |
|---|---|---|
| `GET` | `/master-data/sharing-agreements` | Daftar, filter `payeeType`, `payeeReferenceId`, `status`, `effectiveOn` |
| `GET` | `/master-data/sharing-agreements/{id}` | Satu kesepakatan beserta baris tarifnya |
| `POST` | `/master-data/sharing-agreements` | Susun kesepakatan baru |
| `PUT` | `/master-data/sharing-agreements/{id}` | Ubah kepala kesepakatan |
| `POST` | `/master-data/sharing-agreements/{id}/activation` | Aktifkan — memvalidasi kontrak HR dan kelengkapan baris tarif |
| `POST` | `/master-data/sharing-agreements/{id}/termination` | Hentikan, dengan alasan |
| `GET` | `/master-data/sharing-agreements/{id}/rules` | Baris tarif, filter `effectiveOn` |
| `POST` | `/master-data/sharing-agreements/{id}/rules` | Tambah baris tarif |
| `POST` | `/master-data/sharing-agreements/{id}/rules/{ruleId}/supersede` | Ganti baris tarif dengan versi baru (`MDF-DES-009`) |

**`POST /master-data/sharing-agreements`**

```json
{
  "agreementNumber": "PKS-2026-0142",
  "sourceContractHistoryId": "0f1c2d3e-...",
  "payeeType": "Doctor",
  "payeeReferenceId": "9a8b7c6d-...",
  "effectiveStart": "2026-10-01",
  "effectiveEnd": null,
  "notes": "Tarif sharing mengikuti PKS perpanjangan ke-2",
  "rules": [
    { "tariffId": null, "tariffCategoryId": null, "roleId": "...", "sharingPercentage": 40.00, "effectiveStart": "2026-10-01" },
    { "tariffId": null, "tariffCategoryId": "cat-bedah", "roleId": "...", "sharingPercentage": 55.00, "effectiveStart": "2026-10-01" }
  ]
}
```

**`POST /.../rules/{ruleId}/supersede`**

```json
{
  "sharingPercentage": 45.00,
  "effectiveStart": "2026-11-01",
  "reason": "Penyesuaian tarif hasil negosiasi",
  "expectedRowVersion": "..."
}
```

Menutup baris lama pada `effectiveStart − 1 hari`, membuat baris baru, dan mengisi
`SupersededByRuleId` baris lama. **Tidak pernah** mengubah persentase baris lama.

---

## 4. Periode

| Method | Route | Fungsi |
|---|---|---|
| `GET` | `/fee-periods` | Daftar periode, filter `status`, `year` |
| `GET` | `/fee-periods/{id}` | Satu periode beserta ringkasan angkanya |
| `POST` | `/fee-periods` | Buka periode |
| `POST` | `/fee-periods/{id}/calculation` | **Jalankan perhitungan.** `Idempotency-Key` wajib |
| `POST` | `/fee-periods/{id}/reopen` | Buka kembali dari `Calculated` |
| `POST` | `/fee-periods/{id}/verification` | Verifikasi |
| `POST` | `/fee-periods/{id}/return` | Kembalikan dari `Verified` |
| `POST` | `/fee-periods/{id}/approval` | Setujui — **membuat seluruh handoff ke Finance** |
| `POST` | `/fee-periods/{id}/closure` | Tutup — ditolak `422` bila masih ada layanan belum terhitung |
| `GET` | `/fee-periods/{id}/summary` | Ringkasan: jumlah penerima, total kotor, jumlah yang belum terhitung |

**`POST /fee-periods/{id}/calculation`**

Header: `Idempotency-Key: 3f2a...`

```json
{ "expectedRowVersion": "..." }
```

Balasan `200`:

```json
{
  "success": true,
  "message": "Perhitungan periode 2026-09 selesai",
  "data": {
    "periodId": "...",
    "periodCode": "2026-09",
    "status": "Calculated",
    "calculatedAt": "2026-10-01T08:14:22+07:00",
    "payeeCount": 218,
    "detailCount": 15842,
    "grossTotal": 2841500000.00,
    "unresolvedCount": 37,
    "unresolvedByReason": {
      "PERFORMER_MISSING": 29,
      "RULE_MISSING": 6,
      "CONTRACT_EXPIRED": 2
    }
  }
}
```

`unresolvedByReason` ada di balasan dengan sengaja: petugas langsung tahu apa yang perlu
dilengkapi tanpa membuka layar lain.

**`POST /fee-periods/{id}/closure`** — `422` bila tertahan:

```json
{
  "success": false,
  "code": "MDF_PERIOD_HAS_UNRESOLVED",
  "message": "Periode tidak dapat ditutup: 37 layanan belum dapat dihitung jasanya"
}
```

---

## 5. Hasil jasa

| Method | Route | Fungsi |
|---|---|---|
| `GET` | `/service-fees` | Daftar, filter `periodId`, `payeeType`, `payeeReferenceId`, `status` |
| `GET` | `/service-fees/{id}` | Satu hasil jasa |
| `GET` | `/service-fees/{id}/details` | Rincian per layanan — **inilah jawaban "dari mana angkanya"** |
| `POST` | `/service-fees/{id}/verification` | Verifikasi satu hasil jasa |
| `POST` | `/service-fees/{id}/return` | Kembalikan ke `Calculated` |
| `GET` | `/service-fees/{id}/adjustments` | Daftar koreksi |
| `POST` | `/service-fees/{id}/adjustments` | Ajukan koreksi. `Idempotency-Key` wajib |
| `POST` | `/service-fees/{id}/adjustments/{adjId}/approval` | Setujui koreksi |
| `POST` | `/service-fees/{id}/adjustments/{adjId}/rejection` | Tolak koreksi, dengan alasan |

**`GET /service-fees/{id}/details`** — satu baris balasan:

```json
{
  "id": "...",
  "sourceDomain": "OPERATING_ROOM",
  "sourceDetailId": "opr-case-8812/team-3",
  "invoiceItemId": "...",
  "serviceDate": "2026-09-14",
  "tariffName": "Laparotomi Eksplorasi",
  "roleCode": "ASSISTANT",
  "roleName": "Asisten Operator",
  "baseAmount": 8500000.00,
  "sharingPercentage": 20.00,
  "calculatedAmount": 1700000.00,
  "sharingRuleId": "...",
  "agreementNumber": "PKS-2026-0142"
}
```

`sharingRuleId` dan `agreementNumber` dikembalikan supaya setiap baris dapat ditelusuri ke
kesepakatan asalnya tanpa tebakan (`MDF-DES-010`).

**`POST /service-fees/{id}/adjustments`**

```json
{
  "direction": "Deduction",
  "amount": 250000.00,
  "reason": "Koreksi tindakan 14 Sept yang dibatalkan setelah periode dihitung"
}
```

`422` `MDF_MAKER_CHECKER_VIOLATION` bila penyetuju sama dengan pengaju.

---

## 6. Layanan yang belum dapat dihitung

| Method | Route | Fungsi |
|---|---|---|
| `GET` | `/unresolved-services` | Daftar, filter `periodId`, `reason`, `status`, `sourceDomain` |
| `GET` | `/unresolved-services/{id}` | Satu baris beserta penjelasannya |
| `POST` | `/unresolved-services/{id}/waiver` | Kesampingkan, dengan catatan wajib |
| `GET` | `/unresolved-services/summary` | Rekap per alasan dan per modul sumber |

Tidak ada endpoint yang **menghapus** baris ini. Satu-satunya cara ia berhenti menahan periode
adalah data sumbernya dilengkapi lalu periode dihitung ulang, atau dikesampingkan dengan catatan
yang tercatat.

---

## 7. Penyerahan ke Finance

| Method | Route | Fungsi |
|---|---|---|
| `GET` | `/finance-handoffs` | Daftar, filter `status`, `periodCode` |
| `GET` | `/finance-handoffs/{id}` | Satu penyerahan |
| `POST` | `/finance-handoffs/{id}/acknowledgement` | **Dipanggil Finance** untuk mengonfirmasi |
| `POST` | `/finance-handoffs/{id}/failure` | **Dipanggil Finance** untuk menolak, dengan alasan |
| `POST` | `/finance-handoffs/{id}/resend` | Kirim ulang yang gagal; `HandoffKey` tidak berubah |

Tidak ada `POST /finance-handoffs`. Penyerahan **hanya** lahir dari persetujuan hasil jasa —
tidak pernah dibuat manual.

---

## 8. Kode kesalahan bisnis

| Kode | HTTP | Arti |
|---|---:|---|
| `MDF_ROLE_CODE_DUPLICATE` | 409 | Kode peran sudah dipakai |
| `MDF_ROLE_MAPPING_DUPLICATE` | 409 | Satu peran kamar operasi dipetakan dua kali |
| `MDF_ROLE_IN_USE` | 422 | Peran masih dipakai baris tarif |
| `MDF_AGREEMENT_CONTRACT_MISMATCH` | 422 | Masa berlaku kesepakatan di luar masa kontrak HR |
| `MDF_AGREEMENT_NO_RULES` | 422 | Kesepakatan diaktifkan tanpa baris tarif |
| `MDF_RULE_OVERLAP` | 422 | Dua baris tarif tumpang-tindih untuk lingkup dan peran yang sama |
| `MDF_RULE_PERCENTAGE_EXCEEDED` | 422 | Jumlah persentase seluruh peran melebihi 100% |
| `MDF_PERIOD_OVERLAP` | 409 | Rentang periode tumpang-tindih |
| `MDF_PERIOD_STATE_INVALID` | 422 | Transisi status tidak diizinkan |
| `MDF_PERIOD_HAS_UNRESOLVED` | 422 | Penutupan tertahan layanan yang belum terhitung |
| `MDF_FEE_LOCKED` | 422 | Hasil jasa sudah `HandedOff`, hanya bisa dikoreksi |
| `MDF_MAKER_CHECKER_VIOLATION` | 422 | Pengaju dan penyetuju orang yang sama |
| `MDF_ADJUSTMENT_STATE_INVALID` | 422 | Koreksi sudah final |
| `MDF_WAIVER_NOTE_REQUIRED` | 422 | Pengesampingan tanpa catatan |
| `MDF_HANDOFF_ALREADY_EXISTS` | 409 | Hasil jasa sudah punya penyerahan |
| `MDF_ROWVERSION_MISMATCH` | 409 | Data berubah oleh pengguna lain |

---

## 9. Yang sengaja tidak ada di kontrak ini

| Endpoint yang ditolak | Alasan |
|---|---|
| `POST /service-fees` manual | Hasil jasa **hanya** lahir dari perhitungan periode. Entri manual akan melubangi seluruh jejak audit |
| `PUT /service-fees/{id}` untuk mengubah nilai | Nilai hanya berubah lewat perhitungan ulang atau koreksi berjenjang |
| `DELETE /unresolved-services/{id}` | Menghapus berarti menyembunyikan — persis yang `MF-DEC-018` cegah |
| Endpoint penarikan `DoctorShare` ke Billing | `OPEN DECISION`, menunggu `MF-CQ-08`. Bentuknya ada di `integration-contract.md` |
| Endpoint apa pun untuk radiologi | Ditunda `MF-DEC-015` |
| Endpoint potongan pajak atau kasbon | Milik Finance (`MF-DEC-005`) |
