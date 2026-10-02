# Laporan Perubahan Backend — `BE-ACC-P2-025`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-025` |
| Judul | Coba ulang manual dan abaikan |
| Slice | `P2-2` — Wave B |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-025` (revisi 4, `APPROVED`) |
| Trace | `FR-P2-010`, `011`, `016`, `017`; `ACC-DEC-046`, `049`, `075`, `078`, `092` |
| Kontrak | `ACC-API-0.12` — `POST /{id}/retry` (`AccountingEvent : Retry`), `PATCH /{id}/ignore` (`AccountingEvent : Ignore`); `ACC-STATE-0.4` |
| Dependency | `BE-ACC-P2-021` ✅ |
| Urutan | Dimajukan atas permintaan Rizki 24 September 2026 supaya rincian kejadian dapat diuji lewat layar (`FE-ACC-P2-012`) |
| Commit backend saat dikerjakan | `8535dd56` (branch `rizkiG`), belum di-commit — bertumpuk dengan `021` dan `024` |
| Tanggal | 24 September 2026 |
| Status | **✅ SELESAI — 28 September 2026.** 4 dari 4 acceptance di source dan terbukti dari **response Swagger mentah Rizki** (25 dan 28 September 2026): (1) Coba Ulang Tertahan (`EVT-UJI-113`) dan Gagal (`EVT-UJI-112`); (2) `EVT-UJI-104` berjenis belum terdaftar dipasangkan ke `UJI-BELUM-TERDAFTAR` yang baru didaftarkan lalu terjurnal `JU/2026/09/00008`; (3) Abaikan Gagal beralasan (`EVT-UJI-105`), alasan kosong → `400`, status Terjurnal → `409`; (4) Tertahan → `409`. Build Rizki berhasil, 222 warning. Bukti 24 September 2026 dari laporan agen AI dicabut (bagian 6.3). UAT belum dijalankan — diserahkan ke tim UAT. Riwayat: ✅ 24 September, 🟡 28 September pagi |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `AccountingEvent` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001` |
| Standar endpoint | Transaksi — perpindahan status lewat aksi (`POST /{id}/retry`, `PATCH /{id}/ignore` sesuai kontrak), bukan `PATCH /status` generik |

## 1. Endpoint

