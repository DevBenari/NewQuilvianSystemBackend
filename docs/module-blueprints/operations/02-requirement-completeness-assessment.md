# Modul Operasi — Penilaian Kelengkapan Requirement

| Field | Nilai |
|---|---|
| Blueprint ID | `operations` |
| Revision | `3` |
| Status penilaian | `READY_FOR_DOMAIN_DESIGN` |
| Decision input | `00-interview-decisions.md`, revision 5 |
| Capability input | `01-existing-capability-map.md`, revision 2 |
| Backend SHA | `767470f742bc6f2eebadbd653a873f69d6f93121` |
| Frontend SHA | `400104f2a0f3239c14c40f5905b419977a538450` |

## Kesimpulan Gate

Requirement Modul Operasi **siap diteruskan ke desain domain**. Pemilik kebutuhan menyetujui lifecycle serta seluruh rekomendasi untuk data minimum, kewenangan transition, jadwal, tim, checklist, anestesi, material/implant, recovery, pembatalan, billing, laporan, dan notifikasi.

Kesiapan ini mengizinkan arsitektur domain disusun. Kesiapan ini belum mengizinkan penulisan source aplikasi, migration, endpoint, atau UI.

## Bukti yang Dipakai

| Bukti | Wewenang | Klasifikasi |
|---|---|---|
| Keputusan eksplisit pemilik kebutuhan pada `00-interview-decisions.md` | Menentukan apa yang seharusnya dibangun | `CONFIRMED` |
| Audit source pada `01-existing-capability-map.md` | Menentukan kemampuan yang saat ini tersedia | `CONFIRMED` |
| Asumsi umum tentang isi checklist, skor recovery, dan mekanisme billing | Tidak ada persetujuan rumah sakit | Tidak dipakai sebagai requirement |
| SOP rumah sakit, notulen rapat, atau kebijakan klinis Operasi | Belum tersedia | `MISSING` |
| Persetujuan eksplisit pemilik kebutuhan untuk memakai seluruh opsi rekomendasi | Menutup `OPS-REQ-001` sampai `OPS-REQ-011` | `CONFIRMED` |
| Baseline canonical Operating Theatre | Belum tersedia pada pustaka rujukan | `REFERENCE_ONLY: NOT_YET_AVAILABLE` |

## Penilaian 18 Dimensi

