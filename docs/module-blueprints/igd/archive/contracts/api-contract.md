# API Contract — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `4` |
| `contract_version` | `0.2.0` |
| Status | `approved` — disetujui Product/Domain Owner 14 Agustus 2026 sesuai `IGD-DEC-046`; hash terkunci di manifest |
| Commit diaudit | backend `e5331a0` |
| Pembungkus respons | `ApiResponse<T>.Ok(data, pesan)` dan `ApiResponse<T>.Fail(kode, pesan)` |

Judul setiap bagian ditulis persis sesuai nilai `[Tags(...)]` pada controller, sehingga
pembaca dapat mencocokkannya dengan halaman Swagger tanpa menebak.

Seluruh endpoint memerlukan pengguna yang sudah masuk. Endpoint bertanda
**Rencana (belum tersedia)** belum ada di kode.

---

## Health Services / Emergency Installation Management / Emergency Visit

Base URL: `api/v1/health-services/emergency-installation-management/emergency-visits`

| Method | Path | Kegunaan | Hak akses | Request | Status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kunjungan IGD dengan penyaringan dan halaman | `EmergencyVisit : Read` | Query filter | Sudah ada |
| `GET` | `/{id}` | Detail satu kunjungan | `EmergencyVisit : Read` | Path `id` | Sudah ada |
| `POST` | `/` | Membuat kunjungan IGD baru | `EmergencyVisit : Create` | `CreateEmergencyVisitRequest` | Sudah ada |
| `PUT` | `/{id}` | Mengubah data kunjungan | `EmergencyVisit : Update` | `UpdateEmergencyVisitRequest` | Sudah ada |
| `PATCH` | `/{id}/registration-status` | Mengubah status registrasi | `EmergencyVisit : Update` | Status baru | Sudah ada |
| `PATCH` | `/{id}/visit-status` | Mengubah status kunjungan | `EmergencyVisit : Update` | Status baru | Sudah ada |
| `PATCH` | `/{id}/complete` | Menyelesaikan kunjungan secara klinis dan mengisi `VisitCompletedAt` | `EmergencyVisit : Update` | Alasan dan konfirmasi gate | **Rencana (belum tersedia)** |
| `DELETE` | `/{id}` | Menandai kunjungan terhapus | `EmergencyVisit : Delete` | Path `id` | Sudah ada |

Endpoint `/{id}/complete` berasal dari `IGD-DEC-049`. Ia hanya sah bila seluruh closure gate
klinis dan transfer terpenuhi, dan tidak bergantung pada status billing sesuai `IGD-DEC-021`.

---

## Health Services / Emergency Installation Management / Emergency Triage

Base URL: `api/v1/health-services/emergency-installation-management/emergency-triages`

| Method | Path | Kegunaan | Hak akses | Request | Status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar penilaian triage | `EmergencyTriage : Read` | Query filter | Sudah ada |
| `GET` | `/{id}` | Detail satu penilaian | `EmergencyTriage : Read` | Path `id` | Sudah ada |
| `POST` | `/` | Membuat penilaian triage | `EmergencyTriage : Create` | `CreateEmergencyTriageRequest` | Sudah ada |
| `PUT` | `/{id}` | Mengubah penilaian | `EmergencyTriage : Update` | `UpdateEmergencyTriageRequest` | Sudah ada |
| `PATCH` | `/{id}/triage-status` | Mengubah status penilaian | `EmergencyTriage : Update` | Status baru | Sudah ada |
| `POST` | `/{id}/retriage` | Menilai ulang pasien; penilaian lama menjadi `Superseded` dan penilaian baru menunjuk yang lama | `EmergencyTriage : Update` | `RetriageEmergencyTriageRequest` | Sudah ada (`BE-IGD-004`) |
| `GET` | `/sla-breaches` | Daftar pasien yang melewati `ResponseDueAt` dan belum ditangani | `EmergencyTriage : Read` | Query unit dan rentang waktu | **Rencana (belum tersedia)** |
| `DELETE` | `/{id}` | Menandai penilaian terhapus | `EmergencyTriage : Delete` | Path `id` | Sudah ada |

