# Laporan Perubahan Backend — `BE-RWI-042`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-042` |
| Judul | Daftar pantau kekurangan deposit |
| Slice | `S11` — Deposit dapat diterima dan ditelusuri ke episodenya; `EPIC RI-35a`, gelombang `MVP-1` |
| Roadmap | `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/backend-roadmap.md` bagian 4, kartu `BE-RWI-042` |
| Trace | `RWI-DEC-096`; `FR-RI-177`; `api-contract.md` `0.6.0` `GET /monitoring/deposit-shortfall` |
| Contract version | API `0.6.1` berlaku. Baris `GET /deposit-shortfall` masih berstatus **`Rencana 0.6.0`** pada kontrak |
| Dependency | `BE-BKC-040` pada roadmap `billing-kasir` — **belum dikerjakan**; `BE-RWI-041` — ✅ selesai 10 September 2026 |
| Klasifikasi | `MEDIUM` — satu operasi baca, satu DTO, lima keadaan uji. Tidak dinilai lebih lanjut karena task tidak dieksekusi |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — **nol berkas source ditulis pada task ini** |
| Model | claude-opus-5 |
| Commit backend saat dikerjakan | `4c3a458dac3fd10dcac770adb938bfa2b4e0a7dd` |
| Tanggal | 10 September 2026 |
| Status | ⛔ **`BLOCKED`** — prasyarat data `BE-BKC-040` tidak ada di source. Nol baris source ditulis |

---

## 1. Masalah yang diperbaiki

**Belum diperbaiki apa pun.** Laporan ini mencatat sebab task tidak dapat dikerjakan hari ini,
bukan hasil pekerjaan.

Masalah yang seharusnya ditutup task ini masih utuh. Petugas rawat inap tidak punya satu pun
layar atau jawaban server yang menyebutkan pasien mana saja yang uang mukanya masih kurang dan
sudah berapa lama kurang. Penagihan pelunasan berkala yang diputuskan `RWI-DEC-096` karena itu
masih berupa niat: tidak ada daftar kerja yang dapat dibuka petugas pada pagi hari, sehingga
kekurangan deposit baru ketahuan saat pasien hendak pulang.

**Contoh keadaan yang masih terjadi.** Pasien dirawat sejak 1 September dengan minimum deposit
Rp 5.000.000 dan baru menyetor Rp 2.000.000. Pada 4 September kekurangannya Rp 3.000.000 dan
sudah melewati ambang tindak lanjut 3 hari, sehingga seharusnya muncul pada daftar kerja kasir.
Hari ini keadaan itu tidak muncul di mana pun sampai tagihan final disusun.

---

## 2. Proses bisnis

Alur yang dituju task ini, ditulis supaya jelas bagian mana yang hilang.

1. **Pemicu.** Petugas kasir atau petugas rawat inap membuka daftar pantau kekurangan deposit.
2. **Langkah 1.** Sistem mengambil seluruh episode yang masih berjalan.
3. **Langkah 2.** Untuk setiap episode, sistem **membaca** posisi depositnya dari ringkasan milik
   Billing — berapa minimum kebijakannya, berapa yang sudah diterima, dan berapa kekurangannya.
4. **Langkah 3.** Episode yang lama rawatnya belum melewati ambang tindak lanjut disaring keluar.
   Ambang itu diambil dari pengaturan Rawat Inap, dan sejak `BE-RWI-041` ✅ nilainya sudah dapat
   diubah admin.
5. **Hasil.** Daftar episode beserta angka kekurangan dan lama harinya.
6. **Jalur tidak normal.** Bila ringkasan Billing tidak dapat dibaca, daftar wajib menyatakan
   datanya tidak tersedia. Menampilkan nol pada keadaan itu berarti memberitahu petugas bahwa
   uang muka sudah lunas padahal sistem sedang tidak tahu apa-apa.

