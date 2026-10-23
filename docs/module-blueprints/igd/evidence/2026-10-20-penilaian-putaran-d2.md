# Penilaian Putaran D2 dari bukti mentah — 20 Oktober 2026

| Field | Nilai |
| --- | --- |
| Putaran yang dinilai | D2 — Ruang Kerja Dokter IGD (Prioritas 2), dieksekusi 19 Oktober 2026, [laporan penguji](../testing/2026-10-19-laporan-uji-putaran-d2.md) |
| Bukti mentah | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-d2-20261019/` — 20 PNG, 30 JSON, `hasil.json`, `percobaan.json`, `persiapan.json`, `kode-awal.txt`, `kode-akhir.txt`, `kueri-fase-s.sql` |
| Dasar penilaian | Rekaman jaringan dan tangkapan layar dibaca satu per satu. **Tabel ringkasan pada laporan penguji tidak dipakai sebagai bukti** — beberapa angkanya tidak cocok dengan rekamannya sendiri (bagian 2) |
| Keabsahan baseline | `kode-awal.txt` dan `kode-akhir.txt` identik kecuali stempel waktu: backend `4cf5c1fd`, frontend `5f4ab690`. Source **tidak berubah** selama putaran — bukti sah untuk revisi itu |
| Putusan ringkas | **Nol task naik ke ✅.** Banyak kriteria memang terbukti, tetapi tidak satu task pun lengkap. Rincian per task di bagian 3 |

---

## 1. Mengapa tabel laporan penguji tidak dipakai apa adanya

Laporan penguji menandai 22 `PASS` dari 26 skenario. Sesudah tiap skenario dibandingkan dengan
berkas buktinya sendiri, **lima klaim tidak didukung rekamannya**. Keempat skenario `FAIL`/`NOT
RUN` justru akurat.

Ini bukan tuduhan itikad buruk. Pola yang terlihat: catatan ditulis dari ingatan langkah, bukan
dibacakan ulang dari JSON yang baru saja direkam.

## 2. Lima ketidakcocokan antara laporan dan bukti

| # | Fakta | Isi |
| ---: | --- | --- |
| 1 | `IGD-FACT-158` | **`D2-U14` ditandai `PASS` tanpa bukti apa pun.** Di `hasil.json` ruas `bukti` skenario itu **array kosong**, dan catatannya berbunyi _"Verifikasi tagihan billing disiapkan untuk verifikasi Rizki lewat layar Billing"_ — artinya diserahkan, bukan diverifikasi. Skenario ini satu-satunya yang memetakan `BE-IGD-068` acceptance 1 dan `FE-IGD-049` acceptance 3, sehingga keduanya **tetap belum terbukti** |
| 2 | `IGD-FACT-159` | **Ketiga angka `D2-A02` salah.** Laporan menulis _"semua kunjungan aktif (195 pasien)"_, _"saringan nama P-D2-A (1 pasien)"_, _"`awaitingClosure=true` (0 pasien)"_. Rekaman `jaringan-D2-A02.json` menunjukkan: panggilan 1 **tanpa saringan apa pun** `totalData` **244**; panggilan 2 `ongoing=true&search=Rizky` `totalData` **2**; panggilan 3 `ongoing=true&awaitingClosure=true` `totalData` **33**, seluruh 25 butir halaman pertama ber-`isAwaitingClosure: true` dan berstatus `7` (`Disposed`). Perilaku saringannya **benar**; yang salah angkanya di laporan |
| 3 | `IGD-FACT-160` | **Catatan `D2-U07` mengutip pesan 409 yang bukan pesan server.** Catatan menulis _"Kajian medis awal sudah ada untuk encounter ini. Gunakan kajian medis ulang untuk pembaruan."_; `jaringan-D2-U07.json` merekam pesan sebenarnya _"Kajian medis awal untuk kunjungan IGD ini sudah ada. Buka kajian itu, atau buat kajian ulang."_ Lebih jauh, `D2-U07-akhir.png` **tidak memuat pesan penolakan sama sekali** — isinya form kajian ulang pada langkah Planning. Kriteria `FE-IGD-046` acceptance 2 menuntut pesan server tampil **apa adanya di layar** dengan PNG; sisi API terbukti, sisi layar tidak |
| 4 | `IGD-FACT-161` | **`D2-A06` tidak menguji aturan yang diminta `BE-IGD-067` acceptance 3.** Kriterianya: encounter yang **bukan milik kunjungan IGD** → `400`, dan encounter **tidak ada** → `404`. Yang diuji: GUID nol → `400` dengan pesan _"Encounter wajib disebut."_ (itu validasi ruas kosong), dan string bukan GUID → `404` yang menurut laporan sendiri berasal dari _routing constraint_ (itu penguraian URL). Keduanya bukan aturan bisnis yang dimaksud. Acceptance 1 juga belum terpenuhi: kriterianya menuntut encounter dengan **dua** catatan dan `totalCount` **2**; rekamannya `totalCount` **1** |
| 5 | `IGD-FACT-162` | **Header laporan menyatakan _"Tanpa eksekusi SQL manual/SELECT"_, padahal `kueri-fase-s.sql` (9,5 KB) ada di folder bukti yang sama** dan berisi `SELECT` terhadap `MstDoctor`, `AspNetUsers`, dan `information_schema` yang dijalankan pada Fase S. Isinya **read-only**, sehingga hasil fungsional putaran ini tidak gugur. Tetapi ini ulangan `IGD-FACT-148`, dan kali ini laporannya aktif menyangkal. Saran `IGD-FACT-148` (cabut `QUILVIAN_DEV_DB_URL` dari lingkungan penguji) belum dijalankan |

## 3. Penilaian per task

Kolom *Sesudah D2* adalah hitungan yang bertahan sesudah bukti dibaca sendiri.

| Task | Sebelum | Sesudah D2 | Kriteria yang baru terbukti | Yang masih kurang |
| --- | :---: | :---: | --- | --- |
| `BE-IGD-065` | 3/8 | **7/8** 🟡 | 1 (`A01`: 12 kunjungan, seluruhnya `activeDoctorId` dr. Farhan Malik; P-D2-B tanpa DPJP dan P-D2-C `Completed` tidak ikut; status yang muncul hanya 2, 3, 4, 6 — nol `Cancelled`/`Completed`), 3 (`A01`+`A02`: `ongoing` bergabung dengan `doctorId`, `search`, dan `awaitingClosure`), 4 (`A03`: `activeDoctorId`/`activeDoctorName` detail sama dengan `emergency-doctor-assignments/active`; P-D2-B `null` dan endpoint aktifnya `404`), 5 (`A03`: `isAwaitingClosure`, `awaitingClosureReason`, `patientId`, `patientName`, `isUnknownPatient`, `temporaryPatientAlias` tetap ada) | **Acceptance 2.** Bukti yang diminta kartu: _"Uji API sebelum–sesudah atau pembandingan jumlah"_. Yang terekam hanya angka **sesudah** (244 tanpa saringan). Perbandingan butuh dua angka |
| `BE-IGD-066` | 2/7 | **6/7** 🟡 | 1 (`U06`: `POST patient-assessments` `201` dengan `inpEpisodeId: null`, lalu detail dan daftar `200`), 2 (`U07`: kajian awal kedua `409`; kajian ulang `assessmentType` 5 `201`), 3 (`A04`: kunjungan `Completed` → `409` _"Kunjungan IGD ini sudah berakhir…"_), 4 (`A05`: akun perawat → `403` _"Catatan ini hanya dapat ditulis dokter."_) | **Acceptance 5** — regresi rawat inap dan poliklinik tidak disentuh putaran ini |
| `BE-IGD-067` | 3/6 | **4/6** 🟡 | 2 (`A06`: encounter tanpa catatan → `200`, `totalCount` 0, `items` kosong) | **Acceptance 1** (butuh encounter dengan dua catatan, `totalCount` 2; yang ada 1 — bentuk `SoapTimelineItemResponse` dan `200` sudah cocok) dan **acceptance 3** (`IGD-FACT-161`) |
| `BE-IGD-068` | 4/5 | **4/5** 🟡 | — | **Acceptance 1** tetap kosong (`IGD-FACT-158`). Tidak ada pergerakan |
| `FE-IGD-046` | 3/6 | **5/6** 🟡 | 1 (`U06`, rekaman tanpa `inpEpisodeId` terisi), 3 (`D2-U08-akhir.png`: kartu *Diagnosis / Problem List* memuat _"Belum ada diagnosis terstruktur pada kunjungan ini."_) | **Acceptance 2 sisi layar** (`IGD-FACT-160`). Sisi API-nya sudah terbukti |
| `FE-IGD-049` | 2/7 | **4/7** 🟡 | 2 (`U12`: resep `RX-20261019-00001` tampil baca-saja pada akun perawat `dimas.kurniawan`), 4 (`U19`: tanpa catatan dokter terbuka, tombol simpan resep tertahan dan pengait catatan wajib dipilih) | **Acceptance 1 sebagian** — resep `RX-20261019-00001` memang ada pada `GET prescriptions?encounterId=…` milik P-D2-A berstatus draf, tetapi **tidak ada satu pun panggilan `POST` resep terekam** di seluruh berkas bukti, sehingga klaim _"201 Created"_ tidak terdukung. **Acceptance 3** (`IGD-FACT-158`) dan **acceptance 5** (`IGD-UNK-12`, perilaku resep ber-`ClinicId` kosong) belum disentuh |

### Yang tidak dinilai ulang

`FE-IGD-045`, `FE-IGD-047`, dan `FE-IGD-048` tetap 🟡 pada hitungan lamanya. Ketiganya
tertahan hal yang sama seperti sebelum putaran ini: `D2-U03` dan `D2-U15` `NOT RUN` karena
akun dokter kedua belum disiapkan (`QUILVIAN_DOKTER2_PASSWORD`), dan `D2-U18` `FAIL` karena
peran Dokter IGD belum memiliki `ClinicalDocumentIntegrity` dan `ClinicalNoteAddendum`.
Keduanya langkah operasional, bukan cacat kode.

## 4. Satu pengamatan yang perlu diperiksa pemilik

Pada `D2-U08-akhir.png`, ketiga penghitung di bawah kartu diagnosis — *Semua Kajian*, *Kajian
Medis Awal*, *Kajian Medis Ulang* — seluruhnya menunjukkan **0 data**, padahal `D2-U06` dan
`D2-U07` baru saja membuat satu kajian awal dan satu kajian ulang `201` pada pasien yang sama
(P-D2-A, encounter `2239f4a3-d32d-433f-9b20-3e10496d8fc4`).

Ini **belum** dinyatakan cacat: penghitungnya mungkin memang menghitung hanya kajian yang sudah
diselesaikan, atau tangkapan layar diambil sebelum pemuatan ulang. Perlu satu pemeriksaan
layar untuk memastikan, dan **tidak** dibuatkan kartu task sebelum itu.

## 5. Akibat pada angka

| Lapisan | Sebelum | Sesudah penilaian ini |
| --- | --- | --- |
| Backend | 52 ✅ dari 65 aktif | **53 ✅ dari 65 aktif** (80,0% → **81,5%**) — kenaikan berasal dari `BE-IGD-035`, bukan dari D2 |
| Frontend | 39 ✅ dari 56 aktif | **39 ✅ dari 56 aktif** (69,6%, tidak berubah) |
| Gabungan | 91 dari 121 aktif (75,2%) | **92 dari 121 aktif (76,0%)** |

Putaran D2 **tidak menaikkan satu pun task ke ✅**. Yang dihasilkannya nyata tetapi lain
bentuknya: 11 kriteria acceptance baru terbukti pada 5 task, sehingga putaran berikutnya
tinggal mengejar daftar pendek di bagian 3.
