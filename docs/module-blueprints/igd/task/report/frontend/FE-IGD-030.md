# Laporan Perubahan Frontend — `FE-IGD-030`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-030` |
| Judul | Aksi Tangani Segera pada daftar triage |
| Slice | `IGD-S01` · `EPIC IGD-01` (pendaftaran dan triage) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.8 |
| Trace | `IGD-DEC-128`, `IGD-DEC-104` huruf (b), `IGD-DEC-093`; evidence [`2026-09-16-kunjungan-terjebak-arrived.md`](../../../evidence/2026-09-16-kunjungan-terjebak-arrived.md) `IGD-EV-131`…`IGD-EV-136`. **Coverage gap:** tanpa `FR-IGD-*` |
| Contract version | State `0.4.0` bagian 1 — `approved` lewat `IGD-DEC-093`. **Nol perubahan kontrak, nol endpoint baru** |
| Wewenang UI | Nol layar baru, nol komponen bersama diubah, nol CSS global. Tombol memakai `styles.primaryMiniButton` yang sudah ada; satu kelas penataan letak ditambahkan pada modul CSS fitur (lihat bagian 3.4) |
| Dependency | `BE-IGD-018` ✅ (penjaga transisi); `IGD-DEC-128` ✅ |
| Klasifikasi | `MEDIUM` — satu thunk Redux, satu aksi pada hook, satu dialog konfirmasi, satu tombol tabel; nol layar baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source) + laporan ini pada blueprint IGD |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `2d95904ed` (branch `RizkiV2`) + working tree |
| Commit backend yang dijadikan rujukan | `27351517` (branch `rizkiG`), strict read-only |
| Tanggal | 16 September 2026 |
| Status | ✅ **SELESAI 16 September 2026 — seluruh delapan acceptance criteria terpenuhi dan terbukti lewat layar.** Uji dijalankan pemilik: aksi Tangani Segera memindahkan pasien ke "Sedang ditangani", pengalihan ke layar Assesmen IGD bekerja, dan triase susulan tersimpan tanpa memundurkan status. `npm run build` **berhasil**. Lint dan 859 unit test lulus. Grant izin `EmergencyVisit` + `Update` terbukti ada. **Bukan UAT** — UAT milik tim terpisah dan belum dijalankan |

---

## 1. Keadaan yang ditemukan di awal

Kolom AKSI pada daftar triage hanya punya satu tombol — "Isi Triage" atau "Lihat Riwayat" —
dan tombol itu hanya berpindah layar, tidak pernah menyentuh status kunjungan
(`emergency-triage-patient-table.jsx` baris 86-103 sebelum perubahan).

Tiga pencarian pada frontend `2d95904ed` (`IGD-EV-134`):

| Yang dicari | Hasil |
| --- | --- |
| Pemanggilan route `visit-status` | **nol** |
| Berkas yang menyebut resusitasi | **nol** |
| Pemakaian `EMERGENCY_VISIT_STATUS.*` di luar berkas konstanta | tiga, seluruhnya hanya **membaca** |

Artinya jalur penanganan cepat tidak ada sama sekali. Pasien gawat pun harus menunggu triage
diselesaikan lebih dulu, padahal triage itu sendiri sedang tertolak `409`. Jalur alternatif lewat
resusitasi — satu-satunya tempat lain yang mendorong kunjungan ke `InTreatment` — juga mati
karena `EmergencyResuscitationController` nol pemakai di frontend, temuan yang sudah tercatat
sejak 15 September 2026.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** perawat triage IGD.

Alur penanganan cepat:

1. Pasien gawat tiba dan didaftarkan. Barisnya muncul pada daftar Triage IGD dengan status
   **"Menunggu triage"** (atau "Pasien tiba" untuk baris lama).
2. Perawat menilai pasien tidak boleh menunggu. Pada kolom AKSI baris itu tersedia dua tombol:
   **Isi Triage** dan **Tangani Segera**.
3. Perawat menekan **Tangani Segera**. Muncul dialog konfirmasi berisi nama pasien, penjelasan
   akibatnya, dan satu isian **Alasan (opsional)** — misalnya *"pasien tidak sadarkan diri, tim
   resusitasi sudah dipanggil"*.
4. Perawat menekan **Tangani Segera** di dialog. Status kunjungan berpindah menjadi **"Sedang
   ditangani"**, dan perawat **langsung dialihkan ke layar Assesmen IGD** milik pasien itu —
   tempat asuhannya benar-benar dicatat. Tidak ada klik tambahan.