**Langkah 3 sudah tersedia. Langkah 2 tidak ada sama sekali**, dan langkah 2 itulah yang memasok
seluruh angka pada daftar.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `roadmap/backend-roadmap.md` kartu `BE-RWI-042` | Scope, dependency, kelima acceptance criteria |
| `../../billing-kasir/roadmap/backend-roadmap.md` kartu `BE-BKC-040` | Status prasyaratnya |
| `contracts/api-contract.md` bagian Deposit dan Monitoring | Bentuk jawaban yang dijanjikan |
| `Areas/HealthServices/BillingManagement/**` | Apakah ringkasan deposit per episode benar-benar ada |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientMonitoringController.cs` | Pola daftar pantau yang sudah ada lewat `BE-RWI-029` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | **NONE.** Nol berkas source ditulis |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — nol endpoint dibuat. Baris `GET /deposit-shortfall` tetap `Rencana 0.6.0` |
| Database | `NOT APPLICABLE` — nol migration dibuat, nol perintah dikirim ke database mana pun |
| Keamanan/Auth | `NOT APPLICABLE` — nol `[AccessAction]` dan nol `[AccessPermission]` ditambahkan |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh satu pun endpoint.

---

## 5. Verifikasi

Yang dijalankan adalah **pembuktian bahwa prasyaratnya tidak ada**, bukan pembuktian kemampuan.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cari rute `GET /patient-funds/deposits/episodes/{episodeId}` di seluruh source | **Nol hasil** | `PASS` | `grep -rn "deposits/episodes" --include=*.cs .` |
| Cari tipe ringkasan deposit per episode | **Nol hasil** | `PASS` | `grep -rn "EpisodeDepositSummary\|DepositEpisodeSummary" --include=*.cs .` |
| Cari master kebijakan minimum deposit `BE-BKC-039` | **Nol hasil** | `PASS` | `grep -rn "deposit-policies\|MstDepositPolicy" --include=*.cs .` |
| Cari laporan task `BE-BKC-039` / `BE-BKC-040` | **Tidak ada berkasnya** | `PASS` | `ls docs/module-blueprints/billing-kasir/task/report/backend/` |
| Rute daftar pantau `GET /monitoring/deposit-shortfall` | **Nol hasil** | `PASS` | `grep -rn "deposit-shortfall" --include=*.cs .` |
| `dotnet build` | `NOT RUN` untuk task ini | `NOT RUN` | Nol berkas diubah task ini, sehingga tidak ada yang perlu dibangun |

Uji manual: `NOT APPLICABLE`.

**Tidak dijalankan:** seluruh uji kelima keadaan episode pada kartu task. Alasannya bukan waktu,
melainkan data — tanpa ringkasan Billing, keempat dari lima kriteria tidak punya angka untuk
diuji, dan kriteria kelima tidak punya sumber yang dapat digagalkan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Episode aktif yang kekurangannya di atas nol muncul pada daftar | **Belum terpenuhi** | Angka kekurangan hanya ada pada `BE-BKC-040` yang belum dibuat |
| 2. Episode yang lama rawatnya belum melewati ambang **tidak** muncul | **Belum terpenuhi** | Ambangnya sudah tersedia lewat `BE-RWI-041` ✅, tetapi daftar yang menyaringnya belum ada |
| 3. Episode yang kekurangannya sudah tertutup hilang dari daftar tanpa transaksi lama berubah | **Belum terpenuhi** | Sama seperti kriteria 1 |
| 4. Angka kekurangan pada daftar sama persis dengan ringkasan Billing | **Belum terpenuhi** | Tidak ada ringkasan Billing untuk dibandingkan |
| 5. Bila ringkasan Billing tidak dapat dibaca, daftar menyatakan datanya tidak tersedia | **Belum terpenuhi** | Tidak ada sumber yang dapat digagalkan |

**Definition of Done.** Nol dari tiga butir terpenuhi: endpoint belum ada, DTO belum ada, dan test
kelima keadaan belum ada. Butir `build lulus` tidak berlaku karena nol berkas diubah.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Ambang tindak lanjut yang dipakai daftar ini **sudah** tersedia sejak `BE-RWI-041` ✅ 10 September 2026. Yang menahan tinggal satu, yaitu angka kekurangannya |
| Masalah yang diketahui | `BE-BKC-039` dan `BE-BKC-040` berstatus `BLOCKED_PENDING_OWNER_APPROVAL` lewat `RWI-OQ-053`, dan pemilik `BillingManagement` **belum ditunjuk namanya** pada sumber yang tersedia. Selama pemiliknya belum ada, tidak ada pihak yang dapat menerima kedua task itu ke dalam gelombang deliverynya |
| Risiko tersisa | Penagihan pelunasan berkala pada perawatan panjang tetap tidak punya daftar kerja. Kekurangan uang muka baru terbaca saat tagihan final disusun, yaitu saat pasien sudah hendak pulang dan daya tawarnya paling kecil |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Nol berkas source berubah oleh task ini. Berkas yang berubah pada sesi ini seluruhnya milik `BE-RWI-041` dan `BE-RWI-069` |
| Langkah berikutnya | Tetapkan pemilik `BillingManagement` supaya `RWI-OQ-053` dapat ditutup, lalu kerjakan `BE-BKC-039` dan `BE-BKC-040` pada roadmap `billing-kasir`. Sesudah `BE-BKC-040` ada, task ini dapat langsung dikerjakan tanpa menunggu siapa pun lagi |

---

## 8. Kenapa task ini tidak dikerjakan sebagian

Kartu task menyatakan angka kekurangan **dibaca** dari ringkasan Billing dan **tidak dihitung
ulang** di Rawat Inap. Membuat endpoint yang bentuknya benar tetapi angkanya dihitung sendiri di
sini akan melanggar batas itu, dan justru membuat mesin kedua yang perhitungannya akan menyimpang
dari Billing begitu ada satu saja alokasi atau refund yang tidak ikut terbaca.

Membuat endpoint yang bentuknya benar tetapi angkanya selalu kosong juga bukan pekerjaan yang
selesai sebagian: kriteria 5 justru menuntut sistem membedakan "tidak terbaca" dari "tidak ada
kekurangan", dan endpoint yang selalu kosong mengaburkan tepat perbedaan itu.

Karena itu **nol baris source ditulis**, dan task ini dilaporkan terblokir apa adanya.
