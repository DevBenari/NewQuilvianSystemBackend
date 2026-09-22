# Medical Fee — Matriks Uji Penerimaan

| Field | Nilai |
|---|---|
| Kontrak | `MDF-TEST-1.0` — `locked` 20 September 2026 |
| Blueprint ID | `MF-BP-001` |
| Lapis | `U` unit, `I` integrasi, `E` end-to-end |

Setiap baris menyebut **aturan mana** yang diuji, sehingga tidak ada aturan pada
`validation-matrix.md` dan `state-transition-matrix.md` yang tanpa penjaga.

---

## 1. Daftar peran

| # | Uji | Lapis | Aturan |
|---:|---|:---:|---|
| T-01 | Tambah peran dengan kode baru | I | 1.1–1.3 |
| T-02 | Tambah peran dengan kode yang sudah ada | I | 1.3 |
| T-03 | Petakan `PrimarySurgeon` dua kali | I | 1.5 |
| T-04 | Petakan nilai yang bukan `OprTeamRole` | U | 1.4 |
| T-05 | Hapus peran yang dipakai baris tarif | I | 1.6 |
| T-06 | Pilih peran nonaktif pada baris tarif baru | I | 1.7 |

## 2. Kesepakatan tarif dan baris tarif

| # | Uji | Lapis | Aturan |
|---:|---|:---:|---|
| T-07 | Susun kesepakatan lengkap dengan dua baris tarif | I | 2.1–2.8 |
| T-08 | Kesepakatan menunjuk kontrak milik orang lain | I | 2.5 |
| T-09 | `effectiveStart` mendahului `StartDate` kontrak | I | 2.6 |
| T-10 | `effectiveEnd` melewati `EndDate` kontrak | I | 2.7 |
| T-11 | Dua kesepakatan `Active` tumpang-tindih untuk satu penerima | I | 2.9 |
| T-12 | Aktifkan kesepakatan tanpa baris tarif | I | 2.10 |
| T-13 | `sharingPercentage` bernilai 100.01 | U | 3.1 |
| T-14 | Dua baris lingkup dan peran sama, masa berlaku tumpang-tindih | I | 3.5 |
| T-15 | Jumlah persentase tiga peran = 110% | I | 3.6 |
| T-16 | Jumlah persentase tiga peran = 85% | I | 3.6 — **harus diterima** |
| T-17 | Ganti baris tarif; periksa baris lama tertutup dan `SupersededByRuleId` terisi | I | 3.7, `MDF-DES-009` |
| T-18 | Ubah persentase baris yang sudah dipakai rincian | I | 3.8 |
| T-19 | Masa berlaku baris tarif di luar masa kesepakatannya | I | 3.9 |

T-16 ada dengan sengaja: aturan memeriksa `<= 100`, dan uji yang hanya memeriksa penolakan
akan lolos walau implementasinya keliru memaksa tepat 100%.

## 3. Pemilihan baris tarif

| # | Uji | Lapis | Aturan |
|---:|---|:---:|---|
| T-20 | Baris tingkat 1 dan tingkat 4 sama-sama cocok | U | `sharing-rule.md` bagian 2 — tingkat 1 menang |
| T-21 | Hanya baris tingkat 3 cocok | U | Tingkat 3 dipakai |
| T-22 | Dua baris cocok pada tingkat sama untuk peran sama | U | 5.7 — `422`, bukan dicatat |
| T-23 | Layanan tanggal 30 Sept dengan tarif berubah 1 Okt | U | Memakai tarif lama |
| T-24 | Layanan tanggal 1 Okt dengan tarif berubah 1 Okt | U | Memakai tarif baru |

## 4. Perhitungan

