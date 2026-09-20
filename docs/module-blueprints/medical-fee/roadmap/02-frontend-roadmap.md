# Medical Fee — Roadmap Frontend

## 1. Identitas

```yaml
roadmap_id: MDF-ROADMAP-FE-001
parent_roadmap: MDF-ROADMAP-001 revisi 4
roadmap_revision: 1
roadmap_status: ACTIVE_BLOCKED_ON_UI_BRIEF
blueprint_id: MF-BP-001
blueprint_revision: 2
frontend_commit_sha: 66f36b432
frontend_authority: Product Owner — UI brief BELUM ada
contracts:
  MDF-API-1.0: locked 2026-09-20
  MDF-PERM-1.0: locked 2026-09-20
```

Payung roadmap ada di `00-delivery-roadmap.md`. Pasangan backend-nya ada di
`01-backend-roadmap.md`.

**Berkas ini tidak menyatakan satu pun pekerjaan selesai, dan tidak memutuskan satu pun
rancangan antarmuka.**

## 2. Keadaan frontend sekarang

**Nol.** Audit kemampuan pada `66f36b432` menemukan tidak ada satu pun layar, butir menu, Redux
slice, maupun hook untuk Medical Fee. Seluruh pekerjaan frontend modul ini adalah membangun
dari nol.

Ini berbeda dari Finance, yang setidaknya punya group menu `corporateFinance` dan dua halaman
Petty Cash yang sudah berjalan. Di sini tidak ada apa pun untuk dijadikan titik mula, sehingga
UI brief justru **lebih** menentukan.

## 3. Yang menahan

| Penahan | Menahan apa | Keadaan |
|---|---|---|
| Kontrak API belum terkunci | Kerja paralel dengan backend | **Sudah tercabut** — `MDF-API-1.0` terkunci 20 September 2026 |
| **UI brief belum ada** | Seluruh task `FE-MDF-*` | **Masih berlaku** — keputusan Product Owner |
| Endpoint belum dibangun | Pengujian nyata tiap layar | Menyusul per gelombang backend |

## 4. Kemampuan yang MUST tersedia

Diturunkan dari `03-frontend-architecture.md` bagian 2. Kolom Task menunjukkan siapa yang
memikulnya.

| # | Kemampuan | Untuk siapa | Task |
|---:|---|---|---|
| `F-01` | Menyusun dan memetakan daftar peran | Admin Master | `FE-MDF-005` |
| `F-02`..`F-04` | Kesepakatan tarif, kontrak HR yang ditunjuk, penggantian berversi | Staf | `FE-MDF-001` |
| `F-05`, `F-06` | Membuka periode, menjalankan perhitungan, melihat ringkasannya | Staf, Supervisor, Manajer | `FE-MDF-002` |
| `F-07`, `F-08` | Daftar layanan belum dapat dihitung; pengesampingan bercatatan | Staf, Supervisor, Manajer | `FE-MDF-002` |
| `F-09`, `F-10` | Hasil jasa per penerima beserta rincian per layanan | Seluruhnya | `FE-MDF-002` |
| `F-11`..`F-13` | Verifikasi, persetujuan, penutupan periode | Supervisor, Manajer | `FE-MDF-003` |
| `F-14`, `F-15` | Mengajukan, menyetujui, menolak koreksi | Staf, Manajer | `FE-MDF-003` |
| `F-16` | Melihat status penyerahan ke Finance | Staf, Finance | `FE-MDF-006` |
| `F-17` | Tenaga medis melihat jasanya sendiri | Tenaga medis | `FE-MDF-004` |

`FE-MDF-005` dan `FE-MDF-006` **baru** pada roadmap frontend ini. Revisi payung sebelumnya
melewatkan keduanya: `F-01` dan `F-16` tidak punya task frontend sama sekali, padahal
backend-nya (`BE-MDF-004`, `BE-MDF-014`) ada dan kemampuannya tercantum sebagai MUST.

