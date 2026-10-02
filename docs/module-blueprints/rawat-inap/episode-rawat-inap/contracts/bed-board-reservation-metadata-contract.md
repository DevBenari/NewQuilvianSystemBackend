# Kontrak Metadata Reservasi Papan Tempat Tidur

## Metadata

```yaml
contract_id: RWI-BED-BOARD-RESERVATION-001
contract_version: 1.0.0
status: APPROVED
approved_by: Muhammad Hamzah
approved_at: "2026-09-01"
task_id: BE-RWI-036
endpoint: "GET /api/v1/health-services/inpatient-management/bed-occupancies/bed-board"
change_type: ADDITIVE_RESPONSE
```

Kontrak ini adalah addendum aditif untuk `api-contract.md` versi `0.4.0`. Route, method,
permission, struktur pengelompokan unit/kamar/tempat tidur, dan arti field existing tidak berubah.

## Outcome

Papan tempat tidur menyediakan identitas pemegang dan metadata reservasi aktif yang cukup agar
petugas berwenang dapat mengenali pasien/episode, melihat batas waktu reservasi, dan menjalankan
konfirmasi pasien masuk tanpa menyimpan state reservasi hanya di sesi browser.

## Response aditif pada `BedBoardBedResponse`

| Field | Tipe | Nullability | Semantik |
| --- | --- | --- | --- |
| `HoldingEpisodeId` | `Guid` | nullable | Episode yang sedang memegang tempat tidur melalui penempatan aktif atau reservasi aktif |
| `HoldingEpisodeNumber` | `string` | nullable | Field existing; tetap menunjuk episode pemegang aktif |
| `PatientName` | `string` | nullable | Field existing; tetap menunjuk pasien pada episode pemegang aktif |
| `ReservationId` | `Guid` | nullable | ID reservasi hanya ketika baris benar-benar `IsReserved = true` |
| `ReservationExpiresAt` | `DateTime` | nullable | Batas waktu reservasi hanya ketika baris benar-benar `IsReserved = true` |

JSON memakai kebijakan penamaan existing aplikasi, sehingga nama wire-nya adalah
`holdingEpisodeId`, `holdingEpisodeNumber`, `patientName`, `reservationId`, dan
`reservationExpiresAt`.

## Invariant pembacaan

1. Reservasi dibaca hanya ketika `ReservationStatus = Active`, `IsDelete = false`, dan belum
   melewati `ExpiresAt` setelah proses expiry-on-read existing dijalankan.
2. Penempatan aktif menang atas reservasi bila data tidak konsisten: `IsOccupied = true`,
   `IsReserved = false`, identitas pemegang berasal dari penempatan, sedangkan `ReservationId`
   dan `ReservationExpiresAt` bernilai `null`.
3. Tempat tidur bebas, tertutup, reservasi batal, reservasi kedaluwarsa, dan reservasi terhapus
   tidak mengekspos metadata reservasi.
4. Counter `TotalBed`, `TotalAvailable`, `TotalOccupied`, `TotalReserved`, dan
   `TotalUnavailable` mempertahankan perilaku existing.
5. Permission tetap `InpatientBedOccupancy : Read`; tidak ada permission atau endpoint baru.

## Error dan kompatibilitas

Tidak ada error baru. Perubahan hanya menambah properti nullable pada response dan tidak
mengubah request, status HTTP, atau field existing. Konsumen lama yang mengabaikan field baru
tetap kompatibel.

## Batas scope

- Tidak membuat tabel, kolom database, migration, endpoint, atau aksi tulis baru.
- Tidak mengubah lifecycle reservasi maupun aturan kelayakan penempatan.
- Tidak mengubah frontend; pemakaian field ini dimiliki task frontend terkait.
