# FLOW-BM-MVP-005 — Riwayat penggunaan dan koreksi

**draft**, 10 Oktober 2026; mengikuti [manifest](../blueprint-manifest.md), last_changed_in 0.12.0. Approval amandemen belum ada. Owner produk/domain Muhammad Hamzah; penugasan/SOP nyata tetap BM-G02/03.

Pelaku dan pemicu: Viewer history authorized, correction oleh existing authorized role. Trace: DEC-286–288; AC-402/415/416/418.

```mermaid
flowchart TD
 subgraph pembaca[Pembaca berwenang]
  A["Pilih bed dan periode"] --> B["Buka riwayat penggunaan"]
 end
 subgraph sistem[Sistem]
  B --> C{"Hak dan scope baca sah?"}
  C -- Tidak --> D["Tolak akses tanpa membuka identitas"]
  C -- Ya --> E["Tampilkan seluruh segmen dan versi sesuai hak"]
 end
 subgraph korektor[Petugas koreksi berwenang]
  E --> F{"Perlu koreksi salah catat?"}
  F -- Tidak --> G["Baca riwayat tanpa mengubahnya"]
  F -- Ya --> H["Ajukan koreksi beralasan"]
 end
 subgraph validasi[Sistem]
  H --> I{"Hak, versi dan guard Billing sah?"}
  I -- Tidak --> J["Tolak koreksi; riwayat lama tetap"]
  I -- Ya --> K["Tambahkan versi koreksi; teruskan integrasi existing"]
 end
 K --> G
 J --> E
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Baca riwayat | Pembaca berwenang | Bed/periode dalam scope | Segmen awal, transfer, masih berjalan, dan koreksi | Coba baca ulang bila gagal; tanpa hak tidak membuka identitas |
| Periksa konteks lama | Pembaca | Lokasi/kelas saat kejadian dan sumber konteks legacy | Perubahan master tidak mengubah snapshot lama | Data legacy yang tidak diketahui ditandai, tidak ditebak |
| Ajukan koreksi | Petugas koreksi | Alasan, versi dan hak koreksi existing | Permintaan diperiksa terhadap guard Billing | Jangan menghapus atau mengganti riwayat lama |
| Simpan versi | Sistem | Seluruh guard lolos | Versi baru dan integrasi existing; versi lama tetap | Rollback jika gagal; bila hasil tak pasti ikuti alur14 |

Reservasi/HK/closure audit tidak dihitung sebagai durasi pasien. Correction bukan transfer fisik; kategori tidak memicu harga. End=NULL ongoing; SupersededByCorrectionId menentukan versi efektif Billing.

Kontrak authoritative: [state](../contracts/state-transition-matrix.md), [API](../contracts/api-contract.md), [permission](../contracts/permission-audit-matrix.md), [acceptance](../testing/acceptance-test-matrix.md).
