# FLOW-BM-MVP-004 — Penutupan administratif dan pembukaan

**draft**, 10 Oktober 2026; mengikuti [manifest](../blueprint-manifest.md), last_changed_in 0.12.0. Approval amandemen belum ada. Owner produk/domain Muhammad Hamzah; penugasan/SOP nyata tetap BM-G02/03.

Pelaku dan pemicu: Admin MasterData berwenang; seluruh write path. Trace: DEC-285; AC-412/419/424.

```mermaid
flowchart TD
 subgraph admin[Admin MasterData]
  A["Ajukan penutupan atau perubahan master beralasan"] --> B["Periksa bed yang terdampak"]
 end
 subgraph sistem[Sistem]
  B --> C{"Ada hunian atau pesanan aktif?"}
  C -- Ya --> D["Tolak penutupan; pemegang bed tetap terlihat"]
  C -- Tidak --> E["Tutup bed; Tidak Tersedia; jejak pekerjaan dipertahankan"]
 end
 subgraph pemulihan[Admin MasterData]
  E --> F["Tangani penyebab; ajukan pembukaan beralasan"]
 end
 subgraph kesiapan[Perawat verifikator dan Housekeeping]
  F --> G{"Masih perlu pembersihan?"}
  G -- Ya --> H["Jalankan pembersihan dan pemeriksaan"]
  G -- Tidak --> I["Periksa kesiapan sesuai SOP"]
  H --> J["Sahkan hanya jika seluruh syarat terpenuhi"]
  I --> J
 end
 D --> B
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Ajukan perubahan master | Admin MasterData | Alasan, hak dan seluruh bed terdampak | Permintaan diperiksa terhadap pemegang aktual | Baca ulang bila data berubah |
| Tutup bed | Admin/sistem | Tidak ada hunian atau pesanan aktif | Tidak Tersedia; alasan dan jejak tersimpan | Jangan menyembunyikan pasien atau membatalkan pesanan secara diam-diam |
| Buka kembali | Admin | Penyebab sudah ditangani; alasan | Penutupan dibuka; kesiapan belum diasumsikan | Pertahankan bed tertahan jika penyebab belum selesai |
| Sahkan kesiapan | HK/perawat sesuai peran | Pembersihan bila diperlukan, pemeriksaan sah | Tersedia setelah seluruh syarat lolos | Ikuti alur11; pembukaan administratif tidak menggantikan pengesahan |

PUT/status/availability/create/delete tidak dapat menjadi bypass. Pada konflik data legacy, pasien/reservation aktif tetap terlihat dan pemesanan dilarang, bukan dianggap kosong.

Kontrak authoritative: [state](../contracts/state-transition-matrix.md), [API](../contracts/api-contract.md), [permission](../contracts/permission-audit-matrix.md), [acceptance](../testing/acceptance-test-matrix.md).
