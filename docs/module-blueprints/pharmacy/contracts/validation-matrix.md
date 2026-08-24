# Farmasi — Validation Matrix Routing Depo

Contract version: `PHA-VAL-ROUTING-v1`; status `approved`; keputusan `PHA-DEC-040`; disetujui product/domain owner 21 Agustus 2026.

| Aturan | Berlaku pada | Kondisi | Pesan pengguna | Kode |
| --- | --- | --- | --- | --- |
| Encounter wajib ada | Resolve | Tidak ditemukan/nonaktif | Encounter pasien tidak dapat digunakan untuk menentukan Depo Farmasi. | `PHA_ROUTE_ENCOUNTER_INVALID` |
| Jenis layanan didukung | Resolve | Selain Rawat Jalan, IGD, Rawat Inap | Layanan pasien belum memiliki aturan Depo Farmasi. | `PHA_ROUTE_SERVICE_UNSUPPORTED` |
| Eligibility | Kandidat | Flag tidak memenuhi kontrak | Kandidat dikeluarkan tanpa fallback | internal |
| Tepat satu kandidat | Resolve | Nol kandidat | Depo Farmasi untuk layanan pasien belum dikonfigurasi. | `PHA_ROUTE_NOT_FOUND` |
| Tepat satu kandidat | Resolve | Lebih dari satu | Konfigurasi Depo Farmasi ganda. Hubungi administrator. | `PHA_ROUTE_AMBIGUOUS` |
| Validasi ulang | Sebelum reservasi | Hasil berubah/gagal | Depo Farmasi berubah. Proses dihentikan dan perlu dicoba kembali. | `PHA_ROUTE_CHANGED` |

Contoh: dua Depo IGD aktif pada unit yang sama menghasilkan `PHA_ROUTE_AMBIGUOUS`; sistem tidak memilih berdasarkan data yang pertama dibaca.
