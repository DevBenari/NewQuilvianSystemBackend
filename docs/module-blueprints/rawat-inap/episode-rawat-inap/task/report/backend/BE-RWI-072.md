# Laporan Perubahan Backend — `BE-RWI-072`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-072` |
| Judul | Settlement, refund, dan `Cleared` yang tidak lagi buta |
| Slice | `S12` — Uang selesai sebelum episode ditutup; `EPIC RI-35b`, gelombang `MVP-3` |
| Roadmap | `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/backend-roadmap.md` bagian 4, kartu `BE-RWI-072` |
| Trace | `FR-RI-170`, `FR-RI-171`, `FR-RI-172`; `RWI-RISK-003`; `validation-matrix.md` `0.6.0` bagian 8A baris `Cleared` |
| Contract version | API `0.6.1` berlaku. Baris `GET /deposits/episodes/{episodeId}` masih **`Rencana 0.6.0`** |
| Dependency | `BE-BKC-040` pada roadmap `billing-kasir` — **belum dikerjakan** |
| Klasifikasi | `HEAVY` — menyentuh gerbang penutupan episode dan uang. Tidak dinilai lebih lanjut karena task tidak dieksekusi |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — **nol berkas source ditulis pada task ini** |
| Model | claude-opus-5 |
| Commit backend saat dikerjakan | `4c3a458dac3fd10dcac770adb938bfa2b4e0a7dd` |
| Tanggal | 10 September 2026 |
| Status | ⛔ **`BLOCKED`** — prasyarat data `BE-BKC-040` tidak ada di source. Nol baris source ditulis |

---

## 1. Masalah yang diperbaiki

**Belum diperbaiki apa pun.** Laporan ini mencatat sebab task tidak dapat dikerjakan hari ini.

Masalah yang seharusnya ditutup task ini masih utuh, dan inilah masalah paling mahal di antara
keempat task deposit. Gerbang kelayakan keuangan pada penutupan episode masih **buta**: kasir dapat
menyatakan sebuah episode lunas tanpa satu pun pemeriksaan terhadap angka Billing yang sebenarnya.

**Contoh keadaan yang masih terjadi.** Tagihan final sebuah episode Rp 12.000.000 dan deposit yang
masuk Rp 4.000.000, sehingga kekurangannya Rp 8.000.000. Hari ini kasir tetap dapat menandai
kelayakan keuangan sebagai `Cleared`, episode ditutup, dan pasien pulang. Kekurangan
Rp 8.000.000 itu tidak menahan apa pun karena tidak ada satu baris kode pun yang membacanya.
Keadaan sebaliknya sama buruknya: deposit Rp 10.000.000 atas tagihan Rp 6.000.000 menyisakan
kelebihan Rp 4.000.000 yang seharusnya direfund, dan episode tetap dapat ditutup tanpa refund itu
pernah tercatat.

---

## 2. Proses bisnis

Alur yang dituju task ini.

1. **Pemicu.** Pasien dinyatakan boleh pulang, dan petugas menutup sisi keuangannya.
2. **Langkah 1.** Sistem membaca ringkasan posisi keuangan episode dari Billing: tagihan final,
   total deposit yang diterima, yang sudah dialokasikan, dan yang sudah direfund.
3. **Langkah 2.** Tagihan final dikurangi deposit.
   - Bila hasilnya **kurang**, kekurangan itu harus dibayar lebih dulu.
   - Bila hasilnya **lebih**, kelebihan itu harus direfund lebih dulu.
4. **Langkah 3.** Barulah `MarkFinancialClearanceAsync` menerima `Cleared`.
5. **Hasil.** Episode dapat ditutup dengan uang yang benar-benar selesai.
6. **Jalur tidak normal pertama.** Bila ringkasan Billing **tidak dapat dibaca**, status tidak
   boleh diasumsikan `Cleared`. Jalur normalnya tetap `Pending` atau `Blocked`.
7. **Jalur tidak normal kedua.** `CloseOverride` supervisor tetap dapat menembus gerbang episode
   tanpa menghapus satu pun transaksi Billing, dan episodenya masuk laporan pengecualian.