| ID | Dimensi | Status bukti | Dampak | Penilaian |
|---|---|---|---|---|
| 01 | Tujuan | `CONFIRMED` | - | Modul menangani proses perioperatif dari permintaan dokter sampai pasien diserahterimakan setelah recovery |
| 02 | Aktor | `CONFIRMED` | - | Aktor utama, tim minimum, peran tambahan, dan syarat kewenangan klinis sudah diputuskan |
| 03 | Pemicu/prasyarat | `CONFIRMED` | - | Data minimum permintaan dan sign-off kesiapan sudah diputuskan |
| 04 | Alur utama | `CONFIRMED` | - | Alur dari `Requested` sampai `Completed` beserta kewenangan transition sudah diputuskan; `Completed` berarti catatan operasi, recovery, dan handover telah lengkap |
| 05 | Alur alternatif/exception | `CONFIRMED` | - | Darurat, penundaan, pembatalan, penghentian setelah mulai, recovery gagal, retur, waste, dan koreksi sudah ditentukan pada tingkat bisnis |
| 06 | Data minimum | `CONFIRMED` | - | Data minimum permintaan, operasi, anestesi, material, recovery, dan handoff sudah disetujui |
| 07 | Aturan bisnis/validation | `CONFIRMED` | - | Benturan, buffer terkonfigurasi, kompetensi tim, checklist, dan traceability material sudah diputuskan |
| 08 | Status/perubahan status | `CONFIRMED` | - | Lifecycle dan asal transition `Postponed`/`Cancelled` sudah diputuskan |
| 09 | Peran/authorization | `CONFIRMED` | - | Kewenangan transition dan sign-off utama sudah jelas; permission teknis dirancang di tahap berikutnya |
| 10 | Dependency antarmodul | `CONFIRMED` | - | Ownership pasien, HR, Master Data, Farmasi/Persediaan, Billing, dan unit layanan asal tetap di modul masing-masing |
| 11 | Integrasi internal/eksternal | `CONFIRMED` untuk internal | `NON_BLOCKING_STANDARD` untuk eksternal | Handoff stok dan Billing sudah diputuskan secara bisnis; tidak ada integrasi eksternal yang diminta saat ini |
| 12 | Hasil akhir | `CONFIRMED` | - | Operasi selesai, catatan disahkan, pemakaian direkonsiliasi, recovery diputuskan, dan pasien diserahterimakan |
| 13 | Pembatalan/koreksi | `CONFIRMED` | - | Batas pembatalan, `StoppedEarly`, addendum, retur/waste, reversal, dan audit sudah diputuskan |
| 14 | Audit/histori | `CONFIRMED` sebagian | `NON_BLOCKING_STANDARD` untuk format | Alasan darurat, reschedule, pembatalan, sign-off, penggunaan material, dan addendum harus terlacak; format teknis dapat ditentukan di desain |
| 15 | Notifikasi | `CONFIRMED` | `CONFIGURABLE_DEFAULT` untuk kanal | Kejadian dan penerima terdampak sudah diputuskan; kanal dapat dikonfigurasi |
| 16 | Billing/charge | `CONFIRMED` | - | Charge tindakan saat selesai, material sesuai pemakaian, idempotency, reversal, dan koreksi melalui Billing |
| 17 | Keselamatan klinis | `CONFIRMED` | - | Checklist tiga tahap, sign-off, anestesi, recovery, implant, dan exception darurat sudah diputuskan |
| 18 | Pelaporan/traceability | `CONFIRMED` | `NON_BLOCKING_STANDARD` untuk format | Kelompok laporan minimum dan audit sudah disetujui; format visual ditentukan kemudian |

## Kesiapan per Slice

| Slice | Status kesiapan | Yang boleh berjalan | Yang harus berhenti |
|---|---|---|---|
| Referensi pasien, encounter, dokter, ruang, procedure, consent, tarif | `READY_FOR_DOMAIN_DESIGN` | Definisikan dependency dan ownership reuse | Jangan membuat master duplikat |
| Permintaan operasi | `READY_FOR_DOMAIN_DESIGN` | Rancang relasi order dan data minimum | Jangan menduplikasi patient procedure |
| Kalender, ruang, dan tim | `READY_FOR_DOMAIN_DESIGN` | Rancang reservasi dan pemeriksaan benturan | Jangan memakai jadwal praktik dokter sebagai kalender operasi |
| Persiapan dan checklist | `READY_FOR_DOMAIN_DESIGN` | Rancang checklist berversi dan sign-off | Detail SOP klinis tetap harus dapat dikonfigurasi |
| Pelaksanaan dan anestesi | `READY_FOR_DOMAIN_DESIGN` | Rancang record klinis dan pengesahan | Jangan menyatukan consent dengan catatan anestesi |
| Obat, bahan, alat, dan implant | `READY_FOR_DOMAIN_DESIGN` | Rancang handoff pemakaian/retur/waste | Ownership stok tetap di modul pemilik |
| Recovery dan serah terima | `READY_FOR_DOMAIN_DESIGN` | Rancang decision dan handoff | Skor klinis spesifik harus terkonfigurasi |
| Pembatalan, reschedule, dan koreksi | `READY_FOR_DOMAIN_DESIGN` | Rancang histori dan efek per tahap | Histori tidak boleh dihapus |
| Billing | `READY_FOR_DOMAIN_DESIGN` | Rancang kontrak charge idempotent | Implementasi menunggu owner transaksi Billing tersedia |
| Laporan dan notifikasi | `READY_FOR_DOMAIN_DESIGN` | Rancang event dan read model minimum | Kanal notifikasi tetap konfigurabel |

