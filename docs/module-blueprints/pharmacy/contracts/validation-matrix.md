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

---

# Slice Financial Clearance — `PHA-VAL-CLEARANCE-v1`

Status **draft** · input `PHA-DEC-063`–`070` · 21 September 2026.

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Telaah menunggu kepastian pembayaran | Memulai telaah apoteker | Salinan finansial belum menyatakan resep boleh dikerjakan | Resep ini belum dapat ditelaah karena pembayarannya belum dikonfirmasi kasir. | `PHA_CLR_NOT_CLEARED` |
| Pekerjaan berhenti saat ditahan | Telaah, penyiapan, pemeriksaan akhir | Resep sedang ditahan karena pencabutan | Resep ini ditahan karena ada perubahan tagihan. Pekerjaan dilanjutkan setelah kasir menyelesaikannya. | `PHA_CLR_ON_HOLD` |
| Penyerahan menuntut keadaan beres | Menyerahkan obat | Salinan tidak menyatakan lunas, disetujui penjamin, atau ditiadakan | Obat belum dapat diserahkan karena pembayarannya belum beres. | `PHA_CLR_NOT_SETTLED` |
| Keadaan belum diketahui bukan izin | Seluruh gerbang | Belum pernah ada surat untuk resep ini | Keadaan pembayaran resep ini belum diketahui. Obat belum boleh diserahkan. | `PHA_CLR_UNKNOWN` |
| Salinan tertinggal bukan izin | Seluruh gerbang | Sinkronisasi bermasalah dan salinan diketahui tertinggal | Data pembayaran sedang tidak dapat dipastikan. Coba beberapa saat lagi; jangan menyerahkan obat dulu. | `PHA_CLR_STALE` |
| Surat basi diabaikan | Konsumsi surat | Nomor versi surat lebih rendah dari yang tersimpan | *Tidak tampil ke pengguna* — diabaikan diam-diam, ini keadaan normal | `PHA_CLR_STALE_VERSION` |
| Tidak ada jalan pintas | Seluruh gerbang | Petugas mana pun mencoba melanjutkan tanpa kepastian | *Tidak ada permukaannya* — tidak ada tombol override yang dapat ditekan siapa pun | `PHA_CLR_NO_OVERRIDE` |

## Dua aturan yang paling mudah salah dipahami

**`PHA_CLR_STALE_VERSION` bukan kegagalan.** Surat dapat tiba tidak berurutan, dan mengabaikan
yang lebih tua adalah perilaku yang benar. Ia tidak dicatat sebagai galat, tidak memicu
percobaan ulang, dan tidak pernah ditampilkan ke petugas.

**`PHA_CLR_NO_OVERRIDE` bukan pesan penolakan melainkan ketiadaan permukaan.** `PHA-DEC-067`
memutuskan tidak ada jalur override bagi siapa pun di Farmasi ketika sinkronisasi terganggu —
termasuk Kepala Farmasi dan Supervisor. Yang membedakannya dari keadaan darurat klinis: jalur
darurat punya kebijakannya sendiri yang masih terbuka (`PHA-OQ-027`), dan gangguan teknis bukan
keadaan darurat klinis.

Trace `PHA-DEC-063`, `PHA-DEC-067`, `PHA-DEC-069`. Tests `PHA-AT-CLR-04`–`PHA-AT-CLR-09`.
