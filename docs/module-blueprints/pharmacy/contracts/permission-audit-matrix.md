# Farmasi — Permission dan Audit Routing Depo

Contract version: `PHA-PERM-ROUTING-v1`; status `approved`; disetujui product/domain owner 21 Agustus 2026.

Tidak ada `[AccessPermission(...)]` baru karena tidak ada endpoint baru. Resolver mewarisi authorization workflow pemanggil; resolver tidak boleh dipanggil dari endpoint anonim.

| Tindakan | Permission | Audit/log | Data yang dilarang masuk log |
| --- | --- | --- | --- |
| Resolve internal | Permission workflow resep pemanggil | Correlation, encounter ID, result code, candidate count, latency | Nama pasien, diagnosis, obat, resep |
| Ubah lokasi | Di luar scope; permission Master Data existing | Mengikuti owner Master Data | Data pasien tidak relevan |
