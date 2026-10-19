# Laporan Perubahan Backend — `BE-RWI-245`

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Registrasi pasien bayi atomik dan idempotent |
| Kontrak | `RWI-BAYI-022-2.0.0` |
| Tanggal | 16 Oktober 2026 |
| Status | **Selesai source dan migration**; database belum diterapkan |

## Hasil

Endpoint `POST /patients/newborns` membuat pasien dan nomor RM tersendiri, menurunkan nama
`By. Ny. <ibu>`, serta menentukan `BirthOrder` per `DeliveryCaseId` di backend. Request browser
tidak menerima urutan. Advisory lock PostgreSQL, tabel pencacah, index unik, dan hash
idempotency mencegah nomor kembar dan duplikasi retry.

## Bukti perubahan

- `NewbornPatientRegistrationService.cs`: validasi ibu/kasus, transaksi, dua advisory lock,
  pencacah persisted, number series, dan replay idempotent.
- `PatNewbornBirthSequence.cs` beserta konfigurasi EF.
- `MstPatient`: `DeliveryCaseId`, hash idempotency, fingerprint, dan index unik kasus/urutan.
- `PatientController.cs` dan `PatientDtos.cs`: endpoint serta kontrak request/response.
- Migration `20261016080745_ReviseNewbornAdmissionFlow` beserta Designer dan snapshot.

## Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| Build backend Release | Lulus, 0 error |
| Review concurrency | Alokasi tidak memakai `Count+1`/`Max+1`; lock + counter + unique index |
| Review idempotency | Kunci sama/payload sama replay; payload berbeda menghasilkan konflik |
| Pending model changes | Tidak ada |

Automated test backend tidak tersedia menurut kebijakan repository. Migration sengaja tidak
diaplikasikan ke database.
