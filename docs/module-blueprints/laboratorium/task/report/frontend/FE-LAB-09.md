# Laporan Perubahan Frontend — `FE-LAB-09`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-09` |
| Judul | Tiga layar monitoring per disiplin |
| Slice | `S15` — pemantauan per disiplin (`roadmap/frontend-roadmap.md` bagian 6, gelombang `MVP-3`) |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/frontend-roadmap.md` bagian 6 |
| Trace | `FR-10.1`, `FR-10.2`; `LAB-DEC-025`, `LAB-DEC-026`; `AC-41` |
| Contract version | `LAB-API-v1` r3 grup Lab Monitoring — `approved`, dikunci 2026-09-02 |
| Wewenang UI | Susunan kolom `DEV_DISCRETION`. **Tiga menu terpisah bukan `DEV_DISCRETION`** — itu keputusan `LAB-DEC-025` |
| Dependency | `FE-LAB-08` **selesai**; endpoint dari `BE-LAB-15` **selesai**, diverifikasi langsung pada source backend |
| Klasifikasi | `MEDIUM` — skor 9: repository 1, berkas diperiksa 2, berkas diubah 1, logika bisnis 1, kontrak API 1, database 0, keamanan 0, UI/workflow 3 |
| Task mode | `FRONTEND` — frontend target tulis, backend strict read-only sebagai sumber kebenaran kontrak |
| Target tulis | `QuilvianSystemFrontendDev` — lapis modul `laboratory-management`, store Redux, dan satu berkas uji. `NewQuilvianSystemBackend` — **hanya** laporan ini beserta tautan buktinya pada `roadmap/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | `c71c02a07`, branch `YogaV2` |
| Commit backend yang dijadikan rujukan | `8e48841`, branch `yoga` |
| Tanggal | 2026-09-07 |
| Status | **Selesai.** Ketiga butir DoD terpenuhi; lint, uji, dan build seluruhnya `PASS`. Verifikasi manual tertunda menunggu backend dan data — bagian 5 |

---

## 1. Masalah yang diperbaiki

Kepala instalasi tiap disiplin tidak punya layarnya sendiri.

> Patologi Klinik, Patologi Anatomi, dan Mikrobiologi dikerjakan **petugas yang berbeda**, di
> ruang yang berbeda, dengan alur yang berbeda. Yang tersedia hanya daftar pesanan gabungan
> seluruh laboratorium — dan tidak ada cara melihat "pekerjaan yang masuk ke disiplin saya"
> tanpa menyaringnya sendiri setiap kali.

---

## 2. Proses bisnis

### 2.1 Kenapa tiga menu, bukan satu layar berpenyaring — `LAB-DEC-025`

Ini keputusan yang paling menentukan bentuk task ini, dan paling mudah dilanggar dengan niat
baik: "cukup satu layar, tambahkan penyaring disiplin" terdengar lebih hemat.

> Yang hilang bila itu dilakukan: setiap petugas harus memilih disiplinnya **setiap kali membuka
> layar** — pekerjaan tambahan yang berulang seharian, untuk pilihan yang bagi dirinya tidak
> pernah berubah. Petugas Mikrobiologi tidak pernah ingin melihat Patologi Anatomi.

Kontraknya pun sudah mengikuti keputusan itu: `LabMonitoringQuery` **tidak punya ruas disiplin**.
Disiplin ditentukan **jalur yang dipanggil**, bukan parameter yang dikirim.

### 2.2 Yang dilihat kepala instalasi

| Kolom | Kenapa ada |
| --- | --- |
| Pasien beserta nomor rekam medis | Pengenal utama saat memindai daftar |
| Nomor kunjungan dan jenisnya | Membedakan rawat jalan, rawat inap, dan gawat darurat |
| Wadah layak per total — "2 / 3" | Kemajuan pekerjaan terbaca sekali lihat, tanpa membuka detail |
| Penanda **Memuat CITO** | Satuan daftar ini pesanan; penandanya berarti *sekurang-kurangnya satu* pemeriksaan di dalamnya cito |

**Tidak ada kolom disiplin**, dan ketiadaannya disengaja: setiap layar hanya memuat disiplinnya
sendiri, sehingga kolom itu akan berisi nilai yang sama pada seluruh baris.

