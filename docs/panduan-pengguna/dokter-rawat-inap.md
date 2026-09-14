# Panduan Pengguna — Dokter, Rawat Inap

| Field | Nilai |
| --- | --- |
| Untuk siapa | Admin sistem rumah sakit, admin SDM, dan dokter pengguna layar |
| Layar yang dibahas | Menu **Dokter → Rawat Inap**, beserta **Workspace Dokter** di dalamnya |
| Alamat layar | `/health-services/inpatient-management/doctor-inpatient` |
| Sifat dokumen | Panduan operasional. Bukan kontrak dan bukan keputusan bisnis |
| Tanggal | 9 September 2026 |

---

## 1. Kenapa dokumen ini ada

Keluhan yang paling sering muncul berbunyi seperti ini: *"Dokternya sudah saya jadikan
penanggung jawab pasien, tapi di menunya tetap kosong."*

Kalimat itu memuat satu salah paham yang besar. Menjadi DPJP pada seorang pasien **tidak
memberi izin apa pun** kepada akun dokter tersebut. Keduanya adalah dua hal terpisah yang
dikelola di layar berbeda oleh orang yang berbeda.

Dokumen ini menjelaskan gerbang mana saja yang harus dilewati, siapa yang membukanya, dan
bagaimana membaca pesan yang muncul ketika salah satunya masih tertutup.

---

## 2. Tiga gerbang yang berbeda

Seorang dokter baru dapat bekerja di layar rawat inap setelah **ketiganya** terbuka. Gerbang
yang tertutup menghasilkan gejala yang berbeda-beda, dan itulah kunci untuk mendiagnosisnya.

| # | Gerbang | Artinya | Siapa yang membukanya | Di mana |
| --- | --- | --- | --- | --- |
| 1 | **Identitas dokter** | Akun login terhubung ke data dokter di master | Admin SDM | Sumber Daya Manusia → Dokter |
| 2 | **Hak akses** | Departemen dan Posisi milik akun itu memegang butir hak akses yang dibutuhkan | Admin sistem | Administrator → Settings → Role Access |
| 3 | **Kewenangan pasien** | Dokter tercatat sebagai DPJP aktif pada episode pasien | Petugas admisi atau kepala ruangan | Rawat Inap → Episode |

Peran ketiganya berbeda tajam:

- **Gerbang 1 dan 2 menentukan apakah layar bisa dibuka.**
- **Gerbang 3 menentukan pasien mana yang muncul di dalamnya, dan pada pasien mana dokter
  boleh menulis.**

Membuka gerbang 3 tanpa gerbang 2 adalah penyebab keluhan yang disebut di bagian 1. Pasiennya
sudah ditugaskan, tetapi layarnya menolak terbuka karena izinnya belum ada.

---

## 3. Gerbang 1 — identitas dokter

### 3.1 Apa yang diperiksa sistem

Akun login menyimpan penunjuk ke data dokter. Tanpa penunjuk itu, sistem tidak tahu daftar
pasien siapa yang harus ditampilkan, dan tidak ada dokumentasi klinis yang boleh ditulis atas
nama akun tersebut.

### 3.2 Cara membukanya

1. Buka **Sumber Daya Manusia → Dokter**.
2. Buat data dokter, atau sunting yang sudah ada.
3. Aktifkan pembuatan akun login untuk dokter tersebut.
4. **Isi Departemen dan Posisi utamanya.** Bagian ini sering terlewat dan akibatnya berat,
   lihat peringatan di bawah.
5. Simpan.

> **Peringatan yang menentukan.**
> Akun dokter yang dibuat **tanpa** Departemen dan Posisi utama tidak akan memiliki baris
> organisasi pengguna. Seluruh pemeriksaan hak akses dijoin lewat pasangan Departemen dan
> Posisi, sehingga akun seperti ini **ditolak di hampir setiap menu**, bukan hanya di rawat
> inap. Memberi hak akses di Role Access pun tidak akan menolong, karena tidak ada pasangan
> yang bisa dicocokkan.
>
> Perbaikannya: sunting kembali data dokter, isi Departemen dan Posisi utamanya, lalu simpan.
> Penyimpanan itu yang membuat baris organisasi yang dibutuhkan.

---

## 4. Gerbang 2 — hak akses

### 4.1 Cara kerjanya

