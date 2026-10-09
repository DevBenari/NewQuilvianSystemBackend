# Permintaan Persetujuan — Rencana ulang sesudah wadah dibatalkan (temuan T1)

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-023` |
| `tanggal` | 2026-10-09 |
| `pengaju` | `design-business-module`, menurunkan Amendment Pass putaran 29 |
| `rujukan` | `LAB-DEC-229`; decision log revision 94; `02-backend-architecture.md` bagian 30; `contracts/api-contract.md` bagian 41; `AC-323`..`AC-325`; temuan T1 [`BE-LAB-94.md`](../task/report/backend/BE-LAB-94.md) |
| `status` | ✅ **`disetujui`** 2026-10-09 — Yoga Aji Pratama, pemilik modul: *"setuju kelima butir"*; **kelima butir bagian 3 sesuai usulan** |
| `ditujukan kepada` | Yoga Aji Pratama — pemilik modul Laboratorium |
| `sifat` | Usulan amandemen satu kontrak (`LAB-API-v1` `r46`). Keputusan bisnisnya sudah diambil (`LAB-DEC-229`); yang diminta adalah persetujuan **bentuk aturannya**, terutama tafsir pemeriksaan pendahulu tanpa penanda. **Kode belum diubah** |

---

## 1. Satu paragraf untuk yang tidak punya waktu

Saat wadah dibatalkan lalu direncanakan ulang, CITO hasil keputusan dokter (*Tandai Cito* atau pencabutannya) saat ini
tidak terbawa. Usulan ini menyamakannya dengan pengambilan ulang: keputusan dokter pada pemeriksaan sebelumnya ikut
tersalin. Bila pemeriksaan sebelumnya tidak pernah disentuh dokter, sistem membaca CITO dari permintaan — supaya pemeriksaan
lama yang terlanjur "biasa" karena cacat sebelum `BE-LAB-94` tidak menyebarkan cacat itu lagi. Nol tabel, kolom, endpoint,
izin, migration, maupun perubahan layar.

## 2. Usulan amandemen kontrak

| Kontrak | Revisi usulan | Bagian | Isi |
|---|---|---|---|
| `LAB-API-v1` | `r46` | [41](../contracts/api-contract.md) | Perilaku `urgency` saat rencana ulang dan ambil ulang |

## 3. Lima butir yang diminta persetujuannya

| No | Butir | Usulan | Bila tidak disetujui |
|---|---|---|---|
| 1 | Pemeriksaan pendahulu | Pemeriksaan **terakhir** pesanan dan prosedur yang sama (tidak terhapus); untuk ambil ulang, pemeriksaan di wadah yang digantikan | Dibatasi pemeriksaan di wadah berstatus *Dibatalkan* saja — hasil sama, aturan lebih panjang |
| 2 | **Pendahulu tanpa penanda** | Kesegeraan dibaca dari **permintaan** (bukan disalin) — hasil sama untuk data baru, benar untuk data sebelum `BE-LAB-94` | Disalin mentah — pemeriksaan lama yang terlanjur "biasa" karena cacat menurunkan "biasa" ke wadah baru walau permintaannya CITO |
| 3 | Berlaku juga untuk **ambil ulang** | **Ya** — mengubah bunyi `r45` 40.3 "disalin" menjadi "disalin bila berpenanda, selain itu permintaan" | Ambil ulang tetap menyalin mentah; sampel yang ditolak sebelum rilis dan diambil ulang sesudahnya kehilangan CITO permintaan |
| 4 | Migration | **Tidak diubah** — pemeriksaan di wadah yang dibatalkan tidak dikerjakan; rencana ulang sesudah rilis sudah memakai aturan baru | Migration diperluas — tidak ada data yang membutuhkannya di dev |
| 5 | Pelaksanaan | Satu task kecil `BE-LAB-95` di atas `BE-LAB-94` (berkas yang sama), nol task frontend | — |

## 4. Cara menjawab

Cukup *"setuju kelima butir"*, atau sebutkan nomor butir yang ingin diubah.
