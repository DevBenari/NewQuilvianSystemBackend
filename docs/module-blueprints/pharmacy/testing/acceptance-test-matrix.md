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

---

# Slice Financial Clearance — `PHA-TEST-CLEARANCE-v1`

Status **draft** · input `PHA-DEC-063`–`070` · 21 September 2026.

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `PHA-AT-CLR-01` | `PHA-DEC-063` | Surat menyatakan resep boleh dikerjakan diterima | Integrasi | Resep berpindah dari menunggu pembayaran ke antrean farmasi; salinan finansial terisi beserta nomor versinya |
| `PHA-AT-CLR-02` | `PHA-DEC-063` | Surat bernomor versi lebih rendah tiba setelah yang lebih tinggi | Integrasi | Salinan **tidak** berubah; tidak ada galat; tidak ada percobaan ulang |
| `PHA-AT-CLR-03` | `PHA-DEC-065` | Surat membawa hasil disetujui penjamin | Integrasi | Salinan mencatat disetujui penjamin, bukan lunas; gerbang penyerahan meloloskannya |
| `PHA-AT-CLR-04` | `PHA-DEC-069` | Pencabutan tiba saat resep sedang disiapkan | Integrasi | Keadaan pemenuhan **tetap** dalam penyiapan; kemajuan ke siap serah ditolak; **nol** baris stok bergerak |
| `PHA-AT-CLR-05` | `PHA-DEC-069` | Pencabutan tiba saat resep sudah siap serah | Integrasi | Penyerahan ditolak; keadaan pemenuhan tidak turun |
| `PHA-AT-CLR-06` | `PHA-DEC-069` | Surat pemulihan tiba setelah penahanan | Integrasi | Pekerjaan dilanjutkan dari keadaan terakhir; antrean, telaah, dan penyiapan **tidak** diulang |
| `PHA-AT-CLR-07` | `PHA-DEC-067` | Sinkronisasi gagal berulang sampai salinan dinyatakan tertinggal | Integrasi | Seluruh gerbang menolak; tidak ada permukaan override yang dapat dipanggil peran mana pun |
| `PHA-AT-CLR-08` | `PHA-DEC-067` | Resep yang belum pernah punya surat diperiksa | Unit | Dijawab belum diketahui; gerbang menolak; **bukan** galat |
| `PHA-AT-CLR-09` | `PHA-DES-005` | Telaah apoteker dicoba pada resep yang belum clear | Integrasi | Ditolak dengan alasan yang terbaca petugas, bukan galat teknis |
| `PHA-AT-CLR-10` | `PHA-API-CLEARANCE-v1` | Layar kerja resep dimuat | API | Setiap baris membawa keadaan finansial dan alasan penahanan; hak akses tidak berubah |
| `PHA-AT-CLR-11` | `PHA-DEC-063` | Dua surat untuk resep yang sama diproses bersamaan | Integrasi | Tepat satu baris salinan; nomor versi tertinggi yang menang |
| `PHA-AT-CLR-12` | `PHA-DEC-069` | Pencabutan tiba setelah obat **sudah** diserahkan | Integrasi | **Nol** perubahan di Farmasi; penyerahan tidak ditarik; riwayat utuh |

## Jalur gagal yang wajib dibuktikan

| ID | Skenario gagal | Bukti yang diharapkan |
| --- | --- | --- |
| `PHA-AT-CLR-04-F` | Racikan sudah jadi saat pencabutan tiba | Racikan **tidak** direstock (`PHA-DEC-032`); catatan penyiapan tetap menyatakan sudah diracik |
| `PHA-AT-CLR-07-F` | Kepala Farmasi mencoba melanjutkan resep saat salinan tertinggal | Ditolak. Tidak ada peran mana pun yang dapat melewatinya |
| `PHA-AT-CLR-08-F` | Gerbang dipanggil saat layanan Billing tidak terjangkau | Ditolak fail-closed; layar tetap dapat dimuat dan tidak menampilkan galat merah |

Tiga jalur gagal itu menguji satu hal yang sama: **ketidakpastian finansial tidak pernah boleh
berubah menjadi izin**, dan pekerjaan yang sudah terlanjur dikerjakan tidak pernah dibuang.

Trace `PHA-DEC-063`–`070`, `PHA-DES-001`–`006`.
