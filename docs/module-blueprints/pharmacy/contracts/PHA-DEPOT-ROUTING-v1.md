# PHA-DEPOT-ROUTING-v1 — Routing Depo Pelayanan

| Field | Value |
| --- | --- |
| Contract ID | `PHA-DEPOT-ROUTING-v1` |
| Blueprint | `PHA-BP-001` revision `2` |
| Status | `APPROVED` oleh product/domain owner |
| Approval | Persetujuan pengguna, 20 Agustus 2026 |
| Related decision | `PHA-DEC-040` |
| Backend evidence | `MstDrugStorageLocation`, `EncounterType`, `ServiceUnitId`, dan `ClinicId` pada backend SHA `767470f742bc6f2eebadbd653a873f69d6f93121` |
| Compatibility impact | Kontrak target; implementasi belum tersedia |

## Tujuan

Menentukan tepat satu Depo Farmasi yang melayani resep berdasarkan layanan pasien. Farmasi tidak boleh mengambil stok Gudang Utama atau lokasi karantina untuk memenuhi resep.

## Prasyarat lokasi

Lokasi hanya boleh menjadi kandidat bila aktif, tidak dihapus, bukan Gudang Utama, bukan lokasi karantina, `IsPharmacyLocation = true`, dan `IsAllowDispensing = true`.

**Contoh:** Lokasi “Depo Rawat Jalan A” aktif dan boleh melakukan dispensing, sehingga dapat dipilih. Lokasi “Gudang Utama” ditolak walaupun stoknya tersedia karena `IsMainWarehouse = true`.

## Aturan pemilihan

| Layanan encounter | Prioritas pencocokan |
| --- | --- |
| Rawat Jalan | Cocokkan `ClinicId`; jika tidak tersedia, cocokkan `ServiceUnitId` |
| IGD | Cocokkan `ServiceUnitId` dan tipe lokasi `Emergency` |
| Rawat Inap | Cocokkan `ServiceUnitId` dan tipe lokasi `Pharmacy` |

Hasil wajib tepat satu lokasi. Bila tidak ada kandidat atau terdapat lebih dari satu kandidat pada prioritas yang sama, sistem menolak pemrosesan resep dan meminta konfigurasi lokasi diperbaiki. Sistem tidak boleh memilih lokasi secara acak.

**Contoh:** Encounter Rawat Jalan berasal dari Klinik Penyakit Dalam. Jika tepat satu Depo aktif memiliki `ClinicId` yang sama, Depo tersebut dipilih. Jika dua Depo aktif sama-sama cocok, pemrosesan ditolak agar stok tidak diambil dari lokasi yang salah.

## Hubungan dengan reservasi

Penentuan Depo tidak mereservasi atau mengurangi stok. Reservasi baru dilakukan setelah pembayaran atau jaminan valid dan Farmasi mulai memproses resep. Stok fisik baru berkurang ketika obat berhasil diserahkan.

## Jalur tidak normal

| Kondisi | Hasil |
| --- | --- |
| Tidak ada Depo yang cocok | Tolak pemrosesan dan tampilkan kesalahan konfigurasi |
| Lebih dari satu Depo cocok | Tolak pemrosesan; jangan memilih kandidat pertama |
| Depo nonaktif, karantina, atau Gudang Utama | Keluarkan dari kandidat |
| Depo berubah setelah resep dibuat | Validasi ulang sebelum reservasi; jangan memindahkan reservasi secara diam-diam |

## API

Belum ada endpoint target yang dikunci. Endpoint baru hanya boleh ditetapkan pada fase desain bisnis dan harus diberi label `Rencana (belum tersedia)` sampai benar-benar diimplementasikan.