---

## Health Services / Emergency Installation Management / Emergency Triage Detail

Base URL: `api/v1/health-services/emergency-installation-management/emergency-triage-details`

| Method | Path | Kegunaan | Hak akses | Status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar indikator yang dipilih | `EmergencyTriageDetail : Read` | Sudah ada |
| `GET` | `/{id}` | Detail satu indikator | `EmergencyTriageDetail : Read` | Sudah ada |
| `POST` | `/` | Menambah indikator pada penilaian | `EmergencyTriageDetail : Create` | Sudah ada |
| `PUT` | `/{id}` | Mengubah indikator | `EmergencyTriageDetail : Update` | Sudah ada |
| `DELETE` | `/{id}` | Menandai indikator terhapus | `EmergencyTriageDetail : Delete` | Sudah ada |

---

## Health Services / Emergency Installation Management / Emergency Resuscitation

Base URL: `api/v1/health-services/emergency-installation-management/emergency-resuscitations`

| Method | Path | Kegunaan | Hak akses | Status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar episode resusitasi | `EmergencyResuscitation : Read` | Sudah ada |
| `GET` | `/{id}` | Detail satu episode | `EmergencyResuscitation : Read` | Sudah ada |
| `POST` | `/` | Memulai episode resusitasi | `EmergencyResuscitation : Create` | Sudah ada |
| `PUT` | `/{id}` | Mengubah episode | `EmergencyResuscitation : Update` | Sudah ada |
| `PATCH` | `/{id}/resuscitation-status` | Mengubah status episode | `EmergencyResuscitation : Update` | Sudah ada |
| `DELETE` | `/{id}` | Menandai episode terhapus | `EmergencyResuscitation : Delete` | Sudah ada |

---

## Health Services / Emergency Installation Management / Emergency Observation

Base URL: `api/v1/health-services/emergency-installation-management/emergency-observations`

| Method | Path | Kegunaan | Hak akses | Status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar periode observasi | `EmergencyObservation : Read` | Sudah ada |
| `GET` | `/{id}` | Detail satu periode | `EmergencyObservation : Read` | Sudah ada |
| `POST` | `/` | Memulai observasi | `EmergencyObservation : Create` | Sudah ada |
| `PUT` | `/{id}` | Mengubah observasi | `EmergencyObservation : Update` | Sudah ada |
| `PATCH` | `/{id}/observation-status` | Mengubah status, termasuk eskalasi | `EmergencyObservation : Update` | Sudah ada |
| `DELETE` | `/{id}` | Menandai observasi terhapus | `EmergencyObservation : Delete` | Sudah ada |

---

## Health Services / Emergency Installation Management / Emergency Observation Detail

Base URL: `api/v1/health-services/emergency-installation-management/emergency-observation-details`

| Method | Path | Kegunaan | Hak akses | Status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar catatan berkala | `EmergencyObservationDetail : Read` | Sudah ada |
| `GET` | `/{id}` | Detail satu catatan | `EmergencyObservationDetail : Read` | Sudah ada |
| `POST` | `/` | Menambah catatan berkala | `EmergencyObservationDetail : Create` | Sudah ada |
| `PUT` | `/{id}` | Mengubah catatan | `EmergencyObservationDetail : Update` | Sudah ada |
| `DELETE` | `/{id}` | Menandai catatan terhapus | `EmergencyObservationDetail : Delete` | Sudah ada |

---

## Health Services / Emergency Installation Management / Emergency Procedure Detail

Base URL: `api/v1/health-services/emergency-installation-management/emergency-procedure-details`

| Method | Path | Kegunaan | Hak akses | Status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar detail tindakan khas IGD | `EmergencyProcedureDetail : Read` | Sudah ada |
| `GET` | `/{id}` | Detail satu tindakan | `EmergencyProcedureDetail : Read` | Sudah ada |
| `POST` | `/` | Menambah detail khas IGD pada tindakan klinis | `EmergencyProcedureDetail : Create` | Sudah ada |
| `PUT` | `/{id}` | Mengubah detail | `EmergencyProcedureDetail : Update` | Sudah ada |
| `DELETE` | `/{id}` | Menandai detail terhapus | `EmergencyProcedureDetail : Delete` | Sudah ada |