5. Setelah pasien stabil, perawat kembali ke daftar triage, menekan **Isi Triage** pada baris
   yang sama, dan mengisi pengkajian triage. Penilaian **tersimpan**, dan status **tetap**
   "Sedang ditangani" — tidak mundur.

Pengalihan pada langkah 4 ditambahkan atas keputusan pemilik 16 September 2026, sesudah
implementasi pertama diuji lewat layar. Alasannya: pada pasien gawat, klik tambahan adalah waktu
yang hilang, dan tujuan menekan tombol itu memang untuk menangani sekarang. Sebelum ini perawat
berhenti di daftar triage dengan spanduk berhasil, dan harus berpindah menu sendiri.

Jalur tidak normal:

| Keadaan | Yang dilihat perawat |
| --- | --- |
| Perawat menekan Batal | Dialog tertutup, tidak ada yang terkirim, alasan yang sudah diketik dibuang |
| Route token gagal dibentuk | Pengalihan **tidak** dijalankan. Perawat tetap di daftar, dan spanduk hijau menyebut nama pasien beserta arah berikutnya: *"… dipindahkan ke penanganan. Lanjutkan pada menu Assesmen IGD; penilaian triage tetap dapat diisi menyusul."* |
| Status kunjungan sudah melewati triage | Tombol **Tangani Segera tidak muncul** sama sekali pada baris itu |
| Backend menolak transisi | Spanduk merah berisi pesan backend apa adanya. Endpoint ini menjawab **`400`**, bukan `409` |
| Izin `EmergencyVisit` + `Update` belum diberikan | Spanduk merah berisi pesan penolakan `403` apa adanya |
| Permintaan sedang berjalan | Tombol baris itu berbunyi **"Memproses..."** dan seluruh tombol Tangani Segera dinonaktifkan sampai selesai |

Yang **sengaja tidak** disediakan: pembatalan. Kontrak state tidak punya jalan kembali dari
`InTreatment` ke `WaitingForTriage`, jadi layar tidak boleh menjanjikan sesuatu yang tidak bisa
ditepati. Itulah alasan dialog konfirmasi ada, dan alasan kalimatnya menyebut tindakan ini tidak
dapat dibatalkan dari layar.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `emergency-triage-patient-table.jsx`, `emergency-triage-patient-list-view.jsx`,
  `emergency-triage-retriage-dialog.jsx`
- `use-emergency-management-list.jsx`, `emergency-management-triage-slice.jsx`,
  `emergency-management-triage.service.js`, `emergency-management-triage-utils.jsx`
- `src/components/features/base-features/` (inventaris lengkap), khususnya `confirm-modal.jsx`
  dan `data-table.jsx`
