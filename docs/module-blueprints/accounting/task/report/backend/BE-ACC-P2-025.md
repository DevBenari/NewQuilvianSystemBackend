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
| Status | **✅ SELESAI** — 24 September 2026. 4 dari 4 acceptance di source; build Rizki **berhasil, 0 error, 222 warning**; uji Rizki lulus: Swagger `ignore` ditolak `409` untuk `Tertahan` dan `Terjurnal`, serta Coba Ulang `Tertahan` → `Terjurnal` lewat layar Rincian (bagian 6). Jalur `Gagal` (Abaikan berhasil, Coba Ulang `Gagal`) dibuktikan lewat pembacaan source **atas keputusan Rizki** — belum ada kode yang menghasilkan status `Gagal` sebelum `BE-ACC-P2-023`. Riwayat: 🟡 pada hari yang sama |

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
| 1 | Coba ulang `Gagal`/`Tertahan` memakai aturan terkini | `CobaUlangAsync` → `ProsesKejadianAsync(id, statusAsal, nomorTerakhir + 1, …)` — jalur yang sama dengan `021` | ✅ |
| 2 | `Tertahan` berjenis belum terdaftar dipasangkan ke jenis berkode sama | `ProsesKejadianAsync` mencari jenis aktif berdasarkan `EventTypeCode` tersimpan, lalu mengisi `EventTypeId` | ✅ |
| 3 | Abaikan hanya `Gagal`, alasan wajib | `AbaikanAsync` — `400` alasan kosong, `409` status lain; ubah status bersyarat `WHERE EventStatus = Gagal`. **Uji:** penolakan `409` untuk `Tertahan` dan `Terjurnal` lulus; jalur berhasil pada `Gagal` dibuktikan lewat source (bagian 6.2) | ✅ |
| 4 | `Tertahan` → `Diabaikan` ditolak | `409` dengan pesan "Kejadian Tertahan menunggu aturan posting dan tidak boleh diabaikan…". **Uji:** Swagger pada `EVT-UJI-001` lulus | ✅ |

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
| Uji | **Lulus** — Rizki, 24 September 2026, bagian 6 |

## 6. Bukti uji — Rizki, 24 September 2026

### 6.1 Yang diuji

| Skenario | Jalur | Hasil |
| --- | --- | :---: |
| `PATCH /{id}/ignore` pada `EVT-UJI-001` (`Tertahan`), alasan terisi | Swagger | `409` "Kejadian Tertahan … tidak boleh diabaikan" ✅ |
| `PATCH /{id}/ignore` pada `EVT-UJI-002` (`Terjurnal`) | Swagger | `409` ✅ |
| Daftarkan jenis berkode sama dengan `EVT-UJI-001` + aturan posting, lalu Coba Ulang | Layar Rincian (`FE-ACC-P2-012` skenario 4) | Toast berhasil, `Tertahan` → `Terjurnal`, riwayat percobaan bertambah ✅ — acceptance (1) dan (2) |
| Coba Ulang pada kejadian `Terjurnal` | Layar Rincian (`FE-ACC-P2-012` skenario 5) | Tombol mati ✅ |

### 6.2 Yang dibuktikan lewat source — keputusan Rizki, 24 September 2026

Saat ini **tidak ada jalur kode yang menghasilkan kejadian `Gagal`**: `AccAccountingEventService`
tidak pernah menulis `AccountingEventStatus.Gagal`; status itu baru muncul dari penjadwal
`BE-ACC-P2-023` sesudah tiga coba ulang gagal. Rizki memilih menerima pembacaan source untuk dua
jalur berikut, mengikuti preseden `BE-ACC-P2-031` acceptance (5). Data tidak diubah lewat SQL.

| Jalur | Bukti source |
| --- | --- |
| Abaikan kejadian `Gagal` dengan alasan → `Diabaikan` | `AbaikanAsync`: status harus `Gagal` (selain itu `409`), alasan wajib (`400`), lalu `ExecuteUpdateAsync` bersyarat `WHERE Id = id AND EventStatus = Gagal` — penjaga yang sama yang menolak `Tertahan` dan `Terjurnal` pada 6.1 |
| Coba Ulang kejadian `Gagal` | `CobaUlangAsync` menerima `Gagal` dan `Tertahan` lewat satu pemeriksaan (`is not (Gagal or Tertahan)` → `409`), lalu memanggil `ProsesKejadianAsync` yang sama yang terbukti mengubah `Tertahan` → `Terjurnal` pada 6.1 |

Kedua jalur ini diuji ulang lewat layar begitu `BE-ACC-P2-023` menghasilkan kejadian `Gagal`
sungguhan. UAT belum dijalankan — diserahkan ke tim UAT.

**Perubahan data uji.** `EVT-UJI-001` kini `Terjurnal`; kotak masuk tidak lagi punya kejadian `Tertahan`.

### 6.3 Pembaruan — jalur Gagal diuji di layar, 24 September 2026

Sesudah `BE-ACC-P2-023` menghasilkan kejadian Gagal sungguhan (`EVT-UJI-023A`, `023B`, tanggal 2030-01-15),
Rizki menguji kedua jalur yang di bagian 6.2 baru dibuktikan lewat source:

| Jalur | Hasil |
| --- | :---: |
| Abaikan `EVT-UJI-023A` tanpa alasan → ditolak; dengan alasan "Testing ignore event" → Diabaikan | ✅ |
| Coba Ulang `EVT-UJI-023B` saat periode 2030 belum ada → tetap Gagal; sesudah periode 2030 dibangkitkan → Terjurnal | ✅ |

Bukti source di bagian 6.2 kini diperkuat bukti layar. Rincian di [laporan `BE-ACC-P2-023`](BE-ACC-P2-023.md) bagian 8.
