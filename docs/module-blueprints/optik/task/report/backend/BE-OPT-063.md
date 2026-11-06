# Laporan Perubahan Backend — `BE-OPT-063`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-063` |
| Judul | Uji kontrak diskon Optik pada Billing |
| Slice | Temuan audit `T2` — `EPIC OPT-08` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r6) — Rincian `BE-OPT-063` |
| Trace | `FR-OPT-082`; `OPT-DEC-079`, `084`; `OPT-AC-016`, `OPT-AC-037` baris kedua; `UAT-29`; `T2` |
| Contract version | `OPT-INT-v1.3` `INT-OPT-11`, `OPT-API-v1.4`, `OPT-PERM-v1.3` — `approved` sisi Optik, Yoga Aji Pratama 2026-11-06 |
| Approval task | Yoga Aji Pratama, 2026-11-06 ("lanjutkan sesuai saran kamu", atas tawaran temuan `T2`) |
| Dependency | `BE-OPT-032` ✅, `BE-OPT-061` ✅, `FE-OPT-041` ✅, `OPT-APR-014` ✅ |
| Klasifikasi | `LIGHT` — satu berkas uji, tanpa kode produksi |
| Task mode | `BACKEND` |
| Target tulis | `Tests/QuilvianSystemBackend.OpticalTests/Be063DiscountBillingTests.cs` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `06e40fb7` (branch `yoga`) + `BE-OPT-060`..`062` di working tree |
| Tanggal | 2026-11-06 |
| Status | Selesai dan tervalidasi di harness. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` (uji); `BillingManagement` hanya dipanggil |
| Registry | `Opt` dan `Bil` terdaftar |
| Keberlakuan | `NEW CODE` pada berkas uji saja |
| QBE yang berlaku | Tidak ada; tidak ada kode produksi yang berubah |
| Wewenang terpisah | Source uji ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Audit 2026-11-06 menemukan bahwa diskon Optik hanya lulus dengan API tiruan. Uji backend Optik tidak memuat diskon sama sekali, sehingga kesimpulan "Billing menghitung dengan benar" hanya berasal dari membaca kode (`T2`).

Task ini membuktikannya dengan menjalankan service Billing sungguhan. Tidak ada selisih dengan kontrak, sehingga kode Billing tidak perlu diubah.

---

## 2. Proses bisnis yang dibuktikan

**Pelaku:** petugas penjualan atau Kepala Optik.

**Langkah utama:**

1. Pesanan frame Rp 450.000 + lensa Rp 600.000 ditagihkan. Bagian pasien Rp 1.050.000, minimum uang muka Rp 525.000.
2. Petugas menerapkan `OPT-DISC-10` pada baris frame.
3. Billing mencatat diskon Rp 45.000, langsung berlaku tanpa langkah persetujuan.
4. Baris frame menjadi Rp 405.000, sedangkan baris lensa tetap Rp 600.000.
5. Bagian pasien menjadi Rp 1.005.000, dan minimum uang muka menjadi Rp 502.500.

**Contoh:** terbayar Rp 502.400 ditolak `OPT-VAL-046` dengan pesan minimum Rp 502.500. Sesudah ditambah Rp 100, pesanan diterima masuk pengerjaan.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Kebijakan yang sama pada baris yang sama | Ditolak konflik; tagihan tidak berkurang dua kali |
| Pemanggil mengetik nominal | Ditolak *"Nominal promo ditentukan oleh master policy…"* |
| Tagihan sudah final | Ditolak *"Invoice final tidak dapat menerima perubahan diskon."* |
| Versi tagihan basi | Ditolak *"Data telah berubah. Muat ulang sebelum melanjutkan."* |

**Risiko yang kini terbukti, bukan lagi dugaan.** Billing tidak membatasi sumber maupun nilai kebijakan. Pemanggil langsung tetap bisa memakai kebijakan tanpa awalan `OPT-`, dan kebijakan `OPT-` bernilai di atas 10% tetap berlaku. Batas persentase di Billing adalah 100%. Risiko ini sudah diterima `OPT-DEC-079`, dan kini dipagari uji karakterisasi: bila Billing kelak menegakkan batasnya sendiri, uji itu gagal, sehingga kontrak perlu disegarkan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BillingDiscountService`, `BillingCalculationService`, `DiscountPolicyService`, `MstDiscountPolicy`, `MstDiscountPolicyConfiguration`, `OptOrderPaymentGate`, `PesananUji`, `TestDatabase`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Tests/QuilvianSystemBackend.OpticalTests/Be063DiscountBillingTests.cs` | **Baru.** 8 metode uji, 9 kasus |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE`; perilaku cocok dengan `OPT-INT-v1.3` |
| Database | `NOT APPLICABLE`: tanpa migration |
| Keamanan/Auth | Uji memanggil service langsung, sehingga izin `BillingDiscount : Create` tidak diuji di sini (`OPT-AC-042`) |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Perilaku yang dibuktikan milik endpoint berikut.

