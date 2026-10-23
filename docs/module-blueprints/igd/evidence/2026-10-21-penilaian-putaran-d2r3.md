# Penilaian Putaran D2R-3 dari bukti mentah — 21 Oktober 2026

| Field | Nilai |
| --- | --- |
| Putaran | D2R-3 — lima butir terakhir sisa D2R, dieksekusi Antigravity 21 Oktober 2026 |
| Bukti mentah | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-d2r3-20261021/` — 13 PNG, 4 JSON jaringan, `hasil.json`, `percobaan.json`, `kode-awal.txt`, `kode-akhir.txt` |
| Dasar penilaian | `IGD-DEC-332` — dibaca dari rekaman jaringan dan tangkapan layar |
| Putusan ringkas | **4 dari 4 skenario terbukti. Lima task naik ✅.** Ini putaran pertama yang verifikasinya **membenarkan laporan penguji sepenuhnya** |

---

## 1. Verifikasi per skenario

| ID | Putusan | Dasar |
| --- | --- | --- |
| `D2R3-U01` | **PASS** | `POST clinical-note-addendums/by-document/2/62b9…` → **`201`** *"Koreksi berhasil ditambahkan."* `D2R3-U01-akhir.png` memperlihatkan modal **"Riwayat Addendum Catatan"** terbuka — sebelumnya mengembalikan `null` — dengan **Addendum #1**, *Penulis Koreksi: dr. Jenah Ali*, stempel 21 Okt 2026 11.22, isi koreksi riwayat alergi amoxicillin. Baris pertama di belakang modal bertanda **`Addendum (1)`**, berubah dari `(0)`. Ketiga tuntutan kriteria terpenuhi |
| `D2R3-U02` | **PASS** | `POST patient-assessments` → `409`. `D2R3-U02-akhir.png` memuat **banner merah** berisi kalimat server apa adanya: *"Kajian medis awal untuk kunjungan IGD ini sudah ada. Buka kajian itu, atau buat kajian ulang."* Tertangkap pada percobaan keempat sesudah `IGD-FACT-197` menjelaskan bahwa banner tidak hilang sendiri melainkan berada di area yang dapat di-skrol |
| `D2R3-U03` | **PASS** | Akun perawat `dimas.kurniawan` (tanpa `doctorId`). `GET emergency-visits?ongoing=true` → **`200`**, `totalData` **196** — bukan `403` seperti akun loket. `D2R3-U03-akhir.png`: badge *Pasien saya* **hilang** dari header, saringan tinggal *Semua pasien IGD*, keterangan tampil *"Saringan Pasien saya nonaktif karena akun ini belum terhubung ke data dokter. Daftar menampilkan semua pasien IGD."*, dan daftar terisi **"100 dari 196 pasien"**. Kedua separuh kriteria terpenuhi |
| `D2R3-R01` | **PASS** | Invoice **`BIL-20261021-00000001`**, `encounterId` `7dab5614-06ba-4d4b-9a6c-b73b6f8bb38b`, `serviceType` **IGD**, `runningGrossAmount` 650000. `items` berisi **tepat satu baris**: `sourceDomain` `PHARMACY`, `categoryCode` `DRUG`, `descriptionSnapshot` *"Resep RX-20261021-00001"*, `sourceDetailId` `0ce93a36-4a61-428f-af96-0362a85e2210` — **persis id resep** yang dibuat `D2R2-U03`. **Nol baris jasa konsultasi.** Kedua separuh kriteria terpenuhi sekaligus |

### Catatan atas `D2R3-R01`

Panggilan finalisasi (`PATCH …/complete`) **tidak terekam** pada `jaringan-D2R3-R01.json`, yang
berisi sesi Billing saja. Itu tidak melemahkan putusan: fakta tagih hanya terbit saat finalisasi
(`ConsultationFinalizationService.FinalizeAsync` menetapkan `Completed` di `:141`, menerbitkan
fakta resep di `:252`). Kehadiran baris `PHARMACY` pada invoice **adalah** buktinya bahwa
finalisasi terjadi — lebih kuat daripada rekaman panggilannya.

## 2. Koreksi data yang ditemukan sebelum putaran

Laporan D2R-2 menyebut pasien resep sebagai RM `00-00-02-74`. Pembacaan `jaringan-D2R2-U03.json`
menunjukkan resep `RX-20261021-00001` menempel pada encounter `7dab5614…`, yaitu RM **`00-00-02-48`**
(*Pasien Pengganti 1791259153562_563*). Koreksi ini disampaikan sebelum `D2R3-R01` dijalankan;
tanpa itu pemilik akan membuka pasien yang salah di layar Billing dan melihat tagihan kosong.

## 3. Perubahan status

| Task | Sebelum | Sesudah |
| --- | :---: | :---: |
| `FE-IGD-045` | 9/10 | **10/10 ✅** |
| `FE-IGD-046` | 5/6 | **6/6 ✅** |
| `FE-IGD-048` | 5/6 | **6/6 ✅** |
| `FE-IGD-049` | 6/7 | **7/7 ✅** |
| `BE-IGD-068` | 4/5 | **5/5 ✅** |

| Lapisan | Sebelum | Sesudah |
| --- | --- | --- |
| Backend | 56 ✅ dari 64 aktif (87,5%) | **57 ✅ dari 64 aktif (89,1%)** |
| Frontend | 40 ✅ dari 56 aktif (71,4%) | **44 ✅ dari 56 aktif (78,6%)** |
| Gabungan | 96 dari 120 aktif (80,0%) | **101 dari 120 aktif (84,2%)** |

## 4. Yang tersisa sesudah putaran ini

| # | Butir | Sifat |
| ---: | --- | --- |
| 1 | `IGD-OQ-140` — penyajian penulis catatan bila berbeda dari DPJP | Keputusan pemilik |
| 2 | `IGD-DEC-336` — migration indeks `IX_TrxDoctorConsultation_EncounterId` | Menahan **commit dan rilis**, bukan pengembangan. Dikerjakan sesudah merge `MHamzah` |
| 3 | Gelombang 2 — Putaran D3 (`BE-IGD-070`, `FE-IGD-050`, `051`, `054`, `055`) | Siap jalan, panduan bagian 5 |