Hak akses **tidak** diturunkan dari nama peran, dari jabatan dokter, atau dari status DPJP.
Ia dibaca dari tabel kebijakan yang dicocokkan dengan pasangan **Departemen + Posisi** milik
akun.

Artinya hak akses diberikan kepada **posisi di sebuah departemen**, bukan kepada orang. Semua
dokter yang menempati posisi yang sama di departemen yang sama otomatis memegang hak yang
sama. Ini disengaja: menambah dokter baru tidak menuntut penyetelan izin satu per satu.

### 4.2 Cara memberikannya

1. Buka **Administrator → Settings → Role Access**.
2. Pilih **Departemen** dan **Posisi** milik dokter yang bersangkutan.
3. Centang butir yang dibutuhkan menurut tabel di bagian 4.3.
4. Simpan.

Perubahan langsung berlaku di sisi server. Tetapi daftar izin ikut dibaca saat login, jadi
**dokter tersebut perlu logout lalu login kembali** supaya menunya menyesuaikan.

### 4.3 Butir yang dibutuhkan

**Paket minimum — supaya layar daftar pasien terbuka.**

| Modul di layar Role Access | Menu | Action |
| --- | --- | --- |
| Health Service Inpatient | Inpatient Census | `Read` |

Dengan satu butir ini saja, menu **Dokter → Rawat Inap** sudah menampilkan daftar pasien.

**Paket lengkap — supaya Workspace Dokter dapat dipakai.**

| Modul di layar Role Access | Menu | Action | Dipakai untuk |
| --- | --- | --- | --- |
| Health Service Inpatient | Inpatient Census | `Read` | Daftar pasien pintu masuk |
| Health Service Inpatient | Inpatient Episode | `Read` | Kepala konteks pasien dan verifikasi kewenangan DPJP |
| Health Service Clinical | Patient Allergy | `Read` | Penanda alergi |
| Health Service Clinical | Patient Diagnosis | `Read`, `Create`, `Update` | Daftar masalah dan diagnosis kerja |
| Health Service Clinical | Patient Assessment | `Read`, `Create`, `Update` | Tab Kajian Medis |
| Health Service Clinical | Doctor Consultation | `Read`, `Create`, `Update` | Tab Catatan Perkembangan |
| Health Service Clinical | Patient Integrated Progress Note | `Read`, `Create`, `Update`, `Verify` | Tab Catatan Terpadu. `Verify` hanya untuk DPJP |
| Health Service Clinical | Physician Visit | `Read`, `Create`, `Update`, `Cancel` | Tab Visite |
| Health Service Clinical | Patient Procedure | `Read`, `Create`, `Update` | Tab Resep dan Tindakan |
| Health Service Pharmacy | Prescription | `Read`, `Create` | Tab Resep dan Tindakan |
| Health Service Laboratory Management | Lab Order | `Read`, `Create` | Tab Penunjang |
| Health Service Radiology Management | Rad Order | `Read`, `Create` | Tab Penunjang |
| Health Service Medical Record | Clinical Note Addendum | `Read`, `Create` | Koreksi catatan yang sudah final |

**Pembeda antar peran dokter.**

| Peran | Bedanya dari paket lengkap |
| --- | --- |
| DPJP | Memegang seluruhnya, termasuk `Verify` pada Catatan Terpadu |
| Dokter jaga ruangan | Sama dengan DPJP, **tanpa** `Verify` |
| Dokter konsulen | `Read` pada seluruhnya, ditambah `Create` pada Catatan Terpadu dan Physician Visit |
| Supervisor klinis | `Read` pada seluruhnya, ditambah `Cancel` pada Physician Visit |

> Berikan paket lengkapnya sekaligus. Memberi satu butir dalam satu waktu membuat dokter
> tertahan berulang kali di tab yang berbeda-beda, dan setiap penahanan tampil sebagai pesan
> yang mirip satu sama lain.

---

## 5. Gerbang 3 — kewenangan atas pasien

Setelah dua gerbang pertama terbuka, layarnya terbuka tetapi **daftarnya bisa saja kosong.**
Itu bukan kesalahan. Daftar hanya memuat pasien yang episodenya mencatat dokter tersebut
sebagai dokter aktif.

| Keadaan | Yang terlihat |
| --- | --- |
| Belum ditugaskan pada pasien mana pun | "Belum ada pasien rawat inap yang ditugaskan kepada Anda." |
| Sudah ditugaskan | Pasien muncul beserta lokasi, hari rawat, dan tombol Workspace Dokter |
| Ditugaskan, tetapi penugasannya sudah diakhiri | Pasien hilang dari daftar |