## 5. Task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-MDF-005` | Pengelolaan daftar peran | `F-01`, `MF-DEC-016` | `MDF-API-1.0` §2, `MDF-PERM-1.0` | — | CRUD peran beserta pemetaan ke `OprTeamRole` | `BE-MDF-004` **+ UI brief** | Pemetaan kedua ke nilai `OprTeamRole` yang sama ditolak dengan pesan yang dapat dibaca | `T-01`..`T-06` | **Product Owner** — `DEV_DISCRETION` | Peran terpakai ditampilkan nonaktif, bukan hilang |
| `FE-MDF-001` | Kesepakatan tarif dan baris tarif | `F-02`..`F-04` | `MDF-API-1.0` §3 | — | Kesepakatan, baris tarif, penggantian berversi | `BE-MDF-006` **+ UI brief** | Masa berlaku dan kontrak HR yang ditunjuk terlihat pada layar | `T-07`..`T-19` | Product Owner | Tidak menghitung ulang nilai di klien |
| `FE-MDF-002` | Periode, hasil jasa, rincian, dan daftar belum terhitung | `F-05`..`F-10` | `MDF-API-1.0` §4–6 | — | Ringkasan perhitungan, rincian per layanan, daftar belum terhitung beserta rekap per alasan | `BE-MDF-011` **+ UI brief** | Setiap rincian menampilkan peran, **persentase snapshot**, dan nomor kesepakatan; penutupan yang tertolak menyebut jumlah penahannya beserta tautan ke daftarnya | `T-25`..`T-49` | Product Owner | `Idempotency-Key` dibuat sekali per percobaan, bukan per pengiriman ulang |
| `FE-MDF-003` | Verifikasi, persetujuan, koreksi | `F-11`..`F-15` | `MDF-API-1.0`, `MDF-PERM-1.0` §3 | — | Tombol aksi menurut izin; penyembunyian pasangan maker-checker | `BE-MDF-013` **+ UI brief** | Pengaju tidak melihat tombol setujui pada koreksi yang ia ajukan sendiri | `T-59`..`T-63` | Product Owner | Penyembunyian tombol **bukan** pengganti penjagaan backend |
| `FE-MDF-006` | Pemantauan penyerahan ke Finance | `F-16` | `MDF-API-1.0` §7 | — | Daftar penyerahan beserta status `Created`, `Acknowledged`, `Failed`; pengiriman ulang yang gagal | `BE-MDF-014` **+ UI brief** | Yang `Failed` terlihat beserta alasannya dan dapat dikirim ulang | `T-71`..`T-79` | Product Owner | **Tanpa** tombol membuat penyerahan manual — tidak ada endpoint-nya |
| `FE-MDF-004` | Tenaga medis melihat jasanya sendiri | `F-17` | `MDF-API-1.0`, `MDF-PERM-1.0` §5 | — | Hasil jasa dan rinciannya, terbatas pada dirinya | `BE-MDF-017` **+ UI brief** | Hanya barisnya sendiri yang terlihat | `T-80`..`T-83` | Product Owner — **risiko privasi** | Penyaringan terjadi di backend; layar **tidak** mengandalkan penyembunyian |

## 6. Yang MUST diputuskan UI brief

| # | Yang perlu diputuskan | Kenapa bukan wewenang developer |
|---:|---|---|
| 1 | Penempatan menu — Health Services atau group tersendiri | Modul ini belum punya jejak apa pun di sidebar |
| 2 | Bentuk layar periode — satu halaman bertahap atau halaman terpisah per tahap | Menentukan alur kerja bulanan seluruh tim |
| 3 | Cara menampilkan rincian berjumlah belasan ribu baris | Paging server sudah tersedia; bentuk tampilnya belum diputuskan |
| 4 | Jalur masuk tenaga medis — menu tersendiri atau bagian portal yang sudah ada | Menyangkut siapa yang melihat data penghasilan |
| 5 | Cetak dan unduh | Belum ada keputusan bisnisnya; tidak termasuk rilis pertama |
| 6 | Pemberitahuan saat periode siap diverifikasi | Belum diputuskan |

Butir 3 pantas diperhatikan: satu periode nyata menghasilkan ±15.000 baris rincian untuk ±220
penerima. Menampilkannya tanpa rencana akan membuat layar tidak terpakai justru pada bulan
pertama.

## 7. Perilaku yang MUST benar

Diturunkan dari `03-frontend-architecture.md` bagian 6.

