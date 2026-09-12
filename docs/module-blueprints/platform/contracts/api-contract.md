# API Contract — Platform / Alokasi Nomor Bisnis

| Field | Nilai |
| --- | --- |
| Contract version | `v1` — ✅ **`approved`** `2026-09-09` |
| `last_changed_in` | `v1` |
| Owner | Pemilik kontrak engineering backend — `Andry` |
| `approved_by` / `approved_at` | **`Sukma Giri Pratama`** (`sukmagp`) / `2026-09-09` — approval blueprint `PLT-SLICE-01` |
| `input_revision` | `00-interview-decisions.md` revisi 3 |
| Traceability | `DEC-PLT-005`, `INV-PLT-001`, `INV-PLT-002`, `QBE-CODE-006` |

---

## 1. Permukaan yang paling penting bukan HTTP

Kemampuan inti slice ini — mengalokasikan nomor — **tidak dipaparkan lewat HTTP sama sekali**,
dan itu keputusan kontrak, bukan kelalaian.

Alokasi adalah panggilan dalam proses dari service yang memiliki catatannya:

```csharp
// Kontrak pemakaian (bukan endpoint)
Task<string> AllocateAsync(NumberAllocationRequest request, CancellationToken ct);
```

| Parameter | Tipe | Wajib | Keterangan |
| --- | --- | :---: | --- |
| `SequenceKey` | `string(50)` | Ya | Penanda deret milik modul pemanggil, contoh `BBK_BLOOD_ORDER` |
| `Prefix` | `string(15)` | Ya | Awalan nomor, ditetapkan modul pemanggil (`DEC-PLT-005`) |
| `ResetPolicy` | `string(20)` | Ya | `NEVER` untuk deret baru (`DEC-PLT-004`) |
| `SequenceDigits` | `int` | Ya | Antara `4` dan `12` |
| `ActorUserId` | `Guid` | Ya | Pelaku, disimpan sebagai audit pada baris deret |
| `Instant` | `DateTimeOffset` | Ya | Waktu acuan penghitungan periode |

**Kenapa tidak ada endpoint alokasi.** Nomor yang dapat diminta lewat HTTP dapat terbit tanpa
catatan yang menempel padanya. Itu membuat deret berlubang tanpa sebab yang dapat ditelusuri, dan
membuka jalan bagi nomor yang beredar di luar sistem. `INV-PLT-001` menuntut satu nomor menunjuk
satu catatan; cara paling tegas menjaminnya adalah membuat nomor hanya lahir di dalam pekerjaan
yang membuat catatannya.

---

## 2. Endpoint

#### Platform / Number Series Management / Number Series

Base URL: `api/v1/platform/number-series-management/number-series`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Konfigurasi penyaring dan pengurutan halaman pemantauan | `NumberSeries : Read` | — | `ApiResponse<NumberSeriesFilterMetadataResponse>` | Rencana (belum tersedia) |
| `GET` | `/summary` | Jumlah deret, jumlah scope, dan waktu alokasi terakhir | `NumberSeries : Read` | — | `ApiResponse<NumberSeriesSummaryResponse>` | Rencana (belum tersedia) |
| `GET` | `/` | Daftar deret beserta nilai pencacahnya | `NumberSeries : Read` | `NumberSeriesPagedQuery` | `ApiResponse<PagedResult<NumberSeriesResponse>>` | Rencana (belum tersedia) |
| `GET` | `/{id}` | Detail satu deret pada satu periode | `NumberSeries : Read` | — | `ApiResponse<NumberSeriesResponse>` | Rencana (belum tersedia) · `404` |

**Empat endpoint, seluruhnya baca.** Tidak ada `POST`, `PUT`, `PATCH`, maupun `DELETE`.

| Yang tidak ada | Alasan |
| --- | --- |
| `POST /` | Deret lahir sendiri pada alokasi pertama; tidak ada yang perlu membuatnya |
| `PUT /{id}` dan `PATCH /{id}` | Menyunting pencacah berarti menerbitkan ulang nomor yang sudah menempel pada catatan — melanggar `INV-PLT-001` |
| `DELETE /{id}` | Menghapus baris deret menghilangkan nilai tertinggi yang pernah terbit; alokasi berikutnya akan mengulang dari nol |
| `POST /{id}/reset` | `INV-PLT-002` menyatakan deret berlubang adalah keadaan sah. Menyetel ulang justru merusaknya |
| `GET /options` | Deret bukan isi kotak pilihan; ia bukan master data |

---

## 3. Bentuk response

```jsonc
// NumberSeriesResponse
{
  "id": "…",
  "sequenceKey": "BBK_BLOOD_ORDER",
  "scopeKey": "GLOBAL",
  "resetPolicy": "NEVER",
  "currentValue": 123,
  "lastAllocatedAt": "2026-09-09T08:15:00+07:00"
}
```

```jsonc
// NumberSeriesSummaryResponse
{
  "totalSeries": 3,          // jumlah SequenceKey berbeda
  "totalScope": 3,           // jumlah baris (deret × periode)
  "lastAllocatedAt": "2026-09-09T08:15:00+07:00"
}
```

Seluruh response dibungkus `ApiResponse<T>`, dan daftar memakai `PagedResult<T>`, mengikuti
kontrak response bersama repository.

---

## 4. Kompatibilitas

| Aspek | Dampak |
| --- | --- |
| Endpoint existing | **Nol** yang berubah, berganti nama, atau hilang |
| `BillingNumberSeriesService` | **Tidak disentuh.** Keempat method publiknya tetap seperti sekarang |
| Konsumen frontend existing | **Nol** terdampak — base URL ini baru sepenuhnya |
| Breaking change | **Tidak ada** |