Penugasan DPJP diatur pada **Rawat Inap → Episode**, bukan di layar dokter.

Satu hal yang sering mengejutkan: pasien yang kepergian fisiknya sudah dicatat **tidak lagi
muncul di census** walaupun episodenya belum ditutup. Census diturunkan dari baris penempatan
yang masih aktif, jadi pasien yang sudah meninggalkan ruangan memang tidak punya jalan untuk
muncul kembali di sana.

---

## 6. Cara dokter memakai layarnya

1. Login, lalu buka menu **Dokter → Rawat Inap**.
2. Layar menampilkan seluruh pasien rawat inap yang menjadi tanggung jawab dokter yang sedang
   login. Tidak perlu memilih dokter, dan memang tidak bisa: daftar selalu dibatasi identitas
   dokter pada session.
3. Persempit dengan penyaring unit layanan, kelas perawatan, atau pencarian bebas bila
   pasiennya banyak.
4. Tekan **Workspace Dokter** pada baris pasien untuk membuka dokumentasi klinisnya.

Layar membaca ulang daftarnya setiap kali jendela difokuskan kembali, sehingga pasien yang
pindah atau pulang selagi layar terbuka tidak tertinggal sebagai data basi.

### 6.1 Enam tab di dalam Workspace Dokter

| Tab | Isinya |
| --- | --- |
| Kajian Medis | Kajian medis awal pasien pada episode ini |
| Catatan Perkembangan | Catatan harian dokter beserta waktu pemeriksaannya |
| Catatan Terpadu | Catatan lintas profesi beserta verifikasi DPJP |
| Visite | Riwayat kunjungan dokter pada episode ini |
| Resep & Tindakan | Resep rawat inap dan tindakan dokter |
| Penunjang | Permintaan laboratorium dan radiologi |

---

## 7. Membaca pesan yang muncul

### 7.1 Di layar daftar pasien

| Yang terlihat | Artinya | Perbaikannya |
| --- | --- | --- |
| **"Konteks dokter tidak tersedia"** | Salah satu dari dua hal: akun belum terhubung ke data dokter, atau akun belum memegang `Inpatient Census : Read` | Jalankan pemeriksaan di bagian 7.3, lalu buka gerbang yang sesuai |
| **"Belum ada pasien rawat inap yang ditugaskan kepada Anda."** | Dua gerbang pertama sudah terbuka. Dokter memang belum menjadi DPJP pasien mana pun | Tugaskan lewat Rawat Inap → Episode. Bukan masalah izin |
| **"Tidak ada pasien yang cocok dengan penyaring ini."** | Penyaring terlalu sempit | Tekan Atur Ulang Penyaring |
| **"Daftar pasien gagal dimuat"** | Gangguan jaringan atau server, bukan izin | Tekan Coba Lagi |

> Pesan pertama perlu dibaca hati-hati karena ia menutupi dua sebab sekaligus. Jangan langsung
> menyimpulkan data dokternya bermasalah. Sebab yang jauh lebih sering adalah hak aksesnya
> yang belum diberikan.

### 7.2 Di dalam Workspace Dokter

Tombol tulis dinonaktifkan berdasarkan pemeriksaan yang dijalankan berurutan. Yang pertama
gagal itulah yang ditampilkan.

| Urutan | Pesan | Artinya | Perbaikannya |
| --- | --- | --- | --- |
| 1 | "Akun ini tidak memiliki izin membaca episode" | `Inpatient Episode : Read` belum diberikan | Role Access |
| 2 | "Data pasien tidak dapat dimuat" | Konteks pasien dan episode gagal dibaca | Coba lagi. Selama gagal, seluruh penulisan sengaja ditahan |
| 3 | "Konteks pasien masih dibaca" | Sementara. Sedang memuat | Tunggu |
| 4 | "Episode ini sudah ditutup atau dibatalkan" | Episodenya memang sudah selesai | Koreksi dokumen final memakai addendum |
| 5 | "Akun ini tidak terhubung ke data dokter" | Gerbang 1 belum terbuka | Sumber Daya Manusia → Dokter |
| 6 | "Kewenangan pada episode ini belum dapat diverifikasi" | Penugasan DPJP gagal dibaca atau izinnya ditolak | Beri `Inpatient Episode : Read`, lalu muat ulang |
| 7 | "Anda bukan DPJP yang berlaku pada episode ini" | Gerbang 3. Layar menyebutkan siapa DPJP aktifnya | Alihkan DPJP lewat Rawat Inap → Episode bila memang perlu |

