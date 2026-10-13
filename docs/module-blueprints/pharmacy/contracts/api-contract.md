# Farmasi — API Contract Routing Depo

Contract version: `PHA-API-ROUTING-v1`. Status: `approved`. Owner API: Pharmacy Backend. `approved_by`: product/domain owner. `approved_at`: 21 Agustus 2026.

Tidak ada endpoint publik baru karena routing merupakan aturan internal workflow resep. Jika kelak endpoint diagnostik diperlukan, kontraknya harus melalui revision dan approval security baru; desain ini tidak mengizinkannya secara implisit.

---

# Slice Financial Clearance — `PHA-API-CLEARANCE-v1`

Status **draft** · input `PHA-DEC-063`–`070` · 21 September 2026.

## Tidak ada endpoint baru

Slice ini **tidak menambah satu pun endpoint**. Alasannya bukan kelalaian melainkan konsekuensi
langsung dari keputusan ownership:

| Kemampuan yang wajar diharapkan sebagai endpoint | Mengapa tidak ada |
| --- | --- |
| Menetapkan resep sudah dibayar | Inilah empat endpoint yang dihapus permanen oleh keputusan `1A`. Siapa pun yang boleh mengubah resep dapat menyatakannya lunas |
| Memicu sinkronisasi ulang secara manual | Pemeriksaan ulang dipanggil di dalam proses saat dibutuhkan, bukan lewat tombol. Tombol sinkronisasi manual mengundang kebiasaan menekannya sampai hasilnya menyenangkan |
| Membaca keadaan finansial resep sebagai endpoint tersendiri | Keadaan itu ikut pada response layar kerja resep yang sudah ada — lihat di bawah |

## Perubahan pada response yang sudah ada

Perubahannya **aditif**: tidak ada field yang dihapus maupun berubah arti.

### Health Services / Pharmacy Management / Prescription

Base URL: `api/v1/health-services/pharmacy-management/prescriptions`

| Method | Path | Yang berubah | Hak akses | Status |
| --- | --- | --- | --- | --- |
| `GET` | `/workspace` | Setiap baris resep bertambah keadaan finansial, alasan penahanan bila ada, dan kesegaran salinannya | `Prescription : Read` — **tidak berubah** | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Sama seperti di atas, untuk satu resep | `Prescription : Read` — **tidak berubah** | **Rencana (belum tersedia)** |

Field tambahan bersifat keterangan, bukan kewenangan: menampilkannya tidak memberi siapa pun
kemampuan mengubahnya.

Kode status tidak bertambah. Resep yang keadaan finansialnya belum diketahui tetap dikembalikan
`200` beserta penanda belum diketahui — **bukan** `404` dan **bukan** galat, karena resepnya
sendiri ada dan sah.

Trace `PHA-DEC-063`, `PHA-DEC-067`, `PHA-DEC-069`. Tests `PHA-AT-CLR-10`.

---

## Delta 12 Oktober 2026 — Drug Usage (`BE-BKC-FIX-017`)

| Endpoint | Perubahan |
| --- | --- |
| `POST /pharmacy-management/drug-usages` | `storageLocationId` dan `recordedByWorkforceId` menjadi **opsional**. Kosong → server memakai depo sesuai kunjungan (`PHA-DEPOT-ROUTING-v1`) dan profil petugas akun login; gagal → `422 PHM064`/`PHM063` dengan sebab. GUID kosong tidak pernah menjadi depo/petugas bawaan |
| `POST /pharmacy-management/drug-usages/{id}/record` | Sesudah stok berkurang, pemakaian **tanpa resep** menerbitkan satu fakta tagih per butir ke Billing (`BKC-DEC-138`); kiriman ulang memulihkan fakta yang belum terbit tanpa menyentuh stok |
| `GET /pharmacy-management/drug-usages/{id}` | Field baru `chargeState`, `chargeStateMessage`, `items[].chargeState` (dibaca dari Billing). Status `PhmDrugUsage` tetap `NotBilled`; `Billed` belum ditulis |

Retur atas pemakaian tanpa resep kini mengurangi tagihan butirnya. Rencana: [`PLAN-REPAIR-BKC-001`](../../billing-kasir/docs/plan-repair/plan-repair-001-integrasi-checkout-ke-billing-kasir.md) FIX-03, [laporan](../../billing-kasir/task/report/backend/BE-BKC-FIX-017.md).


## Amandemen PLAN-REPAIR-DOK-006 — 2026-10-13

Khusus repair kelas rawat inap, resolver obat menerima tarif sesuai atau umum kompatibel. NotCovered tetap IsValid=true; ConfigurationMissing tanpa tarif sesuai/umum IsValid=false dan menahan reguler/racikan/item/rekonsiliasi melalui guard existing. PATCH prescription-workspaces/{prescriptionId}/autosave mempertahankan hak PrescriptionWorkspace : Update. Tidak membuat endpoint harga tertunda.

Rujukan normatif delta yang disetujui: [RWI-KELAS-006-1.0](../../rawat-inap/dokter-rawat-inap/contracts/kelas-perawatan-repair-006.md), SHA256 `439623315c8cb4922e4794b7efb2218228d9e88fbd5f25e1473decd5087cd112`. Bagian lama yang bertentangan pada lingkup delta ini digantikan oleh amandemen; bagian lain tetap berlaku.
