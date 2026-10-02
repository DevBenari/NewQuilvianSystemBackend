# Farmasi — Permission dan Audit Routing Depo

Contract version: `PHA-PERM-ROUTING-v1`; status `approved`; disetujui product/domain owner 21 Agustus 2026.

Tidak ada `[AccessPermission(...)]` baru karena tidak ada endpoint baru. Resolver mewarisi authorization workflow pemanggil; resolver tidak boleh dipanggil dari endpoint anonim.

| Tindakan | Permission | Audit/log | Data yang dilarang masuk log |
| --- | --- | --- | --- |
| Resolve internal | Permission workflow resep pemanggil | Correlation, encounter ID, result code, candidate count, latency | Nama pasien, diagnosis, obat, resep |
| Ubah lokasi | Di luar scope; permission Master Data existing | Mengikuti owner Master Data | Data pasien tidak relevan |

---

# Slice Financial Clearance — `PHA-PERM-CLEARANCE-v1`

Status **draft** · input `PHA-DEC-063`–`070` · 21 September 2026.

## Tidak ada butir hak akses baru

Slice ini **tidak menambah satu pun** Resource maupun Action. Itu bukan kekurangan melainkan
inti keamanannya: tidak ada kemampuan baru yang perlu dijaga, karena tidak ada permukaan baru
yang dapat mengubah keadaan finansial.

| Kemampuan | Hak akses | Catatan |
| --- | --- | --- |
| Melihat keadaan finansial resep di layar kerja | `Prescription : Read` — **sudah ada** | Keterangan, bukan kewenangan |
| Memulai telaah, menyiapkan, menyerahkan | Butir yang sudah ada — **tidak berubah** | Gerbang finansial bekerja **di atas** hak akses, bukan menggantikannya |
| Mengubah keadaan finansial | **Tidak ada, dan tidak boleh diadakan** | Empat endpoint lamanya sudah dihapus permanen |

## Kewenangan yang **tidak** dapat dijaga mesin hak akses

| Yang tidak dijaga | Risikonya | Penjaga penggantinya |
| --- | --- | --- |
| Petugas berwenang menyerahkan obat yang belum beres finansialnya | Hak akses menjawab "boleh menyerahkan obat", bukan "obat ini boleh diserahkan" | Gerbang finansial pada empat titik (`PHA-DES-005`). Keduanya harus lolos |
| Penafsiran keadaan belum diketahui sebagai izin | Petugas dapat menyangka data yang kosong berarti aman | Sistem menolak; tidak ada tombol yang dapat ditekan. Pesannya menyatakan eksplisit obat belum boleh diserahkan |
| Keadaan darurat klinis yang sesungguhnya | Sistem tidak dapat membedakan darurat sungguhan dari alasan yang dibuat-buat | **Tidak dijaga, dan jalurnya belum ada.** `PHA-OQ-027` masih terbuka; sampai kebijakannya disahkan, tidak ada jalan melewati gerbang |

Baris ketiga adalah batas yang jujur: slice ini sengaja **tidak** menyediakan jalur darurat,
dan itu berarti keadaan darurat klinis nyata akan tertahan sampai kebijakannya diputuskan.

## Audit

| Kejadian | Dicatat | Isi payload |
| --- | :---: | --- |
| Salinan finansial diperbarui dari surat | Ya | Identitas resep, keadaan lama dan baru, nomor versi, korelasi |
| Surat diabaikan karena bernomor versi lebih rendah | Ya, sebagai catatan biasa | Identitas resep, nomor versi yang diabaikan dan yang tersimpan |
| Gerbang menolak melanjutkan pekerjaan | Ya | Identitas resep, gerbang yang menolak, keadaan finansial saat itu |
| Membaca keadaan finansial | Tidak | Konvensi project: `GET` tidak dicatat |

Payload audit **MUST NOT** memuat nama obat, dosis, aturan pakai, diagnosis, maupun nama pasien
— konsisten dengan aturan yang sudah berlaku pada dokumen ini. Keadaan finansial bukan data
klinis, sehingga mencatatnya aman; yang menyertainya tidak boleh ikut.

## Kolom sensitif

Tabel salinan finansial **tidak memuat satu pun kolom sensitif**, dan tidak memuat satu pun
kolom klinis. Itu hasil rancangan, bukan kebetulan: Billing hanya mengirim identitas dan keadaan.

Trace `PHA-DEC-063`, `PHA-DEC-067`, `PHA-DEC-069`, `PHA-OQ-027`.
