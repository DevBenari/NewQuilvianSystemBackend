# FLOW-BM-MVP-001 — Pemesanan, hunian dan pelepasan

**draft**, 10 Oktober 2026; mengikuti [manifest](../blueprint-manifest.md), last_changed_in 0.12.0. Approval amandemen belum ada. Owner produk/domain Muhammad Hamzah; penugasan/SOP nyata tetap BM-G02/03.

Pelaku dan pemicu: Admisi/perawat existing; pemicu bed yang dibaca ready. Trace: DEC-281/283/289; AC-407/408/413/419/421/426.

```mermaid
flowchart TD
 subgraph petugas[Petugas admisi]
  A["Pilih pasien dan bed yang siap"] --> B["Pesan atau tempatkan pasien"]
 end
 subgraph sistem[Sistem]
  B --> C{"Hak, kesiapan dan pemegang bed masih sah?"}
  C -- Tidak --> D["Tahan perubahan; minta petugas memperbarui pilihan"]
  C -- Ya --> E{"Pemesanan dipakai?"}
  E -- Belum --> F["Pertahankan pesanan sampai dipakai, dibatalkan atau kedaluwarsa"]
  F --> G["Lepaskan pesanan yang belum dipakai tanpa pembersihan baru"]
  E -- Ya --> H["Catat pasien menempati bed"]
 end
 subgraph ruangan[Petugas ruangan]
  H --> I["Catat kepergian fisik yang sah"]
 end
 subgraph pelepasan[Sistem]
  I --> J["Akhiri hunian; bed Menunggu Pembersihan"]
 end
 D --> A
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Pilih dan pesan/tempatkan | Admisi | Episode dan bed dalam scope; data terbaru | Dipesan atau Terisi | Perbarui pilihan setelah penolakan |
| Batalkan pesanan | Petugas berwenang | Pesanan belum dipakai; alasan | Pesanan berakhir; tidak ada pekerjaan bersih baru | Baca ulang jika pesanan sudah berubah |
| Evaluasi batas pesanan | Sistem | Batas waktu server existing | Pesanan kedaluwarsa dilepas sesuai guard | Petugas membaca ulang; timer layar bukan keputusan |
| Catat kepergian | Petugas ruangan | Pasien benar-benar pergi; hunian terkini | Hunian berakhir dan bed tertahan | Jangan melepas hunian pasien berikutnya |

Reservation belum dipakai tidak membuat kebutuhan pembersihan. Closure episode lama tidak memanggil release bed ulang. Error/retry tidak menambah holder atau melepas pasien berikutnya.

Kontrak authoritative: [state](../contracts/state-transition-matrix.md), [API](../contracts/api-contract.md), [permission](../contracts/permission-audit-matrix.md), [acceptance](../testing/acceptance-test-matrix.md).
