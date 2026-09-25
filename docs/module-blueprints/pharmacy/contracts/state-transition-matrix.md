# Farmasi — State Transition Routing Depo

Contract version: `PHA-STATE-ROUTING-v1`; status `approved`; input `PHA-DA-001-r1`; disetujui product/domain owner 21 Agustus 2026.

| Dari | Tindakan | Ke | Pelaku | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `NotEvaluated` | Resolve | `Resolved` | Sistem | Tepat satu kandidat | Proses berhenti |
| `NotEvaluated` | Resolve | `RejectedNoCandidate` | Sistem | Nol kandidat | Tampilkan kesalahan konfigurasi |
| `NotEvaluated` | Resolve | `RejectedAmbiguous` | Sistem | Lebih dari satu kandidat | Tampilkan kesalahan konfigurasi |
| `Resolved` | Revalidate | `Resolved` baru | Sistem | Kandidat tetap tepat satu | Histori evaluasi lama tidak diubah |
| `Resolved` | Revalidate | `Rejected*` | Sistem | Konfigurasi berubah | Jangan memindahkan Depo diam-diam |

Transisi langsung dari rejection ke reservasi atau dispense tidak sah.

---

# Slice Financial Clearance — `PHA-STATE-CLEARANCE-v1`

Status **draft** · input `PHA-DEC-063`–`070` · 21 September 2026. Terpisah dari kontrak Routing
Depo di atas; keduanya tidak saling mengubah.

## Transisi yang selama ini hilang

Inti slice ini adalah satu transisi yang **tidak pernah ada jalur kodenya**, sehingga seluruh
resep rawat jalan macet permanen.

| Dari | Tindakan | Ke | Pelaku | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `WaitingForPayment` | Surat clearance menyatakan resep boleh dikerjakan | `QueuedAtPharmacy` | Sistem, saat mengonsumsi surat | Keadaan clearance menyatakan boleh, dan hasil finansialnya salah satu dari lunas, disetujui penjamin, atau pembayaran ditiadakan | Resep tetap menunggu; tidak ada jalur manual yang dapat memaksanya |

## Keadaan salinan finansial

Salinan ini milik Farmasi, tetapi isinya ditentukan Billing.

| Keadaan | Arti | Boleh melanjutkan pekerjaan? |
| --- | --- | :---: |
| `CLEARED` | Billing menyatakan resep sudah beres secara finansial | **Ya** |
| `REVOKED` | Billing mencabut pernyataan itu | Tidak |
| `UNKNOWN` | Belum pernah ada surat untuk resep ini | Tidak |
| `PENDING_VERIFICATION` | Sinkronisasi sedang bermasalah dan sedang diperiksa | Tidak |
| `STALE` | Salinan diketahui tertinggal dari Billing | Tidak |

Tiga keadaan terakhir **MUST NOT** diperlakukan sebagai izin. `PHA-DEC-067` menyatakan tidak ada
override oleh siapa pun di Farmasi, termasuk Kepala Farmasi maupun Supervisor.

## Perpindahan salinan finansial

| Dari | Tindakan | Ke | Pelaku | Syarat |
| --- | --- | --- | --- | --- |
| `UNKNOWN` | Surat pertama diterima | `CLEARED` atau `REVOKED` | Sistem | Surat sah dan bernomor versi apa pun |
| `CLEARED` | Surat pencabutan diterima | `REVOKED` | Sistem | Nomor versi surat lebih tinggi dari yang tersimpan |
| `REVOKED` | Surat pemulihan diterima | `CLEARED` | Sistem | Nomor versi lebih tinggi |
| Keadaan apa pun | Surat bernomor versi lebih rendah diterima | **Tidak berubah** | Sistem | Surat diabaikan diam-diam; ini keadaan normal, bukan kesalahan |
| Keadaan apa pun | Sinkronisasi gagal berulang | `PENDING_VERIFICATION` lalu `STALE` | Sistem | Sesuai ambang percobaan ulang |
| `PENDING_VERIFICATION` atau `STALE` | Pemeriksaan ulang ke Billing berhasil | `CLEARED` atau `REVOKED` | Sistem | Jawaban Billing menjadi kebenarannya |

## Penahanan finansial atas pekerjaan yang sudah berjalan

`PHA-DEC-069`: resep **ditahan di tempat**, tidak ditarik mundur.

| Keadaan pemenuhan saat pencabutan tiba | Yang terjadi | Yang **tidak** terjadi |
| --- | --- | --- |
| `QueuedAtPharmacy` | Tetap di antrean, telaah tidak boleh dimulai | Tidak ditarik kembali ke menunggu pembayaran |
| `InPreparation` | Tetap dalam penyiapan, tidak boleh naik ke siap serah | Obat yang sudah diracik **tidak** direstock |
| `ReadyToDispense` | Tetap siap serah, penyerahan ditolak | Tidak diturunkan ke keadaan sebelumnya |
| `Dispensed` | **Tidak ada yang berubah** | Obat yang sudah diserahkan tidak dapat ditarik; akibat finansialnya urusan Billing |

Baris terakhir penting: begitu obat berpindah ke tangan pasien, pencabutan clearance tidak
mengubah apa pun di Farmasi. Yang tersisa adalah kewajiban finansial, dan itu milik Billing.

## Pemulihan setelah ditahan

| Dari | Tindakan | Ke | Syarat |
| --- | --- | --- | --- |
| Ditahan pada keadaan mana pun | Surat pemulihan diterima | Melanjutkan **dari keadaan terakhirnya** | Salinan kembali menyatakan boleh |

Resep **MUST NOT** mengulang antrean, telaah apoteker, maupun penyiapan yang sudah selesai
(`PHA-DEC-069`). Apoteker yang sudah meracik tidak meracik dua kali.

## Transisi yang tidak sah — disebutkan supaya tidak coba dibangun

| Transisi | Mengapa dilarang |
| --- | --- |
| `WaitingForPayment` → `QueuedAtPharmacy` tanpa surat | Jalur inilah yang dihapus `RJ-BIL-BE-002` karena siapa pun yang boleh mengubah resep dapat menyatakannya lunas |
| Penurunan keadaan pemenuhan karena pencabutan | Akan membuat catatan berbohong tentang keadaan fisik obat |
| Kemajuan pekerjaan saat salinan tidak menyatakan boleh | Gerbang dipasang di empat titik, bukan hanya penyerahan (`PHA-DES-005`) |
| Perubahan salinan finansial oleh petugas Farmasi | Tidak ada permukaannya, dan tidak boleh diadakan |

Trace `PHA-DEC-063`–`070`, `PHA-DES-001`–`006`. Tests `PHA-AT-CLR-01`–`PHA-AT-CLR-12`.
