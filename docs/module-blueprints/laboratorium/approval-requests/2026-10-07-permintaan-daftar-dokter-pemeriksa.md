# Permintaan Persetujuan — Daftar dokter pemeriksa bagi pengonfirmasi dan jejak konfirmasi pada rincian

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-017` |
| `tanggal` | 2026-10-07 |
| `pengaju` | `design-business-module`, menurunkan amendment pass putaran 24 |
| `rujukan` | `LAB-DEC-200`..`LAB-DEC-203` (BR-139), `LAB-FE-034`; decision log revision 89; `02-backend-architecture.md` bagian 25; `AC-289`..`AC-293`; [`FE-LAB-50.md`](../task/report/frontend/FE-LAB-50.md) bagian 8 |
| `status` | ✅ **`disetujui`** 2026-10-07 — Yoga Aji Pratama, pemilik modul: *"Setuju kelima butir"*; **kelima butir bagian 3 sesuai usulan** |
| `ditujukan kepada` | Yoga Aji Pratama — pemilik modul Laboratorium |
| `sifat` | Usulan amandemen dua kontrak. Keputusan bisnisnya **sudah** diambil 2026-10-07 (putaran 24); yang diminta di sini adalah persetujuan **bentuk kontraknya** — lima butir di bagian 3 |

---

## 1. Satu paragraf untuk yang tidak punya waktu

Saat diuji dengan akun analis, Konfirmasi gagal di dua tempat. Pertama, daftar dokter pemeriksa diambil dari
daftar milik SDM yang hanya terbuka untuk admin dan kios, sehingga analis mendapat *Akses Ditolak* dan tidak dapat
memilih dokter. Kedua, sesudah Konfirmasi berhasil, server tidak mengirim balik siapa yang mengonfirmasi dan kapan,
sehingga baris baru tampil benar sesudah halaman dimuat ulang. Usulan ini menambah **satu daftar dokter milik
Laboratorium** yang dapat dibuka siapa pun yang boleh mengonfirmasi — isinya hanya nama, kode, dan spesialisasi
dokter aktif — dan **mengisi jejak konfirmasi** pada jawaban rincian pesanan seperti yang sudah dijanjikan kontrak
lama. Tidak ada tabel, kolom, izin, maupun aturan validasi baru. Urutan v1 dirilis serempak sesudah keduanya siap.

## 2. Usulan amandemen kontrak

| Kontrak | Revisi usulan | Bagian | Isi |
|---|---|---|---|
| `LAB-API-v1` | `r41` | [36](../contracts/api-contract.md) | `GET /lab-orders/examiner-doctor-options` baru; lima ruas jejak konfirmasi diisi pada `LabOrderDetailResponse` |
| `LAB-PERM-v1` | revision 14 | [16](../contracts/permission-audit-matrix.md) | Endpoint baru dipetakan ke `LabOrder : Confirm`; nol aksi baru |
| `LAB-STATE-v1`, `LAB-VAL-v1` | — | — | **Tidak berubah** |

## 3. Lima butir yang diminta persetujuannya

| No | Butir | Usulan | Bila tidak disetujui |
|---|---|---|---|
| 1 | Letak dan path daftar | **`GET /lab-orders/examiner-doctor-options`** di `LabOrderController` — milik Laboratorium, berdampingan dengan `confirm` | Sebutkan path lain; tidak mengubah perilaku |
| 2 | Isi setiap butir daftar | **`id`, `doctorCode`, `fullName`, `specialistName`** — tanpa subspesialisasi, foto, dan data kontak | Menambah ruas lain (mis. subspesialisasi) aman; menambah data kontak bertentangan dengan `LAB-DEC-201` |
| 3 | Penjaga | **Aksi `LabOrder : Confirm` dipakai ulang** pada endpoint baca ini; nol aksi baru, nol pemberian izin baru | Aksi baca tersendiri berarti satu langkah pemberian izin tambahan bagi analis saat rilis |
| 4 | Halaman dan pencarian | **Bawaan 25, maksimum 50**; cari pada nama, kode, dan spesialisasi | Angka lain tidak mengubah aturan bisnis |
| 5 | Jejak konfirmasi | **Lima ruas `r13` diisi lewat rincian pesanan**; jalur pembuatan pesanan tidak diubah karena pesanan baru belum pernah dikonfirmasi | Tanpa ini baris *Diterima* baru benar sesudah muat ulang (`AC-292` tidak terpenuhi) |

**Contoh sesudah disetujui.** Analis membuka Konfirmasi pada pesanan Ureum *Diterima*, mengetik "adit", memilih
*dr. Contoh Dokter, Sp.PK*, lalu Simpan. Baris tetap *Diterima*, kolom Konfirmasi langsung berisi nama analis,
waktu, dan dokter pemeriksa, dan Proses Pemeriksaan langsung dapat ditekan.

## 4. Usulan task

| Task | Gelombang | Isi |
|---|---|---|
| Backend baru | `MVP-12a` | Endpoint daftar dokter pemeriksa; lima ruas pada `GetDetailAsync`; satu predikat dokter-dapat-dipilih dipakai bersama `VAL-73` |
| Frontend baru | `MVP-12b` | Registry pilihan `labExaminerDoctors`; dialog Konfirmasi memakainya; uji ulang `FE-LAB-50` S4 |
| Langkah rilis | `MVP-12c` | **Serempak**: seluruh backend `MVP-12a` → `LabOrder : Confirm` bagi Analis (dev sudah) → seluruh frontend `MVP-12b` |

## 5. Yang tidak diminta

| Hal | Alasan |
|---|---|
| Mendahulukan dokter lab seperti v1 | `LAB-DEC-200` |
| Izin master dokter SDM bagi analis, atau perubahan `KioskRead` | `LAB-DEC-201`; milik SDM |
| Mengisi otomatis dokter pemeriksa | `LAB-FE-034` |
| Hak Batalkan bagi analis | `LAB-OPEN-052` |

## 6. Cara menyetujui

Balas dengan salah satu:

- *"Setuju kelima butir"* — kontrak `r41` dan revision 14 ditandai `approved`, lalu roadmap disusun.
- *"Setuju kecuali butir N: …"* — butir itu disesuaikan dulu, sisanya `approved`.
- *"Tolak"* beserta alasannya — kembali ke `/grill-me`.

## 7. Sesudah disetujui — ✅ diterapkan 2026-10-07

| Artefak | Perubahan |
|---|---|
| `LAB-API-v1` | `r41` `approved` — bagian 36 |
| `LAB-PERM-v1` | revision 14 `approved` — bagian 16 |
| `LAB-STATE-v1`, `LAB-VAL-v1` | Tidak berubah (`r8`, `r17`) |
| `02-backend-architecture.md`, `03-frontend-architecture.md` | Catatan status kontrak `approved` |
| `04-prd-to-mvp.md` | 26.7: `LAB-REQ-017` ditutup |
| Roadmap | Disusun `/plan-module-delivery` — gelombang `MVP-12` diperluas |
