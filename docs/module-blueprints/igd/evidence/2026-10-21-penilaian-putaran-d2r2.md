# Penilaian Putaran D2R-2 dari bukti mentah — 21 Oktober 2026

| Field | Nilai |
| --- | --- |
| Putaran | D2R-2 — penutup sisa D2R dan verifikasi `IGD-DEC-337`, dieksekusi Antigravity 21 Oktober 2026 |
| Bukti mentah | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-d2r2-20261021/` — 6 PNG, 8 JSON jaringan, `hasil.json`, `percobaan.json`, `data-uji.json`, `kode-awal.txt`, `kode-akhir.txt` |
| Dasar penilaian | `IGD-DEC-332` — dibaca dari rekaman jaringan dan tangkapan layar, bukan dari tabel ringkasan |
| Prasyarat build | **Terpenuhi.** `DoctorConsultationController.cs` diubah 21 Oktober 09:43:37; DLL dibangun 09:53:31 — perbaikan `IGD-DEC-337` masuk ke build yang diuji |
| Putusan ringkas | **`IGD-DEC-337` terverifikasi penuh.** Nol task naik ✅. Satu `FAIL` jujur dengan sebab yang sudah ditunjuk, dua klaim `PASS` tidak didukung bukti, dua temuan baru |

---

## 1. `IGD-DEC-337` — terverifikasi, dan inilah hasil utama putaran ini

| ID | Bukti mentah | Putusan |
| --- | --- | --- |
| `D2R2-A01a` | `POST doctor-consultations` → **`200`**, `CON-20261021-00001` (`e4fd594d…`), ruas `aktor` pada rekaman `dokter.igd.b4@rsmmc.local`, encounter `ccabfdb0…` yang sudah memuat draf terbuka `CON-20261020-00002` milik dokter lain | **PASS** — penghalang ganti shift tercabut |
| `D2R2-A01b` | `POST` kedua oleh dokter yang sama → **`400`** *"Masih ada draf catatan dokter yang belum diselesaikan untuk kunjungan IGD ini."* | **PASS** — penjaga asli masih bekerja, kini per dokter |
| `D2R2-A01c` | `GET soap-timeline` → `200`, `totalCount` **3** | **PASS** — regresi `BE-IGD-067` aman |

Dua sisi keputusan itu terbukti sekaligus: dokter pengganti dapat menulis, dan satu dokter tetap
tidak boleh menumpuk draf. **`IGD-DEC-337` dinyatakan terverifikasi.**

## 2. Verifikasi per skenario sisanya

| ID | Klaim | Putusan | Dasar |
| --- | --- | --- | --- |
| `D2R2-U01` | `FAIL` — modal addendum tidak muncul, bug props `target` vs `documentItem` pada `ActionList` | **`FAIL` benar, dan dilaporkan jujur** | `jaringan-D2R2-U01.json` memuat **2 panggilan, nol panggilan tulis** — konsisten dengan modal yang tidak pernah terbuka. Penguji menandai `FAIL` apa adanya dan menunjuk sebabnya. `FE-IGD-048` acceptance 2 tetap belum terbukti, tetapi kini **sebabnya diketahui** |
| `D2R2-U02` | `PASS` — banner `409` tertangkap | **Sisi API terbukti, sisi layar TIDAK** | `jaringan-D2R2-U02.json` memuat `409` dengan kalimat tepat. Tetapi `D2R2-U02-akhir.png` memperlihatkan tab *Pengkajian Medis* **tanpa banner penolakan** — isinya kartu diagnosis, penghitung kajian, dan Riwayat Kajian Medis. **Ini kegagalan ketiga berturut-turut pada butir yang sama** (`D2-U07`, `D2R-U04`, kini `D2R2-U02`) |
| `D2R2-U03` | `PASS` | **PASS** | `POST pharmacy-management/prescriptions` → **`201`** *"Header resep berhasil dibuat."*, `RX-20261021-00001` (`0ce93a36…`). Resep benar-benar terbentuk pada catatan yang masih terbuka — berbeda dari dua putaran sebelumnya yang selalu `400` |
| `D2R2-U04` | `PASS` — payload `clinicId: null`, respons `clinicId: null` dan `clinicName: null` | **Pengamatannya sah, redaksinya keliru** | Pada rekaman, payload `POST` **tidak memuat ruas `clinicId` sama sekali**, dan respons pembuatan tidak mengembalikan `clinicId` maupun `clinicName`. Perilaku yang benar untuk dicatat: *resep IGD terbentuk tanpa mengirim `ClinicId`, dan respons pembuatan tidak membawa ruas klinik*. Kriterianya hanya menuntut pencatatan apa adanya, sehingga tetap terpenuhi dengan redaksi yang dikoreksi |
| `D2R2-U05` | `PASS` — saringan nonaktif, daftar *Semua* tetap tampil | **TIDAK TERBUKTI** | Dua hal. (1) Akun yang dipakai `rendy.saputra@rsmmc.local` adalah **Petugas Loket**, bukan akun berperan dokter yang kebetulan tanpa `doctorId` — yang diuji karena itu tembok perizinan, bukan jalur *tanpa identitas dokter*. (2) Separuh kedua kriteria gagal di rekamannya sendiri: `GET emergency-visits` → **`403`** *"Anda tidak memiliki akses ke menu atau fitur ini."*, sehingga daftar *Semua pasien IGD* **tidak tampil** |
| `D2R2-R01` | `NOT RUN` | **Benar** | Wewenang pemilik di layar Billing |
| `D2R2-U06` | `PASS` — No. Rujukan tersimpan dan terbukti saat dibuka ulang | **Sebagian** | `POST patient-encounters/admin` → **`200`**, `ENC-RSMMC-00522`, dan `referralNumber: "RUJ-20261021-00889"` memang **terkirim pada payload** — pendaftaran kali ini benar-benar diselesaikan, bukan berhenti di Langkah 5. Namun nomor itu **tidak muncul pada respons**, dan **tidak ada satu pun panggilan baca-ulang** yang merekamnya kembali. Rekaman juga memuat `POST` kedua yang ditolak **`403`**. Penyimpanannya terbukti; **pembacaan ulangnya belum** |

## 3. Dua temuan baru

### 3.1 `IGD-FACT-190` — catatan ditandatangani DPJP, bukan penulisnya

`D2R2-A01c` memperlihatkan ketiga butir lini masa bernama **dr. Jenah Ali**, termasuk
`CON-20261021-00001` yang dibuat Dokter B. Sebabnya ada di source:

```csharp
DoctorId = queue?.DoctorId ?? request.DoctorId ?? encounter.DoctorId ?? Guid.Empty
```

`DoctorConsultationController.cs:602`. Payload Dokter B tidak mengirim `doctorId`, dan kunjungan
IGD tidak punya baris antrean, sehingga nilainya jatuh ke **DPJP kunjungan** — bukan penulis.

Ini melemahkan tujuan `IGD-DEC-337` pada titik yang paling penting: dokter pengganti kini **dapat**
menulis, tetapi tulisannya **tercatat atas nama dokter lain**. Pada rekam medis, pertanggungjawaban
siapa menulis apa adalah inti.

Identitas penulis sebenarnya **sudah tersimpan** — `StartedByUserId` dan `CreateBy`, yang dipakai
`EnsureSoleAuthorAsync`. Jadi datanya ada; yang kurang hanya penyajiannya. Dua jalan keluar:

| Opsi | Isi | Catatan |
| --- | --- | --- |
| (a) | Isi `DoctorId` dari dokter yang menulis bila berbeda dari DPJP | Mengubah arti `DoctorId` yang sudah dipakai modul lain — berisiko |
| (b) | Biarkan `DoctorId` sebagai DPJP, tambahkan penyajian penulis (`StartedByUserId`) pada lini masa dan *Catatan Saya* | Aditif, tidak mengubah arti ruas lama |

Rekomendasi penilai: **opsi (b)**. Diangkat sebagai `IGD-OQ-140`; keputusan pemilik.

### 3.2 `IGD-FACT-191` — sebab kegagalan addendum sudah ditunjuk

`D2R2-U01` menandai `FAIL` dan menyebut sebabnya: ketidakcocokan props `target` lawan
`documentItem` pada `ActionList`, sehingga modal addendum tidak pernah terbuka. Rekamannya
konsisten — nol panggilan tulis. Ini **pertama kalinya** butir itu punya sebab yang dapat
dikerjakan, sesudah dua putaran hanya berstatus belum terbukti.

## 4. Tertib bukti — yang membaik dan yang belum

| Hal | Keadaan |
| --- | --- |
| Prasyarat build | **Dipatuhi.** DLL 09:53:31 lebih baru dari perubahan source 09:43:37 |
| Pelaporan `FAIL` | **Membaik.** `D2R2-U01` dilaporkan `FAIL` apa adanya beserta sebab teknisnya, tanpa dipaksa `PASS` |
| Tangkapan layar saat pesan tampil | **Belum.** Kegagalan ketiga berturut-turut pada butir yang sama (`D2R2-U02`) |
| Kecocokan klaim dengan rekaman | **Belum.** `D2R2-U05` diklaim `PASS` padahal rekamannya `403`; `D2R2-U04` menyebut nilai ruas yang tidak ada di rekaman |

## 5. Akibat pada status task

| Task | Sebelum | Sesudah | Sisa |
| --- | :---: | :---: | --- |
| `FE-IGD-049` | 4/7 | **6/7 🟡** | Acceptance 1 terbukti (`201`) dan 5 terbukti (pengamatan). **Sisa acceptance 3** — fakta tagih di layar Billing, menunggu `D2R2-R01` |
| `FE-IGD-045` | 9/10 | **9/10 🟡** | Acceptance 4 — `D2R2-U05` menguji akun yang salah dan daftarnya `403` |
| `FE-IGD-046` | 5/6 | **5/6 🟡** | Acceptance 2 sisi layar, kegagalan ketiga |
| `FE-IGD-048` | 5/6 | **5/6 🟡** | Acceptance 2 — kini dengan sebab yang ditunjuk (`IGD-FACT-191`) |
| `BE-IGD-068` | 4/5 | **4/5 🟡** | Acceptance 1 — `D2R2-R01` `NOT RUN` |

| Lapisan | Nilai |
| --- | --- |
| Backend | 56 ✅ dari 64 aktif (87,5%) |
| Frontend | 40 ✅ dari 56 aktif (71,4%) |
| Gabungan | **96 dari 120 aktif (80,0%)** — tidak berubah |

## 6. Sisa yang benar-benar tinggal lima

| # | Butir | Yang dibutuhkan | Pemilik |
| ---: | --- | --- | --- |
| 1 | `FE-IGD-048` acc 2 | Perbaiki props `ActionList` (`IGD-FACT-191`), lalu ulangi `D2R2-U01` | Frontend |
| 2 | `FE-IGD-046` acc 2 | Tangkap layar **selagi banner `409` masih tampil** — bukan sesudahnya | Penguji |
| 3 | `FE-IGD-045` acc 4 | Akun **berperan dokter** yang tidak tertaut baris dokter, bukan akun loket; daftar *Semua* harus benar-benar tampil | Rizki + penguji |
| 4 | `FE-IGD-049` acc 3 + `BE-IGD-068` acc 1 | Buka rincian tagihan pasien `P-X-B` di layar Billing | Rizki |
| 5 | `IGD-OQ-140` | Keputusan penyajian penulis catatan | Pemilik |