> **Dua aturan keselamatan yang sengaja dipertahankan.**
> Kegagalan membaca kewenangan **tidak pernah** diperlakukan sebagai izin menulis. Dan
> kegagalan membaca riwayat alergi selalu ditampilkan menonjol, tidak pernah disembunyikan,
> karena ketiadaan penanda mudah terbaca sebagai "pasien tidak punya alergi" dan bagi
> peresepan itu berbahaya.

### 7.3 Pemeriksaan cepat yang menyelesaikan perdebatan

Sambil login sebagai dokter yang bermasalah, buka alamat ini di tab browser yang sama:

```
https://<alamat-server>/api/v1/Auth/permissions
```

Jawabannya adalah daftar pasangan resource dan action yang **benar-benar** dipegang akun itu.

| Jawaban | Kesimpulan | Perbaikannya |
| --- | --- | --- |
| Daftarnya kosong | Akun belum punya Departemen dan Posisi aktif | Bagian 3.2, termasuk peringatannya |
| Terisi, tanpa `InpatientCensus` / `Read` | Pasangan Departemen dan Posisi sudah ada, izinnya belum diberikan | Bagian 4.3 |
| Ada `InpatientCensus` / `Read`, tetapi layar tetap menolak | Izinnya baru diberikan setelah dokter login | Logout lalu login kembali |

Satu langkah ini memisahkan gerbang 1 dari gerbang 2 tanpa perlu menebak.

---

## 8. Salah paham yang paling sering terjadi

| Yang dikira | Yang sebenarnya |
| --- | --- |
| "Sudah jadi DPJP, berarti sudah punya akses" | Penugasan DPJP menentukan **pasien mana** yang muncul. Izin membuka layarnya diberikan terpisah di Role Access |
| "Hak akses diberikan per dokter" | Hak akses diberikan per **Departemen + Posisi**. Semua dokter pada posisi yang sama memegang hak yang sama |
| "Izinnya sudah dicentang, kok masih ditolak" | Daftar izin dibaca saat login. Dokter perlu logout lalu login kembali |
| "Layarnya kosong berarti izinnya kurang" | Daftar kosong beserta kalimat "Belum ada pasien rawat inap yang ditugaskan" justru menandakan izinnya **sudah benar** |
| "Cukup beri Inpatient Census saja" | Cukup untuk daftar pasien. Workspace Dokter menuntut butir lain pada tiap tabnya |
| "Akunnya bisa dibuat dulu, departemennya menyusul" | Tanpa Departemen dan Posisi, akun itu ditolak di hampir seluruh menu sistem |

---

## 9. Ringkasan untuk admin yang sedang menyiapkan dokter baru

1. **Sumber Daya Manusia → Dokter.** Buat data dokter, aktifkan akun login, **isi Departemen
   dan Posisi utamanya**, simpan.
2. **Administrator → Settings → Role Access.** Pilih Departemen dan Posisi tadi, centang paket
   pada bagian 4.3, simpan. Cukup sekali per posisi, bukan per dokter.
3. **Rawat Inap → Episode.** Tugaskan dokter itu sebagai DPJP pada pasiennya.
4. Minta dokter tersebut login, lalu buka **Dokter → Rawat Inap**.
5. Bila masih tertolak, jalankan pemeriksaan pada bagian 7.3 sebelum menebak penyebabnya.

---

## 10. Rujukan teknis

Pembaca yang ingin menelusuri sampai ke source dapat mulai dari berkas berikut.

| Yang dicari | Berkas |
| --- | --- |
| Penjaga hak akses pada endpoint census | `Areas/HealthServices/InPatientManagement/Controllers/InpatientCensusController.cs` |
| Cara hak akses dievaluasi | `Services/Security/AccessPermissionService.cs` |
| Pembuatan baris organisasi pengguna untuk dokter | `Areas/Corporate/HumanResource/MasterData/Workforce/Controllers/DoctorController.cs` |
| Peta peran ke butir hak akses | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/contracts/permission-audit-matrix.md` |
| Peta peran pada sub-modul episode | `docs/module-blueprints/rawat-inap/episode-rawat-inap/contracts/permission-audit-matrix.md` |