### 2.3 Penyaring — identik pada ketiga layar

Sepuluh penyaring: periode, jenis kunjungan, jenis kunjungan baru/lama, unit layanan, ruangan,
penjamin, status pesanan, status wadah, penanda cito, dan pencarian bebas.

Ruangan **tersaring menurut unit layanan** yang dipilih, dan mengganti unit mengosongkan
ruangan yang sudah dipilih — ruangan itu berada di bawah unit sebelumnya.

### 2.4 Jalur tidak normal

| Keadaan | Yang terlihat |
| --- | --- |
| Tautan menyebut disiplin yang tidak dikenal | Pesan salah tautan. **Tidak** dialihkan diam-diam ke disiplin lain |
| Tidak ada pesanan pada penyaring | Keterangan yang menjelaskan kenapa kosong dan apa yang dapat diubah |
| Permintaan gagal | Pesan dari server ditampilkan apa adanya |
| Penyaring diubah | Halaman kembali ke satu |
| Berpindah menu disiplin | Penyaring disiplin sebelumnya **tetap utuh** saat kembali |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk menetapkan |
| --- | --- |
| `roadmap/frontend-roadmap.md` bagian 6 | Cakupan, DoD, dan alasan tiga menu terpisah |
| `contracts/api-contract.md` grup Lab Monitoring | Ketiga jalur beserta alasan tertulisnya |
| `Areas/.../LabMonitoringController.cs` dan `LabMonitoringDtos.cs` | Kontrak sebenarnya: tiga route, bentuk penyaring, dan seluruh ruas jawabannya |
| `Areas/.../RegistrationManagement/Enums/` | Nilai `EncounterType`, `VisitType`, `EncounterPaymentType` untuk pilihan penyaring |
| `use-lab-worklist.jsx` beserta ujinya | Pola hook, penyaring, dan pagination dari `FE-LAB-08` |
| `lab-order-list-view.jsx` | Pola `DataFilter` dan `ResourceFilterSelect` berantai |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../constants/.../lab-monitoring-constants.jsx` | **Baru.** **Satu** definisi penyaring, tiga konfigurasi disiplin yang menunjuk objek yang sama, kolom, dan salinan teks |
| `.../hooks/.../lab-monitoring-rules.js` | **Baru.** Aturan murni: pengenalan disiplin, penyusunan parameter, dan dua pembaca ringkasan baris |
| `.../services/.../lab-monitoring.service.js` | **Baru.** Satu fungsi bersegmen jalur; segmen tak dikenal ditolak sebelum jadi permintaan |
| `.../state/slice/.../lab-monitoring-slice.jsx` | **Baru.** State **per disiplin**, dibentuk dari daftar disiplin yang sama |
| `.../hooks/.../use-lab-monitoring.jsx` | **Baru.** Satu controller melayani ketiga layar |
| `.../view/.../lab-monitoring/lab-monitoring-view.jsx` | **Baru.** **Satu** view untuk ketiga menu |
| `.../view/.../lab-monitoring/lab-monitoring-table-columns.jsx` | **Baru.** **Satu** susunan kolom |
| `src/app/.../lab-monitoring/{clinical-pathology,anatomic-pathology,microbiology}/page.jsx` | **Baru.** Tiga route, masing-masing sembilan baris |
| `src/style/.../lab-monitoring/lab-monitoring.module.css` | **Baru.** Tiga kelas |
| `src/lib/state/store.jsx` | Satu potongan Redux didaftarkan |
| `tests/unit/lab-monitoring-rules.test.mjs` | **Baru.** Empat belas uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Tidak ada perubahan.** Layar mengonsumsi `LAB-API-v1` r3 apa adanya |
| Database | **Tidak ada dampak sama sekali** |
| Keamanan/Auth | Ketiga endpoint memakai `LabMonitoring : Read`. Ketiga layar **baca saja** — tidak ada satu pun tindakan yang mengubah data |

### 3.4 Keputusan dan selisih yang perlu diketahui

| No | Butir | Penjelasan |
| ---: | --- | --- |
| 1 | **Ketiga disiplin menunjuk objek penyaring yang sama, bukan salinannya** | DoD menuntut penyaringnya identik pada ketiga layar. Menyalinnya tiga kali membuat ketiganya **pasti** bercabang cepat atau lambat — satu penyaring ditambahkan di satu layar dan terlupa di dua lainnya, tanpa ada yang gagal. Dengan satu objek bersama, "identik" menjadi sifat struktural, bukan janji yang perlu dijaga manusia |
| 2 | **Ujinya memeriksa identitas objek, bukan kesamaan isi** | Tiga salinan yang kebetulan sama hari ini akan lolos uji kesamaan isi, dan baru ketahuan bercabang setelah terlambat. Uji identitas gagal pada percabangan **pertama** |
| 3 | **Disiplin yang tidak dikenal tidak jatuh ke disiplin mana pun** | Memilihkan disiplin bawaan akan menampilkan pesanan Patologi Klinik kepada petugas Mikrobiologi yang salah membuka tautan. Itu kekeliruan paling mahal di layar ini: daftarnya tampak wajar dan tidak ada yang menandainya keliru. Layar berhenti dengan pesan salah tautan |
| 4 | **Segmen jalur divalidasi di service, bukan hanya di layar** | Segmen yang tidak dikenal ditolak sebelum menjadi permintaan, supaya kekeliruannya terbaca sebagai kesalahan pemanggilan — bukan sebagai `404` yang tampak seperti gangguan jaringan |
| 5 | **State disimpan per disiplin** | Ketiga layar adalah tiga menu yang dibuka bergantian. Menyatukan statenya membuat berpindah menu ikut mengosongkan penyaring yang baru saja disusun petugas, dan ia akan menyusunnya ulang setiap kali menengok disiplin tetangga |
| 6 | **Tidak ada kolom disiplin pada tabel** | Setiap layar hanya memuat disiplinnya sendiri, sehingga kolom itu akan berisi nilai yang sama pada seluruh baris |
| 7 | **Tombol pindah menu, bukan penyaring disiplin** | Kedua disiplin lain tampil sebagai tautan menu di kepala layar. Bedanya bukan kosmetik: petugas tidak pernah "memilih disiplin", ia berpindah menu — dan menu terakhirnya tetap menjadi menu yang ia buka besok |
| 8 | **`sortLatestFirst={false}` dipasang lagi di sini** | Alasannya sama dengan `FE-LAB-08`: `DataTable` mengurutkan ulang datanya sendiri secara bawaan. Urutan daftar ini datang dari backend, dan tidak boleh disusun ulang tabel dengan aturannya sendiri |

---

## 4. Dokumentasi endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Monitoring

| Method | Path | Dipakai layar | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/clinical-pathology` | Monitoring Patologi Klinik | `LabMonitoring : Read` |
| `GET` | `/anatomic-pathology` | Monitoring Patologi Anatomi | `LabMonitoring : Read` |
| `GET` | `/microbiology` | Monitoring Mikrobiologi | `LabMonitoring : Read` |