- `emergency-triage.module.css`
- Backend (read-only): `EmergencyVisitController.cs` baris 403-448,
  `EmergencyVisitService.cs` baris 387-423, `EmergencyTriageController.cs` baris 296-309

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/slice/.../emergency-management-triage-slice.jsx` | Thunk `startEmergencyImmediateCare` (PATCH `visit-status` dengan `InTreatment`), cabang state `immediateCare`, reducer `clearEmergencyImmediateCareState`, tiga `addCase` |
| `src/lib/hooks/.../use-emergency-management-list.jsx` | Aksi `startImmediateCare` dan `clearImmediateCare`; empat nilai keadaan diekspor ke view |
| `src/components/view/.../emergency-triage-patient-list-view.jsx` | Dialog konfirmasi `ConfirmModal` beserta isian alasan opsional, spanduk berhasil dan gagal, penyambungan ke tabel, dan **pengalihan ke layar Assesmen IGD** sesudah status berpindah |
| `src/components/view/.../components/emergency-triage-patient-table.jsx` | Tombol **Tangani Segera** pada kolom AKSI, tampil hanya untuk `Arrived` dan `WaitingForTriage`; label "Memproses..." saat permintaan berjalan |
| `src/style/.../emergency-triage.module.css` | Satu kelas `.tableActionCell` — penataan letak dua tombol dalam satu sel |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensinya mengikuti pola modul ini apa adanya:
`view → hook → slice (thunk) → InstanceAxios`. Nol arsitektur state atau HTTP tandingan, nol
komponen baru, nol berkas baru pada `src/`.

Thunk-nya ditulis di slice triage yang sudah ada, bersebelahan dengan
`fetchEmergencyTriagePatients`, karena aksinya memang milik daftar pasien. Base URL kunjungan
(`EMERGENCY_VISIT_LIST_URL`) dipakai ulang, bukan disusun ulang. Angka enum diambil dari
konstanta `EMERGENCY_VISIT_STATUS`, tidak diketik tangan.

### 3.4 Gerbang keputusan base component

`UI GATE: REUSE` untuk seluruh elemen, dengan satu baris `COMPOSE` dan satu baris penataan letak.

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Tombol aksi kedua pada baris tabel | `styles.primaryMiniButton` | Sudah dipakai tombol "Isi Triage" pada berkas yang sama; di dalam tabel gayanya di-override menjadi putih bergaris tepi (`emergency-triage.module.css` baris 552-576) | `REUSE` | Dipakai apa adanya, tanpa varian warna baru |
| Dialog konfirmasi | `ConfirmModal` | `src/components/features/base-features/confirm-modal.jsx`; dipakai 10+ modul lain, antara lain `accounting-period-view.jsx` dan `doctor-discount-approvals-view.jsx` | `REUSE` | `variant="warning"`, `confirmLabel`, `loading`, `onConfirm`, `onCancel`, `onHide` |
| Isian alasan opsional di dalam dialog | `ConfirmModal` prop `requireReason` | Prop itu **mewajibkan** alasan — tombol konfirmasi mati saat kosong. Kriteria 4 menuntut alasan **opsional** | `COMPOSE` | `children` berisi `Form.Group` + `Form.Control` react-bootstrap, mengikuti bentuk isian alasan milik `ConfirmModal` sendiri (`fw-medium mb-1`, `rows={3}`) |
| Spanduk berhasil dan gagal | `styles.successBanner`, `styles.errorBanner` | Sudah ada pada modul CSS yang sama; `errorBanner` sudah dipakai layar ini | `REUSE` | Dipakai apa adanya |
| Badge status kunjungan | `styles.statusBadge` + `resolveEmergencyVisitStatusLabel` | Sudah ada, tidak disentuh | `REUSE` | Tidak diubah |
| Dua tombol berdampingan dalam satu sel | — | Modul ini tidak punya kelas sel aksi; `.headingActions` ada tetapi rata kanan, sedangkan sel AKSI rata tengah | Penataan letak | `.tableActionCell` — hanya `display`, `flex-wrap`, `align-items`, `justify-content`, dan `gap: 8px` mengikuti `.headingActions`. **Nol warna, nol tipografi, nol bayangan** |

Yang **tidak** dipakai beserta alasannya: `BaseButton` tidak dipakai karena tombol saudaranya
pada sel yang sama adalah `<button>` biasa bergaya modul, dan mencampur keduanya dalam satu sel
justru melahirkan dua bahasa tombol; `.ghostButton` tidak dipakai karena tinggi minimumnya 40px
sedangkan tombol tabel 34px, dan ia tidak punya penyesuaian khusus tabel.

**Satu pilihan diserahkan kepada pemilik** — lihat bagian 8: kedua tombol pada sel itu kini
tampil identik, dibedakan hanya oleh tulisannya.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat daftar | Perilaku `DataTable` yang sudah ada — *"Mengambil pasien IGD..."* |
| Kosong | *"Belum ada pasien IGD."* — tidak berubah |
| Aksi sedang berjalan | Tombol baris itu berbunyi *"Memproses..."*; seluruh tombol Tangani Segera dinonaktifkan; tombol konfirmasi dialog menampilkan pemintal |
| Berhasil | Daftar dimuat ulang sendiri, lalu perawat **dialihkan ke layar Assesmen IGD** milik pasien itu. Bila pengalihan tidak dapat dijalankan, spanduk hijau bernama pasien tampil sebagai gantinya |
| Gagal | Spanduk merah berisi pesan backend apa adanya. Daftar **tidak** dimuat ulang, supaya pesannya tidak langsung tertimpa |
| Tanpa hak akses | Penolakan `403` tampil pada spanduk merah yang sama, dengan pesan dari backend |
| Status tidak memenuhi syarat | Tombolnya tidak dirender sama sekali — perawat tidak pernah dihadapkan pada aksi yang pasti ditolak |

---

## 5. Endpoint yang dikonsumsi

#### EmergencyVisit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/v1/health-services/emergency-installation-management/emergency-visits/{id}/visit-status` | Memindahkan kunjungan ke `InTreatment` tanpa menunggu triage | `EmergencyVisit : Update` |
| `GET` | `/v1/health-services/emergency-installation-management/emergency-visits` | Memuat ulang daftar sesudah status berubah (sudah ada sebelumnya) | `EmergencyVisit : Read` |