## Decision Log yang Harus Ditutup

| Decision ID | Status bukti | Dampak | Pertanyaan yang harus diputuskan | Pemilik |
|---|---|---|---|---|
| `OPS-REQ-001` | `CONFIRMED` | - | Data minimum permintaan disetujui melalui rekomendasi | Pemilik kebutuhan |
| `OPS-REQ-002` | `CONFIRMED` | - | Lifecycle dan kewenangan transition disetujui | Pemilik kebutuhan |
| `OPS-REQ-003` | `CONFIRMED` | - | Prioritas, estimasi, buffer, dan reschedule disetujui | Pemilik kebutuhan |
| `OPS-REQ-004` | `CONFIRMED` | - | Tim minimum dan validasi kewenangan disetujui | Pemilik kebutuhan |
| `OPS-REQ-005` | `CONFIRMED` | - | Checklist tiga tahap dan jalur darurat disetujui | Pemilik kebutuhan |
| `OPS-REQ-006` | `CONFIRMED` | - | Catatan operasi dan anestesi minimum disetujui | Pemilik kebutuhan |
| `OPS-REQ-007` | `CONFIRMED` | - | Pemakaian, retur, waste, implant, dan koreksi disetujui | Pemilik kebutuhan |
| `OPS-REQ-008` | `CONFIRMED` | - | Recovery dan serah terima disetujui | Pemilik kebutuhan |
| `OPS-REQ-009` | `CONFIRMED` | - | Pembatalan dan `StoppedEarly` disetujui | Pemilik kebutuhan |
| `OPS-REQ-010` | `CONFIRMED` | - | Charge, pemakaian aktual, idempotency, dan reversal disetujui | Pemilik kebutuhan |
| `OPS-REQ-011` | `CONFIRMED` | - | Laporan dan notifikasi minimum disetujui | Pemilik kebutuhan |

## Contoh Keputusan yang Menutup Blocker

### Pembatalan pada tahap berbeda

Pembatalan pukul 08.00 sebelum bahan digunakan menghasilkan `Cancelled`. Barang utuh diverifikasi untuk retur. Setelah operasi `In Progress`, kasus tidak lagi dibatalkan; bila tindakan harus dihentikan, kasus diselesaikan sebagai `Completed` dengan hasil `StoppedEarly`, lalu stok dan tagihan mengikuti pemakaian aktual.

### Status belum ditentukan

`Cancelled` hanya dapat dicapai dari `Requested`, `Scheduled`, atau `Ready` oleh dokter bedah atau dokter anestesi. `Postponed` dapat dicapai dari `Requested` atau `Scheduled` oleh koordinator, lalu kembali ke `Scheduled` setelah jadwal baru tersedia.

### Recovery belum lengkap

Pasien yang belum memenuhi kriteria recovery tetap dipantau atau dipindahkan ke unit yang sesuai, misalnya ICU. Dokter anestesi menetapkan keputusan klinis dan perawat mencatat serah terima kepada unit penerima.

## Konflik

Tidak ditemukan konflik antar-keputusan pemilik kebutuhan. Gap saat ini berbentuk informasi yang belum tersedia, bukan keputusan yang saling bertentangan.

## Handoff

1. Seluruh decision pemblokir sudah ditutup oleh persetujuan pemilik kebutuhan.
2. Kirim seluruh slice ke `hospital-domain-architect` setelah pemilik kebutuhan menyetujui kelanjutan.
3. Pertahankan catatan bahwa baseline canonical Operating Theatre belum tersedia; arsitektur harus bersandar pada decision log, capability map, dan aturan repository.

Penilaian ini tidak menghasilkan entity, tabel, endpoint, UI, atau task implementasi.