**Langkah 4 sudah ada** — `MarkFinancialClearanceAsync` berjalan sejak `BE-RWI-024`.
**Langkah 1 tidak ada**, dan tanpa langkah 1 tidak ada satu pun angka untuk menjalankan langkah 2,
3, 6, maupun 7.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `roadmap/backend-roadmap.md` kartu `BE-RWI-072` | Scope, dependency, kelima acceptance criteria |
| `../../billing-kasir/roadmap/backend-roadmap.md` kartu `BE-BKC-040` | Status prasyaratnya |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs:299` | Bentuk `MarkFinancialClearanceAsync` yang sudah ada |
| `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs:140` | Ketersediaan tagihan final per kunjungan |
| `Areas/HealthServices/BillingManagement/**` | Apakah ringkasan deposit **per episode** benar-benar ada |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | **NONE.** Nol berkas source ditulis |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — nol endpoint dibuat atau diubah |
| Database | `NOT APPLICABLE` — nol migration dibuat, nol perintah dikirim ke database mana pun |
| Keamanan/Auth | `NOT APPLICABLE` — nol atribut hak akses ditambahkan atau diubah |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh satu pun endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cari rute ringkasan deposit per episode | **Nol hasil** | `PASS` | `grep -rn "deposits/episodes" --include=*.cs .` |
| Cari tipe ringkasan deposit per episode | **Nol hasil** | `PASS` | `grep -rn "EpisodeDepositSummary\|DepositEpisodeSummary" --include=*.cs .` |
| Pastikan tagihan final memang sudah tersedia | **Ada** | `PASS` | `BillingInvoicesController.cs:140` rute `encounters/{encounterId}/charge-summary` |
| Pastikan gerbang clearance memang sudah ada | **Ada** | `PASS` | `InpDischargeService.Closure.cs:299` `MarkFinancialClearanceAsync` |
| Cari laporan task `BE-BKC-040` | **Tidak ada berkasnya** | `PASS` | `ls docs/module-blueprints/billing-kasir/task/report/backend/` |
| `UAT-37` s.d. `UAT-40` | `NOT RUN` | `NOT RUN` | Tidak ada angka posisi deposit per episode untuk diuji |
| `dotnet build` | `NOT RUN` untuk task ini | `NOT RUN` | Nol berkas diubah task ini |

Uji manual: `NOT APPLICABLE`.

**Tidak dijalankan:** keempat UAT dan uji jalur gagal-aman. Alasannya bukan waktu: uji gagal-aman
menuntut adanya sumber yang dapat digagalkan, dan sumber itu belum dibuat.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tagihan final lebih besar dari deposit menghasilkan kekurangan, dan `Cleared` ditolak 422 sebelum dibayar | **Belum terpenuhi** | Posisi deposit per episode belum dapat dibaca |
| 2. Deposit lebih besar dari tagihan final menghasilkan kelebihan, dan `Cleared` ditolak sebelum refund tercatat | **Belum terpenuhi** | Sama seperti kriteria 1 |
| 3. Refund tersimpan sebagai transaksi terpisah; tiga penerimaan sebelumnya tetap utuh | **Belum terpenuhi** | Jalur refund `POST /financial-exceptions/refunds` **sudah ada**, tetapi belum tersambung ke gerbang ini |
| 4. Bila ringkasan Billing tidak dapat dibaca, status tidak boleh diasumsikan `Cleared` | **Belum terpenuhi** | Tidak ada pembacaan yang dapat gagal |
| 5. `CloseOverride` supervisor tetap menembus gerbang tanpa menghapus transaksi Billing | **Belum terpenuhi** | Perilaku override yang ada belum diuji terhadap gerbang baru yang belum dibuat |

**Definition of Done.** Nol dari empat butir terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tiga dari empat bahan task ini **sudah ada dan berjalan**: alokasi deposit, refund beserta persetujuannya, dan tagihan final. Yang hilang hanya satu, yaitu ringkasan yang menyatukan ketiganya per episode |
| Masalah yang diketahui | `BE-BKC-040` berstatus `BLOCKED_PENDING_OWNER_APPROVAL` lewat `RWI-OQ-053`, dan pemilik `BillingManagement` belum ditunjuk namanya pada sumber yang tersedia |
| Risiko tersisa | **Risiko keuangan aktif, bukan risiko teoretis.** Selama gerbang ini buta, setiap episode dapat ditutup dengan uang yang belum selesai, ke dua arah sekaligus: kekurangan yang tidak tertagih dan kelebihan yang tidak direfund. `RWI-RISK-003` yang sudah dicabut sebagai jalur normal sejak `0.5.0` karena itu **secara nyata masih berlaku** di source hari ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Nol berkas source berubah oleh task ini |
| Langkah berikutnya | Tetapkan pemilik `BillingManagement`, tutup `RWI-OQ-053`, lalu kerjakan `BE-BKC-039` dan `BE-BKC-040`. Sesudah itu task ini dapat dikerjakan penuh di dalam Rawat Inap tanpa satu pun tulisan ke tabel Billing |

---

## 8. Kenapa task ini tidak dikerjakan sebagian

Kriteria 4 adalah alasan utamanya. Kriteria itu menuntut sistem membedakan **"ringkasan Billing
tidak dapat dibaca"** dari **"tidak ada kekurangan"**, dan menolak menganggap keadaan pertama
sebagai lunas.

Bila gerbang dibangun sekarang di atas sumber yang belum ada, satu-satunya perilaku yang mungkin
adalah selalu gagal membaca. Menurut kriteria 4 itu berarti `Cleared` **selalu** ditolak, sehingga
tidak ada satu pun episode yang dapat ditutup mulai hari ini. Menurut jalur sebaliknya —
memperlakukan sumber yang belum ada sebagai "tidak ada kekurangan" — hasilnya justru persis
kesalahan yang paling ingin dicegah kartu task ini.

Keduanya merugikan. Karena itu **nol baris source ditulis**, dan task ini dilaporkan terblokir apa
adanya sampai `BE-BKC-040` benar-benar ada.