Badan permintaan yang dikirim:

```json
{ "visitStatus": 4, "notes": "pasien tidak sadarkan diri" }
```

`treatmentStartedAt` **sengaja tidak dikirim**. Backend mengisinya sendiri saat target
`InTreatment` (`EmergencyVisitController.cs` baris 426-427); mengirimnya dari layar hanya
melahirkan dua sumber waktu yang bisa berbeda.

Jawaban yang mungkin diterima:

| Kode | Arti | Yang dilakukan layar |
| --- | --- | --- |
| `200` | Status berubah | Spanduk berhasil, daftar dimuat ulang |
| `400` | Transisi tidak diizinkan `CanTransition` | Pesan backend ditampilkan apa adanya |
| `403` | Izin `EmergencyVisit` + `Update` belum diberikan | Pesan penolakan ditampilkan apa adanya |
| `404` | Kunjungan tidak ditemukan | Pesan backend ditampilkan apa adanya |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa keluaran galat | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit` | `859/859` lulus | `PASS` | Keluaran perintah |
| Grep anti-regresi warna/tipografi pada CSS baru | Nihil | `PASS` | `grep -nEi "#[0-9a-f]{3,8}\b\|rgba?(\|font-size\|font-weight\|line-height\|!important"` pada blok `.tableActionCell` |
| Grep anti-regresi kelas bootstrap pada JSX | Nihil | `PASS` | `grep -nE "className=\"btn\|btn-primary\|fw-(bold\|semibold\|light)\|fs-[0-9]\|<table"` pada kedua berkas view |
| `npm run test:unit` | Tidak dijalankan | `NOT RUN` | Diketahui gagal karena glob skrip — masalah lama yang sama seperti `FE-IGD-023`, `024`, `028` |
| `npm run build` | Berhasil | `PASS` | Dijalankan Rizki 16 September 2026 |
| Menekan Tangani Segera pada satu pasien | **Berhasil.** BAGUS SETIAWAN berpindah dari "Pasien tiba" menjadi "Sedang ditangani"; barisnya menyisakan aksi "Lihat Riwayat" | `PASS` | Dijalankan pemilik 16 September 2026, tangkapan layar daftar triage. Grant izin `EmergencyVisit` + `Update` terbukti ada — permintaan tidak dijawab `403` |
| Pengalihan ke layar Assesmen IGD sesudah berhasil | **Berhasil.** Ditekan pada NABILA PUTRI MAHARANI, layar berpindah ke Assesmen IGD miliknya | `PASS` | Dijalankan pemilik 16 September 2026 |
| Mengisi triage susulan pada pasien `InTreatment` | **Berhasil.** Penilaian tersimpan dan status **tetap** "Sedang ditangani" — tidak mundur | `PASS` | Dijalankan pemilik 16 September 2026. Membuktikan `IGD-DEC-104` huruf (b) bekerja ujung ke ujung |

Uji manual: **`PASS`**. `NOT FEASIBLE` bagi agent — backend berjalan, kredensial perawat, dan
grant izin tidak tersedia pada lingkungannya — sehingga **dijalankan pemilik** pada 16 September
2026. Ketiga skenario lulus: aksi utama, pengalihan ke layar Assesmen IGD, dan triage susulan.

**Bukan UAT.** Yang di atas adalah verifikasi pengembangan oleh pemilik modul. UAT dijalankan tim
terpisah dan **belum** dilakukan untuk task ini.

**Tidak dijalankan:** `npm run test:e2e` dan `test:uat` (tidak diminta task).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Aksi tampil hanya untuk `Arrived` dan `WaitingForTriage` | **Terpenuhi pada source; belum diuji lewat layar** | `bolehDitanganiSegera()` pada `emergency-triage-patient-table.jsx` membandingkan terhadap konstanta enum |
| 2. Meminta konfirmasi lebih dulu, memakai pola yang sudah ada | **Terpenuhi pada source** | `ConfirmModal` base component; tidak ada pengiriman langsung dari tombol |
| 3. Berhasil → `InTreatment`, badge berubah tanpa muat ulang halaman penuh | **Terpenuhi, dengan delta** | **Terbukti lewat layar 16 September 2026:** BAGUS SETIAWAN berpindah menjadi "Sedang ditangani" tanpa muat ulang halaman penuh. **Delta:** sejak keputusan pemilik hari yang sama, perawat kini **dialihkan** ke layar Assesmen IGD sesudah berhasil, sehingga perubahan badge itu tidak lagi ia amati di daftar. Daftar tetap dimuat ulang lewat thunk, bukan `location.reload` |
| 4. Alasan boleh diisi dan dikirim sebagai `notes`; dikosongkan tetap sah | **Terpenuhi pada source** | Isian opsional pada dialog; thunk mengirim `notes` hasil `trim()`, string kosong tetap dikirim dan diterima backend |
| 5. Penolakan tampil apa adanya, termasuk `400` dan `403`; tanpa penangan galat tandingan | **Terpenuhi pada source** | `normalizeErrorMessage` yang sudah ada dipakai; tidak ada pemetaan kode status sendiri |
| 6. Triage susulan tetap tersimpan dan status tetap `InTreatment` | **Terpenuhi** | **Terbukti lewat layar 16 September 2026:** penilaian tersimpan dan status tetap "Sedang ditangani". `IGD-DEC-104` huruf (b) bekerja ujung ke ujung, bukan hanya di kode |
| 7. `TreatmentStartedAt` tidak dikirim dari layar | **Terpenuhi** | Badan permintaan hanya memuat `visitStatus` dan `notes` |
| 8. Layar Resusitasi tidak dibangun dan tidak disentuh | **Terpenuhi** | Nol berkas resusitasi pada diff; pencarian "resusitasi" pada `src/` tetap nol hasil |

**Definition of Done gelombang R3.8: seluruh butir terpenuhi.** Lint, unit test, `npm run build`,
catatan uji layar, laporan tracked, roadmap dan traceability, serta nol komponen bersama/CSS
global. Uji lewat layar dijalankan pemilik dan hasilnya dicatat apa adanya — utang lama
*"alur simpan lewat layar belum pernah dijalankan sungguhan"* yang berlaku sejak roadmap
revision `1` akhirnya lunas untuk jalur ini. **Tanpa UAT PASS** — UAT milik tim terpisah.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` dari lint dan test |
| Masalah yang diketahui | `NONE` yang menahan. Grant izin `EmergencyVisit` + `Update` terbukti ada. Satu pilihan tampilan masih terbuka — kedua tombol pada kolom AKSI tampil identik; lihat bagian 8 di bawah |
| Dependency backend | `NONE` yang menahan. Endpoint dan penjaga transisinya sudah ada sejak `BE-IGD-018` ✅ |
| Perubahan sampingan | `NONE`. Pekerjaan `FE-IGD-028` yang belum di-commit pada working tree **tidak** disentuh |
| Interupsi | `NONE` |
| Status Git | **Sudah di-commit** pemilik pada `36f122af9` *"memperbaiki action button in triage pasien IGD"* (branch `RizkiV2`), bersama `FE-IGD-028` dan `FE-IGD-029`. Working tree frontend bersih |
| Langkah berikutnya | `NONE` yang menahan. Commit sudah dilakukan (`36f122af9`) dan pilihan tampilan sudah ditutup. Serahkan ke tim UAT bila dikehendaki |