Ketiganya menerima penyaring yang sama persis dan mengembalikan
`PagedResult<LabMonitoringItemResponse>`. **Tidak satu pun menerima ruas disiplin.**

---

## 5. Verifikasi

| Perintah atau skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `tests/unit/lab-monitoring-rules.test.mjs` | `pass 14, fail 0` | `PASS` |
| Seluruh uji unit (`--test tests/unit/`) | `pass 533, fail 0` | `PASS` |
| `npm run lint:errors` | Bersih, tanpa keluaran | `PASS` |
| `npm run build` | `Compiled successfully`; ketiga route terbentuk | `PASS` |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/ — PASS`
(`npm run test:unit` tetap tidak dapat dipakai apa adanya pada Node `v20.20.2`; alasannya sama
dengan yang dicatat `FE-LAB-05.md`, dan `package.json` tidak disentuh).

**Uji yang membuktikan butir DoD:**

| Yang dibuktikan | Uji |
| --- | --- |
| Penyaring identik tanpa duplikasi | `ketiga disiplin menunjuk OBJEK penyaring yang sama, bukan salinannya` |
| Tiga route terpisah, bukan satu berpenyaring | `tepat tiga disiplin, masing-masing dengan jalur dan route sendiri` |
| `LAB-DEC-025` — penyaring tidak punya ruas disiplin | `definisi penyaring TIDAK punya ruas disiplin` |
| Disiplin tidak pernah ikut pada permintaan | `disiplin TIDAK PERNAH ikut pada parameter permintaan` |
| Tautan keliru tidak dialihkan diam-diam | `kunci yang tidak dikenal TIDAK jatuh ke disiplin mana pun` |

### Verifikasi manual

`MANUAL TEST: NOT FEASIBLE`

Alasannya konkret: ketiga layar menuntut data campuran — pesanan pada **ketiga** disiplin
sekaligus, karena yang dibuktikan `AC-41` justru bahwa masing-masing layar **hanya** menampilkan
disiplinnya sendiri. Backend yang dihentikan pada sesi sebelumnya juga belum dijalankan kembali.

**Yang wajib diperiksa manual begitu backend berjalan dan datanya tersedia:**

| No | Yang diperiksa | Yang diharapkan |
| ---: | --- | --- |
| 1 | Membuka ketiga layar dengan data campuran | Masing-masing **hanya** menampilkan pesanan disiplinnya sendiri (`AC-41`) |
| 2 | Membandingkan penyaring ketiga layar | Sepuluh penyaring yang sama persis, tanpa satu pun selisih |
| 3 | Menyusun penyaring di satu disiplin, pindah menu, lalu kembali | Penyaringnya **tetap utuh**, tidak tereset |
| 4 | Memilih unit layanan lalu membuka pilihan ruangan | Ruangan tersaring menurut unitnya |
| 5 | Mengganti unit layanan setelah ruangan dipilih | Ruangan **kosong kembali** |
| 6 | Menggabungkan periode, status pesanan, dan pencarian | Ketiganya berlaku bersamaan, bukan saling menimpa |
| 7 | Berpindah halaman lalu mengubah penyaring | Halaman kembali ke satu |
| 8 | Membuka route disiplin yang tidak ada | Pesan salah tautan, bukan daftar disiplin lain |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-41` — tiga daftar pantau sejajar, masing-masing memuat disiplinnya sendiri | Terpenuhi di sisi layar | Tiga route memanggil tiga jalur backend yang berbeda; disiplin tidak pernah dikirim sebagai parameter, sehingga tidak ada jalan bagi satu layar menampilkan disiplin lain |

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Tiga route ada | Terpenuhi | Ketiganya terbentuk pada keluaran `npm run build` |
| Komponen tabel dan penyaring dipakai bersama **tanpa duplikasi** | Terpenuhi | Satu view, satu susunan kolom, satu definisi penyaring, satu hook, satu berkas gaya. Ketiga halaman route masing-masing sembilan baris dan hanya meneruskan kunci disiplinnya |
| Penyaringnya identik pada ketiganya | Terpenuhi | Ditegakkan **secara struktural** — ketiga konfigurasi menunjuk objek yang sama, dan uji memeriksa identitas objeknya |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run lint:errors` bersih. `npm run build` tanpa error |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | **Pertama, verifikasi manual belum dijalankan** — delapan skenario pada bagian 5; `AC-41` baru terbukti secara struktural, belum pada layar dengan data campuran sungguhan. **Kedua**, bila kelak satu disiplin benar-benar membutuhkan penyaring yang berbeda, memecah objek penyaing bersama adalah **keputusan produk** yang perlu diputuskan lebih dulu — bukan perubahan teknis yang dapat diselundupkan lewat salinan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Satu berkas `M`, delapan entri `??`. **Tidak ada** `git add`, `commit`, maupun `push` |
| Langkah berikutnya | 1. Menjalankan backend, lalu menjalankan verifikasi manual seluruh task frontend yang tertunda — `FE-LAB-05`, `FE-LAB-07`, `FE-LAB-08`, dan `FE-LAB-09`. 2. Mengisi data induk instansi dan dokter perujuk, penahan terakhir `FE-LAB-05`. 3. **Seluruh sembilan task frontend Laboratorium selesai** — berikutnya `verify-module-readiness` untuk sign-off kesiapan modul |
