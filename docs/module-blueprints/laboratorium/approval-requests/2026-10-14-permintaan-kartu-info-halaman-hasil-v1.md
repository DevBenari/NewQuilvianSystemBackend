# Permintaan Persetujuan — Kartu informasi halaman hasil v1

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-025` |
| `tanggal` | 2026-10-14 |
| `pengaju` | Sesi pembaruan dokumen Lab 2026-10-14 (temuan sapuan status: `r48` ada di kode, belum ada di kontrak) |
| `rujukan` | `LAB-EVD-013`; `LAB-REQ-024` (`r47`); commit BE `36422a4d` dan FE `9aba9929d` (2026-10-13); `contracts/api-contract.md` bagian 43 |
| `status` | ✅ **`disetujui`** 2026-10-14 — Yoga Aji Pratama, pemilik modul: *"saya setuju buatkan surat permintaan persetujuannya dan bagian kontraknya"* |
| `ditujukan kepada` | Yoga Aji Pratama — pemilik modul Laboratorium |
| `sifat` | Amandemen aditif `LAB-API-v1` `r48`, **persetujuan susulan**. Kodenya sudah ada sejak 2026-10-13. Nol tabel, kolom, migration, endpoint, maupun izin |

---

## 1. Ringkasan

Halaman hasil v1 (Patologi Klinik, Mikrobiologi, Patologi Anatomi) menampilkan dua kartu di atas
hasil: *Informasi Pasien* dan *Informasi Pemeriksaan*. Isinya antara lain Poliklinik/Kamar, Kelas,
Penjamin, Dokter Perujuk/DPJP, Tanggal Sampling, Tanggal Pemeriksaan, dan Penanggung Jawab Analis.
Rincian order V2 belum mengirim data itu. Sumbernya sudah ada di database.

Pada 2026-10-13 backend menambah sembilan ruas baca untuk kartu itu dan menandainya `r48` di
komentar kode. Kontraknya belum ditulis dan belum ada permintaan persetujuan. Surat ini menutup
selisih itu.

## 2. Butir yang dimintakan persetujuan

| No | Butir | Keputusan |
|---|---|---|
| 1 | Delapan ruas baru pada `LabOrderDetailResponse`, hanya diisi `GET /lab-orders/{id}`: `clinicName`, `roomName`, `patientClassName`, `paymentType`, `payerName`, `referringDoctorName`, `firstCollectedAt`, `firstExaminedAt` | ✅ Disetujui |
| 2 | Satu ruas baru `resultEnteredByName` pada `LabExaminationResultFormResponse`, yaitu nama pengisi hasil untuk *Penanggung Jawab Analis* | ✅ Disetujui |
| 3 | Sumber `payerName`, `firstCollectedAt`, dan `firstExaminedAt` sama dengan daftar pantau `r47`, supaya satu order terbaca sama di daftar dan di halaman hasil | ✅ Disetujui |
| 4 | Persetujuan ini berlaku susulan untuk kode yang sudah ada sejak 2026-10-13. Kode tidak perlu diubah | ✅ Disetujui |

## 3. Privasi dan dampak

| Hal | Isi |
|---|---|
| Hak akses | Tidak berubah. `GET /lab-orders/{id}` tetap `LabOrder : Read`; kedua jalur hasil tetap `LabExamination : Read` |
| Privasi | Data kunjungan dan penjamin yang sama sudah dibuka daftar pantau (`r47`) kepada petugas laboratorium. Nama pengisi hasil sejenis dengan nama pemvalidasi dan perilis yang sudah ada sejak `r34` |
| Konsumen lama | Nol dampak. Ruas baru diabaikan pembaca lama |

## 4. Di luar permintaan ini

- Verifikasi dengan akun asli **sudah dijalankan 2026-10-14** sesudah surat ini disetujui: HTTP 8/8 ruas pada empat pesanan dan peramban 54/54 dengan dr. Bima dan Gilang (`testing/acceptance-test-matrix.md`, amandemen 2026-10-14). Itu langkah pembuktian, bukan bagian kontrak.
- Status konfirmasi dokter lewat WhatsApp di kartu v1 tetap menunggu layanan WhatsApp (sama dengan `LAB-REQ-024` bagian 3).
