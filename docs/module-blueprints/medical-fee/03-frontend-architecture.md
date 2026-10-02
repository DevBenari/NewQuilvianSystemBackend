# Medical Fee — Arsitektur Frontend

| Field | Nilai |
|---|---|
| Blueprint ID | `MF-BP-001` |
| Revision | `1` — `draft` |
| Frontend SHA | `66f36b432` |
| Kewenangan | **Product Owner.** Belum ada UI brief yang disetujui |
| Tanggal | 20 September 2026 |

---

## 1. Batas dokumen ini

Dokumen ini **bukan** rancangan antarmuka. Ia menetapkan **kontrak fungsional**: kemampuan apa
yang MUST dapat dilakukan pengguna, data apa yang MUST terlihat, dan pembatasan apa yang MUST
berlaku. Tata letak, komponen, navigasi, dan gaya visual adalah kewenangan Product Owner dan
belum diputuskan.

Alasannya tercatat: audit kemampuan menemukan **nol** klaim frontend untuk modul ini —
tidak ada layar, menu, Redux slice, maupun hook yang sudah ada. Merancang antarmuka tanpa UI
brief berarti mengambil keputusan yang bukan milik pass ini.

Yang tertulis di bawah adalah hal-hal yang **tidak boleh** diputuskan sendiri oleh frontend
karena berakar pada aturan bisnis yang sudah disetujui.

## 2. Kemampuan yang MUST tersedia

| # | Kemampuan | Untuk siapa | Berakar pada |
|---:|---|---|---|
| F-01 | Menyusun dan memetakan daftar peran | Admin Master | `MF-DEC-016` |
| F-02 | Menyusun kesepakatan tarif dan baris tarifnya | Staf Medical Fee | `MF-DEC-004`, `MF-DEC-012` |
| F-03 | Melihat kontrak HR yang ditunjuk satu kesepakatan | Staf Medical Fee | `MDF-DES-008` |
| F-04 | Mengganti tarif dengan versi baru bermasa berlaku | Staf Medical Fee | `MDF-DES-009` |
| F-05 | Membuka periode dan menjalankan perhitungan | Staf Medical Fee | `MF-DEC-006` |
| F-06 | Melihat ringkasan hasil perhitungan satu periode | Staf, Supervisor, Manajer | — |
| F-07 | **Melihat daftar layanan yang belum dapat dihitung, berikut alasannya** | Staf, Supervisor | `MF-DEC-018` |
| F-08 | Mengesampingkan layanan belum terhitung dengan catatan wajib | Manajer | `MF-DEC-018` |
| F-09 | Melihat hasil jasa per penerima | Staf, Supervisor, Manajer | — |
| F-10 | **Melihat rincian per layanan: peran, persentase, kesepakatan asal** | Seluruhnya | `MDF-DES-010` |
| F-11 | Memverifikasi hasil jasa dan periode | Supervisor | `MF-DEC-009` |
| F-12 | Menyetujui periode | Manajer | `MF-DEC-009` |
| F-13 | Menutup periode | Manajer | `MDF-DES-016` |
| F-14 | Mengajukan koreksi dengan alasan | Staf | `MF-DEC-007` |
| F-15 | Menyetujui atau menolak koreksi | Manajer | `MF-DEC-009` |
| F-16 | Melihat status penyerahan ke Finance | Staf, Finance | `MDF-DES-018` |
| F-17 | **Tenaga medis melihat jasanya sendiri beserta rinciannya** | Tenaga medis | `MDF-DES-011`, `MF-DEC-010` |

F-07 dan F-10 adalah dua kemampuan yang membedakan modul ini dari keadaan sekarang. Tanpa F-07,
layanan yang terlewat tetap tak terlihat — persis masalah yang dilaporkan di meeting. Tanpa
F-10, pertanyaan "mengapa jasa saya sekian" tetap dijawab dengan membuka Excel.

## 3. Yang MUST terlihat pada setiap rincian jasa

Ini bukan saran tata letak, melainkan daftar data yang MUST sampai ke pengguna (`MDF-DES-010`):

| Data | Mengapa wajib |
|---|---|
| Nama layanan dan tanggalnya | Menaut angka ke pekerjaan nyata |
| Modul asal layanan | Membedakan operasi, tindakan poli, dan laboratorium |
| Peran yang dipakai | Porsi berbeda menurut peran |
| Nilai kotor baris (`BaseAmount`) | Dasar perhitungan |
| Persentase yang dipakai | **Snapshot**, bukan tarif yang berlaku sekarang |
| Hasilnya (`CalculatedAmount`) | — |
| Nomor kesepakatan tarif asal | Menjawab "aturan mana yang dipakai" tanpa menebak |

Menyembunyikan persentase atau nomor kesepakatan akan mengembalikan modul ini ke keadaan
sekarang: angka yang benar tetapi tidak dapat dipertanggungjawabkan.

## 4. Matriks kewenangan UI

Frontend MUST menyembunyikan atau menonaktifkan aksi yang tidak berizin. Itu **bukan** pengganti
pemeriksaan backend — backend tetap menolak sendiri (`permission-audit-matrix.md`).