#### Health Services / Billing Management / Billing / Invoices

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/discounts` | Menerapkan kebijakan pada baris tagihan | `BillingDiscount : Create` |

#### Health Services / Optical Management / Opt Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{id}/payment-status` | Bagian pasien dan minimum uang muka sesudah diskon | `OptOrder : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet`/`node` berjalan | `PASS` | `tasklist` |
| `dotnet test … --filter Be063` | 9 lulus, 0 gagal | `PASS` | Keluaran `dotnet test` |
| Uji mutasi 1: perhitungan persen dibuat 20 kali lipat | 5 kasus gagal; `BillingDiscountService.cs` dipulihkan (`cmp` identik) | `PASS` | Keluaran `dotnet test` |
| Uji mutasi 2: kalkulasi ulang sesudah diskon dilewati | 2 kasus gagal; berkas dipulihkan (`cmp` identik) | `PASS` | Keluaran `dotnet test` |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 293 lulus, 0 gagal (284 lama + 9 baru); waktu uji 5 menit 22 detik, di luar build | `PASS` | Keluaran `dotnet test` |
| `git status` folder Billing sesudah uji | Tidak ada perubahan | `PASS` | Keluaran Git |

QBE checker: `NOT APPLICABLE`, karena tidak ada berkas produksi yang berubah.

Uji manual: `NOT FEASIBLE`. Kebijakan `OPT-DISC-10` dan pesanan belum ada di devYoga, dan task ini tidak menulis ke basis data.

**Tidak dijalankan:** uji di PostgreSQL. Kunci `Serializable` dan kunci advisory Billing di harness memakai SQLite dengan fungsi pengganti, sehingga penerapan serentak tetap menunggu `B5`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `OPT-AC-016`: Rp 450.000 × 10% tercatat Rp 45.000, disetujui tanpa langkah persetujuan | Terpenuhi | `OPT_AC_016_*`; mutasi 1 |
| (2) `OPT-AC-037`, `UAT-29`: bagian pasien berkurang Rp 45.000, termasuk pada kalkulasi tersimpan | Terpenuhi di harness; layar di `FE-OPT-041` | `OPT_AC_037_UAT_29_*`; mutasi 2 |
| (3) Status bayar dan minimum uang muka mengikuti angka baru | Terpenuhi | `Status_bayar_dan_syarat_uang_muka_*` |
| (4) Baris lensa tidak berubah | Terpenuhi | `OPT_AC_037_UAT_29_*` |
| (5) Kebijakan sama pada baris sama ditolak | Terpenuhi | `Kebijakan_sama_pada_baris_sama_*` |
| (6) Nominal yang diketik ditolak | Terpenuhi | `Nominal_yang_diketik_pemanggil_ditolak` |
| (7) Tagihan final menolak diskon | Terpenuhi; statusnya diatur langsung di data uji, bukan lewat alur finalisasi | `Tagihan_final_menolak_diskon` |
| (8) Karakterisasi risiko `OPT-DEC-079` | Terpenuhi dengan 10,5%, bukan 50% yang semula tertulis (bagian 7) | `OPT_DEC_079_*` |
| DoD: laporan dan traceability diperbarui | Terpenuhi | Laporan ini; roadmap; traceability |

Tambahan di luar kriteria: versi tagihan basi ditolak.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kriteria (8) semula meminta 50%, tetapi harness menolaknya. SQLite menyimpan desimal sebagai teks, sehingga `CK_MstDiscountPolicy_Value` membandingkan `'50.0' <= '100'` secara huruf. Ini artefak harness, bukan aturan Billing: `DiscountPolicyService` dan PostgreSQL menerima sampai 100%. Uji memakai 10,5%, yang tetap membuktikan tidak ada batas di 10% |
| Masalah yang diketahui | Tidak ada selisih dengan kontrak |
| Risiko tersisa | Belum berjalan di PostgreSQL; belum memakai akun petugas; kebijakan `OPT-DISC-10` belum ada di data. Salah isi nilai kebijakan langsung memengaruhi tagihan, sehingga Yasmina perlu memeriksanya saat membuat kebijakan |
| Perubahan sampingan | `NONE`. `BillingDiscountService.cs` hanya diubah sementara untuk mutasi, lalu dipulihkan |
| Interupsi | `NONE` |
| Status Git | `Be063DiscountBillingTests.cs` berada di folder yang diabaikan Git (`T3`); perlu pengecualian `.gitignore` atau `git add -f`. Tidak ada kode produksi yang berubah |
| Langkah berikutnya | Jawab `OPT-OQ-024`; Billing membuat `OPT-DISC-10`; uji alur asli |
