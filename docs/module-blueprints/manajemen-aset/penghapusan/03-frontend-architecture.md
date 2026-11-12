# Penghapusan & Pelepasan Aset — Arsitektur Frontend

| Field | Value |
|---|---|
| Sub-modul | `penghapusan` — `AST-HPS-1.0.0` `draft` |
| Route dasar | `src/app/corporate/asset-management/disposal/` dan `/reports/disposal` |

## 1. Kebutuhan layar

| ID | Layar | Menu | Endpoint |
|---|---|---|---|
| `SCR-AST-HPS-01` | Usulan Penghapusan (daftar + formulir) | 10.1 | `GET/POST /disposal/proposals`, `/reviewers` |
| `SCR-AST-HPS-02` | Pemeriksaan Kelayakan Penghapusan (usulan yang menunggu tim saya) | 10.2 | `GET /disposal/proposals?status=DalamPemeriksaan&awaitingMe=true`, `/feasibility` |
| `SCR-AST-HPS-03` | Persetujuan | 10.3 | `GET ?status=MenungguPersetujuan,MenungguPersetujuanDirektur&awaitingMe=true`, `/approve`, `/approve-director` |
| `SCR-AST-HPS-04` | Pemindahtanganan | 10.4 | `GET ?status=Disetujui`, `PUT /{id}/items/{itemId}/transfer` |
| `SCR-AST-HPS-05` | Berita Acara (finalisasi dan cetak) | 10.5 | `GET ?status=Dipindahtangankan,Disetujui,Final`, `/finalize`, `/document` |
| `SCR-AST-HPS-06` | Riwayat Aset Dihapus (+ ajukan/putuskan pemulihan) | 10.6 | `GET /disposal/disposed-assets`, `/disposal/restorations` |
| `SCR-AST-HPS-07` | Laporan Penghapusan | 11.5 | `GET /disposal/reports/disposals`, `/export` |
| `SCR-AST-HPS-08` | Detail Usulan (layar anak) | — | `GET /{id}`, `/{id}/amounts` |

## 2. Peta butir menu

Peta seluruh modul: [`../02-module-map.md`](../02-module-map.md) 3.1. Butir milik sub-modul ini: grup
**Penghapusan & Pelepasan Aset** (6 butir) dan **Laporan Penghapusan**. `SCR-AST-HPS-08` dibuka dari baris
`SCR-AST-HPS-01..07` dan dari tautan "Usulkan penghapusan" pada Detail Aset.

## 3. Skema fitur

### 3.1 Detail usulan — `SCR-AST-HPS-08`

```text
+- AST-HPS-000012 - Usang (3 aset) ------------------------------- SCR-AST-HPS-08 -+
| MenungguPersetujuanDirektur  Total nilai buku Rp 60.000.000 > batas Rp 50.000.000* |
| Pengusul: Petugas A  Tim: Pegawai B, C   Kepala Bagian Aset: setuju 14/08          |
| [Tunjuk tim] [Catat kelayakan] [Setujui/Tolak] [Catat pelepasan] [Finalisasi]      |
| [Batalkan] [Cetak berita acara]                                                    |
+------------------------------------------------------------------------------------+
| Aset | Kondisi | Kelayakan | Catatan | Metode | Nilai buku* | Pelepasan* | Laba/rugi* |
+------------------------------------------------------------------------------------+
  * hanya untuk peran ber-AssetDisposal : ViewAmount
```

| Wilayah | Sumber | Hak akses | Kosong / gagal |
|---|---|---|---|
| Kepala dan butir | `GET /{id}` | `AssetDisposal : Read` | Gagal → layar pesan + coba lagi |
| Kolom rupiah dan total | `GET /{id}/amounts` | `AssetDisposal : ViewAmount` | Tidak dirender tanpa hak |
| Tunjuk tim | `POST /{id}/reviewers` | `AssetDisposal : AssignReviewer` | — |
| Catat kelayakan | `POST /{id}/feasibility` | `AssetDisposal : RecordFeasibility` | Tampil hanya bagi anggota tim |
| Setujui/Tolak | `/approve` / `/approve-director` | `ApproveAsset` / `ApproveDirector` | Disembunyikan bagi pengusul/pemeriksa |
| Catat pelepasan | `PUT /items/{itemId}/transfer` | `AssetDisposal : RecordTransfer` | Disembunyikan untuk alasan Hilang |
| Finalisasi | `/finalize` | `AssetDisposal : Finalize` | 409 penyusutan → tautan ke `SCR-AST-NLI-04` |
| Cetak | `/document` | `AssetDisposal : Read` | — |

Tombol mengikuti `AvailableActions` dari backend.

### 3.2 Layar daftar (`01..07`)

Bentuk daftar bersama `register`. Kosong: `HPS-02` "Tidak ada usulan yang menunggu pemeriksaan Anda." ·
`HPS-03` "Tidak ada usulan yang menunggu persetujuan Anda." · `HPS-06` "Belum ada aset yang dihapus."
`SCR-AST-HPS-06` memuat tombol **[Pulihkan]** per baris (`AssetRestoration : Create`) dan antrean pemulihan
(`AssetRestoration : Approve`).

## 4. Aksi per peran

Dari [`contracts/permission-audit-matrix.md`](contracts/permission-audit-matrix.md) bagian 1.

## 5. Data dan status

Label dari `asset-status-labels.js`; rupiah dari backend.

## 6. Penanganan keadaan

Sama dengan `register` bagian 6. Konfirmasi finalisasi memakai `confirm-modal.jsx` dengan kalimat
"Aset akan dikeluarkan dari register dan tidak dapat dibatalkan kecuali lewat Pemulihan Aset."

## 7. Kewenangan UI

Tata letak dan rupa: `DEV_DISCRETION`. Rupiah tersembunyi dari peran tanpa hak: wajib.
