# Integration Contract — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `4` |
| `contract_version` | `0.2.0` |
| Status | `approved` — disetujui Product/Domain Owner 14 Agustus 2026 sesuai `IGD-DEC-046` |
| Commit diaudit | backend `e5331a0` |

## Ruang lingkup

Modul IGD **tidak memanggil sistem di luar aplikasi** pada revisi ini. Tidak ada integrasi ke
BPJS, SATUSEHAT, laboratorium eksternal, maupun layanan pihak ketiga lain yang terbukti di
source code pada commit `e5331a0`.

Yang ada adalah keterkaitan **antar modul di dalam satu aplikasi**, dan itu terjadi melalui
relasi basis data, bukan melalui pemanggilan jaringan. Karena itu tidak diperlukan kontrak
integrasi berupa endpoint, antrean pesan, atau webhook.

Dokumen ini tetap dibuat agar pembaca dapat membedakan "memang tidak diperlukan" dari
"terlupa ditulis", sesuai aturan struktur keluaran.

## Keterkaitan antar modul di dalam aplikasi

| Modul lain | Cara terhubung | Arah | Catatan |
| --- | --- | --- | --- |
| Registration Management | `TrxEmergencyVisit.EncounterId` | IGD menunjuk | Encounter dimiliki Registration |
| Patient Management | `TrxEmergencyVisit.PatientId` | IGD menunjuk | Boleh kosong untuk pasien tidak dikenal |
| Clinical Management | `PatientProcedureId`, `PatientVitalSignId`, `ProgressNoteId` | IGD menunjuk | Tidak ada penyalinan data klinis |
| Master Data | `ServiceUnitId`, `RoomId`, `BedId` | IGD menunjuk | Relasi ruangan dan bed menunggu entity final |
| Workflow Management | `TrxWorkflowInstance.ReferenceType` dan `ReferenceId` | IGD menunjuk | Engine generik; IGD tidak membangun kerangka approval sendiri |
| Billing Management | Melalui `EncounterId` yang sama | Tidak ada relasi langsung | Billing **bukan** syarat penyelesaian klinis |
| Pharmacy, Laboratory, Radiology | Melalui `EncounterId` yang sama | Tidak ada relasi langsung | Order dan hasil dimiliki modul masing-masing |

Arah ketergantungan selalu satu arah: IGD bergantung pada modul pusat, dan modul pusat tidak
mengetahui adanya IGD.

## Proses terjadwal di dalam aplikasi

| Proses | Pemicu | Frekuensi | Status |
| --- | --- | --- | --- |
| `EmergencyTriageSlaMonitorHostedService` | Waktu | Berkala | **Baru** — memindai `ResponseDueAt` yang terlampaui lalu menandai `IsSlaBreached` |

Proses ini berjalan di dalam aplikasi yang sama, bukan integrasi eksternal. Ia mengikuti pola
lima hosted service yang sudah ada pada modul Human Resource, sehingga tidak memerlukan
mekanisme penjadwalan baru.

Sifat yang wajib dipenuhi:

- **Idempotent** — menjalankan pemindaian dua kali tidak menghasilkan penandaan ganda.
- **Tidak memblokir pelayanan** — kegagalan pemindaian tidak boleh menghalangi triage,
  penanganan, maupun penyelesaian kunjungan.
- **Tidak mengubah data klinis** — hanya mengisi penanda breach dan waktunya.

## Bila kebutuhan integrasi eksternal muncul

Dokumen ini ditinjau ulang apabila salah satu berikut terjadi:

1. IGD perlu mengirim atau menerima data ke sistem di luar aplikasi;
2. rujukan keluar perlu terhubung ke sistem fasilitas tujuan;
3. pelaporan wajib ke sistem pemerintah dilakukan langsung dari modul IGD.

Ketiganya belum menjadi kebutuhan pada revisi ini dan **tidak boleh** dirancang lebih dulu
tanpa keputusan pemilik yang berwenang.