| Aksi | Izin yang dibutuhkan | Disembunyikan bila tidak ada |
|---|---|:---:|
| Susun / ubah peran | `MedicalFeeRole.Manage` | Ya |
| Susun / ubah kesepakatan tarif | `MedicalFeeSharingAgreement.Manage` | Ya |
| Aktifkan kesepakatan | `MedicalFeeSharingAgreement.Approve` | Ya |
| Jalankan perhitungan | `MedicalFeePeriod.Calculate` | Ya |
| Verifikasi | `MedicalFeePeriod.Verify` / `MedicalFeeServiceFee.Verify` | Ya |
| Setujui periode | `MedicalFeePeriod.Approve` | Ya |
| Tutup periode | `MedicalFeePeriod.Close` | Ya |
| Ajukan koreksi | `MedicalFeeServiceFee.RequestAdjustment` | Ya |
| Setujui / tolak koreksi | `MedicalFeeServiceFee.ApproveAdjustment` | Ya |
| Kesampingkan layanan belum terhitung | `MedicalFeeUnresolvedService.Waive` | Ya |
| Konfirmasi penyerahan | `MedicalFeeHandoff.Acknowledge` | Ya |

### 4.1 Aturan pemisahan wewenang di layar

Pengguna yang sudah memverifikasi satu periode MUST NOT melihat tombol setujui pada periode itu,
walau izinnya ada. Hal yang sama berlaku pada koreksi: pengaju MUST NOT melihat tombol setujui
pada koreksi yang ia ajukan sendiri.

Ini menghemat satu putaran gagal, bukan menggantikan penjagaan. Backend tetap menolak lewat
service **dan** check constraint (`MDF-DES-017`).

## 5. Pembatasan data untuk tenaga medis

`MedicalFeeServiceFee.View` yang dipegang tenaga medis **hanya** membuka barisnya sendiri, dan
penyaringannya terjadi di backend. Frontend MUST NOT mengandalkan penyembunyian di layar untuk
ini.

| Yang terlihat | Yang tidak |
|---|---|
| Hasil jasa dan rincian miliknya | Milik orang lain |
| Kesepakatan tarif dirinya sendiri | Milik orang lain |
| Riwayat koreksi pada jasanya | Rekap periode seluruh rumah sakit |
| — | Daftar layanan belum terhitung seluruh rumah sakit |

## 6. Perilaku yang MUST benar

| # | Keadaan | Perilaku |
|---:|---|---|
| 1 | Perhitungan periode berjalan | Menunjukkan bahwa proses sedang berjalan; MUST NOT mengizinkan perhitungan kedua bersamaan |
| 2 | Perhitungan selesai | Menampilkan jumlah penerima, total, **dan jumlah yang belum terhitung per alasan** |
| 3 | Penutupan periode ditolak | Menyebut jumlah layanan yang menahannya, dengan tautan ke daftarnya |
| 4 | `409` bentrok concurrency | Memberi tahu bahwa data berubah, memuat ulang, tanpa mengirim ulang diam-diam |
| 5 | Perintah pengubah nilai uang | Membawa `Idempotency-Key` yang dibuat sekali per percobaan, **bukan** per pengiriman ulang |
| 6 | Percobaan ulang setelah jaringan putus | Memakai `Idempotency-Key` yang sama |
| 7 | Nilai uang | Ditampilkan 2 desimal; MUST NOT dibulatkan ulang di frontend |
| 8 | Persentase pada rincian | Ditampilkan apa adanya sebagai snapshot, tanpa dibandingkan dengan tarif berjalan |
| 9 | Daftar belum terhitung kosong | Dinyatakan sebagai keadaan baik, bukan halaman kosong tanpa penjelasan |
| 10 | Layanan radiologi | MUST NOT ditampilkan sama sekali pada rilis ini (`MF-DEC-015`) |

Butir 5 dan 6 mudah terbalik. `Idempotency-Key` yang dibuat ulang setiap kali tombol ditekan
justru menghapus perlindungannya.

## 7. Yang MUST NOT dilakukan frontend

| Larangan | Alasan |
|---|---|
| Menghitung ulang nilai jasa di sisi klien | Satu-satunya sumber angka adalah backend; dua tempat perhitungan akan berbeda |
| Membulatkan ulang nilai uang | `fee-calculation.md` bagian 4 — pembulatan hanya terjadi sekali |
| Menyediakan cara menghapus layanan belum terhitung | Tidak ada endpoint-nya, dan itu disengaja |
| Menyediakan entri hasil jasa manual | Tidak ada endpoint-nya (`api-contract.md` bagian 9) |
| Mengandalkan penyembunyian tombol sebagai penjagaan wewenang | Backend yang menjaga |
| Menampilkan `DoctorShare` sebagai hasil Medical Fee | `OPEN DECISION`, `MF-CQ-08` |

## 8. Yang belum diputuskan dan menunggu Product Owner

| Hal | Catatan |
|---|---|
| Penempatan menu | Health Services atau kelompok tersendiri |
| Bentuk layar periode | Satu halaman bertahap, atau halaman terpisah per tahap |
| Cara menampilkan 15 ribu baris rincian | Paging server sudah tersedia; bentuk tampilnya belum diputuskan |
| Jalur masuk tenaga medis | Menu tersendiri atau bagian dari portal yang sudah ada |
| Cetak dan unduh | Belum ada keputusan bisnis; tidak termasuk rilis pertama |
| Pemberitahuan saat periode siap diverifikasi | Belum diputuskan |

Seluruhnya MUST diputuskan lewat UI brief sebelum pekerjaan frontend dimulai. Kontrak fungsional
pada dokumen ini berlaku apa pun bentuk yang dipilih.