#### Corporate - Accounting - Accounting Event

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/retry` | Memproses ulang kejadian `Gagal` atau `Tertahan` memakai jenis dan aturan posting terkini | `AccountingEvent : Retry` | — | `ApiResponse<AccountingEventDetailDto>` |
| `PATCH` | `/{id}/ignore` | Menandai kejadian `Gagal` sebagai `Diabaikan`, alasan wajib | `AccountingEvent : Ignore` | `IgnoreAccountingEventRequest { Reason }` | `ApiResponse<AccountingEventDetailDto>` |

Kode status: `200`; `400` alasan kosong atau lebih dari 500 karakter; `404`; `409` status tidak
sesuai (termasuk `Tertahan` → Abaikan dengan pesan yang menjelaskan jalan keluarnya) atau status
berubah bersamaan.

## 2. Pemetaan acceptance

| # | Acceptance | Bukti | Status |
| ---: | --- | --- | :---: |
| 1 | Coba ulang `Gagal`/`Tertahan` memakai aturan terkini | `CobaUlangAsync` → `ProsesKejadianAsync(id, statusAsal, nomorTerakhir + 1, …)` — jalur yang sama dengan `021`. **Uji:** `EVT-UJI-113` Tertahan → Terjurnal sesudah aturan dibuat; `EVT-UJI-112` Gagal → Terjurnal sesudah periode dibangkitkan (bagian 6.1) | ✅ |
| 2 | `Tertahan` berjenis belum terdaftar dipasangkan ke jenis berkode sama | `ProsesKejadianAsync` mencari jenis aktif berdasarkan `EventTypeCode` tersimpan, lalu mengisi `EventTypeId`. **Uji:** `EVT-UJI-104` (`EVENT_TYPE_NOT_REGISTERED`) → jenis `UJI-BELUM-TERDAFTAR` didaftarkan dengan `accountingEventCount: 0` → Coba Ulang → Terjurnal `JU/2026/09/00008` → jenis yang sama kini `accountingEventCount: 1` (bagian 6.2, 025-4) | ✅ |
| 3 | Abaikan hanya `Gagal`, alasan wajib | `AbaikanAsync` — `400` alasan kosong, `409` status lain; ubah status bersyarat `WHERE EventStatus = Gagal`. **Uji:** `EVT-UJI-105` Gagal → Diabaikan beralasan (bagian 6.1); `EVT-UJI-101` Terjurnal → `409`; alasan kosong → `400` (bagian 6.2, 025-2 dan 025-3) | ✅ |
| 4 | `Tertahan` → `Diabaikan` ditolak | `409` dengan pesan "Kejadian Tertahan menunggu aturan posting dan tidak boleh diabaikan…". **Uji:** `EVT-UJI-104` Tertahan → `409` pesan persis (bagian 6.2, 025-1) | ✅ |

## 3. Keputusan implementasi

| Hal | Pilihan | Alasan |
| --- | --- | --- |
| Nomor percobaan | Nomor terbesar + 1 | Unique `(AccountingEventId, AttemptNumber)` |
| `AttemptCount` | Tidak dinaikkan oleh coba ulang manual | Menurut bagian 22.5, `AttemptCount` menghitung coba ulang penjadwal saja |
| Coba ulang `Gagal` yang ternyata aturan posting-nya hilang | Menjadi `Tertahan` | Keadaan sebenarnya kejadian itu; **delta** — `ACC-STATE-0.4` tidak menulis `Gagal` → `Tertahan` |
| Pelaku jurnal | `SystemActorUserId` bila diisi, bila tidak pengguna yang menekan | Sama dengan `021` |
| Log | `EntityId`, kode HTTP, status, alasan tahan, nomor jurnal — **alasan abaikan tidak dilog** (teks bebas) | `QBE-LOG-001`, `NFR-004` |

## 4. Berkas

| Berkas | Status |
| --- | --- |
| `AccountingEvent/Controllers/AccountingEventController.cs` | Diperbarui — `Retry`, `Ignore` |
| `AccountingEvent/Services/AccAccountingEventService.cs` | Diperbarui — `CobaUlangAsync`, `AbaikanAsync` |
| `AccountingEvent/DTOs/AccountingEventDtos.cs` | Diperbarui — `IgnoreAccountingEventRequest` |

## 5. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build` | **Berhasil** — Rizki, 24 September 2026, 0 error, 222 warning |
| Uji | **Lulus** — response Swagger mentah Rizki 25 dan 28 September 2026, bagian 6.1 dan 6.2 |

## 6. Bukti uji

### 6.1 Terbukti — response Swagger mentah, Rizki, 25 September 2026