---

## Health Services / Emergency Installation Management / Emergency Disposition

Base URL: `api/v1/health-services/emergency-installation-management/emergency-dispositions`

| Method | Path | Kegunaan | Hak akses | Status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar keputusan akhir | `EmergencyDisposition : Read` | Sudah ada |
| `GET` | `/{id}` | Detail satu keputusan | `EmergencyDisposition : Read` | Sudah ada |
| `POST` | `/` | Membuat keputusan tindak lanjut | `EmergencyDisposition : Create` | Sudah ada |
| `PUT` | `/{id}` | Mengubah keputusan | `EmergencyDisposition : Update` | Sudah ada |
| `PATCH` | `/{id}/disposition-status` | Mengubah status keputusan | `EmergencyDisposition : Update` | Sudah ada |
| `DELETE` | `/{id}` | Menandai keputusan terhapus | `EmergencyDisposition : Delete` | Sudah ada |

---

## Health Services / Emergency Installation Management / Emergency Transfer

Base URL: `api/v1/health-services/emergency-installation-management/emergency-transfers`

| Method | Path | Kegunaan | Hak akses | Status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar perpindahan pasien | `EmergencyTransfer : Read` | Sudah ada |
| `GET` | `/{id}` | Detail satu perpindahan | `EmergencyTransfer : Read` | Sudah ada |
| `POST` | `/` | Mengajukan perpindahan | `EmergencyTransfer : Create` | Sudah ada |
| `PUT` | `/{id}` | Mengubah perpindahan | `EmergencyTransfer : Update` | Sudah ada |
| `PATCH` | `/{id}/transfer-status` | Mengubah status perpindahan | `EmergencyTransfer : Update` | Sudah ada |
| `DELETE` | `/{id}` | Menandai perpindahan terhapus | `EmergencyTransfer : Delete` | Sudah ada |

---

## Kode status dan artinya

| Kode | Arti teknis | Arti bagi pengguna |
| --- | --- | --- |
| `200` | Berhasil | Permintaan diproses dan datanya tersedia |
| `400` | Permintaan tidak valid | Isian tidak lengkap, formatnya salah, atau melanggar aturan bisnis |
| `401` | Belum masuk | Sesi habis; pengguna perlu masuk ulang |
| `403` | Tidak berwenang | Sudah masuk tetapi tidak punya hak untuk tindakan ini |
| `404` | Tidak ditemukan | Data sudah ditandai terhapus atau tidak pernah ada |
| `409` | Bentrok | Transisi status tidak sah, atau data sedang diubah pihak lain |

## Bentuk balasan

Contoh berhasil:

```json
{
  "statusCode": 200,
  "success": true,
  "message": "Detail triage berhasil diambil.",
  "data": {
    "id": "00000000-0000-0000-0000-000000000000",
    "triageStatus": "Completed",
    "responseDueAt": "2026-08-14T03:30:00Z",
    "isSlaBreached": false
  }
}
```

Contoh gagal:

```json
{
  "statusCode": 409,
  "success": false,
  "message": "Penilaian triage yang sudah dibatalkan tidak dapat dinilai ulang.",
  "data": null
}
```

Seluruh data pada contoh adalah data samaran.

## Ringkasan perubahan kontrak

| Perubahan | Jenis | Dampak kompatibilitas |
| --- | --- | --- |
| `POST /emergency-triages/{id}/retriage` | Endpoint baru | Aditif, tidak memutus pemakai lama |
| `GET /emergency-triages/sla-breaches` | Endpoint baru | Aditif |
| `PATCH /emergency-visits/{id}/complete` | Endpoint baru | Aditif |
| Nilai `Completed` pada `EmergencyVisitStatus` | Nilai enum baru | **Berpotensi memutus** pemakai yang memetakan status secara eksklusif; frontend wajib menangani nilai baru |
| `isSlaBreached` dan `slaBreachedAt` pada respons triage | Field baru | Aditif |