| # | Keadaan | Perilaku |
|---:|---|---|
| 1 | Perhitungan periode berjalan | Menunjukkan proses berjalan; MUST NOT mengizinkan perhitungan kedua bersamaan |
| 2 | Perhitungan selesai | Menampilkan jumlah penerima, total, **dan jumlah belum terhitung per alasan** |
| 3 | Penutupan periode ditolak | Menyebut jumlah layanan yang menahannya, dengan tautan ke daftarnya |
| 4 | `409` bentrok concurrency | Memberi tahu data berubah, memuat ulang, tanpa mengirim ulang diam-diam |
| 5 | Perintah pengubah nilai uang | Membawa `Idempotency-Key` yang dibuat **sekali per percobaan** |
| 6 | Percobaan ulang setelah jaringan putus | Memakai `Idempotency-Key` yang **sama** |
| 7 | Nilai uang | 2 desimal; MUST NOT dibulatkan ulang di klien |
| 8 | Persentase pada rincian | Ditampilkan apa adanya sebagai snapshot, tanpa dibandingkan dengan tarif berjalan |
| 9 | Daftar belum terhitung kosong | Dinyatakan sebagai keadaan baik, bukan halaman kosong tanpa penjelasan |
| 10 | Layanan radiologi | MUST NOT ditampilkan sama sekali (`MF-DEC-015`) |

Butir 5 dan 6 mudah terbalik. `Idempotency-Key` yang dibuat ulang setiap kali tombol ditekan
justru menghapus perlindungannya.

## 8. Yang frontend MUST NOT lakukan

| Larangan | Alasan |
|---|---|
| Menghitung ulang nilai jasa di klien | Satu-satunya sumber angka adalah backend |
| Membulatkan ulang nilai uang | Pembulatan hanya terjadi sekali, saat `CalculatedAmount` dihitung |
| Menyediakan cara menghapus layanan belum terhitung | Tidak ada endpoint-nya, dan itu disengaja (`MF-DEC-018`) |
| Menyediakan entri hasil jasa manual | Tidak ada endpoint-nya (`api-contract.md` bagian 9) |
| Menyediakan tombol membuat penyerahan ke Finance | Penyerahan hanya lahir dari persetujuan |
| Mengandalkan penyembunyian tombol sebagai penjagaan wewenang | Backend yang menjaga |
| Menampilkan `DoctorShare` sebagai hasil Medical Fee | `MF-CQ-08` belum dijawab |
| Mulai dengan data tiruan yang bentuknya ditebak | `MDF-API-1.0` sudah terkunci; bentuk itulah yang dipakai |

## 9. Urutan yang disarankan

```text
UI brief turun
   ↓
FE-MDF-005   setelah BE-MDF-004    (daftar peran — paling kecil, menetapkan pola)
   ↓
FE-MDF-001   setelah BE-MDF-006    (kesepakatan tarif)
   ↓
FE-MDF-002   setelah BE-MDF-011    (periode, hasil jasa, belum terhitung — layar inti)
   ↓
FE-MDF-003   setelah BE-MDF-013    (verifikasi, persetujuan, koreksi)
   ↓
FE-MDF-006   setelah BE-MDF-014    (pemantauan penyerahan)
FE-MDF-004   setelah BE-MDF-017    (portal tenaga medis — boleh paralel dengan FE-MDF-006)
```

`FE-MDF-002` adalah layar yang paling menentukan apakah modul ini dipakai atau ditinggalkan.
Dua kemampuan di dalamnya yang membedakannya dari keadaan sekarang: daftar layanan yang belum
dapat dihitung (`F-07`) dan rincian yang menjelaskan asal setiap rupiah (`F-10`). Tanpa
keduanya, orang akan kembali membuka Excel.

## 10. Definition of Done frontend

| # | Kriteria |
|---:|---|
| 1 | UI brief disetujui Product Owner sebelum task pertama dimulai |
| 2 | Bentuk data mengikuti `MDF-API-1.0` apa adanya — nol tebakan |
| 3 | Aksi disembunyikan atau dinonaktifkan sesuai `MDF-PERM-1.0` |
| 4 | Nol perhitungan dan nol pembulatan nilai uang di klien |
| 5 | `409` memuat ulang data dan memberi tahu pengguna |
| 6 | `Idempotency-Key` dibuat sekali per percobaan, dipakai ulang saat mencoba lagi |
| 7 | Setiap rincian menampilkan peran, persentase snapshot, dan nomor kesepakatan |
| 8 | Penutupan yang tertolak menyebut jumlah penahannya beserta tautan |
| 9 | Layar tenaga medis diuji dengan dua akun berbeda, bukan hanya satu |
| 10 | Nol layar untuk radiologi, entri bebas kasir, maupun `DoctorShare` |
