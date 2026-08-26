# Farmasi — Acceptance Test Matrix Routing Depo

Contract version: `PHA-TEST-ROUTING-v1`; status `approved`; disetujui product/domain owner 21 Agustus 2026.

| Requirement | Skenario | Jenis test | Bukti |
| --- | --- | --- | --- |
| `PHA-DEC-040` | Rawat Jalan memiliki satu Clinic match | Integration | ID Depo yang cocok dikembalikan |
| `PHA-DEC-040` | Rawat Jalan tanpa Clinic match memakai ServiceUnit fallback | Integration | Tepat satu fallback dipilih |
| `PHA-DEC-040` | IGD cocok ServiceUnit dan type Emergency | Integration | Depo IGD dipilih |
| `PHA-DEC-040` | Rawat Inap cocok ServiceUnit dan type Pharmacy | Integration | Depo rawat inap dipilih |
| Eligibility | Lokasi gudang utama/karantina/non-dispensing/nonaktif | Unit/integration | Lokasi dikeluarkan |
| Tepat satu hasil | Nol kandidat | Integration | `PHA_ROUTE_NOT_FOUND`, tanpa mutation |
| Tepat satu hasil | Dua kandidat sama prioritas | Integration | `PHA_ROUTE_AMBIGUOUS`, tanpa fallback |
| `PHA-DEC-041` | Resolve berhasil | Integration | Tidak ada perubahan stok/payment |
| Revalidation | Lokasi dinonaktifkan setelah resolve awal | Integration | Revalidation gagal dan reservasi tidak dipanggil |
| Privacy | Routing gagal | Test logging | Log tidak memuat nama pasien atau detail resep |
| Cancellation | Request dibatalkan | Unit/integration | Operasi berhenti melalui cancellation token |
