# FLOW-DNH-MVP-003 — Impor susunan denah dan kesiapan awal

**approved** Muhammad Hamzah, 2026-10-16 (`RWI-DEC-324`), ditulis 15 Oktober 2026; mengikuti [manifest](../blueprint-manifest.md), last_changed_in 0.13.0. Owner produk/domain Muhammad Hamzah; penanggung jawab keperawatan untuk pengesahan produksi.

Pelaku dan pemicu: admin impor berwenang. Dev/UAT: sesudah data rawat inap DB Hamzah dibersihkan. Produksi: sekali, sesudah RS mengesahkan susunan kamar. Prasyarat: kelas resmi sudah diterapkan bersama konfigurasi urutan kelas; unit rawat inap tersedia. Trace: DEC-306/321/322/323; AC-446/459–461.

```mermaid
flowchart TD
 subgraph admin[Admin impor]
  A["Buka Impor dari Master Bed"] --> B["Periksa tabel kamar dari denah"]
  B --> C["Petakan setiap kamar ke unit; pilih kamar lama bila sudah ada"]
  C --> D["Jalankan pemeriksaan"]
 end
 subgraph sistem[Sistem]
  D --> E{"Ada kamar yang ditolak?"}
  E -- Ya --> F["Laporkan alasan per kamar"]
  E -- Tidak --> G["Siap diimpor"]
  H["Simpan kamar dan bed yang lolos sekaligus"] --> I{"Lingkungan uji?"}
  I -- Ya --> J["Bed baru langsung Tersedia sebagai data uji"]
  I -- Tidak --> K["Bed baru Tidak Tersedia sampai disahkan"]
 end
 subgraph keputusan[Admin impor]
  F --> L{"Perbaiki dulu?"}
  L -- Ya --> C
  L -- Tidak --> M["Impor kamar yang lolos saja"]
  G --> M
 end
 M --> H
 subgraph perawat[Perawat verifikator]
  K --> N["Periksa dan sahkan bed per lantai"]
 end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Siapkan tabel | Admin | Data denah frontend: lantai, nomor, kelas, jumlah bed dan boks, arah | Tabel kamar lengkap | Kelas belum ada di master: terapkan kelas resmi dulu |
| Petakan unit | Admin | Unit rawat inap yang diakui RS | Setiap kamar punya unit; kamar lama dipilih bila sudah ada | Kamar tanpa unit ditolak |
| Periksa | Sistem | Tabel kamar | Ringkasan dibuat/diperbarui/tanpa perubahan/ditolak beserta alasan | Perbaiki pemetaan atau lanjutkan tanpa kamar yang ditolak |
| Impor | Sistem | Kamar yang lolos | Kamar dan bed tersimpan bersamaan; catatan impor | Simpan gagal: tidak ada yang tersimpan; ulangi |
| Ulang impor | Admin | Tabel yang sama | Tidak ada kamar maupun bed baru | Kamar lama yang berbeda dari denah hanya dilaporkan |
| Sahkan bed (produksi) | Perawat verifikator | Pemeriksaan fisik per bed | Bed Tersedia | Belum siap: catat alasan; ikuti alur 11 |

Importer tidak pernah menghapus kamar, tidak mengubah kelas, unit, maupun bed kamar yang sudah ada, dan tidak membuat unit baru. Di produksi, bed hasil impor tidak dapat langsung Tersedia.

Kontrak authoritative: [state](../contracts/state-transition-matrix.md), [API](../contracts/api-contract.md), [validasi](../contracts/validation-matrix.md), [acceptance](../testing/acceptance-test-matrix.md).