| Jalur | Bukti | Acceptance |
| --- | --- | --- |
| Coba Ulang kejadian **Tertahan** memakai aturan terkini | `EVT-UJI-113` Tertahan `POSTING_RULE_MISSING`; sesudah aturan `UJI-026-A` dibuat, `POST /{id}/retry` → `200` "berhasil dijurnal sebagai JU/2026/09/00007", `journalStatus: Posted` | (1) |
| Coba Ulang kejadian **Gagal** | `EVT-UJI-112` Gagal oleh penjadwal; sesudah periode 2031 dibangkitkan, `POST /{id}/retry` → `200` "berhasil dijurnal sebagai JU/2031/01/00001" (percobaan #5) | (1) |
| Abaikan kejadian **Gagal** dengan alasan | `EVT-UJI-105`: `eventStatus: 5`, `ignoreReason: "Membersihkan data uji"`; sebelumnya Gagal oleh penjadwal (`attemptCount: 3`) | (3) jalur berhasil |

Rinciannya di [laporan `BE-ACC-P2-023`](BE-ACC-P2-023.md) bagian 8.1 dan
[laporan `BE-ACC-P2-026`](BE-ACC-P2-026.md) bagian 8. Nol SQL langsung. UAT belum dijalankan.

### 6.2 Skenario Swagger — dijalankan Rizki 28 September 2026, semuanya lulus

Semua URL diawali `/api/v1/corporate/accounting`. Kirim balik response body mentah setiap langkah.
Jalankan **berurutan**: 025-1 sampai 025-3 memakai `EVT-UJI-104` selagi masih Tertahan, sebelum 025-4
mengubahnya menjadi Terjurnal.

**025-1 — Tertahan tidak boleh diabaikan (acceptance 4)**
`PATCH /accounting-events/1db62e3f-76db-4f8c-965e-fd4af0acf72e/ignore` (`EVT-UJI-104`, Tertahan)
```json
{ "reason": "Uji tolak abaikan kejadian Tertahan" }
```
Diharapkan: `409` "Kejadian Tertahan menunggu aturan posting dan tidak boleh diabaikan. Lengkapi aturannya lalu coba ulang."

**025-2 — Status selain Gagal tidak boleh diabaikan (acceptance 3)**
`PATCH /accounting-events/3f0e128c-a91d-4a50-885e-7ca3767967a2/ignore` (`EVT-UJI-101`, Terjurnal)
```json
{ "reason": "Uji tolak abaikan kejadian Terjurnal" }
```
Diharapkan: `409` "Hanya kejadian Gagal yang dapat diabaikan; kejadian ini berstatus Terjurnal."

**025-3 — Alasan wajib (acceptance 3)**
`PATCH /accounting-events/1db62e3f-76db-4f8c-965e-fd4af0acf72e/ignore`
```json
{ "reason": "" }
```
Diharapkan: `400` "Alasan mengabaikan kejadian wajib diisi, maksimal 500 karakter." — alasan diperiksa lebih dulu daripada status.

**025-4 — Kejadian berjenis belum terdaftar dipasangkan ke jenis berkode sama (acceptance 2)**
`EVT-UJI-104` tertahan `EVENT_TYPE_NOT_REGISTERED` dengan kode `UJI-BELUM-TERDAFTAR` dan `eventTypeName: null`.

1. `POST /event-types`
   ```json
   { "eventTypeCode": "UJI-BELUM-TERDAFTAR", "eventTypeName": "Uji Belum Terdaftar", "sourceModule": "Finance", "eventKind": 1 }
   ```
   Diharapkan `201`; catat `data.id` sebagai `{typeId}`.
2. `POST /posting-rules` — akun sama dengan aturan `UJI-026-A`
   ```json
   {
     "legalEntityId": "3bf63974-a754-4b20-81ee-70894f6fb058",
     "eventTypeId": "{typeId}",
     "journalTypeId": "87796dbd-edf2-46c1-8f7c-0f6ff17ab2ae",
     "treatment": 1,
     "lines": [
       { "lineNumber": 1, "componentCode": "TOTAL", "accountId": "d81b4195-641b-458a-9284-1da568446ee8", "costCenterId": null, "side": 1, "description": "Uji 025 debit" },
       { "lineNumber": 2, "componentCode": "TOTAL", "accountId": "da1f4129-e36e-4882-90e6-b9fc14413db8", "costCenterId": null, "side": 2, "description": "Uji 025 kredit" }
     ]
   }
   ```
   Diharapkan `201`; catat `data.id` sebagai `{ruleId}`.
3. `POST /accounting-events/1db62e3f-76db-4f8c-965e-fd4af0acf72e/retry` (tanpa body)
   Diharapkan `200`, `eventStatus: 4`, `journalNumber` terisi, dan **`eventTypeName: "Uji Belum Terdaftar"`**
   — nama itu hanya muncul bila `EventTypeId` kejadian sudah dipasangkan ke jenis yang baru didaftarkan.
4. Bersihkan: `PATCH /posting-rules/{ruleId}/deactivate` lalu `PATCH /event-types/{typeId}/deactivate`, keduanya `200`.

Langkah 3 menambah satu jurnal Posted Rp 500.000 di September 2026 pada akun uji. Bila Rizki tidak
menginginkan jurnal itu, acceptance (2) dapat diterima lewat pembacaan source — keputusan Rizki.

### 6.3 Bukti yang dicabut

Bagian 6.1–6.3 versi 24 September 2026 disusun dari laporan agen AI penguji: penolakan `409` pada
`EVT-UJI-001` dan `EVT-UJI-002`, `EVT-UJI-001` Tertahan → Terjurnal, serta jalur Gagal pada
`EVT-UJI-023A`/`023B`. Pada 25 September 2026 `GET /accounting-events?Search=EVT-UJI-001` masih
menjawab Tertahan `EVENT_TYPE_NOT_REGISTERED` dengan `attemptCount: 0`, dan `EVT-UJI-023A`/`023B` tidak
ada di Kotak Masuk. Seluruh bukti versi itu **tidak dipakai**.

### 6.4 Hasil skenario 6.2 — response Swagger mentah, Rizki, 28 September 2026

| Skenario | Response sebenarnya | Acceptance | Hasil |
| --- | --- | --- | :---: |
| 025-1 `PATCH …/1db62e3f-…/ignore` (`EVT-UJI-104` Tertahan) | `409` "Kejadian Tertahan menunggu aturan posting dan tidak boleh diabaikan. Lengkapi aturannya lalu coba ulang." (09.08) | (4) | ✅ |
| 025-2 `PATCH …/3f0e128c-…/ignore` (`EVT-UJI-101` Terjurnal) | `409` "Hanya kejadian Gagal yang dapat diabaikan; kejadian ini berstatus Terjurnal." (09.09) | (3) | ✅ |
| 025-3 `PATCH …/1db62e3f-…/ignore` alasan `""` | `400` "Alasan mengabaikan kejadian wajib diisi, maksimal 500 karakter." (09.10) | (3) | ✅ |
| 025-4 langkah 1 `POST /event-types` `UJI-BELUM-TERDAFTAR` | `201`; `id 1041ae33-…`, `accountingEventCount: 0` | (2) | ✅ |
| 025-4 langkah 2 `POST /posting-rules` | `201` "Aturan posting untuk jenis kejadian UJI-BELUM-TERDAFTAR berhasil disimpan."; aturan `8ec42fe7-…` | (2) | ✅ |
| 025-4 langkah 3 `POST …/1db62e3f-…/retry` | `200` "Kejadian EVT-UJI-104 berhasil dijurnal sebagai JU/2026/09/00008.", `journalStatus: Posted`, percobaan #2 berhasil 09.15.02 | (1), (2) | ✅ |
| 025-4 langkah 4 `PATCH …/8ec42fe7-…/deactivate`, `PATCH /event-types/1041ae33-…/deactivate` | Keduanya `200`; jenis kini `isActive: false`, **`accountingEventCount: 1`** — kejadian sudah terhubung ke jenis ini | (2) | ✅ |

Bukti pemasangan jenis (acceptance 2) adalah perubahan `accountingEventCount` jenis `UJI-BELUM-TERDAFTAR`
dari `0` menjadi `1`: angka itu menghitung kejadian ber-`EventTypeId` jenis tersebut, sedangkan
`EVT-UJI-104` diterima tanpa `EventTypeId`. Nol SQL langsung. Data uji tambahan: jurnal Posted
`JU/2026/09/00008` Rp 500.000 di September 2026.