### Pilihan tampilan — **ditutup 16 September 2026**

Kedua tombol pada kolom AKSI **tampil identik** — putih bergaris tepi — dan hanya dibedakan
tulisannya. Itu konsekuensi dari memakai ulang `primaryMiniButton` apa adanya, sesuai batas
wewenang UI task ini.

Dua pilihan diajukan kepada pemilik:

1. **Biarkan identik.** Nol gaya tombol baru, paling aman terhadap konsistensi visual.
   Konsekuensinya, aksi yang memindahkan pasien ke penanganan terlihat sama ringannya dengan
   aksi membuka formulir.
2. **Beri tombol Tangani Segera gaya penekan sendiri** — satu kelas tambahan pada modul CSS
   fitur. Diajukan sebagai rekomendasi agent, karena pada layar gawat darurat perbedaan bobot
   dua aksi sebaiknya terlihat sebelum dibaca.

**Pemilik memilih pilihan 1** pada 16 September 2026, berbeda dari rekomendasi agent. Tidak ada
perubahan kode yang diperlukan — pilihan 1 adalah keadaan yang sudah terpasang. Konsekuensi yang
diterima: kedua aksi tampil dengan bobot visual yang sama, dan pembedanya adalah tulisan tombol
beserta dialog konfirmasi yang muncul sebelum aksi berjalan. Pilihan ini dapat ditinjau ulang bila
pemakaian di lapangan menunjukkan perawat salah menekan.