| # | Uji | Lapis | Aturan |
|---:|---|:---:|---|
| T-25 | Operasi bertim tiga orang | E | `MDF-DES-013` — tiga rincian |
| T-26 | Jumlah `CalculatedAmount` = `GrossAmount` | I | 5.10 |
| T-27 | `BaseAmount` tidak berkurang oleh diskon promo | I | 5.8, `MF-DEC-013` |
| T-28 | `BaseAmount` tidak berkurang oleh diskon dokter | I | 5.8 |
| T-29 | `SharingPercentage` pada rincian tetap walau tarif diubah sesudahnya | I | `MDF-DES-010` |
| T-30 | `SharingRuleId` selalu terisi | I | 5.11 |
| T-31 | Pembulatan 2 desimal `AwayFromZero` | U | 5.9 |
| T-32 | Tiga peran dengan persentase tidak habis dibagi | U | `fee-calculation.md` bagian 4 — selisih tidak dialokasikan ulang |
| T-33 | Hitung ulang dua kali, hasil identik | E | `MDF-DES-015` |
| T-34 | Hitung ulang tidak menggandakan rincian | E | `MDF-DES-015` |
| T-35 | Hitung ulang mempertahankan koreksi yang sudah disetujui | I | `MDF-DES-015` butir 5 |
| T-36 | Perhitungan dengan `Idempotency-Key` yang sama dua kali | I | 4.6, `MDF-DES-006` |
| T-37 | Dua perhitungan periode bersamaan | I | `MDF-DES-007` — satu menang |
| T-38 | Layanan radiologi pada rentang periode | I | `integration-contract.md` 2.4 — diabaikan, tidak dicatat |

## 5. Layanan belum dapat dihitung

| # | Uji | Lapis | Aturan |
|---:|---|:---:|---|
| T-39 | `LabOrder` tanpa `ExaminerDoctorId` | I | 5.1 → `PERFORMER_MISSING` |
| T-40 | Penerima tanpa kesepakatan berlaku | I | 5.2 → `AGREEMENT_MISSING` |
| T-41 | Tidak ada baris tarif cocok | I | 5.3 → `RULE_MISSING` |
| T-42 | Layanan setelah kontrak berakhir | I | 5.4 → `CONTRACT_EXPIRED` |
| T-43 | Peran sumber tidak terpetakan | I | 5.5 → `ROLE_UNMAPPED` |
| T-44 | Layanan sama tidak muncul dua kali setelah hitung ulang | I | 5.12 |
| T-45 | Lengkapi pelaksana lalu hitung ulang | E | Status menjadi `Resolved`, jasa terbit |
| T-46 | Kesampingkan tanpa catatan | I | 7.1 |
| T-47 | Kesampingkan tanpa izin `Waive` | I | 7.2 |
| T-48 | Kembalikan baris `Waived` ke `Open` | I | 7.3 |
| T-49 | Tidak ada endpoint penghapus | I | `api-contract.md` bagian 9 |

## 6. Siklus status

| # | Uji | Lapis | Aturan |
|---:|---|:---:|---|
| T-50 | `Open` → `Calculated` → `Verified` → `Approved` → `Closed` | E | `state-transition-matrix.md` 1 |
| T-51 | `Open` → `Approved` langsung | I | 4.4 |
| T-52 | `Closed` → apa pun | I | 4.11 |
| T-53 | Buka kembali dari `Calculated` menghapus rincian | I | `state-transition-matrix.md` 1 |
| T-54 | Hitung ulang saat `Verified` | I | 4.10 |
| T-55 | Verifikasi periode saat masih ada hasil jasa `Calculated` | I | 4.7 |
| T-56 | `expectedRowVersion` usang | I | 4.5 → `409` |
| T-57 | Dua pengguna menyetujui periode bersamaan | I | 4.5 → satu `409` |
| T-58 | Hasil jasa `HandedOff` diubah nilainya | I | 6.4 |

## 7. Pemisahan wewenang

| # | Uji | Lapis | Aturan |
|---:|---|:---:|---|
| T-59 | Pemverifikasi menyetujui periode | I | 4.8 |
| T-60 | Pengaju menyetujui koreksinya sendiri — lewat service | I | 6.8 |
| T-61 | Pengaju menyetujui koreksinya sendiri — **langsung ke repository**, melewati service | I | 6.8 lapis DB — check constraint MUST menolak |
| T-62 | Penyusun tarif mengaktifkan kesepakatannya sendiri | I | `permission-audit-matrix.md` 3 |
| T-63 | Peran jabatan tidak memegang pasangan izin yang dilarang | U | `permission-audit-matrix.md` 3 |

T-61 adalah uji yang paling mudah dilupakan dan paling berharga: ia membuktikan lapis ketiga
benar-benar ada, bukan hanya tertulis di dokumen.

## 8. Koreksi

| # | Uji | Lapis | Aturan |
|---:|---|:---:|---|
| T-64 | Ajukan koreksi penambah, setujui, periksa `FinalAmount` | E | 6.1 |
| T-65 | Koreksi tanpa alasan | U | 6.7 |
| T-66 | Koreksi bernilai nol atau negatif | U | 6.6 |
| T-67 | Koreksi pengurang melebihi `GrossAmount` | I | 6.11 |
| T-68 | Ubah koreksi yang sudah `Approved` | I | 6.9 |
| T-69 | Tolak koreksi tanpa `rejectionReason` | I | 6.10 |
| T-70 | Ajukan koreksi dengan `Idempotency-Key` sama dua kali | I | 6.12 |

## 9. Penyerahan ke Finance

| # | Uji | Lapis | Aturan |
|---:|---|:---:|---|
| T-71 | Persetujuan periode membuat satu handoff per hasil jasa | E | 7.4 |
| T-72 | Nilai handoff = `FinalAmount`, kotor | I | `MF-DEC-005` |
| T-73 | Handoff kedua untuk hasil jasa sama | I | 7.5 |
| T-74 | Handoff dibuat untuk hasil jasa belum `Approved` | I | 7.4 |
| T-75 | ACK dengan `HandoffKey` keliru | I | 7.8 |
| T-76 | Kirim ulang yang gagal; `HandoffKey` tidak berubah | I | 7.7 |
| T-77 | Penolakan tanpa `failureReason` | I | 7.9 |
| T-78 | Handoff dan persetujuan gagal bersama saat transaksi dibatalkan | I | `MDF-DES-018` |
| T-79 | Tidak ada endpoint pembuat handoff manual | I | `api-contract.md` bagian 7 |

## 10. Pembatasan data

| # | Uji | Lapis | Aturan |
|---:|---|:---:|---|
| T-80 | Dokter A meminta hasil jasa dokter B | I | `permission-audit-matrix.md` 5 |
| T-81 | Dokter A meminta daftar tanpa filter | I | Hanya barisnya sendiri |
| T-82 | Dokter A membuka rincian milik B lewat id langsung | I | `404`, bukan `403` yang membocorkan keberadaannya |
| T-83 | Dokter A membuka rekap periode | I | `403` |
| T-84 | Staf Medical Fee membuka seluruh penerima | I | Berhasil |

T-82 memakai `404` dengan sengaja: `403` atas id tertentu membocorkan bahwa hasil jasa dengan id
itu ada.

## 11. Audit

| # | Uji | Lapis | Aturan |
|---:|---|:---:|---|
| T-85 | Perhitungan tercatat beserta jumlah penerima dan total | I | `permission-audit-matrix.md` 6 |
| T-86 | Pembukaan kembali tercatat beserta alasan | I | Idem |
| T-87 | Penggantian baris tarif tercatat persentase lama dan baru | I | Idem |
| T-88 | Pengesampingan tercatat beserta catatan | I | Idem |
| T-89 | Akses ke hasil jasa orang lain tercatat | I | Idem |
| T-90 | Soft delete mengisi `DeleteBy` dan `DeleteDateTime` | U | `IdentityModel` |

## 12. Yang sengaja tidak diuji pada revisi ini

| Tidak diuji | Alasan |
|---|---|
| Alir `DoctorShare` ke Billing | `MDF-11` `OPEN DECISION`, `MF-CQ-08` |
| Jasa dari entri bebas kasir | `MDF-07` `OPEN DECISION`, `MF-CQ-05` |
| Pembagian tim di luar kamar operasi | Menunggu `MF-CQ-07` |
| Perhitungan jasa radiologi | Ditunda `MF-DEC-015` |
| Potongan PPh 21 dan kasbon | Milik Finance (`FIN-BP-001`) |
| Pembayaran ke tenaga medis | Milik Finance |

## 13. Cakupan

| Sumber aturan | Jumlah aturan | Ada penjaga |
|---|---:|---:|
| `validation-matrix.md` | 61 | 61 |
| `state-transition-matrix.md` | 5 mesin status | 5 |
| Invariant `erd/fee-calculation.md` | 13 | 13 |
| Invariant `erd/sharing-rule.md` | 7 | 7 |
| Pemisahan wewenang | 3 pasangan | 3 |

Sembilan puluh uji untuk delapan puluh sembilan aturan. Yang berlebih adalah T-16 dan T-61 —
keduanya menguji bahwa aturan berlaku **ke arah yang benar**, bukan sekadar menolak sesuatu.
